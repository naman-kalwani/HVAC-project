using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Validation;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.ConsoleApp;

public static partial class Program
{
    private static async Task AddLocationWizard(HvacDbContext db)
    {
        Console.WriteLine("\n═══════ ADD NEW CUSTOMER LOCATION / SITE ═══════");
        int customerId;
        if (_currentUser!.Role == UserRole.EndUser)
        {
            customerId = _currentUser.CustomerId!.Value;
        }
        else
        {
            var customers = await db.CanonicalCustomers.ToListAsync();
            for (int i = 0; i < customers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. [{customers[i].CanonicalCode}] {customers[i].CustomerName}");
            }
            string cChoice = Prompt($"Select Customer [1-{customers.Count}]: ");
            if (!int.TryParse(cChoice, out int cIdx) || cIdx < 1 || cIdx > customers.Count)
            {
                PrintError("Invalid customer selection.");
                return;
            }
            customerId = customers[cIdx - 1].Id;
        }

        string siteName = PromptValidated("Facility / Site Name (e.g. Downtown Superstore #12): ", val => InputValidator.ValidateRequiredText(val, "Site Name", 2, 100));
        string address = PromptValidated("Street Address: ", val => InputValidator.ValidateRequiredText(val, "Address", 5, 200));
        string city = PromptValidated("City: ", val => InputValidator.ValidateRequiredText(val, "City", 2, 75));
        string state = PromptValidated("State (2-letter code, e.g. CA, NY): ", val => InputValidator.ValidateRequiredText(val, "State", 2, 20));
        string zip = PromptValidated("Postal Code: ", val => InputValidator.ValidateRequiredText(val, "Postal Code", 4, 15));
        string accessNotes = Prompt("Technician Access Instructions: ");
        string contactName = Prompt("On-Site Contact Name: ");
        string contactPhone = PromptValidated("On-Site Contact Phone (10 digits): ", InputValidator.ValidatePhone);

        string siteCode = $"LOC-{new Random().Next(10000, 99999)}";

        var site = new SiteLocation
        {
            SiteCode = siteCode,
            CanonicalCustomerId = customerId,
            SiteName = siteName,
            AddressLine = address,
            City = city,
            State = state.ToUpper(),
            PostalCode = zip,
            AccessInstructions = accessNotes,
            SiteContactName = contactName,
            SiteContactPhone = contactPhone,
            IsActive = true
        };

        db.SiteLocations.Add(site);
        await db.SaveChangesAsync();

        PrintSuccess($"Location '{siteName}' [{siteCode}] registered successfully!");
    }

    private static async Task ListCustomers(HvacDbContext db)
    {
        IQueryable<CanonicalCustomer> query = db.CanonicalCustomers
            .Include(c => c.BrandAccounts)
            .Include(c => c.Sites);

        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            query = query.Where(c => c.BrandAccounts.Any(b => b.VendorId == _currentUser.VendorId));
        }
        else if (_currentUser.Role == UserRole.EndUser)
        {
            query = query.Where(c => c.Id == _currentUser.CustomerId);
        }

        var customers = await query.ToListAsync();
        var rows = customers.Select(c => new string[]
        {
            c.CanonicalCode,
            c.CustomerName,
            c.CustomerType.ToString(),
            c.Sites.Count.ToString(),
            c.PrimaryContactEmail ?? "N/A",
            c.Status.ToString()
        }).ToList();

        TableFormatter.PrintTable("Canonical Customer Registry",
            new[] { "Customer Code", "Customer Name", "Type", "Sites", "Primary Email", "Status" },
            rows,
            new[] { 15, 26, 14, 8, 26, 10 });
    }

    private static async Task ListLocations(HvacDbContext db)
    {
        IQueryable<SiteLocation> query = db.SiteLocations
            .Include(s => s.Customer)
            .Include(s => s.PrimaryVendor);

        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            query = query.Where(s => s.PrimaryVendorId == _currentUser.VendorId);
        }
        else if (_currentUser.Role == UserRole.EndUser)
        {
            query = query.Where(s => s.CanonicalCustomerId == _currentUser.CustomerId);
        }

        var sites = await query.ToListAsync();
        var rows = sites.Select(s => new string[]
        {
            s.SiteCode,
            s.SiteName,
            $"{s.AddressLine}, {s.City}, {s.State}",
            s.Customer?.CustomerName ?? "N/A",
            s.PrimaryVendor?.BrandName ?? "Unassigned"
        }).ToList();

        TableFormatter.PrintTable("Site Locations & Facilities",
            new[] { "Site Code", "Site Name", "Address", "Customer", "Vendor" },
            rows,
            new[] { 12, 24, 30, 22, 20 });
    }

    private static async Task ListAssets(HvacDbContext db)
    {
        IQueryable<AssetEquipment> query = db.AssetEquipment
            .Include(a => a.SiteLocation);

        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            query = query.Where(a => a.SourceVendorId == _currentUser.VendorId || 
                                     (a.SiteLocation != null && a.SiteLocation.PrimaryVendorId == _currentUser.VendorId));
        }
        else if (_currentUser.Role == UserRole.EndUser)
        {
            query = query.Where(a => a.SiteLocation != null && a.SiteLocation.CanonicalCustomerId == _currentUser.CustomerId);
        }

        var assets = await query.ToListAsync();
        var rows = assets.Select(a => new string[]
        {
            a.Id.ToString(),
            a.AssetTag,
            a.Category.ToString(),
            a.Manufacturer,
            a.ModelNumber ?? "N/A",
            a.SerialNumber ?? "N/A",
            a.SiteLocation?.SiteName ?? "N/A"
        }).ToList();

        TableFormatter.PrintTable("HVAC & Commercial Refrigeration Equipment",
            new[] { "ID", "Asset Tag", "Category", "Manufacturer", "Model", "Serial#", "Location Site" },
            rows,
            new[] { 6, 14, 20, 16, 18, 16, 24 });
    }

    private static async Task CreateAssetWizard(HvacDbContext db)
    {
        Console.WriteLine("\n═══════ REGISTER NEW EQUIPMENT ASSET ═══════");

        IQueryable<SiteLocation> sitesQuery = db.SiteLocations.Include(s => s.Customer);
        if (_currentUser!.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null))
        {
            sitesQuery = sitesQuery.Where(s => s.PrimaryVendorId == _currentUser.VendorId);
        }
        else if (_currentUser.Role == UserRole.EndUser)
        {
            sitesQuery = sitesQuery.Where(s => s.CanonicalCustomerId == _currentUser.CustomerId);
        }

        var sites = await sitesQuery.ToListAsync();
        if (!sites.Any())
        {
            PrintError("No locations available to register equipment under.");
            return;
        }

        Console.WriteLine("Select Location:");
        for (int i = 0; i < sites.Count; i++)
        {
            Console.WriteLine($"{i + 1}. [{sites[i].SiteCode}] {sites[i].SiteName} ({sites[i].Customer?.CustomerName})");
        }

        string sChoice = Prompt($"Select Site [1-{sites.Count}]: ");
        if (!int.TryParse(sChoice, out int siteIdx) || siteIdx < 1 || siteIdx > sites.Count)
        {
            PrintError("Invalid site selection.");
            return;
        }
        var selectedSite = sites[siteIdx - 1];

        string assetTag = PromptValidated("Asset Tag / Asset ID (e.g. RTU-00190, CH-00420): ", InputValidator.ValidateAssetTag);

        bool tagExists = await db.AssetEquipment.AnyAsync(a => a.AssetTag.ToLower() == assetTag.ToLower());
        if (tagExists)
        {
            PrintError($"An equipment unit with Asset Tag '{assetTag}' already exists.");
            return;
        }

        Console.WriteLine("Category: 1. RooftopUnit | 2. Chiller | 3. Boiler | 4. WalkInCoolerFreezer | 5. RefrigerationRack");
        string catChoice = Prompt("Select Category [1-5]: ");
        AssetCategory category = catChoice switch
        {
            "2" => AssetCategory.Chiller,
            "3" => AssetCategory.Boiler,
            "4" => AssetCategory.WalkInCoolerFreezer,
            "5" => AssetCategory.RefrigerationRack,
            _ => AssetCategory.RooftopUnit
        };

        string mfr = PromptValidated("Manufacturer (OEM brand): ", val => InputValidator.ValidateRequiredText(val, "Manufacturer", 2, 100));
        string model = Prompt("Model Number: ");
        string serial = Prompt("Serial Number: ");
        string refrigerant = Prompt("Refrigerant Type (e.g. R-410A, R-404A): ");

        var newAsset = new AssetEquipment
        {
            AssetTag = assetTag,
            SiteLocationId = selectedSite.Id,
            Category = category,
            Manufacturer = mfr,
            ModelNumber = model,
            SerialNumber = serial,
            RefrigerantType = refrigerant,
            InstallDate = DateOnly.FromDateTime(DateTime.UtcNow),
            WarrantyEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)),
            Status = AssetStatus.Active,
            SourceVendorId = (_currentUser.Role == UserRole.VendorAdmin || (_currentUser.Role == UserRole.ServiceCoordinator && _currentUser.VendorId != null)) ? _currentUser.VendorId : selectedSite.PrimaryVendorId
        };

        db.AssetEquipment.Add(newAsset);
        await db.SaveChangesAsync();

        PrintSuccess($"Asset '{newAsset.AssetTag}' ({newAsset.Category}) registered successfully at {selectedSite.SiteName}!");
    }
}
