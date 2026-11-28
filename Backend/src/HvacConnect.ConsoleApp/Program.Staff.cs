using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Services;
using HvacConnect.Core.Validation;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.ConsoleApp;

public static partial class Program
{
    private static async Task ViewMyTechnicianSchedule(HvacDbContext db)
    {
        await RenderTechnicianSchedule(db, _currentUser!);
    }

    private static async Task ViewAnyTechnicianSchedule(HvacDbContext db)
    {
        Console.WriteLine("\n─── SELECT TECHNICIAN TO VIEW SCHEDULE ───");

        IQueryable<User> query = db.Users
            .Include(u => u.Vendor)
            .Where(u => u.Role == UserRole.Technician && u.IsActive);

        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            query = query.Where(u => u.VendorId == _currentUser.VendorId);
        }

        var techs = await query.ToListAsync();
        if (!techs.Any())
        {
            PrintError("No active technicians found in your scope.");
            return;
        }

        foreach (var technician in techs)
        {
            await RefreshTechnicianAvailabilityAsync(db, technician.Id);
        }

        var rows = techs.Select(t => new string[]
        {
            t.Id.ToString(),
            $"{t.FirstName} {t.LastName}",
            t.EmployeeCode ?? "N/A",
            t.Specialisation ?? "General HVAC",
            t.Vendor?.BrandName ?? "Local",
            t.AvailabilityStatus.ToString()
        }).ToList();

        TableFormatter.PrintPagedTable("Available Technicians", 
            new[] { "ID", "Name", "Code", "Specialisation", "Vendor", "Duty Status" }, rows);

        string tChoice = Prompt("\nEnter Technician ID to view schedule (or ENTER to cancel): ");
        if (string.IsNullOrWhiteSpace(tChoice)) return;
        if (!int.TryParse(tChoice, out int techId))
        {
            PrintError("Enter a valid technician ID, or press ENTER to cancel.");
            return;
        }

        var targetTech = techs.FirstOrDefault(t => t.Id == techId);
        if (targetTech != null)
        {
            await RenderTechnicianSchedule(db, targetTech);
        }
        else
        {
            PrintError($"Technician with ID {techId} was not found.");
        }
    }

    private static async Task RenderTechnicianSchedule(HvacDbContext db, User tech)
    {
        await RefreshTechnicianAvailabilityAsync(db, tech.Id);

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n═══════ TECHNICIAN PROFILE & JOB SCHEDULE ═══════");
        Console.ResetColor();
        Console.WriteLine($"Name:           {tech.FirstName} {tech.LastName}");
        Console.WriteLine($"Employee Code:  {tech.EmployeeCode ?? "N/A"}");
        Console.WriteLine($"Specialisation: {tech.Specialisation ?? "General HVAC"}");
        Console.WriteLine($"Vendor:         {tech.Vendor?.BrandName ?? "Independent"}");
        Console.WriteLine($"Availability:   {tech.AvailabilityStatus}");
        Console.WriteLine($"Phone:          {tech.PhoneNumber ?? "N/A"}");

        // Fetch all active / assigned work orders for this technician
        var tickets = await db.ServiceTickets
            .Include(t => t.SiteLocation)
            .Include(t => t.Asset)
            .Include(t => t.Customer)
            .Where(t => t.AssignedTechnicianId == tech.Id && t.Status != TicketStatus.Closed)
            .OrderBy(t => t.TargetResolutionAt)
            .ToListAsync();

        if (!tickets.Any())
        {
            Console.WriteLine("\n  [INFO] No active jobs or work orders currently assigned to this technician.");
            return;
        }

        var rows = new List<string[]>();
        foreach (var t in tickets)
        {
            var liveSla = t.TargetResolutionAt.HasValue
                ? SlaCalculator.ComputeState(t.CreatedAt, t.TargetResolutionAt.Value, t.ResolvedAt, DateTime.UtcNow)
                : SLAState.WithinSLA;

            string loc = $"{t.SiteLocation?.SiteName} ({t.SiteLocation?.City})";
            string deadline = t.TargetResolutionAt.HasValue ? t.TargetResolutionAt.Value.ToString("yyyy-MM-dd HH:mm UTC") : "No SLA";

            rows.Add(new string[]
            {
                t.TicketNumber,
                t.Status.ToString(),
                $"[{liveSla}]",
                t.Priority.ToString(),
                t.Asset?.AssetTag ?? "N/A",
                loc,
                deadline,
                t.Title
            });
        }

        TableFormatter.PrintTable($"Assigned Jobs for {tech.FirstName} {tech.LastName} ({tickets.Count} active)",
            new[] { "Ticket#", "Status", "SLA State", "Priority", "Asset", "Location / Address", "Deadline", "Title" },
            rows,
            new[] { 10, 12, 12, 10, 12, 24, 20, 28 });
    }

    private static async Task AssignTicketMenuEntry(HvacDbContext db)
    {
        Console.WriteLine("\n─── ASSIGN / REASSIGN TECHNICIAN TO TICKET ───");
        Console.Write("Enter Ticket# to assign: ");
        string? ticketNum = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(ticketNum)) return;

        var ticket = await db.ServiceTickets
            .Include(t => t.Asset)
            .Include(t => t.SiteLocation)
            .FirstOrDefaultAsync(t => t.TicketNumber.ToLower() == ticketNum.ToLower());

        if (ticket == null)
        {
            PrintError($"Ticket '{ticketNum}' not found.");
            return;
        }

        await AssignTechnicianSmartWizard(db, ticket);
    }

    private static async Task AddVendorWizard(HvacDbContext db)
    {
        Console.WriteLine("\n═══════ REGISTER NEW ACQUIRED CONTRACTOR (VENDOR) ═══════");
        string brandName = PromptValidated("Brand Name (e.g. Pacific Climate Solutions): ", val => InputValidator.ValidateOrganizationName(val, "Brand Name", 2, 150));
        string legalName = PromptValidated("Legal Entity Name: ", val => InputValidator.ValidateOrganizationName(val, "Legal Entity Name", 2, 200, required: false));
        string region = PromptValidated("Territory / Region (e.g. West Coast, Mid-Atlantic): ", val => InputValidator.ValidateRequiredText(val, "Region", 2, 100));
        string contactName = PromptValidated("Primary Operations Contact Name: ", val => InputValidator.ValidateContactName(val, "Primary Operations Contact Name", required: false));
        string email = PromptValidated("Primary Contact Email: ", InputValidator.ValidateEmail);
        string phone = PromptValidated("Primary Contact Phone (10 digits): ", InputValidator.ValidatePhone);

        Console.WriteLine("\nCreate the initial Vendor Admin login for this vendor:");
        string adminFirstName = PromptValidated("Admin First Name: ", val => InputValidator.ValidatePersonName(val, "First Name"));
        string adminLastName = PromptValidated("Admin Last Name: ", val => InputValidator.ValidatePersonName(val, "Last Name"));
        string adminEmail = PromptValidated("Admin Login Email: ", InputValidator.ValidateEmail);
        string adminPassword = PromptValidated("Initial Password (min 8 chars, upper/lower/digit/special): ", InputValidator.ValidatePassword);

        if (await db.Users.AnyAsync(u => u.Email.ToLower() == adminEmail.ToLower()))
        {
            PrintError($"A user account with email '{adminEmail}' already exists. Vendor was not created.");
            return;
        }

        string prefix = new string(brandName.Split(' ').Select(w => w.FirstOrDefault()).Where(c => char.IsLetter(c)).Take(3).ToArray()).ToUpper();
        if (prefix.Length < 3) prefix = (prefix + "VND").Substring(0, 3);
        string vendorCode = $"{prefix}-{new Random().Next(100, 999)}";

        var vendor = new Vendor
        {
            VendorCode = vendorCode,
            BrandName = brandName,
            LegalEntityName = string.IsNullOrWhiteSpace(legalName) ? brandName + " LLC" : legalName,
            Region = region,
            PrimaryContactName = contactName,
            PrimaryContactEmail = email,
            PrimaryContactPhone = phone,
            Status = VendorStatus.Active,
            OnboardingPhase = OnboardingPhase.Phase1_Coexist,
            IsActive = true
        };

        db.Vendors.Add(vendor);

        db.VendorOnboardingChecklists.AddRange(
            VendorOnboardingChecklistService.CreateInitialTasks(vendor, _currentUser!.Id));

        using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await db.SaveChangesAsync();
            var vendorAdmin = new User
            {
                Email = adminEmail,
                PasswordHash = PasswordService.HashPassword(adminPassword),
                FirstName = adminFirstName,
                LastName = adminLastName,
                Role = UserRole.VendorAdmin,
                VendorId = vendor.Id,
                IsActive = true
            };
            db.Users.Add(vendorAdmin);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            PrintSuccess($"Vendor '{brandName}' registered [{vendorCode}]. Vendor Admin login created for {adminEmail}.");
            Console.WriteLine("The vendor can sign in using 'Custom Email & Password Login' (option 6) and the password you set.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            PrintError($"Vendor and admin account were not saved: {ex.Message}");
        }
    }

    private static async Task AddVendorAdminWizard(HvacDbContext db)
    {
        Console.WriteLine("\n=== CREATE VENDOR ADMIN LOGIN ===");
        var vendors = await db.Vendors.Where(v => v.IsActive).OrderBy(v => v.BrandName).ToListAsync();
        if (vendors.Count == 0) { PrintError("No active vendors are available."); return; }
        for (int i = 0; i < vendors.Count; i++) Console.WriteLine($"{i + 1}. {vendors[i].BrandName} [{vendors[i].VendorCode}]");
        if (!int.TryParse(Prompt($"Select vendor [1-{vendors.Count}]: "), out int choice) || choice < 1 || choice > vendors.Count)
        { PrintError("Invalid vendor selection."); return; }

        var vendor = vendors[choice - 1];
        string firstName = PromptValidated("Admin First Name: ", val => InputValidator.ValidatePersonName(val, "First Name"));
        string lastName = PromptValidated("Admin Last Name: ", val => InputValidator.ValidatePersonName(val, "Last Name"));
        string email = PromptValidated("Admin Login Email: ", InputValidator.ValidateEmail);
        string password = PromptValidated("Initial Password (min 8 chars, upper/lower/digit/special): ", InputValidator.ValidatePassword);
        if (await db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower()))
        { PrintError($"A user account with email '{email}' already exists."); return; }

        db.Users.Add(new User
        {
            Email = email, PasswordHash = PasswordService.HashPassword(password), FirstName = firstName, LastName = lastName,
            Role = UserRole.VendorAdmin, VendorId = vendor.Id, IsActive = true
        });
        await db.SaveChangesAsync();
        PrintSuccess($"Vendor Admin login created for {vendor.BrandName}: {email}");
        Console.WriteLine("The vendor signs in with 'Custom Email & Password Login' (option 6) and the password you set.");
    }

    private static async Task AddTechnicianWizard(HvacDbContext db)
    {
        Console.WriteLine("\n═══════ ADD NEW FIELD TECHNICIAN ═══════");

        int vendorId;
        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            vendorId = _currentUser.VendorId!.Value;
        }
        else
        {
            var vendors = await db.Vendors.Where(v => v.IsActive).ToListAsync();
            if (!vendors.Any())
            {
                PrintError("No active vendors available. Please add a vendor first.");
                return;
            }

            Console.WriteLine("Select Vendor:");
            for (int i = 0; i < vendors.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{vendors[i].VendorCode}] {vendors[i].BrandName}");
            }
            string vChoice = Prompt($"Select Vendor [1-{vendors.Count}]: ");
            if (!int.TryParse(vChoice, out int vIdx) || vIdx < 1 || vIdx > vendors.Count)
            {
                PrintError("Invalid vendor selection.");
                return;
            }
            vendorId = vendors[vIdx - 1].Id;
        }

        string firstName = PromptValidated("First Name: ", val => InputValidator.ValidatePersonName(val, "First Name"));
        string lastName = PromptValidated("Last Name: ", val => InputValidator.ValidatePersonName(val, "Last Name"));
        string email = PromptValidated("Work Email: ", InputValidator.ValidateEmail);
        string password = PromptValidated("Initial Password: ", InputValidator.ValidatePassword);
        string empCode = PromptValidated("Employee Code (e.g. TECH-201): ", InputValidator.ValidateEmployeeCode);
        string phone = PromptValidated("Dispatch Phone (10 digits): ", InputValidator.ValidatePhone);

        Console.WriteLine("Specialisation: 1. HVAC / Chillers | 2. Commercial Refrigeration | 3. Boilers & Heating | 4. General RTU");
        string spec;
        while (true)
        {
            string sChoice = Prompt("Select Specialisation [1-4]: ");
            spec = sChoice switch
            {
                "1" => "HVAC / Chillers",
                "2" => "Commercial Refrigeration",
                "3" => "Boilers & Heating",
                "4" => "General RTU / HVAC",
                _ => string.Empty
            };
            if (spec.Length > 0) break;
            PrintError("Select a specialisation from 1 to 4.");
        }

        bool emailExists = await db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (emailExists)
        {
            PrintError($"An account with email '{email}' already exists.");
            return;
        }

        var tech = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = PasswordService.HashPassword(password),
            Role = UserRole.Technician,
            VendorId = vendorId,
            EmployeeCode = string.IsNullOrWhiteSpace(empCode) ? $"TECH-{new Random().Next(100, 999)}" : empCode,
            PhoneNumber = phone,
            Specialisation = spec,
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        };

        db.Users.Add(tech);
        await db.SaveChangesAsync();

        PrintSuccess($"Technician '{firstName} {lastName}' ({tech.EmployeeCode}) added successfully to roster!");
    }

    private static async Task AddServiceCoordinatorWizard(HvacDbContext db)
    {
        Console.WriteLine("\n═══════ ADD NEW SERVICE COORDINATOR (DISPATCHER) ═══════");

        int vendorId;
        string vendorName;
        if (_currentUser!.Role == UserRole.VendorAdmin)
        {
            if (_currentUser.VendorId is not int currentVendorId)
            {
                PrintError("Your Vendor Admin account is not associated with a vendor.");
                return;
            }

            var vendor = await db.Vendors.FirstOrDefaultAsync(v => v.Id == currentVendorId && v.IsActive);
            if (vendor == null)
            {
                PrintError("Your vendor is not active. A Service Coordinator cannot be created.");
                return;
            }

            vendorId = vendor.Id;
            vendorName = vendor.BrandName;
        }
        else if (_currentUser.Role == UserRole.SuperAdmin)
        {
            var vendors = await db.Vendors
                .Where(v => v.IsActive && v.Status == VendorStatus.Active)
                .OrderBy(v => v.BrandName)
                .ToListAsync();
            if (vendors.Count == 0)
            {
                PrintError("No active vendors are available to assign the Service Coordinator to.");
                return;
            }

            Console.WriteLine("Select the vendor for this Service Coordinator:");
            for (int i = 0; i < vendors.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {vendors[i].BrandName} [{vendors[i].VendorCode}]");
            }

            if (!int.TryParse(Prompt($"Select vendor [1-{vendors.Count}]: "), out int vendorChoice) ||
                vendorChoice < 1 ||
                vendorChoice > vendors.Count)
            {
                PrintError("Invalid vendor selection. Service Coordinator was not created.");
                return;
            }

            var selectedVendor = vendors[vendorChoice - 1];
            vendorId = selectedVendor.Id;
            vendorName = selectedVendor.BrandName;
        }
        else
        {
            PrintError("Only Super Admins and Vendor Admins can create Service Coordinators.");
            return;
        }

        string firstName = PromptValidated("First Name: ", val => InputValidator.ValidatePersonName(val, "First Name"));
        string lastName = PromptValidated("Last Name: ", val => InputValidator.ValidatePersonName(val, "Last Name"));
        string email = PromptValidated("Work Email: ", InputValidator.ValidateEmail);
        string password = PromptValidated("Initial Password: ", InputValidator.ValidatePassword);
        string phone = PromptValidated("Desk Phone (10 digits): ", InputValidator.ValidatePhone);

        bool emailExists = await db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        if (emailExists)
        {
            PrintError($"An account with email '{email}' already exists.");
            return;
        }

        var coord = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = PasswordService.HashPassword(password),
            Role = UserRole.ServiceCoordinator,
            VendorId = vendorId,
            PhoneNumber = phone,
            IsActive = true
        };

        db.Users.Add(coord);
        await db.SaveChangesAsync();

        PrintSuccess($"Service Coordinator '{firstName} {lastName}' created for {vendorName}!");
    }

    private static async Task ListVendorsOnly(HvacDbContext db)
    {
        var vendors = await db.Vendors.Include(v => v.Users).ToListAsync();
        var rows = vendors.Select(v => new string[]
        {
            v.VendorCode,
            v.BrandName,
            v.Region,
            v.Status.ToString(),
            v.OnboardingPhase.ToString(),
            v.Users.Count(u => u.Role == UserRole.Technician).ToString()
        }).ToList();

        TableFormatter.PrintPagedTable("Acquired Vendors",
            new[] { "Vendor Code", "Brand Name", "Territory / Region", "Status", "Onboarding Phase", "Field Techs" },
            rows,
            new[] { 13, 28, 22, 12, 18, 12 });
    }

    private static async Task ListTechniciansAndStaff(HvacDbContext db)
    {
        IQueryable<User> query = db.Users.Include(u => u.Vendor)
            .Where(u => u.Role == UserRole.Technician || u.Role == UserRole.ServiceCoordinator);

        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            query = query.Where(u => u.VendorId == _currentUser.VendorId);
        }

        var staff = await query.OrderBy(u => u.Role).ThenBy(u => u.FirstName).ToListAsync();
        foreach (var technician in staff.Where(s => s.Role == UserRole.Technician))
        {
            await RefreshTechnicianAvailabilityAsync(db, technician.Id);
        }

        var rows = staff.Select(s => new string[]
        {
            s.Id.ToString(),
            $"{s.FirstName} {s.LastName}",
            s.Role.ToString(),
            s.Role == UserRole.Technician ? (s.Specialisation ?? "General HVAC") : "Central Dispatch Desk",
            s.Vendor?.BrandName ?? "Corporate",
            s.AvailabilityStatus.ToString()
        }).ToList();

        TableFormatter.PrintPagedTable("Field Technicians & Service Coordinators",
            new[] { "ID", "Name", "Role", "Specialisation / Unit", "Vendor", "Duty Status" },
            rows,
            new[] { 6, 20, 18, 24, 24, 14 });
    }
}
