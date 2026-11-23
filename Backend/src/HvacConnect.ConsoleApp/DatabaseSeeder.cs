using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Services;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.ConsoleApp;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(HvacDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await VendorOnboardingChecklistService.EnsureSchemaAndTemplatesAsync(db);

        // Check if already seeded
        if (await db.Users.AnyAsync()) return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[INFO] Database is empty. Seeding initial test data for all 5 roles...");
        Console.ResetColor();

        // 1. Create Super Admin
        var admin = new User
        {
            Email = "admin@hvacconnect.com",
            PasswordHash = PasswordService.HashPassword("Admin@123"),
            FirstName = "Shivansh",
            LastName = "Rastogi",
            Role = UserRole.SuperAdmin,
            IsActive = true
        };
        db.Users.Add(admin);
        await db.SaveChangesAsync();

        // 2. Create Acquired Contractor (Vendor)
        var vendor = new Vendor
        {
            VendorCode = "PCS-001",
            BrandName = "Pacific Climate Solutions",
            LegalEntityName = "Pacific Climate Solutions LLC",
            Region = "West Coast (CA, OR, WA)",
            Status = VendorStatus.Active,
            OnboardingPhase = OnboardingPhase.Phase1_Coexist,
            PrimaryContactName = "Jordan Lee",
            PrimaryContactEmail = "jordan.lee@pacificclimate.com",
            PrimaryContactPhone = "415-555-0199"
        };
        db.Vendors.Add(vendor);
        await db.SaveChangesAsync();

        // 3. Create Vendor Admin & Technician
        var vendorAdmin = new User
        {
            Email = "jordan.lee@pacificclimate.com",
            PasswordHash = PasswordService.HashPassword("Vendor@123"),
            FirstName = "Jordan",
            LastName = "Lee",
            Role = UserRole.VendorAdmin,
            VendorId = vendor.Id,
            IsActive = true
        };

        var technician = new User
        {
            Email = "casey.reed@pacificclimate.com",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Casey",
            LastName = "Reed",
            EmployeeCode = "TECH-104",
            Specialisation = "HVAC / Chillers",
            Role = UserRole.Technician,
            VendorId = vendor.Id,
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        };

        // 4. Create Service Coordinator
        var coordinator = new User
        {
            Email = "coordinator@hvacconnect.com",
            PasswordHash = PasswordService.HashPassword("Coord@123"),
            FirstName = "Alex",
            LastName = "Morgan",
            Role = UserRole.ServiceCoordinator,
            IsActive = true
        };

        db.Users.AddRange(vendorAdmin, technician, coordinator);
        await db.SaveChangesAsync();

        // 5. Create Canonical Customer & Facility Manager (End User)
        var customer = new CanonicalCustomer
        {
            CanonicalCode = "CUST-00128",
            CustomerName = "Apex Retail Group, LLC",
            NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName("Apex Retail Group, LLC"),
            CustomerType = CustomerType.Retail,
            PrimaryContactName = "Marcus Vance",
            PrimaryContactEmail = "m.vance@apexretail.example",
            BillingAddress = "100 Market St, San Francisco, CA 94105",
            Status = CustomerStatus.Active
        };
        db.CanonicalCustomers.Add(customer);
        await db.SaveChangesAsync();

        var endUser = new User
        {
            Email = "m.vance@apexretail.example",
            PasswordHash = PasswordService.HashPassword("User@123"),
            FirstName = "Marcus",
            LastName = "Vance",
            Role = UserRole.EndUser,
            CustomerId = customer.Id,
            IsActive = true
        };
        db.Users.Add(endUser);

        // 6. Link Brand Customer Account
        var brandAccount = new BrandCustomerAccount
        {
            CanonicalCustomerId = customer.Id,
            VendorId = vendor.Id,
            LocalAccountNumber = "A-104",
            LocalCustomerName = "Apex Retail Group",
            ContactEmail = "m.vance@apexretail.example",
            BillingAddress = "100 Market St, San Francisco, CA",
            AccountStatus = "Active"
        };
        db.BrandCustomerAccounts.Add(brandAccount);

        // 7. Create Site Location
        var site = new SiteLocation
        {
            SiteCode = "LOC-00812",
            CanonicalCustomerId = customer.Id,
            PrimaryVendorId = vendor.Id,
            SiteName = "Apex Supercenter #104",
            AddressLine = "450 Mission Bay Blvd",
            City = "San Francisco",
            State = "CA",
            PostalCode = "94158",
            OperatingHours = "06:00 - 23:00",
            AccessInstructions = "Loading dock entrance on 4th St. Ring bell for manager.",
            SiteContactName = "Dave Miller",
            SiteContactPhone = "415-555-0144"
        };
        db.SiteLocations.Add(site);
        await db.SaveChangesAsync();

        // 8. Create Asset Equipment
        var asset = new AssetEquipment
        {
            AssetTag = "RTU-00182",
            SiteLocationId = site.Id,
            Category = AssetCategory.RooftopUnit,
            Manufacturer = "Carrier",
            ModelNumber = "WeatherMaster 48HC",
            SerialNumber = "48HC178902A",
            RefrigerantType = "R-410A",
            InstallDate = new DateOnly(2021, 6, 15),
            WarrantyEndDate = new DateOnly(2026, 6, 15),
            Status = AssetStatus.Active,
            SourceVendorId = vendor.Id
        };
        db.AssetEquipment.Add(asset);
        await db.SaveChangesAsync();

        // 9. Create Active Service Ticket
        var (respTime, resTime) = SlaCalculator.GetDefaultDurations(TicketPriority.High);
        var ticket = new ServiceTicket
        {
            TicketNumber = "TKT-2408",
            CanonicalCustomerId = customer.Id,
            SiteLocationId = site.Id,
            VendorId = vendor.Id,
            AssetEquipmentId = asset.Id,
            Title = "RTU Compressor Short Cycling & Temperature Alert",
            Description = "Main grocery floor temperature reached 76F. Rooftop Unit 2 compressor tripping every 10 minutes on high pressure limit.",
            Priority = TicketPriority.High,
            Status = TicketStatus.Assigned,
            RaisedByUserId = endUser.Id,
            AssignedTechnicianId = technician.Id,
            TargetResponseAt = DateTime.UtcNow.Add(respTime),
            TargetResolutionAt = DateTime.UtcNow.Add(resTime),
            SLAState = SLAState.WithinSLA
        };
        db.ServiceTickets.Add(ticket);
        await db.SaveChangesAsync();

        // Add Ticket System Comment
        var comment = new TicketComment
        {
            TicketId = ticket.Id,
            AuthorUserId = coordinator.Id,
            CommentType = CommentType.Assignment,
            CommentText = $"Assigned to Casey Reed ({vendor.BrandName}) by dispatcher Alex Morgan."
        };
        db.TicketComments.Add(comment);

        // 10. Seed Onboarding Checklists
        db.VendorOnboardingChecklists.AddRange(
            VendorOnboardingChecklistService.CreateInitialTasks(vendor, admin.Id));

        await db.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[SUCCESS] Seed complete! Test accounts:");
        Console.WriteLine(" - SuperAdmin:  admin@hvacconnect.com / Admin@123");
        Console.WriteLine(" - VendorAdmin: jordan.lee@pacificclimate.com / Vendor@123");
        Console.WriteLine(" - Coordinator: coordinator@hvacconnect.com / Coord@123");
        Console.WriteLine(" - Technician:  casey.reed@pacificclimate.com / Tech@123");
        Console.WriteLine(" - EndUser:     m.vance@apexretail.example / User@123");
        Console.ResetColor();
    }
}

