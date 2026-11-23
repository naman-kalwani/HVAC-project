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
        // If the second seeded agency already exists, the full multi-tenant seed was run.
        if (await db.Vendors.AnyAsync(v => v.VendorCode == "DMS-002")) return;

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("[INFO] Seeding comprehensive multi-tenant dataset (3 Agencies, 5 Roles, Customers, Sites, Assets, Tickets)...");
        Console.ResetColor();

        // ════════════════════════════════════════════════════════════════════════════════
        // 1. Acquired Contractors (Vendors / Agencies)
        // ════════════════════════════════════════════════════════════════════════════════
        var vendorPcs = await db.Vendors.FirstOrDefaultAsync(v => v.VendorCode == "BCC-001");
        if (vendorPcs == null)
        {
            vendorPcs = new Vendor
            {
                VendorCode = "BCC-001",
                BrandName = "Bharat Climate Care",
                LegalEntityName = "Bharat Climate Care Private Limited",
                Region = "South India (Karnataka, Tamil Nadu, Kerala)",
                Status = VendorStatus.Active,
                OnboardingPhase = OnboardingPhase.Phase1_Coexist,
                PrimaryContactName = "Arjun Mehta",
                PrimaryContactEmail = "arjun.mehta@bharatclimate.example",
                PrimaryContactPhone = "9876500101"
            };
            db.Vendors.Add(vendorPcs);
            await db.SaveChangesAsync();
        }

        var vendorMms = await db.Vendors.FirstOrDefaultAsync(v => v.VendorCode == "DMS-002");
        if (vendorMms == null)
        {
            vendorMms = new Vendor
            {
                VendorCode = "DMS-002",
                BrandName = "Deccan Mechanical Services",
                LegalEntityName = "Deccan Mechanical Services Private Limited",
                Region = "West India (Maharashtra, Goa)",
                Status = VendorStatus.Active,
                OnboardingPhase = OnboardingPhase.Phase1_Coexist,
                PrimaryContactName = "Kavita Rao",
                PrimaryContactEmail = "kavita.rao@deccanmechanical.example",
                PrimaryContactPhone = "9876500102"
            };
            db.Vendors.Add(vendorMms);
            await db.SaveChangesAsync();
        }

        var vendorTsh = await db.Vendors.FirstOrDefaultAsync(v => v.VendorCode == "HHS-003");
        if (vendorTsh == null)
        {
            vendorTsh = new Vendor
            {
                VendorCode = "HHS-003",
                BrandName = "Himalayan HVAC Solutions",
                LegalEntityName = "Himalayan HVAC Solutions Private Limited",
                Region = "North India (Delhi, Haryana, Uttarakhand)",
                Status = VendorStatus.Active,
                OnboardingPhase = OnboardingPhase.Phase2_Cutover,
                PrimaryContactName = "Rohit Sharma",
                PrimaryContactEmail = "rohit.sharma@himalayanhvac.example",
                PrimaryContactPhone = "9876500103"
            };
            db.Vendors.Add(vendorTsh);
            await db.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // 2. Canonical Customers
        // ════════════════════════════════════════════════════════════════════════════════
        var custApex = await db.CanonicalCustomers.FirstOrDefaultAsync(c => c.CanonicalCode == "CUST-00128");
        if (custApex == null)
        {
            custApex = new CanonicalCustomer
            {
                CanonicalCode = "CUST-00128",
                CustomerName = "Aarohan Retail Group Private Limited",
                NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName("Aarohan Retail Group Private Limited"),
                CustomerType = CustomerType.Retail,
                PrimaryContactName = "Rahul Verma",
                PrimaryContactEmail = "rahul.verma@aarohanretail.example",
                PrimaryContactPhone = "9876500201",
                BillingAddress = "12 MG Road, Bengaluru, Karnataka 560001",
                Status = CustomerStatus.Active
            };
            db.CanonicalCustomers.Add(custApex);
            await db.SaveChangesAsync();
        }

        var custGreatLakes = await db.CanonicalCustomers.FirstOrDefaultAsync(c => c.CanonicalCode == "CUST-00245");
        if (custGreatLakes == null)
        {
            custGreatLakes = new CanonicalCustomer
            {
                CanonicalCode = "CUST-00245",
                CustomerName = "Sahyadri Cold Chain Private Limited",
                NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName("Sahyadri Cold Chain Private Limited"),
                CustomerType = CustomerType.Industrial,
                PrimaryContactName = "Sneha Kulkarni",
                PrimaryContactEmail = "sneha.kulkarni@sahyadricold.example",
                PrimaryContactPhone = "9876500202",
                BillingAddress = "44 FC Road, Pune, Maharashtra 411004",
                Status = CustomerStatus.Active
            };
            db.CanonicalCustomers.Add(custGreatLakes);
            await db.SaveChangesAsync();
        }

        var custEmpire = await db.CanonicalCustomers.FirstOrDefaultAsync(c => c.CanonicalCode == "CUST-00392");
        if (custEmpire == null)
        {
            custEmpire = new CanonicalCustomer
            {
                CanonicalCode = "CUST-00392",
                CustomerName = "Narmada Hospitality Group",
                NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName("Narmada Hospitality Group"),
                CustomerType = CustomerType.Commercial,
                PrimaryContactName = "Ananya Iyer",
                PrimaryContactEmail = "ananya.iyer@narmadahospitality.example",
                PrimaryContactPhone = "9876500203",
                BillingAddress = "8 Jubilee Hills Road, Hyderabad, Telangana 500033",
                Status = CustomerStatus.Active
            };
            db.CanonicalCustomers.Add(custEmpire);
            await db.SaveChangesAsync();
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // 3. Brand Customer Accounts (linking Canonical Customers to Agencies)
        // ════════════════════════════════════════════════════════════════════════════════
        if (!await db.BrandCustomerAccounts.AnyAsync(b => b.VendorId == vendorPcs.Id && b.CanonicalCustomerId == custApex.Id))
        {
            db.BrandCustomerAccounts.Add(new BrandCustomerAccount
            {
                CanonicalCustomerId = custApex.Id,
                VendorId = vendorPcs.Id,
                LocalAccountNumber = "B-104",
                LocalCustomerName = "Aarohan Retail Group",
                ContactEmail = "rahul.verma@aarohanretail.example",
                BillingAddress = "12 MG Road, Bengaluru, Karnataka",
                AccountStatus = "Active"
            });
        }

        if (!await db.BrandCustomerAccounts.AnyAsync(b => b.VendorId == vendorMms.Id && b.CanonicalCustomerId == custGreatLakes.Id))
        {
            db.BrandCustomerAccounts.Add(new BrandCustomerAccount
            {
                CanonicalCustomerId = custGreatLakes.Id,
                VendorId = vendorMms.Id,
                LocalAccountNumber = "D-220",
                LocalCustomerName = "Sahyadri Cold Chain",
                ContactEmail = "sneha.kulkarni@sahyadricold.example",
                BillingAddress = "44 FC Road, Pune, Maharashtra",
                AccountStatus = "Active"
            });
        }

        if (!await db.BrandCustomerAccounts.AnyAsync(b => b.VendorId == vendorTsh.Id && b.CanonicalCustomerId == custEmpire.Id))
        {
            db.BrandCustomerAccounts.Add(new BrandCustomerAccount
            {
                CanonicalCustomerId = custEmpire.Id,
                VendorId = vendorTsh.Id,
                LocalAccountNumber = "H-501",
                LocalCustomerName = "Narmada Hospitality",
                ContactEmail = "ananya.iyer@narmadahospitality.example",
                BillingAddress = "8 Jubilee Hills Road, Hyderabad, Telangana",
                AccountStatus = "Active"
            });
        }
        await db.SaveChangesAsync();

        // ════════════════════════════════════════════════════════════════════════════════
        // 4. Users (Multiple Users across all 5 Roles)
        // ════════════════════════════════════════════════════════════════════════════════

        // Role 1: Super Admins (Parent Company - Group-Wide Oversight)
        await AddUserIfMissing(db, new User
        {
            Email = "admin@hvacconnect.in",
            PasswordHash = PasswordService.HashPassword("Admin@123"),
            FirstName = "Aarav",
            LastName = "Deshmukh",
            Role = UserRole.SuperAdmin,
            PhoneNumber = "9876500001",
            IsActive = true
        });

        await AddUserIfMissing(db, new User
        {
            Email = "meera.nair@hvacconnect.in",
            PasswordHash = PasswordService.HashPassword("Admin@123"),
            FirstName = "Meera",
            LastName = "Nair",
            Role = UserRole.SuperAdmin,
            PhoneNumber = "9876500002",
            IsActive = true
        });

        // Role 2: Vendor Admins (One per Agency / Acquired Contractor)
        await AddUserIfMissing(db, new User
        {
            Email = "arjun.mehta@bharatclimate.example",
            PasswordHash = PasswordService.HashPassword("Vendor@123"),
            FirstName = "Arjun",
            LastName = "Mehta",
            Role = UserRole.VendorAdmin,
            VendorId = vendorPcs.Id,
            PhoneNumber = "9876500101",
            IsActive = true
        });

        await AddUserIfMissing(db, new User
        {
            Email = "kavita.rao@deccanmechanical.example",
            PasswordHash = PasswordService.HashPassword("Vendor@123"),
            FirstName = "Kavita",
            LastName = "Rao",
            Role = UserRole.VendorAdmin,
            VendorId = vendorMms.Id,
            PhoneNumber = "9876500102",
            IsActive = true
        });

        await AddUserIfMissing(db, new User
        {
            Email = "rohit.sharma@himalayanhvac.example",
            PasswordHash = PasswordService.HashPassword("Vendor@123"),
            FirstName = "Rohit",
            LastName = "Sharma",
            Role = UserRole.VendorAdmin,
            VendorId = vendorTsh.Id,
            PhoneNumber = "9876500103",
            IsActive = true
        });

        // Role 3: Service Coordinators (Agency-Scoped Dispatchers)
        var coordPcs = await AddUserIfMissing(db, new User
        {
            Email = "priya.nair@bharatclimate.example",
            PasswordHash = PasswordService.HashPassword("Coord@123"),
            FirstName = "Priya",
            LastName = "Nair",
            Role = UserRole.ServiceCoordinator,
            VendorId = vendorPcs.Id,
            PhoneNumber = "9876500111",
            IsActive = true
        });

        await AddUserIfMissing(db, new User
        {
            Email = "vikram.singh@bharatclimate.example",
            PasswordHash = PasswordService.HashPassword("Coord@123"),
            FirstName = "Vikram",
            LastName = "Singh",
            Role = UserRole.ServiceCoordinator,
            VendorId = vendorPcs.Id,
            PhoneNumber = "9876500112",
            IsActive = true
        });

        var coordMms = await AddUserIfMissing(db, new User
        {
            Email = "rohan.patil@deccanmechanical.example",
            PasswordHash = PasswordService.HashPassword("Coord@123"),
            FirstName = "Rohan",
            LastName = "Patil",
            Role = UserRole.ServiceCoordinator,
            VendorId = vendorMms.Id,
            PhoneNumber = "9876500121",
            IsActive = true
        });

        var coordTsh = await AddUserIfMissing(db, new User
        {
            Email = "neha.kapoor@himalayanhvac.example",
            PasswordHash = PasswordService.HashPassword("Coord@123"),
            FirstName = "Neha",
            LastName = "Kapoor",
            Role = UserRole.ServiceCoordinator,
            VendorId = vendorTsh.Id,
            PhoneNumber = "9876500131",
            IsActive = true
        });

        // Role 4: Field Technicians (Scoped to Agency with Specialisations)
        var techPcs1 = await AddUserIfMissing(db, new User
        {
            Email = "dev.shah@bharatclimate.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Dev",
            LastName = "Shah",
            EmployeeCode = "TECH-104",
            Specialisation = "HVAC / Chillers",
            Role = UserRole.Technician,
            VendorId = vendorPcs.Id,
            PhoneNumber = "9876500141",
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        });

        var techPcs2 = await AddUserIfMissing(db, new User
        {
            Email = "sanjay.rao@bharatclimate.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Sanjay",
            LastName = "Rao",
            EmployeeCode = "TECH-105",
            Specialisation = "Commercial Refrigeration",
            Role = UserRole.Technician,
            VendorId = vendorPcs.Id,
            PhoneNumber = "9876500142",
            AvailabilityStatus = TechnicianAvailability.OnJob,
            IsActive = true
        });

        var techMms1 = await AddUserIfMissing(db, new User
        {
            Email = "amit.joshi@deccanmechanical.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Amit",
            LastName = "Joshi",
            EmployeeCode = "TECH-201",
            Specialisation = "Boilers & Heating",
            Role = UserRole.Technician,
            VendorId = vendorMms.Id,
            PhoneNumber = "9876500151",
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        });

        var techMms2 = await AddUserIfMissing(db, new User
        {
            Email = "isha.kulkarni@deccanmechanical.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Isha",
            LastName = "Kulkarni",
            EmployeeCode = "TECH-202",
            Specialisation = "General RTU / HVAC",
            Role = UserRole.Technician,
            VendorId = vendorMms.Id,
            PhoneNumber = "9876500152",
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        });

        var techTsh1 = await AddUserIfMissing(db, new User
        {
            Email = "karan.bhatia@himalayanhvac.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Karan",
            LastName = "Bhatia",
            EmployeeCode = "TECH-301",
            Specialisation = "HVAC / Chillers",
            Role = UserRole.Technician,
            VendorId = vendorTsh.Id,
            PhoneNumber = "9876500161",
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        });

        var techTsh2 = await AddUserIfMissing(db, new User
        {
            Email = "pooja.rawat@himalayanhvac.example",
            PasswordHash = PasswordService.HashPassword("Tech@123"),
            FirstName = "Pooja",
            LastName = "Rawat",
            EmployeeCode = "TECH-302",
            Specialisation = "Commercial Refrigeration",
            Role = UserRole.Technician,
            VendorId = vendorTsh.Id,
            PhoneNumber = "9876500162",
            AvailabilityStatus = TechnicianAvailability.Available,
            IsActive = true
        });

        // Role 5: End Users (Facility Managers scoped to their Customer)
        var userApex1 = await AddUserIfMissing(db, new User
        {
            Email = "rahul.verma@aarohanretail.example",
            PasswordHash = PasswordService.HashPassword("User@123"),
            FirstName = "Rahul",
            LastName = "Verma",
            Role = UserRole.EndUser,
            CustomerId = custApex.Id,
            PhoneNumber = "9876500201",
            IsActive = true
        });

        var userApex2 = await AddUserIfMissing(db, new User
        {
            Email = "kavya.menon@aarohanretail.example",
            PasswordHash = PasswordService.HashPassword("User@123"),
            FirstName = "Kavya",
            LastName = "Menon",
            Role = UserRole.EndUser,
            CustomerId = custApex.Id,
            PhoneNumber = "9876500204",
            IsActive = true
        });

        var userGreatLakes1 = await AddUserIfMissing(db, new User
        {
            Email = "sneha.kulkarni@sahyadricold.example",
            PasswordHash = PasswordService.HashPassword("User@123"),
            FirstName = "Sneha",
            LastName = "Kulkarni",
            Role = UserRole.EndUser,
            CustomerId = custGreatLakes.Id,
            PhoneNumber = "9876500202",
            IsActive = true
        });

        var userEmpire1 = await AddUserIfMissing(db, new User
        {
            Email = "ananya.iyer@narmadahospitality.example",
            PasswordHash = PasswordService.HashPassword("User@123"),
            FirstName = "Ananya",
            LastName = "Iyer",
            Role = UserRole.EndUser,
            CustomerId = custEmpire.Id,
            PhoneNumber = "9876500203",
            IsActive = true
        });

        await db.SaveChangesAsync();

        // ════════════════════════════════════════════════════════════════════════════════
        // 5. Site Locations (Scoped to Customer & Servicing Agency)
        // ════════════════════════════════════════════════════════════════════════════════
        var siteApex1 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00812",
            CanonicalCustomerId = custApex.Id,
            PrimaryVendorId = vendorPcs.Id,
            SiteName = "Aarohan Central Bengaluru Store",
            AddressLine = "18 Residency Road",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560025",
            OperatingHours = "06:00 - 23:00",
            AccessInstructions = "Use the service entrance beside the east loading bay. Report to the facilities desk.",
            SiteContactName = "Nikhil Rao",
            SiteContactPhone = "9876500211"
        });

        var siteApex2 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00813",
            CanonicalCustomerId = custApex.Id,
            PrimaryVendorId = vendorPcs.Id,
            SiteName = "Aarohan Hosur Distribution Centre",
            AddressLine = "Plot 24, SIPCOT Industrial Complex",
            City = "Hosur",
            State = "Tamil Nadu",
            PostalCode = "635126",
            OperatingHours = "24/7 Operations",
            AccessInstructions = "Check in at the main security gate and carry a valid site access pass.",
            SiteContactName = "Aditi Kumar",
            SiteContactPhone = "9876500212"
        });

        var siteGl1 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00901",
            CanonicalCustomerId = custGreatLakes.Id,
            PrimaryVendorId = vendorMms.Id,
            SiteName = "Sahyadri Pune Cold Storage Hub",
            AddressLine = "Plot 7, Chakan MIDC Phase II",
            City = "Pune",
            State = "Maharashtra",
            PostalCode = "410501",
            OperatingHours = "05:00 - 22:00",
            AccessInstructions = "Refrigeration plant room key is held at the main operations desk.",
            SiteContactName = "Pranav Desai",
            SiteContactPhone = "9876500221"
        });

        var siteGl2 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00902",
            CanonicalCustomerId = custGreatLakes.Id,
            PrimaryVendorId = vendorMms.Id,
            SiteName = "Sahyadri Nashik Logistics Depot",
            AddressLine = "Survey 18, Ambad Industrial Estate",
            City = "Nashik",
            State = "Maharashtra",
            PostalCode = "422010",
            OperatingHours = "07:00 - 19:00",
            AccessInstructions = "Use the north service gate and call the facility manager on arrival.",
            SiteContactName = "Madhuri Joshi",
            SiteContactPhone = "9876500222"
        });

        var siteEmp1 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00951",
            CanonicalCustomerId = custEmpire.Id,
            PrimaryVendorId = vendorTsh.Id,
            SiteName = "Narmada Grand Hotel Hyderabad",
            AddressLine = "Road No. 2, Banjara Hills",
            City = "Hyderabad",
            State = "Telangana",
            PostalCode = "500034",
            OperatingHours = "24/7 Hospitality",
            AccessInstructions = "Chiller room access is through the basement service corridor. Sign in with security.",
            SiteContactName = "Suresh Reddy",
            SiteContactPhone = "9876500231"
        });

        var siteEmp2 = await AddSiteIfMissing(db, new SiteLocation
        {
            SiteCode = "LOC-00952",
            CanonicalCustomerId = custEmpire.Id,
            PrimaryVendorId = vendorTsh.Id,
            SiteName = "Narmada Lakeview Suites",
            AddressLine = "6 Tank Bund Road",
            City = "Hyderabad",
            State = "Telangana",
            PostalCode = "500080",
            OperatingHours = "24/7 Hospitality",
            AccessInstructions = "Use the rear service entrance and register at the security desk.",
            SiteContactName = "Divya Rao",
            SiteContactPhone = "9876500232"
        });

        await db.SaveChangesAsync();

        // ════════════════════════════════════════════════════════════════════════════════
        // 6. Equipment Assets
        // ════════════════════════════════════════════════════════════════════════════════
        var assetApex1 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "RTU-00182",
            SiteLocationId = siteApex1.Id,
            Category = AssetCategory.RooftopUnit,
            Manufacturer = "Carrier",
            ModelNumber = "WeatherMaster 48HC",
            SerialNumber = "48HC178902A",
            RefrigerantType = "R-410A",
            InstallDate = new DateOnly(2021, 6, 15),
            WarrantyEndDate = new DateOnly(2026, 6, 15),
            Status = AssetStatus.Active,
            SourceVendorId = vendorPcs.Id
        });

        var assetApex2 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "CH-00401",
            SiteLocationId = siteApex2.Id,
            Category = AssetCategory.Chiller,
            Manufacturer = "Trane",
            ModelNumber = "CenTraVac CVHE",
            SerialNumber = "CVHE882190B",
            RefrigerantType = "R-514A",
            InstallDate = new DateOnly(2019, 4, 10),
            WarrantyEndDate = new DateOnly(2024, 4, 10),
            Status = AssetStatus.Active,
            SourceVendorId = vendorPcs.Id
        });

        var assetGl1 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "REF-00910",
            SiteLocationId = siteGl1.Id,
            Category = AssetCategory.RefrigerationRack,
            Manufacturer = "Hussmann",
            ModelNumber = "Protocol Parallel",
            SerialNumber = "HUSS-PR-4401",
            RefrigerantType = "R-448A",
            InstallDate = new DateOnly(2022, 1, 20),
            WarrantyEndDate = new DateOnly(2027, 1, 20),
            Status = AssetStatus.Active,
            SourceVendorId = vendorMms.Id
        });

        var assetGl2 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "BLR-00215",
            SiteLocationId = siteGl2.Id,
            Category = AssetCategory.Boiler,
            Manufacturer = "Cleaver-Brooks",
            ModelNumber = "CB-LE Packaged",
            SerialNumber = "CB771029C",
            InstallDate = new DateOnly(2020, 9, 5),
            WarrantyEndDate = new DateOnly(2025, 9, 5),
            Status = AssetStatus.Active,
            SourceVendorId = vendorMms.Id
        });

        var assetEmp1 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "CH-00830",
            SiteLocationId = siteEmp1.Id,
            Category = AssetCategory.Chiller,
            Manufacturer = "York",
            ModelNumber = "YK Centrifugal",
            SerialNumber = "YK991040D",
            RefrigerantType = "R-134a",
            InstallDate = new DateOnly(2023, 3, 1),
            WarrantyEndDate = new DateOnly(2028, 3, 1),
            Status = AssetStatus.Active,
            SourceVendorId = vendorTsh.Id
        });

        var assetEmp2 = await AddAssetIfMissing(db, new AssetEquipment
        {
            AssetTag = "REF-00720",
            SiteLocationId = siteEmp2.Id,
            Category = AssetCategory.WalkInCoolerFreezer,
            Manufacturer = "Zero Zone",
            ModelNumber = "Modular Low-Temp",
            SerialNumber = "ZZ-LT-5519",
            RefrigerantType = "R-404A",
            InstallDate = new DateOnly(2022, 8, 12),
            WarrantyEndDate = new DateOnly(2027, 8, 12),
            Status = AssetStatus.Active,
            SourceVendorId = vendorTsh.Id
        });

        await db.SaveChangesAsync();

        // ════════════════════════════════════════════════════════════════════════════════
        // 7. Active Service Tickets Across Agencies & Customers
        // ════════════════════════════════════════════════════════════════════════════════
        var (hResp, hRes) = SlaCalculator.GetDefaultDurations(TicketPriority.High);
        var (cResp, cRes) = SlaCalculator.GetDefaultDurations(TicketPriority.Critical);
        var (mResp, mRes) = SlaCalculator.GetDefaultDurations(TicketPriority.Medium);

        // Ticket 1: Bharat Climate Care -> Aarohan Bengaluru -> Dev Shah
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-2408"))
        {
            var t1 = new ServiceTicket
            {
                TicketNumber = "TKT-2408",
                CanonicalCustomerId = custApex.Id,
                SiteLocationId = siteApex1.Id,
                VendorId = vendorPcs.Id,
                AssetEquipmentId = assetApex1.Id,
                Title = "RTU Compressor Short Cycling & Temperature Alert",
                Description = "Main sales floor temperature reached 24C. Rooftop Unit 2 compressor is tripping on the high-pressure limit.",
                Priority = TicketPriority.High,
                Status = TicketStatus.Assigned,
                RaisedByUserId = userApex1.Id,
                OwnerCoordinatorId = coordPcs.Id,
                ClaimedAt = DateTime.UtcNow.AddHours(-3),
                AssignedTechnicianId = techPcs1.Id,
                AssignedByUserId = coordPcs.Id,
                AssignedAt = DateTime.UtcNow.AddHours(-2),
                TargetResponseAt = DateTime.UtcNow.Add(hResp),
                TargetResolutionAt = DateTime.UtcNow.Add(hRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t1);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t1.Id,
                AuthorUserId = userApex1.Id,
                CommentType = CommentType.UserComment,
                CommentText = "Compressor is making intermittent humming sound. Ambient room temp rising."
            });
            db.TicketComments.Add(new TicketComment
            {
                TicketId = t1.Id,
                AuthorUserId = coordPcs.Id,
                CommentType = CommentType.Assignment,
                CommentText = $"Ticket claimed by coordinator {coordPcs.FirstName} {coordPcs.LastName}."
            });
            db.TicketComments.Add(new TicketComment
            {
                TicketId = t1.Id,
                AuthorUserId = coordPcs.Id,
                CommentType = CommentType.Assignment,
                CommentText = $"Assigned to {techPcs1.FirstName} {techPcs1.LastName} ({vendorPcs.BrandName}) by dispatcher {coordPcs.FirstName} {coordPcs.LastName}."
            });
        }

        // Ticket 2: Bharat Climate Care -> Aarohan Hosur Distribution Centre -> Sanjay Rao
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-2409"))
        {
            var t2 = new ServiceTicket
            {
                TicketNumber = "TKT-2409",
                CanonicalCustomerId = custApex.Id,
                SiteLocationId = siteApex2.Id,
                VendorId = vendorPcs.Id,
                AssetEquipmentId = assetApex2.Id,
                Title = "Chiller Secondary Loop Low Flow Alarm",
                Description = "Secondary chilled water loop pump delta-P dropped below 12 PSI. Flow switch fault triggered.",
                Priority = TicketPriority.Critical,
                Status = TicketStatus.InProgress,
                RaisedByUserId = userApex2.Id,
                OwnerCoordinatorId = coordPcs.Id,
                ClaimedAt = DateTime.UtcNow.AddHours(-1),
                AssignedTechnicianId = techPcs2.Id,
                AssignedByUserId = coordPcs.Id,
                AssignedAt = DateTime.UtcNow.AddMinutes(-45),
                TargetResponseAt = DateTime.UtcNow.Add(cResp),
                TargetResolutionAt = DateTime.UtcNow.Add(cRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t2);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t2.Id,
                AuthorUserId = techPcs2.Id,
                CommentType = CommentType.StatusChange,
                CommentText = "On site at the Hosur distribution centre. Inspecting the strainer and pump differential-pressure transmitter."
            });
        }

        // Ticket 3: Deccan Mechanical Services -> Sahyadri Pune Cold Storage Hub -> Amit Joshi
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-3101"))
        {
            var t3 = new ServiceTicket
            {
                TicketNumber = "TKT-3101",
                CanonicalCustomerId = custGreatLakes.Id,
                SiteLocationId = siteGl1.Id,
                VendorId = vendorMms.Id,
                AssetEquipmentId = assetGl1.Id,
                Title = "Refrigeration Rack #2 Suction Pressure Drop",
                Description = "Low-temp suction manifold pressure dropped to 8 PSIG. Risk of cold storage room thaw.",
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Assigned,
                RaisedByUserId = userGreatLakes1.Id,
                OwnerCoordinatorId = coordMms.Id,
                ClaimedAt = DateTime.UtcNow.AddHours(-2),
                AssignedTechnicianId = techMms1.Id,
                AssignedByUserId = coordMms.Id,
                AssignedAt = DateTime.UtcNow.AddHours(-1),
                TargetResponseAt = DateTime.UtcNow.Add(cResp),
                TargetResolutionAt = DateTime.UtcNow.Add(cRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t3);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t3.Id,
                AuthorUserId = userGreatLakes1.Id,
                CommentType = CommentType.UserComment,
                CommentText = "Rack B freezer zone temperature is -19C (target is -26C). Immediate dispatch requested."
            });
        }

        // Ticket 4: Deccan Mechanical Services -> Sahyadri Nashik Logistics Depot -> Isha Kulkarni
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-3102"))
        {
            var t4 = new ServiceTicket
            {
                TicketNumber = "TKT-3102",
                CanonicalCustomerId = custGreatLakes.Id,
                SiteLocationId = siteGl2.Id,
                VendorId = vendorMms.Id,
                AssetEquipmentId = assetGl2.Id,
                Title = "Boiler Burner Lockout - Error Code E-14",
                Description = "Cleaver-Brooks main boiler failing ignition sequence on second trial. Flame scanner signal weak.",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.InProgress,
                RaisedByUserId = userGreatLakes1.Id,
                OwnerCoordinatorId = coordMms.Id,
                ClaimedAt = DateTime.UtcNow.AddHours(-4),
                AssignedTechnicianId = techMms2.Id,
                AssignedByUserId = coordMms.Id,
                AssignedAt = DateTime.UtcNow.AddHours(-3),
                TargetResponseAt = DateTime.UtcNow.Add(mResp),
                TargetResolutionAt = DateTime.UtcNow.Add(mRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t4);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t4.Id,
                AuthorUserId = techMms2.Id,
                CommentType = CommentType.StatusChange,
                CommentText = "Cleaning optical flame scanner lens and checking gas pilot pressure solenoid."
            });
        }

        // Ticket 5: Himalayan HVAC Solutions -> Narmada Grand Hotel Hyderabad -> Karan Bhatia
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-4201"))
        {
            var t5 = new ServiceTicket
            {
                TicketNumber = "TKT-4201",
                CanonicalCustomerId = custEmpire.Id,
                SiteLocationId = siteEmp1.Id,
                VendorId = vendorTsh.Id,
                AssetEquipmentId = assetEmp1.Id,
                Title = "Chiller #1 Thrust Bearing High Temperature Warning",
                Description = "York YK chiller bearing temperature exceeded 79C. Vibration sensor alert level 2.",
                Priority = TicketPriority.High,
                Status = TicketStatus.Assigned,
                RaisedByUserId = userEmpire1.Id,
                OwnerCoordinatorId = coordTsh.Id,
                ClaimedAt = DateTime.UtcNow.AddHours(-2),
                AssignedTechnicianId = techTsh1.Id,
                AssignedByUserId = coordTsh.Id,
                AssignedAt = DateTime.UtcNow.AddHours(-1),
                TargetResponseAt = DateTime.UtcNow.Add(hResp),
                TargetResolutionAt = DateTime.UtcNow.Add(hRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t5);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t5.Id,
                AuthorUserId = techTsh1.Id,
                CommentType = CommentType.Assignment,
                CommentText = "Assigned by dispatch. Arriving at the Hyderabad site with a lube-oil analysis test kit."
            });
        }

        // Ticket 6: Open & UNCLAIMED (Shared Coordinator Queue) - No Owner, No Vendor
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-4202"))
        {
            var t6 = new ServiceTicket
            {
                TicketNumber = "TKT-4202",
                CanonicalCustomerId = custEmpire.Id,
                SiteLocationId = siteEmp2.Id,
                VendorId = null, // No vendor initially
                OwnerCoordinatorId = null, // Unclaimed in shared queue
                ClaimedAt = null,
                AssignedTechnicianId = null,
                AssignedByUserId = null,
                AssignedAt = null,
                AssetEquipmentId = assetEmp2.Id,
                Title = "Walk-in Freezer Door Seal & Defrost Failure",
                Description = "Ice buildup around perimeter of kitchen walk-in freezer door. Electric defrost timer sticking.",
                Priority = TicketPriority.Medium,
                Status = TicketStatus.Open,
                RaisedByUserId = userEmpire1.Id,
                TargetResponseAt = DateTime.UtcNow.Add(mResp),
                TargetResolutionAt = DateTime.UtcNow.Add(mRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t6);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t6.Id,
                AuthorUserId = userEmpire1.Id,
                CommentType = CommentType.UserComment,
                CommentText = "Kitchen staff reported frost buildup inside evaporator coils."
            });
        }

        // Ticket 7: Open & UNCLAIMED (Emergency Queue) - No Owner, No Vendor
        if (!await db.ServiceTickets.AnyAsync(t => t.TicketNumber == "TKT-5001"))
        {
            var t7 = new ServiceTicket
            {
                TicketNumber = "TKT-5001",
                CanonicalCustomerId = custApex.Id,
                SiteLocationId = siteApex1.Id,
                VendorId = null, // No vendor initially
                OwnerCoordinatorId = null, // Unclaimed in shared queue
                ClaimedAt = null,
                AssignedTechnicianId = null,
                AssignedByUserId = null,
                AssignedAt = null,
                AssetEquipmentId = assetApex1.Id,
                Title = "Walk-in Cooler High Temp Alarm — Perishable Spoilage Risk",
                Description = "Produce walk-in temperature reached 9C. Compressor head pressure is cycling high.",
                Priority = TicketPriority.Critical,
                Status = TicketStatus.Open,
                RaisedByUserId = userApex1.Id,
                TargetResponseAt = DateTime.UtcNow.Add(cResp),
                TargetResolutionAt = DateTime.UtcNow.Add(cRes),
                SLAState = SLAState.WithinSLA
            };
            db.ServiceTickets.Add(t7);
            await db.SaveChangesAsync();

            db.TicketComments.Add(new TicketComment
            {
                TicketId = t7.Id,
                AuthorUserId = userApex1.Id,
                CommentType = CommentType.UserComment,
                CommentText = "Emergency request logged by facility manager Rahul Verma. Requires immediate coordinator dispatch."
            });
        }

        // ════════════════════════════════════════════════════════════════════════════════
        // 8. Vendor Onboarding Checklists
        // ════════════════════════════════════════════════════════════════════════════════
        var checklistActor = await db.Users.SingleAsync(user => user.Email == "admin@hvacconnect.in");
        foreach (var vendor in new[] { vendorPcs, vendorMms, vendorTsh })
        {
            if (!await db.VendorOnboardingChecklists.AnyAsync(item => item.VendorId == vendor.Id))
            {
                db.VendorOnboardingChecklists.AddRange(
                    VendorOnboardingChecklistService.CreateInitialTasks(vendor, checklistActor.Id));
            }
        }

        await db.SaveChangesAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[SUCCESS] Comprehensive multi-agency seed complete!");
        Console.WriteLine(" ─── SEEDED TEST CREDENTIALS (sign in with email) ───");
        Console.WriteLine("   Super Admin:       admin@hvacconnect.in / Admin@123");
        Console.WriteLine("   Vendor Admin:      arjun.mehta@bharatclimate.example / Vendor@123");
        Console.WriteLine("   Coordinator:       priya.nair@bharatclimate.example / Coord@123");
        Console.WriteLine("   Technician:        dev.shah@bharatclimate.example / Tech@123");
        Console.WriteLine("   End User:          rahul.verma@aarohanretail.example / User@123");
        Console.ResetColor();
    }

    private static async Task<User> AddUserIfMissing(HvacDbContext db, User user)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == user.Email.ToLower());
        if (existing != null) return existing;
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<SiteLocation> AddSiteIfMissing(HvacDbContext db, SiteLocation site)
    {
        var existing = await db.SiteLocations.FirstOrDefaultAsync(s => s.SiteCode == site.SiteCode);
        if (existing != null) return existing;
        db.SiteLocations.Add(site);
        await db.SaveChangesAsync();
        return site;
    }

    private static async Task<AssetEquipment> AddAssetIfMissing(HvacDbContext db, AssetEquipment asset)
    {
        var existing = await db.AssetEquipment.FirstOrDefaultAsync(a => a.AssetTag == asset.AssetTag);
        if (existing != null) return existing;
        db.AssetEquipment.Add(asset);
        await db.SaveChangesAsync();
        return asset;
    }
}
