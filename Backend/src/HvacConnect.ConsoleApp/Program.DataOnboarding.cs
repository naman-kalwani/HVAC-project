using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Validation;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HvacConnect.ConsoleApp;

public static partial class Program
{
    private static async Task RunImportWizard(HvacDbContext db) => await VendorImportPipeline.RunAsync(db, _currentUser!, Prompt, PrintError, PrintSuccess);

    private static async Task RunDuplicateReview(HvacDbContext db)
    {
        Console.WriteLine("\n=== IMPORT DUPLICATE / CHANGE REVIEW ===");
        var candidates = await db.DuplicateCandidates.Include(d => d.ImportJob).ThenInclude(j => j!.Vendor)
            .Include(d => d.ImportRow).Include(d => d.ExistingCanonicalCustomer)
            .Where(d => d.Decision == DuplicateDecision.PendingReview).OrderBy(d => d.CreatedAt).ToListAsync();
        if (candidates.Count == 0) { Console.WriteLine("No pending duplicate reviews."); return; }

        foreach (var candidate in candidates)
        {
            var data = VendorImportPipeline.Deserialize(candidate.ImportRow?.MappedDataJson);
            string existing = candidate.ExistingCanonicalCustomer?.CustomerName ?? "(customer not found)";
            if (candidate.EntityType == "Sites")
                existing = await VendorImportPipeline.ExistingSiteLabelAsync(db, candidate, data) ?? existing;
            else if (candidate.EntityType == "Assets")
                existing = await VendorImportPipeline.ExistingAssetLabelAsync(db, candidate, data) ?? existing;

            Console.WriteLine($"\n[{candidate.DuplicateCode}] {candidate.EntityType} from {candidate.ImportJob?.Vendor?.BrandName} / {candidate.ImportJob?.OriginalFileName}, row {candidate.ImportRow?.RowNumber}");
            Console.WriteLine($"Imported: {candidate.CandidateName}");
            Console.WriteLine($"Existing: {existing}");
            Console.WriteLine($"Match: {candidate.ConfidenceScore}% ({candidate.ConfidenceLevel}) — {candidate.MatchReason}");
            Console.WriteLine("[A] Accept imported changes | [R] Reject changes and keep existing | [N] Keep as separate record | [S] Skip row");
            string action = Prompt("Decision: ").Trim().ToUpperInvariant();
            if (action is not ("A" or "R" or "N" or "S")) { PrintError("Invalid choice; leaving this item pending."); continue; }
            string notes = PromptValidated("Review notes: ", val => InputValidator.ValidateRequiredText(val, "Review notes", 3, 500));
            var vendor = candidate.ImportJob == null ? null : await db.Vendors.FindAsync(candidate.ImportJob.VendorId);

            if (action == "A")
            {
                await VendorImportPipeline.AcceptAsync(db, candidate, data, vendor);
                candidate.Decision = DuplicateDecision.LinkedToCustomer;
                candidate.ImportRow!.MatchOutcome = "ExistingChanged";
                candidate.ImportJob!.UpdatedRows++;
            }
            else if (action == "N")
            {
                await VendorImportPipeline.KeepSeparateAsync(db, candidate, data, vendor);
                candidate.Decision = DuplicateDecision.KeepSeparate;
                candidate.ImportRow!.MatchOutcome = "New";
            }
            else
            {
                candidate.Decision = DuplicateDecision.Skipped;
                candidate.ImportRow!.MatchOutcome = action == "R" ? "ExistingUnchanged" : "Skipped";
                candidate.ImportJob!.SkippedRows++;
            }
            candidate.ReviewNotes = notes;
            candidate.ReviewedByUserId = _currentUser!.Id;
            candidate.ReviewedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        foreach (var job in candidates.Where(c => c.ImportJob != null).Select(c => c.ImportJob!).DistinctBy(j => j.Id))
        {
            bool pending = await db.DuplicateCandidates.AnyAsync(d => d.ImportJobId == job.Id && d.Decision == DuplicateDecision.PendingReview);
            job.Status = pending ? ImportJobStatus.DuplicateReview : ImportJobStatus.Committed;
            if (!pending) job.CompletedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        foreach (var vendorId in candidates.Where(candidate => candidate.ImportJob != null)
                     .Select(candidate => candidate.ImportJob!.VendorId).Distinct())
        {
            await VendorOnboardingChecklistService.RefreshAutomaticMilestonesAsync(db, vendorId, _currentUser!.Id);
        }
        PrintSuccess("Review decisions saved.");
    }

    private static async Task ViewOnboardingChecklists(HvacDbContext db)
    {
        var vendors = await db.Vendors.OrderBy(v => v.BrandName).ToListAsync();
        if (vendors.Count == 0) { PrintError("No vendors are available."); return; }

        var rows = vendors.Select((vendor, index) => new[]
        {
            (index + 1).ToString(), vendor.VendorCode, vendor.BrandName,
            vendor.OnboardingPhase.ToString(), vendor.Status.ToString()
        }).ToList();
        TableFormatter.PrintPagedTable("Vendor Onboarding", new[] { "#", "Vendor Code", "Vendor", "Phase", "Status" }, rows, new[] { 4, 14, 28, 20, 18 });

        string vendorChoice = Prompt($"Select vendor to manage [1-{vendors.Count}] (0 to return): ");
        if (!int.TryParse(vendorChoice, out int choice))
        {
            PrintError("Enter a valid vendor number, or 0 to return.");
            return;
        }
        if (choice == 0) return;
        if (choice < 1 || choice > vendors.Count) { PrintError("Invalid vendor selection."); return; }
        await ManageVendorChecklist(db, vendors[choice - 1]);
    }

    private static async Task ManageVendorChecklist(HvacDbContext db, Vendor vendor)
    {
        while (true)
        {
            var tasks = await db.VendorOnboardingChecklists
                .Where(item => item.VendorId == vendor.Id)
                .OrderBy(item => item.Phase).ThenBy(item => item.SortOrder).ThenBy(item => item.Id)
                .ToListAsync();
            var userIds = tasks.Where(item => item.CompletedByUserId.HasValue).Select(item => item.CompletedByUserId!.Value).Distinct().ToArray();
            var users = await db.Users.Where(user => userIds.Contains(user.Id))
                .ToDictionaryAsync(user => user.Id, user => $"{user.FirstName} {user.LastName}".Trim());
            var rows = tasks.Select(item => new[]
            {
                item.Id.ToString(), item.Phase,
                VendorOnboardingChecklistService.IsAutomatic(item.TaskCode) ? "Auto" : "Manual",
                item.IsCompleted ? "Completed" : "Pending",
                item.TaskDescription,
                item.CompletedByUserId.HasValue && users.TryGetValue(item.CompletedByUserId.Value, out var by) ? by : "-"
            }).ToList();
            TableFormatter.PrintTable($"{vendor.BrandName} Onboarding Checklist", new[] { "ID", "Phase", "Mode", "Status", "Milestone", "Verified By" }, rows, new[] { 6, 10, 8, 12, 48, 22 });

            var phase1 = tasks.Where(item => item.Phase == "Phase 1").ToList();
            var phase2 = tasks.Where(item => item.Phase == "Phase 2").ToList();
            Console.WriteLine($"Phase 1: {phase1.Count(item => item.IsCompleted)}/{phase1.Count} complete | Phase 2: {phase2.Count(item => item.IsCompleted)}/{phase2.Count} complete");
            Console.WriteLine("Auto items follow import and duplicate-review data; Manual items require Super Admin confirmation.");
            Console.WriteLine("[C] Complete / reopen manual item  [A] Add item  [E] Edit item text  [D] Remove pending custom item");
            Console.WriteLine("[P] Approve Phase 1 and activate Phase 2  [F] Mark Fully Unified  [0] Back");
            switch (Prompt("Checklist action: ").Trim().ToUpperInvariant())
            {
                case "C": await CompleteOrReopenChecklistItem(db, vendor, tasks); break;
                case "A": await AddChecklistItem(db, vendor); break;
                case "E": await EditChecklistItem(db, vendor, tasks); break;
                case "D": await RemoveChecklistItem(db, vendor, tasks); break;
                case "P": await ApproveVendorPhase1(db, vendor, tasks); break;
                case "F": await CompleteVendorOnboarding(db, vendor, tasks); break;
                case "0": return;
                default: PrintError("Choose one of the listed actions."); break;
            }
        }
    }

    private static async Task CompleteOrReopenChecklistItem(HvacDbContext db, Vendor vendor, List<VendorOnboardingChecklist> tasks)
    {
        if (vendor.OnboardingPhase == OnboardingPhase.Completed) { PrintError("This vendor is fully unified; its checklist is locked."); return; }
        if (!int.TryParse(Prompt("Checklist item ID: "), out int id)) { PrintError("Enter a valid item ID."); return; }
        var item = tasks.FirstOrDefault(task => task.Id == id);
        if (item == null) { PrintError("Checklist item not found for this vendor."); return; }
        if (IsChecklistPhaseLocked(vendor, item.Phase)) { PrintError($"{item.Phase} is approved and locked."); return; }
        if (item.Phase == "Phase 2" && !vendor.Phase1ApprovedAt.HasValue)
        { PrintError("Approve Phase 1 before completing Phase 2 milestones."); return; }
        if (VendorOnboardingChecklistService.IsAutomatic(item.TaskCode))
        {
            PrintError("This milestone is completed by an import or duplicate-review operation; update the underlying records instead.");
            return;
        }

        if (!item.IsCompleted && item.TaskCode == "cutover_approval")
        {
            if (!DateOnly.TryParse(Prompt("Planned cutover date (yyyy-MM-dd): "), CultureInfo.InvariantCulture, DateTimeStyles.None, out var cutoverDate))
            { PrintError("Enter the cutover date in yyyy-MM-dd format."); return; }
            vendor.CutoverDate = cutoverDate;
            vendor.OnboardingPhase = OnboardingPhase.Phase2_Cutover;
        }
        else if (item.IsCompleted && item.TaskCode == "cutover_approval")
        {
            vendor.CutoverDate = null;
            vendor.OnboardingPhase = OnboardingPhase.Phase2_Planning;
        }

        item.IsCompleted = !item.IsCompleted;
        item.CompletedByUserId = item.IsCompleted ? _currentUser!.Id : null;
        item.CompletedAt = item.IsCompleted ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        PrintSuccess(item.IsCompleted ? "Checklist milestone completed." : "Checklist milestone reopened.");
    }

    private static async Task AddChecklistItem(HvacDbContext db, Vendor vendor)
    {
        if (vendor.OnboardingPhase == OnboardingPhase.Completed) { PrintError("This vendor is fully unified; its checklist is locked."); return; }
        string phase = Prompt("Add to [1] Phase 1 or [2] Phase 2: ").Trim() switch
        {
            "1" => "Phase 1", "2" => "Phase 2", _ => ""
        };
        if (phase == "") { PrintError("Choose Phase 1 or Phase 2."); return; }
        if (IsChecklistPhaseLocked(vendor, phase)) { PrintError($"{phase} is approved and locked."); return; }
        string description = PromptValidated("Milestone description: ", value => InputValidator.ValidateRequiredText(value, "Milestone", 5, 250));
        int order = (await db.VendorOnboardingChecklists.Where(item => item.VendorId == vendor.Id && item.Phase == phase)
            .MaxAsync(item => (int?)item.SortOrder) ?? 0) + 1;
        db.VendorOnboardingChecklists.Add(new VendorOnboardingChecklist
        {
            VendorId = vendor.Id, Phase = phase, TaskDescription = description, SortOrder = order
        });
        await db.SaveChangesAsync();
        PrintSuccess("Checklist item added.");
    }

    private static async Task EditChecklistItem(HvacDbContext db, Vendor vendor, List<VendorOnboardingChecklist> tasks)
    {
        if (vendor.OnboardingPhase == OnboardingPhase.Completed) { PrintError("This vendor is fully unified; its checklist is locked."); return; }
        if (!int.TryParse(Prompt("Checklist item ID to edit: "), out int id)) { PrintError("Enter a valid item ID."); return; }
        var item = tasks.FirstOrDefault(task => task.Id == id);
        if (item == null) { PrintError("Checklist item not found for this vendor."); return; }
        if (IsChecklistPhaseLocked(vendor, item.Phase)) { PrintError($"{item.Phase} is approved and locked."); return; }
        item.TaskDescription = PromptValidated("New milestone description: ", value => InputValidator.ValidateRequiredText(value, "Milestone", 5, 250));
        await db.SaveChangesAsync();
        PrintSuccess("Checklist item updated. Automatic completion remains attached to this milestone.");
    }

    private static async Task RemoveChecklistItem(HvacDbContext db, Vendor vendor, List<VendorOnboardingChecklist> tasks)
    {
        if (vendor.OnboardingPhase == OnboardingPhase.Completed) { PrintError("This vendor is fully unified; its checklist is locked."); return; }
        if (!int.TryParse(Prompt("Pending custom checklist item ID to remove: "), out int id)) { PrintError("Enter a valid item ID."); return; }
        var item = tasks.FirstOrDefault(task => task.Id == id);
        if (item == null) { PrintError("Checklist item not found for this vendor."); return; }
        if (IsChecklistPhaseLocked(vendor, item.Phase)) { PrintError($"{item.Phase} is approved and locked."); return; }
        if (item.TaskCode != null || item.IsCompleted) { PrintError("Only pending custom checklist items can be removed. Core milestones and completed items are retained."); return; }
        db.VendorOnboardingChecklists.Remove(item);
        await db.SaveChangesAsync();
        PrintSuccess("Pending custom checklist item removed.");
    }

    private static async Task ApproveVendorPhase1(HvacDbContext db, Vendor vendor, List<VendorOnboardingChecklist> tasks)
    {
        if (vendor.Phase1ApprovedAt.HasValue) { PrintWarning("Phase 1 is already approved."); return; }
        var phase1 = tasks.Where(item => item.Phase == "Phase 1").ToList();
        if (phase1.Count == 0 || phase1.Any(item => !item.IsCompleted))
        { PrintError("Complete every Phase 1 milestone before approving cutover."); return; }
        vendor.Phase1ApprovedAt = DateTime.UtcNow;
        vendor.Phase1ApprovedByUserId = _currentUser!.Id;
        vendor.OnboardingPhase = OnboardingPhase.Phase2_Planning;
        await db.SaveChangesAsync();
        PrintSuccess("Phase 1 approved. Phase 2 is now active.");
    }

    private static async Task CompleteVendorOnboarding(HvacDbContext db, Vendor vendor, List<VendorOnboardingChecklist> tasks)
    {
        if (!vendor.Phase1ApprovedAt.HasValue) { PrintError("Approve Phase 1 before finalizing the vendor."); return; }
        if (vendor.OnboardingPhase == OnboardingPhase.Completed) { PrintWarning("Vendor is already fully unified."); return; }
        var phase2 = tasks.Where(item => item.Phase == "Phase 2").ToList();
        if (phase2.Count == 0 || phase2.Any(item => !item.IsCompleted))
        { PrintError("Complete every Phase 2 milestone before marking the vendor fully unified."); return; }
        vendor.OnboardingPhase = OnboardingPhase.Completed;
        vendor.Status = VendorStatus.Active;
        vendor.CompletedAt = DateTime.UtcNow;
        vendor.CompletedByUserId = _currentUser!.Id;
        await db.SaveChangesAsync();
        PrintSuccess($"{vendor.BrandName} is now a Fully Unified Vendor.");
    }

    private static bool IsChecklistPhaseLocked(Vendor vendor, string phase) =>
        vendor.OnboardingPhase == OnboardingPhase.Completed ||
        (phase == "Phase 1" && vendor.Phase1ApprovedAt.HasValue);
}
