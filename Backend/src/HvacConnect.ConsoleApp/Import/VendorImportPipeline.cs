using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Services;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.ConsoleApp;

internal static class VendorImportPipeline
{
    private sealed record Field(string Key, string Label, bool Required, string[] Aliases);
    private static readonly Dictionary<string, Field[]> Fields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Customers"] = new[]
        {
            F("customer_name", "Customer / company name", true, "customer", "customer name", "client", "company", "company name", "account name", "organization", "business name"),
            F("account_number", "Vendor local account number", false, "account number", "account no", "account id", "customer number", "customer id", "customer code", "legacy id"),
            F("customer_type", "Customer type", false, "customer type", "business type", "industry"),
            F("contact_name", "Primary contact name", false, "contact name", "primary contact", "primary contact name", "contact person", "contact person name"),
            F("email", "Contact email", false, "email", "email address", "contact email", "primary email"),
            F("phone", "Contact phone", false, "phone", "telephone", "contact phone", "phone number"),
            F("billing_address", "Billing address", false, "billing address", "address", "street address", "address line")
        },
        ["Sites"] = new[]
        {
            F("customer_name", "Customer / company name", true, "customer", "customer name", "client", "company", "account name"),
            F("site_name", "Site / facility name", true, "site name", "site", "location name", "location", "facility", "facility name", "store name", "building name"),
            F("address", "Street address", true, "address", "street", "street address", "address line", "site address"),
            F("city", "City", true, "city", "town"), F("state", "State / province", true, "state", "province"),
            F("postal_code", "Postal / ZIP code", true, "postal code", "zip", "zip code", "postcode"),
            F("site_type", "Site type", false, "site type", "location type", "facility type"),
            F("timezone", "Time zone", false, "timezone", "time zone"),
            F("contact_name", "Site contact name", false, "site contact", "contact name", "site manager"),
            F("contact_phone", "Site contact phone", false, "site phone", "contact phone", "phone")
        },
        ["Assets"] = new[]
        {
            F("customer_name", "Customer / company name", true, "customer", "customer name", "client", "company", "account name"),
            F("site_name", "Site / facility name", true, "site name", "site", "location name", "location", "facility", "store"),
            F("asset_tag", "Asset tag (generated if blank)", false, "asset tag", "asset number", "asset id", "equipment id", "equipment number", "tag"),
            F("legacy_asset_id", "Vendor legacy asset ID", false, "legacy asset id", "legacy id", "equipment code"),
            F("category", "Equipment category", false, "category", "equipment type", "asset type", "type"),
            F("manufacturer", "Manufacturer / OEM", true, "manufacturer", "make", "oem", "brand"),
            F("model_number", "Model number", false, "model", "model number", "model no"),
            F("serial_number", "Serial number", false, "serial", "serial number", "serial no"),
            F("refrigerant_type", "Refrigerant type", false, "refrigerant", "refrigerant type"),
            F("install_date", "Install date", false, "install date", "installation date"),
            F("warranty_end_date", "Warranty end date", false, "warranty end", "warranty end date", "warranty expiration")
        }
    };

    private static Field F(string key, string label, bool required, params string[] aliases) => new(key, label, required, aliases);

    public static async Task RunAsync(HvacDbContext db, User actor, Func<string, string> prompt, Action<string> error, Action<string> success)
    {
        Console.WriteLine("\n=== MULTI-VENDOR CSV / EXCEL IMPORT ===");
        var vendors = await db.Vendors.Where(v => v.IsActive).OrderBy(v => v.BrandName).ToListAsync();
        if (vendors.Count == 0) { error("No active vendors. Add a vendor first."); return; }
        int count = 0;
        while (true)
        {
            Console.WriteLine("\nVendors:");
            for (int i = 0; i < vendors.Count; i++) Console.WriteLine($"{i + 1}. {vendors[i].BrandName} [{vendors[i].VendorCode}]");
            if (!int.TryParse(prompt($"Select vendor [1-{vendors.Count}]: "), out int vi) || vi < 1 || vi > vendors.Count) { error("Invalid vendor selection."); return; }
            var vendor = vendors[vi - 1];
            Console.WriteLine("Record type: 1. Customers  2. Sites / locations  3. Equipment assets");
            string type = prompt("Select record type [1-3]: ") switch { "1" => "Customers", "2" => "Sites", "3" => "Assets", _ => "" };
            if (type == "") { error("Invalid record type."); return; }
            string path = prompt("CSV or Excel .xlsx file path (blank to finish): ");
            if (path.Length == 0) break;
            if (!File.Exists(path)) { error($"File not found: {path}"); continue; }
            try { if (await ImportOneAsync(db, actor, vendor, type, path, prompt, error, success)) count++; }
            catch (Exception ex) { error($"Could not import {Path.GetFileName(path)}: {ex.Message}"); }
            if (!prompt("Import another file for any vendor? (y/n): ").Equals("y", StringComparison.OrdinalIgnoreCase)) break;
        }
        if (count == 0) Console.WriteLine("No files imported.");
        else success($"Staged {count} file(s). Open menu option 12 to review duplicate records.");
    }

    private static async Task<bool> ImportOneAsync(HvacDbContext db, User actor, Vendor vendor, string type, string path, Func<string, string> prompt, Action<string> error, Action<string> success)
    {
        var rows = await ImportFileReader.ReadAsync(path);
        var headers = rows.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var mapping = await MapHeadersAsync(db, actor, vendor, type, headers, prompt, error);
        if (mapping == null) { Console.WriteLine("Import cancelled before any rows were staged."); return false; }
        var job = new ImportJob
        {
            JobCode = "IMP-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), VendorId = vendor.Id, EntityType = type,
            OriginalFileName = Path.GetFileName(path), FileSizeBytes = new FileInfo(path).Length,
            FileHash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(path))), UploadedByUserId = actor.Id,
            TotalRows = rows.Count, Status = ImportJobStatus.Validated, MappingConfigJson = JsonSerializer.Serialize(mapping)
        };
        db.ImportJobs.Add(job); await db.SaveChangesAsync();
        int errors = 0, warnings = 0, duplicates = 0;
        for (int i = 0; i < rows.Count; i++)
        {
            var mapped = mapping.Where(m => rows[i].ContainsKey(m.Value)).ToDictionary(m => m.Key, m => rows[i][m.Value], StringComparer.OrdinalIgnoreCase);
            var staged = new ImportRow { ImportJobId = job.Id, RowNumber = i + 2, RawDataJson = JsonSerializer.Serialize(rows[i]), MappedDataJson = JsonSerializer.Serialize(mapped) };
            db.ImportRows.Add(staged); await db.SaveChangesAsync();
            try
            {
                staged.MatchOutcome = await ProcessRowAsync(db, job, vendor, staged, mapped);
                if (staged.MatchOutcome == "PossibleDuplicate") duplicates++;
                if (staged.MatchOutcome?.StartsWith("Error:") == true) { staged.ValidationStatus = "Error"; staged.ValidationIssuesJson = JsonSerializer.Serialize(new[] { staged.MatchOutcome[6..] }); errors++; }
                if (staged.MatchOutcome?.StartsWith("Warning:") == true) { staged.ValidationStatus = "Warning"; staged.ValidationIssuesJson = JsonSerializer.Serialize(new[] { staged.MatchOutcome[8..] }); warnings++; }
            }
            catch (Exception ex)
            {
                staged.ValidationStatus = "Error"; staged.ValidationIssuesJson = JsonSerializer.Serialize(new[] { ex.Message }); staged.MatchOutcome = "Error"; errors++;
            }
            await db.SaveChangesAsync();
        }
        job.ValidRows = rows.Count - errors; job.ErrorRows = errors; job.WarningRows = warnings; job.DuplicateRows = duplicates;
        job.Status = duplicates == 0 ? ImportJobStatus.Committed : ImportJobStatus.DuplicateReview;
        if (duplicates == 0) job.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await VendorOnboardingChecklistService.RefreshAutomaticMilestonesAsync(db, vendor.Id, actor.Id);
        success($"{job.JobCode} — {vendor.BrandName} / {type} / {job.OriginalFileName}: {rows.Count} rows, {duplicates} duplicates, {warnings} warnings, {errors} errors.");
        return true;
    }

    private static async Task<Dictionary<string, string>?> MapHeadersAsync(HvacDbContext db, User actor, Vendor vendor, string type, List<string> headers, Func<string, string> prompt, Action<string> error)
    {
        var saved = await db.VendorMappingTemplates.Where(t => t.VendorId == vendor.Id && t.EntityType == type).OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt).FirstOrDefaultAsync();
        Dictionary<string, string> map = new(StringComparer.OrdinalIgnoreCase);
        if (saved != null)
        {
            try { map = JsonSerializer.Deserialize<Dictionary<string, string>>(saved.MappingConfigJson) ?? map; } catch (JsonException) { }
            map = map.Where(x => headers.Contains(x.Value, StringComparer.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
            if (map.Count > 0) Console.WriteLine($"Using saved mapping '{saved.TemplateName}'.");
        }
        var usedHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in Fields[type])
        {
            if (!map.TryGetValue(field.Key, out var savedHeader)) continue;
            if (!usedHeaders.Add(Normalize(savedHeader)))
            {
                map.Remove(field.Key);
                Console.WriteLine($"Saved mapping for '{field.Label}' reused column '{savedHeader}'. Please map it again.");
            }
        }
        foreach (var field in Fields[type])
        {
            if (map.ContainsKey(field.Key)) continue;
            var aliases = field.Aliases.Append(field.Key).Select(Normalize).ToHashSet(StringComparer.OrdinalIgnoreCase);
            string? match = headers.FirstOrDefault(h => aliases.Contains(Normalize(h)) && !usedHeaders.Contains(Normalize(h)));
            if (match != null) { map[field.Key] = match; usedHeaders.Add(Normalize(match)); continue; }
            Console.WriteLine($"\nMap {field.Label}{(field.Required ? " [required]" : " [optional]")}:");
            for (int i = 0; i < headers.Count; i++) Console.WriteLine($" {i + 1}. {headers[i]}{(usedHeaders.Contains(Normalize(headers[i])) ? " (already mapped)" : "")}");
            while (true)
            {
                string answer = prompt(field.Required ? "Column number: " : "Column number, Enter to skip: ");
                if (answer.Length == 0 && !field.Required) break;
                if (int.TryParse(answer, out int n) && n >= 1 && n <= headers.Count)
                {
                    string selected = headers[n - 1];
                    if (usedHeaders.Contains(Normalize(selected))) { error($"Column '{selected}' is already mapped to another field. Choose a different column or leave this optional field blank."); continue; }
                    map[field.Key] = selected;
                    usedHeaders.Add(Normalize(selected));
                    break;
                }
                error("Select a valid column number.");
            }
        }
        while (true)
        {
            Console.WriteLine("\nReview column mapping:");
            for (int i = 0; i < Fields[type].Length; i++)
            {
                var field = Fields[type][i];
                Console.WriteLine($"{i + 1}. {field.Label}: {map.GetValueOrDefault(field.Key, "(not mapped)")}{(field.Required ? " [required]" : " [optional]")}");
            }
            string decision = prompt("[Y] Confirm and continue  [E] Edit a mapping  [X] Cancel this file: ").Trim().ToUpperInvariant();
            if (decision == "Y") break;
            if (decision == "X") return null;
            if (decision != "E") { error("Enter Y, E, or X."); continue; }

            string fieldChoice = prompt($"Select field to edit [1-{Fields[type].Length}]: ");
            if (!int.TryParse(fieldChoice, out int fieldIndex) || fieldIndex < 1 || fieldIndex > Fields[type].Length)
            { error("Select a listed field."); continue; }
            var editField = Fields[type][fieldIndex - 1];
            if (map.TryGetValue(editField.Key, out var oldHeader)) usedHeaders.Remove(Normalize(oldHeader));
            Console.WriteLine($"Choose the source column for {editField.Label}:");
            for (int i = 0; i < headers.Count; i++) Console.WriteLine($" {i + 1}. {headers[i]}{(usedHeaders.Contains(Normalize(headers[i])) ? " (already mapped)" : "")}");
            while (true)
            {
                string selection = prompt(editField.Required ? "Column number: " : "Column number, or Enter to leave unmapped: ");
                if (selection.Length == 0 && !editField.Required) { map.Remove(editField.Key); break; }
                if (!int.TryParse(selection, out int columnIndex) || columnIndex < 1 || columnIndex > headers.Count)
                { error("Select a listed column."); continue; }
                string selectedHeader = headers[columnIndex - 1];
                if (usedHeaders.Contains(Normalize(selectedHeader))) { error($"'{selectedHeader}' is already assigned to another field."); continue; }
                map[editField.Key] = selectedHeader;
                usedHeaders.Add(Normalize(selectedHeader));
                break;
            }
        }
        if (prompt("Save this mapping for this vendor and record type? (y/n): ").Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            string name = prompt("Template name (Enter for Default): "); if (name.Length == 0) name = "Default";
            var template = await db.VendorMappingTemplates.FirstOrDefaultAsync(t => t.VendorId == vendor.Id && t.EntityType == type && t.TemplateName == name);
            if (template == null) { template = new VendorMappingTemplate { VendorId = vendor.Id, EntityType = type, TemplateName = name, CreatedByUserId = actor.Id }; db.VendorMappingTemplates.Add(template); }
            template.MappingConfigJson = JsonSerializer.Serialize(map); template.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync();
        }
        return map;
    }

    private static string Normalize(string value) => Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9]", "");

    private static async Task<string> ProcessRowAsync(HvacDbContext db, ImportJob job, Vendor vendor, ImportRow row, Dictionary<string, string> data)
    {
        string Get(string key) => data.GetValueOrDefault(key, "").Trim();
        if (job.EntityType == "Customers")
        {
            string name = Get("customer_name");
            if (name.Length < 2) return "Error:Customer name is missing or too short.";
            string accountNumber = Get("account_number");
            if (accountNumber.Length > 0)
            {
                var account = await db.BrandCustomerAccounts.Include(a => a.CanonicalCustomer)
                    .FirstOrDefaultAsync(a => a.VendorId == vendor.Id && a.LocalAccountNumber == accountNumber);
                if (account?.CanonicalCustomer != null)
                    return await AddCandidateAsync(db, job, vendor, row, data, account.CanonicalCustomer, "Customers", 100, "Vendor account number already exists.");
            }
            var match = await FindCustomerMatchAsync(db, data);
            if (match.HasValue)
                return await AddCandidateAsync(db, job, vendor, row, data, match.Value.customer, "Customers", match.Value.score, match.Value.reason);
            var created = await CreateCustomerAsync(db, vendor, job, row, data);
            row.ResolvedCanonicalCustomerId = created.Id;
            return "New";
        }

        var customer = await FindCustomerAsync(db, Get("customer_name"));
        if (customer == null) return "Error:Customer not found. Import and review customers before their sites or assets.";
        if (job.EntityType == "Sites")
        {
            if (new[] { "site_name", "address", "city", "state", "postal_code" }.Any(k => Get(k).Length == 0))
                return "Error:Site name, street address, city, state, and postal code are required.";
            string siteName = Get("site_name"), siteAddress = Get("address");
            var oldSite = await db.SiteLocations.FirstOrDefaultAsync(s => s.CanonicalCustomerId == customer.Id &&
                s.SiteName.ToLower() == siteName.ToLower() && s.AddressLine.ToLower() == siteAddress.ToLower());
            if (oldSite != null)
                return await AddCandidateAsync(db, job, vendor, row, data, customer, "Sites", 100, $"Site {oldSite.SiteCode} has the same customer, name, and address.");
            await CreateSiteAsync(db, vendor, customer, data);
            row.ResolvedCanonicalCustomerId = customer.Id;
            return "New";
        }
        if (job.EntityType == "Assets")
        {
            if (Get("manufacturer").Length == 0 || Get("site_name").Length == 0) return "Error:Manufacturer and site name are required.";
            string assetSiteName = Get("site_name");
            var site = await db.SiteLocations.FirstOrDefaultAsync(s => s.CanonicalCustomerId == customer.Id && s.SiteName.ToLower() == assetSiteName.ToLower());
            if (site == null) return "Error:Site not found for this customer. Import its site before importing assets.";
            var old = await FindAssetAsync(db, vendor.Id, data);
            if (old != null)
            {
                var ownerId = await db.SiteLocations.Where(s => s.Id == old.SiteLocationId).Select(s => s.CanonicalCustomerId).FirstAsync();
                var owner = await db.CanonicalCustomers.FindAsync(ownerId);
                if (owner != null) return await AddCandidateAsync(db, job, vendor, row, data, owner, "Assets", 100, $"Asset {old.AssetTag} matches by serial number or vendor legacy ID.");
            }
            await CreateAssetAsync(db, vendor, site, data, false);
            row.ResolvedCanonicalCustomerId = customer.Id;
            return "New";
        }
        return "Error:Unsupported entity type.";
    }

    private static async Task<(CanonicalCustomer customer, short score, string reason)?> FindCustomerMatchAsync(HvacDbContext db, Dictionary<string, string> data)
    {
        var customers = await db.CanonicalCustomers.ToListAsync();
        (CanonicalCustomer customer, short score, string reason)? best = null;
        foreach (var customer in customers)
        {
            var result = DuplicateMatchingEngine.EvaluateMatch(data.GetValueOrDefault("customer_name", ""), data.GetValueOrDefault("email"), data.GetValueOrDefault("phone"),
                customer.CustomerName, customer.PrimaryContactEmail, customer.PrimaryContactPhone);
            if (result != null && (!best.HasValue || result.Value.score > best.Value.score)) best = (customer, result.Value.score, result.Value.reason);
        }
        return best;
    }

    private static Task<string> AddCandidateAsync(HvacDbContext db, ImportJob job, Vendor vendor, ImportRow row, Dictionary<string, string> data, CanonicalCustomer customer, string entity, short score, string reason)
    {
        string label = entity switch
        {
            "Customers" => data.GetValueOrDefault("customer_name", ""),
            "Sites" => data.GetValueOrDefault("site_name", ""),
            _ => $"{data.GetValueOrDefault("manufacturer", "")} {data.GetValueOrDefault("model_number", "")} / {data.GetValueOrDefault("serial_number", "")}".Trim()
        };
        db.DuplicateCandidates.Add(new DuplicateCandidate
        {
            DuplicateCode = "DUP-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(), ImportJobId = job.Id,
            ImportRowId = row.Id, EntityType = entity, CandidateVendorId = vendor.Id, CandidateName = label,
            Email = data.GetValueOrDefault("email"), Phone = data.GetValueOrDefault("phone"), AccountNumber = data.GetValueOrDefault("account_number"),
            Address = data.GetValueOrDefault("billing_address") ?? data.GetValueOrDefault("address"), ExistingCanonicalCustomerId = customer.Id,
            ConfidenceScore = score, ConfidenceLevel = score >= 85 ? DuplicateConfidenceLevel.High : DuplicateConfidenceLevel.Medium, MatchReason = reason
        });
        return Task.FromResult("PossibleDuplicate");
    }

    private static async Task<CanonicalCustomer?> FindCustomerAsync(HvacDbContext db, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        string normalized = DuplicateMatchingEngine.NormalizeCustomerName(name);
        return await db.CanonicalCustomers.FirstOrDefaultAsync(c => c.NormalizedName == normalized);
    }

    private static async Task<CanonicalCustomer> CreateCustomerAsync(HvacDbContext db, Vendor vendor, ImportJob job, ImportRow row, Dictionary<string, string> data, bool keepSeparate = false)
    {
        string name = data.GetValueOrDefault("customer_name", "").Trim();
        var customer = new CanonicalCustomer
        {
            CanonicalCode = await UniqueCodeAsync("CUST-", code => db.CanonicalCustomers.AnyAsync(c => c.CanonicalCode == code)),
            CustomerName = name, NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName(name),
            CustomerType = Enum.TryParse<CustomerType>(data.GetValueOrDefault("customer_type")?.Replace(" ", ""), true, out var type) ? type : CustomerType.Commercial,
            PrimaryContactName = BlankToNull(data.GetValueOrDefault("contact_name")), PrimaryContactEmail = BlankToNull(data.GetValueOrDefault("email")),
            PrimaryContactPhone = BlankToNull(data.GetValueOrDefault("phone")), BillingAddress = BlankToNull(data.GetValueOrDefault("billing_address"))
        };
        db.CanonicalCustomers.Add(customer); await db.SaveChangesAsync();
        await UpsertAccountAsync(db, customer, vendor, job, row, data, keepSeparate);
        return customer;
    }

    private static async Task UpsertAccountAsync(HvacDbContext db, CanonicalCustomer customer, Vendor vendor, ImportJob job, ImportRow row, Dictionary<string, string> data, bool keepSeparate = false)
    {
        string number = data.GetValueOrDefault("account_number", "").Trim();
        if (number.Length == 0) number = $"{job.JobCode}-{row.RowNumber}";
        var account = await db.BrandCustomerAccounts.FirstOrDefaultAsync(a => a.VendorId == vendor.Id && a.LocalAccountNumber == number);
        if (account != null && account.CanonicalCustomerId != customer.Id)
        {
            if (!keepSeparate) throw new InvalidOperationException($"Vendor account number {number} is linked to another customer.");
            number = $"{number}-COPY-{job.JobCode[^5..]}-{row.RowNumber}";
            account = await db.BrandCustomerAccounts.FirstOrDefaultAsync(a => a.VendorId == vendor.Id && a.LocalAccountNumber == number);
            if (account != null) throw new InvalidOperationException("Could not generate a unique vendor account number for the separate record.");
        }
        if (account == null) { account = new BrandCustomerAccount { VendorId = vendor.Id, CanonicalCustomerId = customer.Id, LocalAccountNumber = number }; db.BrandCustomerAccounts.Add(account); }
        account.LocalCustomerName = data.GetValueOrDefault("customer_name", customer.CustomerName);
        account.ContactEmail = BlankToNull(data.GetValueOrDefault("email")); account.ContactPhone = BlankToNull(data.GetValueOrDefault("phone"));
        account.BillingAddress = BlankToNull(data.GetValueOrDefault("billing_address")); account.FirstImportJobId ??= job.Id; account.LastImportJobId = job.Id;
        account.SourceSystem = "CSV/XLSX Import"; await db.SaveChangesAsync();
    }

    private static async Task CreateSiteAsync(HvacDbContext db, Vendor vendor, CanonicalCustomer customer, Dictionary<string, string> data)
    {
        db.SiteLocations.Add(new SiteLocation
        {
            SiteCode = await UniqueCodeAsync("LOC-", code => db.SiteLocations.AnyAsync(s => s.SiteCode == code)),
            CanonicalCustomerId = customer.Id, PrimaryVendorId = vendor.Id, SiteName = data.GetValueOrDefault("site_name", "").Trim(),
            SiteType = BlankToNull(data.GetValueOrDefault("site_type")) ?? "Commercial", AddressLine = data.GetValueOrDefault("address", "").Trim(),
            City = data.GetValueOrDefault("city", "").Trim(), State = data.GetValueOrDefault("state", "").Trim(), PostalCode = data.GetValueOrDefault("postal_code", "").Trim(),
            TimeZone = BlankToNull(data.GetValueOrDefault("timezone")) ?? "America/New_York", SiteContactName = BlankToNull(data.GetValueOrDefault("contact_name")),
            SiteContactPhone = BlankToNull(data.GetValueOrDefault("contact_phone"))
        });
        await db.SaveChangesAsync();
    }

    private static async Task<AssetEquipment?> FindAssetAsync(HvacDbContext db, int vendorId, Dictionary<string, string> data)
    {
        string serial = data.GetValueOrDefault("serial_number", "").Trim(), legacy = data.GetValueOrDefault("legacy_asset_id", "").Trim();
        return await db.AssetEquipment.FirstOrDefaultAsync(a => (serial != "" && a.SerialNumber != null && a.SerialNumber.ToLower() == serial.ToLower()) || (legacy != "" && a.SourceVendorId == vendorId && a.LegacyAssetId == legacy));
    }

    private static async Task CreateAssetAsync(HvacDbContext db, Vendor vendor, SiteLocation site, Dictionary<string, string> data, bool separate)
    {
        string tag = data.GetValueOrDefault("asset_tag", "").Trim();
        if (tag.Length == 0 || await db.AssetEquipment.AnyAsync(a => a.AssetTag == tag)) tag = await UniqueCodeAsync("AST-", code => db.AssetEquipment.AnyAsync(a => a.AssetTag == code));
        if (tag.Length > 30) tag = tag[..30];
        string legacy = data.GetValueOrDefault("legacy_asset_id", "").Trim();
        if (separate && legacy.Length > 0 && await db.AssetEquipment.AnyAsync(a => a.SourceVendorId == vendor.Id && a.LegacyAssetId == legacy)) legacy = "";
        if (legacy.Length > 0 && await db.AssetEquipment.AnyAsync(a => a.SourceVendorId == vendor.Id && a.LegacyAssetId == legacy)) throw new InvalidOperationException($"Legacy asset ID {legacy} already exists for this vendor.");
        string serial = data.GetValueOrDefault("serial_number", "").Trim();
        if (separate && serial.Length > 0 && await db.AssetEquipment.AnyAsync(a => a.SerialNumber == serial)) serial = "";
        db.AssetEquipment.Add(new AssetEquipment
        {
            AssetTag = tag, SiteLocationId = site.Id, SourceVendorId = vendor.Id, LegacyAssetId = BlankToNull(legacy), Category = ParseAssetCategory(data.GetValueOrDefault("category", "")),
            Manufacturer = data.GetValueOrDefault("manufacturer", "").Trim(), ModelNumber = BlankToNull(data.GetValueOrDefault("model_number")),
            SerialNumber = BlankToNull(serial), RefrigerantType = BlankToNull(data.GetValueOrDefault("refrigerant_type")),
            InstallDate = ParseDate(data.GetValueOrDefault("install_date")), WarrantyEndDate = ParseDate(data.GetValueOrDefault("warranty_end_date"))
        });
        await db.SaveChangesAsync();
    }

    private static async Task<string> UniqueCodeAsync(string prefix, Func<string, Task<bool>> exists)
    {
        for (int i = 0; i < 25; i++) { string value = prefix + Random.Shared.Next(100000, 999999); if (!await exists(value)) return value; }
        return prefix + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
    private static string? BlankToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static AssetCategory ParseAssetCategory(string? value)
    {
        string category = Normalize(value ?? "");
        if (category.Contains("chiller")) return AssetCategory.Chiller;
        if (category.Contains("boiler")) return AssetCategory.Boiler;
        if (category.Contains("walkin") || category.Contains("freezer") || category.Contains("cooler")) return AssetCategory.WalkInCoolerFreezer;
        if (category.Contains("rack") || category.Contains("refrigeration")) return AssetCategory.RefrigerationRack;
        return AssetCategory.RooftopUnit;
    }
    private static DateOnly? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) return date;
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double serial) && serial > 0 && serial < 100000) return DateOnly.FromDateTime(DateTime.FromOADate(serial));
        return null;
    }

    public static Dictionary<string, string> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new(StringComparer.OrdinalIgnoreCase);
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new(StringComparer.OrdinalIgnoreCase); }
        catch (JsonException) { return new(StringComparer.OrdinalIgnoreCase); }
    }

    public static async Task<string?> ExistingSiteLabelAsync(HvacDbContext db, DuplicateCandidate candidate, Dictionary<string, string> data)
    {
        var site = await FindSiteAsync(db, candidate.ExistingCanonicalCustomerId, data);
        return site == null ? null : $"{site.SiteCode}: {site.SiteName} — {site.AddressLine}, {site.City}";
    }

    public static async Task<string?> ExistingAssetLabelAsync(HvacDbContext db, DuplicateCandidate candidate, Dictionary<string, string> data)
    {
        var asset = await FindAssetAsync(db, candidate.CandidateVendorId, data);
        return asset == null ? null : $"{asset.AssetTag}: {asset.Manufacturer} {asset.ModelNumber} / {asset.SerialNumber}";
    }

    public static async Task AcceptAsync(HvacDbContext db, DuplicateCandidate candidate, Dictionary<string, string> data, Vendor? vendor)
    {
        string Get(string key) => data.GetValueOrDefault(key, "").Trim();
        if (candidate.EntityType == "Customers")
        {
            var customer = await db.CanonicalCustomers.FindAsync(candidate.ExistingCanonicalCustomerId);
            if (customer != null)
            {
                if (Get("customer_name").Length > 0) { customer.CustomerName = Get("customer_name"); customer.NormalizedName = DuplicateMatchingEngine.NormalizeCustomerName(customer.CustomerName); }
                if (Get("contact_name").Length > 0) customer.PrimaryContactName = Get("contact_name");
                if (Get("email").Length > 0) customer.PrimaryContactEmail = Get("email");
                if (Get("phone").Length > 0) customer.PrimaryContactPhone = Get("phone");
                if (Get("billing_address").Length > 0) customer.BillingAddress = Get("billing_address");
                customer.UpdatedAt = DateTime.UtcNow;
                if (vendor != null && candidate.ImportJob != null && candidate.ImportRow != null) await UpsertAccountAsync(db, customer, vendor, candidate.ImportJob, candidate.ImportRow, data);
            }
        }
        else if (candidate.EntityType == "Sites")
        {
            var site = await FindSiteAsync(db, candidate.ExistingCanonicalCustomerId, data);
            if (site != null)
            {
                if (Get("site_name").Length > 0) site.SiteName = Get("site_name");
                if (Get("address").Length > 0) site.AddressLine = Get("address");
                if (Get("city").Length > 0) site.City = Get("city");
                if (Get("state").Length > 0) site.State = Get("state");
                if (Get("postal_code").Length > 0) site.PostalCode = Get("postal_code");
                if (Get("contact_name").Length > 0) site.SiteContactName = Get("contact_name");
                if (Get("contact_phone").Length > 0) site.SiteContactPhone = Get("contact_phone");
                if (Get("site_type").Length > 0) site.SiteType = Get("site_type");
                if (Get("timezone").Length > 0) site.TimeZone = Get("timezone");
            }
        }
        else if (candidate.EntityType == "Assets")
        {
            var asset = await FindAssetAsync(db, candidate.CandidateVendorId, data);
            if (asset != null)
            {
                if (Get("manufacturer").Length > 0) asset.Manufacturer = Get("manufacturer");
                if (Get("model_number").Length > 0) asset.ModelNumber = Get("model_number");
                if (Get("serial_number").Length > 0) asset.SerialNumber = Get("serial_number");
                if (Get("refrigerant_type").Length > 0) asset.RefrigerantType = Get("refrigerant_type");
                if (Get("category").Length > 0) asset.Category = ParseAssetCategory(Get("category"));
                if (ParseDate(data.GetValueOrDefault("install_date")) is { } installDate) asset.InstallDate = installDate;
                if (ParseDate(data.GetValueOrDefault("warranty_end_date")) is { } warrantyDate) asset.WarrantyEndDate = warrantyDate;
                asset.UpdatedAt = DateTime.UtcNow;
            }
        }
        await db.SaveChangesAsync();
    }

    public static async Task KeepSeparateAsync(HvacDbContext db, DuplicateCandidate candidate, Dictionary<string, string> data, Vendor? vendor)
    {
        if (vendor == null || candidate.ImportJob == null || candidate.ImportRow == null) return;
        if (candidate.EntityType == "Customers")
            await CreateCustomerAsync(db, vendor, candidate.ImportJob, candidate.ImportRow, data, keepSeparate: true);
        else if (candidate.EntityType == "Sites")
        {
            var customer = await db.CanonicalCustomers.FindAsync(candidate.ExistingCanonicalCustomerId);
            if (customer != null) await CreateSiteAsync(db, vendor, customer, data);
        }
        else if (candidate.EntityType == "Assets")
        {
            var duplicate = await FindAssetAsync(db, candidate.CandidateVendorId, data);
            int siteId = duplicate?.SiteLocationId ?? 0;
            if (siteId == 0)
            {
                string siteName = data.GetValueOrDefault("site_name", "");
                siteId = await db.SiteLocations.Where(s => s.CanonicalCustomerId == candidate.ExistingCanonicalCustomerId && s.SiteName == siteName).Select(s => s.Id).FirstOrDefaultAsync();
            }
            var site = siteId == 0 ? null : await db.SiteLocations.FindAsync(siteId);
            if (site != null) await CreateAssetAsync(db, vendor, site, data, true);
        }
    }

    private static async Task<SiteLocation?> FindSiteAsync(HvacDbContext db, int customerId, Dictionary<string, string> data)
    {
        string name = data.GetValueOrDefault("site_name", ""), address = data.GetValueOrDefault("address", "");
        return await db.SiteLocations.FirstOrDefaultAsync(s => s.CanonicalCustomerId == customerId && s.SiteName.ToLower() == name.ToLower() && (address == "" || s.AddressLine.ToLower() == address.ToLower()));
    }
}
