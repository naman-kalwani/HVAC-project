# Backend Project and File Guide

This guide explains the files in `Backend/HvacConnect.sln` in everyday language. It covers the console application and its supporting projects, plus the sample data and diagrams. Generated `bin` and `obj` files are left out because they are temporary files created by .NET.

## The big picture

Think of the backend as three parts:

1. **ConsoleApp** is the part a person uses. It shows menus and asks questions.
2. **Core** holds the main business rules and the shapes of the information.
3. **Infrastructure** saves and retrieves that information in PostgreSQL.

When the program starts, it reads its settings, prepares the database, adds demo records, and then shows a sign-in screen. After sign-in, the menus depend on the user's role.

## Projects

### `src/HvacConnect.ConsoleApp`

The interactive command-line program. It handles sign-in, user menus, ticket work, staff, customer records, and importing vendor files. Its commands call Core rules and use Infrastructure to access the database.

### `src/HvacConnect.Core`

The shared rulebook. It describes the people and business records the system understands and contains reusable rules such as password checking, SLA timing, duplicate matching, and input validation. It does not display menus or connect directly to PostgreSQL.

### `src/HvacConnect.Infrastructure`

The database layer. It tells Entity Framework Core how the Core records map to PostgreSQL tables, and contains the migrations that create or update those tables.

## Console application files

| File | In simple terms |
|---|---|
| `src/HvacConnect.ConsoleApp/Program.cs` | Starts the application, reads settings, prepares and seeds the database, then shows the right menu for the signed-in role. |
| `src/HvacConnect.ConsoleApp/Program.Authentication.cs` | Shows email sign-in and customer sign-up, checks passwords, and handles failed-login lockouts. |
| `src/HvacConnect.ConsoleApp/Program.ConsoleHelpers.cs` | Shared helpers for prompts and readable success, warning, and error messages. |
| `src/HvacConnect.ConsoleApp/Program.CustomerAssets.cs` | Lets users view customers, sites, and equipment, and add sites or equipment. |
| `src/HvacConnect.ConsoleApp/Program.DataOnboarding.cs` | Starts vendor imports, reviews possible duplicates, and displays vendor onboarding checklists. |
| `src/HvacConnect.ConsoleApp/Program.Staff.cs` | Handles staff and vendor management, technician schedules, and assignment entry points. |
| `src/HvacConnect.ConsoleApp/Program.Tickets.cs` | Lists tickets, shows ticket details and comments, manages ticket actions, assigns technicians, and creates tickets. |
| `src/HvacConnect.ConsoleApp/DatabaseSeeder.cs` | Adds the sample agencies, users, customers, sites, assets, tickets, and other demo records to the database. |
| `src/HvacConnect.ConsoleApp/TableFormatter.cs` | Turns lists of records into aligned text tables for the console. |
| `src/HvacConnect.ConsoleApp/HvacConnect.ConsoleApp.csproj` | The ConsoleApp project recipe: .NET version, dependencies, and links to the Core and Infrastructure projects. |
| `src/HvacConnect.ConsoleApp/appsettings.json` | The database connection and the `ResetOnStartup` setting. When reset is enabled, startup deletes and recreates the configured database. |

### Import files used by the console application

| File | In simple terms |
|---|---|
| `src/HvacConnect.ConsoleApp/Import/ImportFileReader.cs` | Opens CSV and `.xlsx` files and turns their rows into data the import process can work with. |
| `src/HvacConnect.ConsoleApp/Import/VendorImportPipeline.cs` | Guides vendor data through column matching, validation, duplicate checks, staging, and acceptance/rejection. |

## Core project files

| File | In simple terms |
|---|---|
| `src/HvacConnect.Core/Class1.cs` | Default empty class left from creating the project; it is not part of the main HVAC workflows. |
| `src/HvacConnect.Core/HvacConnect.Core.csproj` | Lists the Core project's .NET version and packages used for validation and secure password hashing. |
| `src/HvacConnect.Core/Domain/Entities/Entities.cs` | Defines the main records the system stores, such as users, vendors, customers, sites, equipment, tickets, comments, and import records. |
| `src/HvacConnect.Core/Domain/Enums/DomainEnums.cs` | Defines the fixed choices used by those records, such as user roles, ticket status, priority, SLA state, and import status. |
| `src/HvacConnect.Core/Services/DuplicateMatchingEngine.cs` | Compares imported customer details with existing customers and estimates whether they may be the same customer. |
| `src/HvacConnect.Core/Services/PasswordService.cs` | Safely turns passwords into salted hashes and checks a password against a saved hash. |
| `src/HvacConnect.Core/Services/SlaCalculator.cs` | Calculates response/resolution time limits and whether a ticket is within SLA, at risk, or breached. |
| `src/HvacConnect.Core/Validation/InputValidator.cs` | Contains common checks for fields such as names, email addresses, passwords, phone numbers, and asset tags. |

## Infrastructure project files

| File | In simple terms |
|---|---|
| `src/HvacConnect.Infrastructure/Class1.cs` | Default empty class left from creating the project; it is not part of the database workflow. |
| `src/HvacConnect.Infrastructure/HvacConnect.Infrastructure.csproj` | Lists the Infrastructure project's packages for PostgreSQL, Entity Framework Core, CSV support, and naming conventions. |
| `src/HvacConnect.Infrastructure/Data/HvacDbContext.cs` | The bridge between the C# records and database tables. It defines relationships, indexes, and the schema name (`HvacDbContext.SchemaName`). |
| `src/HvacConnect.Infrastructure/Data/HvacDbContextFactory.cs` | Gives Entity Framework command-line tools a way to create the database context when generating migrations, without starting the interactive app. |
| `src/HvacConnect.Infrastructure/Migrations/20261123173756_InitialCreate.cs` | The first database instruction set: creates the `Staging` schema, tables, relationships, and indexes. |
| `src/HvacConnect.Infrastructure/Migrations/20261123173756_InitialCreate.Designer.cs` | Generated description of the model as it looked when the initial migration was created. Normally, let EF Core regenerate this file. |
| `src/HvacConnect.Infrastructure/Migrations/HvacDbContextModelSnapshot.cs` | Generated picture of the latest database model. EF Core compares it with the current model when preparing future migrations. |

## Backend root files

| File | In simple terms |
|---|---|
| `HvacConnect.sln` | Opens the three backend projects together as one solution in Visual Studio or the .NET CLI. |
| `global.json` | Tells .NET which SDK version to use for this backend solution. |
| `SETUP_AND_EXPLANATION.md` | Setup instructions and a longer explanation of the architecture and project goals. |
| `er-diagram.mmd` | Editable Mermaid text that describes the database tables and how they connect. |
| `er-diagram-viewer.html` | A browser page for viewing the entity-relationship diagram. |

## Sample import files

These are practice files for trying the import wizard. They are data examples, not application code.

| File | In simple terms |
|---|---|
| `import_wizard_files/hvac_customers_two_vendors.csv` | Customer and service details from two vendors in one CSV, useful for seeing how vendor records can overlap. |
| `import_wizard_files/vendor_1_customer.csv` | Customer import example for the first vendor. |
| `import_wizard_files/vendor_2_customer.csv` | Customer import example for the second vendor. |

## End-to-end import test data

These files are arranged to exercise the importer with multiple vendors and record types. The `alpha` and `beta` sets include customers, sites, and equipment; the `gamma` workbook is an Excel customer-import example.

| File | In simple terms |
|---|---|
| `test-data/import-e2e/vendor_alpha_customers.csv` | Customer records for the alpha test vendor. |
| `test-data/import-e2e/vendor_alpha_sites.csv` | Site/location records for the alpha test vendor. |
| `test-data/import-e2e/vendor_alpha_assets.csv` | Equipment records for the alpha test vendor. |
| `test-data/import-e2e/vendor_beta_customers.csv` | Customer records for the beta test vendor, with a different set of column names. |
| `test-data/import-e2e/vendor_beta_sites.csv` | Site/location records for the beta test vendor. |
| `test-data/import-e2e/vendor_beta_assets.csv` | Equipment records for the beta test vendor. |
| `test-data/import-e2e/vendor_gamma_customer.xlsx` | Excel workbook used to try importing customer data from `.xlsx`. |

## A simple way to follow the code

For sign-in, start with `Program.Authentication.cs`. For tickets, start with `Program.Tickets.cs`, then look at the related records in Core's `Entities.cs` and the table rules in Infrastructure's `HvacDbContext.cs`. For vendor imports, start with `Program.DataOnboarding.cs`, then follow `VendorImportPipeline.cs` and `ImportFileReader.cs`.

The browser frontend and the separate API work are outside this guide's scope; this guide covers the projects in `Backend/HvacConnect.sln`.
