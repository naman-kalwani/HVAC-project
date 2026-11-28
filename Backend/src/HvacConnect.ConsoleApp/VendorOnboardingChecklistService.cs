using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.ConsoleApp;

internal static class VendorOnboardingChecklistService
{
    private sealed record Milestone(string Code, string Phase, string Description, int Order, bool AutoComplete = false);

    private static readonly Milestone[] Milestones =
    [
        new("vendor_profile", "Phase 1", "Vendor profile and primary contact recorded", 1, true),
        new("imports_staged", "Phase 1", "Customer, site and asset import jobs staged with confirmed mappings", 2, true),
        new("duplicates_resolved", "Phase 1", "All imported duplicate candidates reviewed and resolved", 3, true),
        new("registry_committed", "Phase 1", "Customer, site and asset records committed to the registry", 4, true),
        new("coexistence_verified", "Phase 1", "Coexistence verified; legacy system remains primary", 5),
        new("quality_95", "Phase 2", "At least 95% of customer, site, and asset data is valid", 1, true),
        new("cutover_approval", "Phase 2", "Cutover date and business approval recorded", 2),
        new("technician_readiness", "Phase 2", "Technicians have completed training and a sample ticket", 3),
        new("reconciliation", "Phase 2", "Final customer, site, and asset data review completed", 4),
        new("production_cutover", "Phase 2", "Vendor is ready to handle live service work in HVAC Connect", 5)
    ];

    private static readonly Dictionary<string, string> LegacyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Initial intake questionnaire completed"] = "vendor_profile",
        ["Source system credentials and data owner identified"] = "vendor_profile",
        ["Customer and Location CSV import executed"] = "imports_staged",
        ["Customer, location and asset CSV exports received"] = "imports_staged",
        ["Source columns mapped and validated against platform schema"] = "imports_staged",
        ["Duplicate resolution review complete"] = "duplicates_resolved",
        ["Duplicate candidates reviewed and linked in registry"] = "duplicates_resolved",
        ["Data quality threshold >= 95% confirmed"] = "quality_95",
        ["Business leadership approval and planned cutover date established"] = "cutover_approval",
        ["Technicians trained on mobile app & ticket closure"] = "technician_readiness",
        ["Final data reconciliation and cutover plan approved"] = "reconciliation",
        ["Legacy software transitioned to read-only archive"] = "production_cutover"
    };

    public static async Task EnsureSchemaAndTemplatesAsync(HvacDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            $"ALTER TABLE \"{HvacDbContext.SchemaName}\".vendor_onboarding_checklists ADD COLUMN IF NOT EXISTS task_code character varying(50);");

        var untagged = await db.VendorOnboardingChecklists
            .Where(item => item.TaskCode == null)
            .ToListAsync();

        foreach (var item in untagged)
        {
            if (!LegacyCodes.TryGetValue(item.TaskDescription, out var code))
                continue;

            var milestone = Milestones.Single(definition => definition.Code == code);
            item.TaskCode = code;
            item.Phase = milestone.Phase;
            item.TaskDescription = milestone.Description;
            item.SortOrder = milestone.Order;
        }

        foreach (var vendor in await db.Vendors.ToListAsync())
        {
            var existing = await db.VendorOnboardingChecklists
                .Where(item => item.VendorId == vendor.Id)
                .ToListAsync();

            foreach (var definition in Milestones)
            {
                var matching = existing.Where(item => item.TaskCode == definition.Code).ToList();
                if (matching.Count == 0)
                {
                    var created = new VendorOnboardingChecklist
                    {
                        VendorId = vendor.Id,
                        TaskCode = definition.Code,
                        Phase = definition.Phase,
                        TaskDescription = definition.Description,
                        SortOrder = definition.Order
                    };
                    if (definition.Code == "vendor_profile")
                    {
                        created.IsCompleted = true;
                        created.CompletedByUserId = vendor.Users.FirstOrDefault(user => user.Role == UserRole.SuperAdmin)?.Id;
                        created.CompletedAt = vendor.CreatedAt;
                    }
                    db.VendorOnboardingChecklists.Add(created);
                    existing.Add(created);
                    continue;
                }

                var primary = matching[0];
                primary.Phase = definition.Phase;
                primary.TaskDescription = definition.Description;
                primary.SortOrder = definition.Order;
                if (definition.Code == "vendor_profile" && !primary.IsCompleted)
                {
                    primary.IsCompleted = true;
                    primary.CompletedAt = vendor.CreatedAt;
                }

                foreach (var duplicate in matching.Skip(1))
                {
                    if (!primary.IsCompleted && duplicate.IsCompleted)
                    {
                        primary.IsCompleted = true;
                        primary.CompletedByUserId = duplicate.CompletedByUserId;
                        primary.CompletedAt = duplicate.CompletedAt;
                    }
                    db.VendorOnboardingChecklists.Remove(duplicate);
                    existing.Remove(duplicate);
                }
            }
        }

        await db.SaveChangesAsync();
        foreach (var vendor in await db.Vendors.OrderBy(vendor => vendor.Id).ToListAsync())
        {
            int? actorId = await db.ImportJobs.Where(job => job.VendorId == vendor.Id)
                .OrderByDescending(job => job.CreatedAt)
                .Select(job => (int?)job.UploadedByUserId)
                .FirstOrDefaultAsync();
            await RefreshAutomaticMilestonesAsync(db, vendor.Id, actorId);
        }
    }

    public static List<VendorOnboardingChecklist> CreateInitialTasks(Vendor vendor, int actorUserId)
    {
        var createdAt = DateTime.UtcNow;
        return Milestones.Select(definition => new VendorOnboardingChecklist
        {
            Vendor = vendor,
            TaskCode = definition.Code,
            Phase = definition.Phase,
            TaskDescription = definition.Description,
            SortOrder = definition.Order,
            IsCompleted = definition.Code == "vendor_profile",
            CompletedByUserId = definition.Code == "vendor_profile" ? actorUserId : null,
            CompletedAt = definition.Code == "vendor_profile" ? createdAt : null
        }).ToList();
    }

    public static async Task RefreshAutomaticMilestonesAsync(HvacDbContext db, int vendorId, int? actorUserId)
    {
        var vendor = await db.Vendors.FindAsync(vendorId);
        if (vendor == null) return;

        var jobs = await db.ImportJobs
            .Where(job => job.VendorId == vendorId)
            .OrderBy(job => job.CreatedAt)
            .ThenBy(job => job.Id)
            .ToListAsync();

        var requiredTypes = new[] { "Customers", "Sites", "Assets" };
        var latestJobs = requiredTypes.Select(type => jobs
            .Where(job => job.EntityType.Equals(type, StringComparison.OrdinalIgnoreCase))
            .LastOrDefault()).ToList();
        bool allTypesStaged = latestJobs.All(job => job != null && !string.IsNullOrWhiteSpace(job.MappingConfigJson));

        var vendorJobIds = jobs.Select(job => job.Id).ToArray();
        bool hasPendingDuplicates = vendorJobIds.Length > 0 && await db.DuplicateCandidates
            .AnyAsync(candidate => vendorJobIds.Contains(candidate.ImportJobId) && candidate.Decision == DuplicateDecision.PendingReview);

        bool allTypesCommitted = allTypesStaged && latestJobs.All(job => job!.Status == ImportJobStatus.Committed);
        bool noPendingDuplicates = !hasPendingDuplicates;
        var totalRows = latestJobs.Where(job => job != null).Sum(job => job!.TotalRows);
        var validRows = latestJobs.Where(job => job != null).Sum(job => job!.ValidRows);
        bool qualityPassed = allTypesCommitted && totalRows > 0 && (double)validRows / totalRows >= 0.95;

        await SetAutoCompletionAsync(db, vendorId, "imports_staged", allTypesStaged, actorUserId);
        await SetAutoCompletionAsync(db, vendorId, "duplicates_resolved", allTypesStaged && noPendingDuplicates, actorUserId);
        await SetAutoCompletionAsync(db, vendorId, "registry_committed", allTypesCommitted && noPendingDuplicates, actorUserId);
        await SetAutoCompletionAsync(db, vendorId, "quality_95", qualityPassed, actorUserId);

        await db.SaveChangesAsync();
    }

    private static async Task SetAutoCompletionAsync(HvacDbContext db, int vendorId, string code, bool complete, int? actorUserId)
    {
        var items = await db.VendorOnboardingChecklists
            .Where(item => item.VendorId == vendorId && item.TaskCode == code)
            .ToListAsync();
        if (items.Count == 0)
        {
            // Ensure templates are loaded for operation-triggered updates.
            return;
        }

        foreach (var item in items)
        {
            // Automatic milestones mirror current database evidence, so they must be
            // able to reopen when a later import or duplicate review invalidates it.
            if (item.IsCompleted == complete) continue;
            item.IsCompleted = complete;
            item.CompletedByUserId = complete ? actorUserId : null;
            item.CompletedAt = complete ? DateTime.UtcNow : null;
        }
    }

    public static bool IsAutomatic(string? taskCode) =>
        Milestones.Any(item => item.Code == taskCode && item.AutoComplete);

    public static bool IsCoreMilestone(string? taskCode) =>
        Milestones.Any(item => item.Code == taskCode);

    public static IReadOnlyList<(string Code, string Phase, string Description, int Order, bool Automatic)> GetDefinitions() =>
        Milestones.Select(item => (item.Code, item.Phase, item.Description, item.Order, item.AutoComplete)).ToList();
}
