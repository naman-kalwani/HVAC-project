using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HvacConnect.ConsoleApp;

public static partial class Program
{
    private static User? _currentUser;

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintBanner();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Retrieve the connection string – throw a clear error if it is missing
        string? connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            PrintError("Connection string 'DefaultConnection' not found in appsettings.json.");
            return;
        }

        var optionsBuilder = new DbContextOptionsBuilder<HvacDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
                          options.MigrationsHistoryTable("__EFMigrationsHistory", HvacDbContext.SchemaName))
                      .UseSnakeCaseNamingConvention();

        HvacDbContext db;
        try
        {
            db = new HvacDbContext(optionsBuilder.Options);

            bool resetDatabase = config.GetValue("Database:ResetOnStartup", false);
            string? resetArgument = args.FirstOrDefault(a =>
                a.Equals("--reset-db", StringComparison.OrdinalIgnoreCase) ||
                a.StartsWith("--reset-db=", StringComparison.OrdinalIgnoreCase));

            if (resetArgument != null)
            {
                int resetArgumentIndex = Array.IndexOf(args, resetArgument);
                if (resetArgument.StartsWith("--reset-db=", StringComparison.OrdinalIgnoreCase))
                {
                    bool.TryParse(resetArgument.Split('=', 2)[1], out resetDatabase);
                }
                else if (resetArgumentIndex >= 0 &&
                         resetArgumentIndex + 1 < args.Length &&
                         bool.TryParse(args[resetArgumentIndex + 1], out bool resetValue))
                {
                    resetDatabase = resetValue;
                }
                else
                {
                    resetDatabase = true;
                }
            }

            if (resetDatabase)
            {
                Console.WriteLine("[DATABASE] Reset-on-startup is enabled. Recreating database from scratch...");
                await db.Database.EnsureDeletedAsync();
            }

            await db.Database.MigrateAsync();
            await DatabaseSeeder.SeedAsync(db);
        }
        catch (Exception ex)
        {
            PrintError($"Could not connect to PostgreSQL database: {ex.Message}");
            Console.WriteLine("Make sure PostgreSQL is running on localhost:5432 and credentials match appsettings.json.");
            return;
        }

        while (true)
        {
            if (_currentUser == null)
            {
                await ShowLoginMenu(db);
            }
            else
            {
                await ShowRoleDashboard(db);
            }
        }
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔═════════════════════════════════════════════════════════════════════════╗
║     HVAC CONNECT — COMMERCIAL HVAC & REFRIGERATION CONSOLE PLATFORM     ║
║        [Phase 1: Direct Database Service Layer & Operations]            ║
╚═════════════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    private static async Task ShowRoleDashboard(HvacDbContext db)
    {
        Console.WriteLine($"\n═════════ {_currentUser!.Role.ToString().ToUpper()} WORKSPACE ═════════");
        Console.WriteLine($"Logged User: {_currentUser.FirstName} {_currentUser.LastName} ({_currentUser.Email})" +
                          (_currentUser.Vendor != null ? $" | Vendor: {_currentUser.Vendor.BrandName}" : "") +
                          (_currentUser.Customer != null ? $" | Customer: {_currentUser.Customer.CustomerName}" : ""));

        var menuActions = GetMenuForRole(_currentUser.Role);
        foreach (var item in menuActions)
        {
            Console.WriteLine($"{item.Key}. {item.Value.Title}");
        }
        Console.WriteLine("L. Logout");
        Console.Write("Select action: ");

        string? choice = Console.ReadLine()?.Trim().ToUpper();
        if (choice == "L")
        {
            _currentUser = null;
            Console.WriteLine("Logged out successfully.");
            return;
        }

        if (menuActions.TryGetValue(choice ?? "", out var actionItem))
        {
            await actionItem.Handler(db);
        }
        else
        {
            PrintError($"Access Denied (403 Forbidden): Option '{choice}' is not authorized for role [{_currentUser.Role}].");
        }
    }

    private class MenuOption
    {
        public string Title { get; set; } = string.Empty;
        public Func<HvacDbContext, Task> Handler { get; set; } = null!;
    }

    private static Dictionary<string, MenuOption> GetMenuForRole(UserRole role)
    {
        var menu = new Dictionary<string, MenuOption>();

        switch (role)
        {
            case UserRole.SuperAdmin:
                menu.Add("1", new MenuOption { Title = "View All Service Tickets & SLA Watchdog (Group-Wide Table)", Handler = ListTicketsMenu });
                menu.Add("2", new MenuOption { Title = "View Canonical Customer Registry", Handler = ListCustomers });
                menu.Add("3", new MenuOption { Title = "View All Locations & Sites", Handler = ListLocations });
                menu.Add("4", new MenuOption { Title = "View Equipment Assets & History", Handler = ListAssets });
                menu.Add("5", new MenuOption { Title = "View Acquired Vendors", Handler = ListVendorsOnly });
                menu.Add("6", new MenuOption { Title = "View Technicians & Staff Across Group", Handler = ListTechniciansAndStaff });
                menu.Add("7", new MenuOption { Title = "View Any Technician's Job Schedule", Handler = ViewAnyTechnicianSchedule });
                menu.Add("8", new MenuOption { Title = "Add Acquired Vendor", Handler = AddVendorWizard });
                menu.Add("9", new MenuOption { Title = "Add Field Technician", Handler = AddTechnicianWizard });
                menu.Add("10", new MenuOption { Title = "Add Service Coordinator (Dispatcher)", Handler = AddServiceCoordinatorWizard });
                menu.Add("11", new MenuOption { Title = "Data Ingestion: Import Wizard & File Staging", Handler = RunImportWizard });
                menu.Add("12", new MenuOption { Title = "Duplicate Review Queue", Handler = RunDuplicateReview });
                menu.Add("13", new MenuOption { Title = "Vendor Onboarding Checklists & Approval", Handler = ViewOnboardingChecklists });
                menu.Add("14", new MenuOption { Title = "Add Equipment Asset to Registry", Handler = CreateAssetWizard });
                menu.Add("15", new MenuOption { Title = "Create Vendor Admin Login for Existing Vendor", Handler = AddVendorAdminWizard });
                break;

            case UserRole.VendorAdmin:
                menu.Add("1", new MenuOption { Title = "View Vendor Service Tickets & SLA Watchdog", Handler = ListTicketsMenu });
                menu.Add("2", new MenuOption { Title = "Assign / Reassign Technician to Ticket", Handler = AssignTicketMenuEntry });
                menu.Add("3", new MenuOption { Title = "View Local Technicians & Staff", Handler = ListTechniciansAndStaff });
                menu.Add("4", new MenuOption { Title = "View Technician Job Schedule", Handler = ViewAnyTechnicianSchedule });
                menu.Add("5", new MenuOption { Title = "View Serviced Customers", Handler = ListCustomers });
                menu.Add("6", new MenuOption { Title = "View Serviced Locations & Sites", Handler = ListLocations });
                menu.Add("7", new MenuOption { Title = "View Serviced Equipment Assets", Handler = ListAssets });
                menu.Add("8", new MenuOption { Title = "Add Technician to Local Vendor Roster", Handler = AddTechnicianWizard });
                menu.Add("9", new MenuOption { Title = "Raise New Service Ticket for Vendor", Handler = CreateTicketWizard });
                menu.Add("10", new MenuOption { Title = "Register Equipment Asset at Serviced Site", Handler = CreateAssetWizard });
                break;

            case UserRole.ServiceCoordinator:
                menu.Add("1", new MenuOption { Title = "Dispatch Queue & SLA Countdown Watchdog", Handler = ListTicketsMenu });
                menu.Add("2", new MenuOption { Title = "Assign / Reassign Technician to Ticket", Handler = AssignTicketMenuEntry });
                menu.Add("3", new MenuOption { Title = "View Field Technicians Roster & Workload", Handler = ListTechniciansAndStaff });
                menu.Add("4", new MenuOption { Title = "View Any Technician's Job Schedule", Handler = ViewAnyTechnicianSchedule });
                menu.Add("5", new MenuOption { Title = "View Customers", Handler = ListCustomers });
                menu.Add("6", new MenuOption { Title = "View Locations & Access Notes", Handler = ListLocations });
                menu.Add("7", new MenuOption { Title = "View Equipment Assets", Handler = ListAssets });
                menu.Add("8", new MenuOption { Title = "Raise New Service Ticket (Incoming Call / Dispatch)", Handler = CreateTicketWizard });
                break;

            case UserRole.Technician:
                menu.Add("1", new MenuOption { Title = "My Assigned Service Tickets & Work Orders", Handler = ListTicketsMenu });
                menu.Add("2", new MenuOption { Title = "View My Job Schedule & Profile", Handler = ViewMyTechnicianSchedule });
                menu.Add("3", new MenuOption { Title = "View Assigned Machine Specs & History", Handler = ListAssets });
                menu.Add("4", new MenuOption { Title = "View Site Locations & Access Directions", Handler = ListLocations });
                break;

            case UserRole.EndUser:
                menu.Add("1", new MenuOption { Title = "My Service Requests & Repair Status", Handler = ListTicketsMenu });
                menu.Add("2", new MenuOption { Title = "My Registered Facilities & Locations", Handler = ListLocations });
                menu.Add("3", new MenuOption { Title = "Add New Facility / Location", Handler = AddLocationWizard });
                menu.Add("4", new MenuOption { Title = "My Registered HVAC & Refrigeration Assets", Handler = ListAssets });
                menu.Add("5", new MenuOption { Title = "Raise Emergency / Service Request", Handler = CreateTicketWizard });
                menu.Add("6", new MenuOption { Title = "Register New Equipment Asset", Handler = CreateAssetWizard });
                break;
        }

        return menu;
    }
}
