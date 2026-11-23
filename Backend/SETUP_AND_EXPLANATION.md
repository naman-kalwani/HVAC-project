# HVAC Connect — Backend Setup Guide & Architecture Walkthrough

This document contains two parts:
1. **Machine Setup & Run Guide** (Exact instructions to extract, configure, and run on a new computer).
2. **Project Walkthrough & Step-by-Step Explanation** (Clear script to explain the system architecture, design decisions, and what was built).

---

## Part 1: Setup Guide for the New Device

### 1. Prerequisites on the Target Device
Make sure the target machine has the following installed:
1. **.NET SDK (8.0 LTS)**:
   - Check by running: `dotnet --version` (should show `8.0.x`)
   - Download from: [https://dotnet.microsoft.com/download/dotnet/8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **PostgreSQL (v15+) & pgAdmin / CLI**:
   - Ensure the PostgreSQL service is running on port `5432`.
   - Default user `postgres` with known password (e.g. `postgres`).

---

### 2. Extract and Prepare the Project
1. Extract the shared `Backend.zip` into your working folder (e.g., `C:\Projects\Backend` or `D:\HVAC\Backend`).
2. Open PowerShell or Command Prompt inside the `Backend` folder.
3. Clean temporary build artifacts to avoid path mismatches:
   ```powershell
   dotnet clean
   ```

---

### 3. Configure Database Connection
Open `src/HvacConnect.ConsoleApp/appsettings.json` and adjust the connection string to match your local PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hvac_connect;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  },
  "Database": {
    "ResetOnStartup": true
  }
}
```

> **Database behavior:** The application applies EF Core migrations and then seeds demo data on startup. `ResetOnStartup: true` deletes and recreates the configured database every time the application starts. Set it to `false` to preserve data and only apply pending migrations. You can also override this for one run with `--reset-db=false` or `--reset-db=true`.
>
> The database schema is configured by `HvacDbContext.SchemaName` in `src/HvacConnect.Infrastructure/Data/HvacDbContext.cs`. To use another schema, change that constant and create a new EF Core migration.

---

### 4. Restore and Build
Run the following from the root `Backend` directory:
```powershell
dotnet restore
dotnet build
```

---

### 5. Launch the Console Platform
Run the console application directly:
```powershell
dotnet run --project src/HvacConnect.ConsoleApp/HvacConnect.ConsoleApp.csproj
```

On startup, sign in with a seeded email/password or choose customer sign-up. Demo account credentials are printed after a fresh seed.

---

## Part 2: Step-by-Step Explanation (How to Present This Work)

Use this script/outline when explaining what was built to evaluators, team members, or stakeholders:

### Step 1: The Problem & Architectural Philosophy
> *"HVAC Connect solves two core industry problems for commercial HVAC & refrigeration groups:*
> 1. **Acquisition Data Consolidation**: Newly acquired contractors bring messy, fragmented legacy customer and equipment records that must be deduplicated and standardized without operational disruption.
> 2. **Active SLA Watchdog & Service Dispatching**: Enforcing strict, contractual response/resolution countdowns on high-value commercial assets (RTUs, Chillers, Boilers, Supermarket Refrigeration).
> 
> *Instead of prematurely building a heavy web layer, we adopted a **Console-First Clean Architecture**. We implemented 100% of the domain rules, database schemas, deduplication algorithms, and SLA watchdog directly in reusable core libraries. This allows us to prove all business operations against a live database before attaching web APIs and UI."*

---

### Step 2: Clean 3-Tier Decoupled Architecture
> *"We decoupled the solution into distinct projects inside one solution:*
> 
> - **`HvacConnect.Core`**: Completely framework-agnostic. Contains all 13 domain models, pure SLA calculation algorithms, text-normalization engines, and password hashing security.
> - **`HvacConnect.Infrastructure`**: Owns data persistence using **Entity Framework Core with Npgsql** targeting PostgreSQL. Configured with PostgreSQL sequences, GIN Trigram similarity indexes, and optimistic concurrency tokens (`xmin`).
> - **`HvacConnect.ConsoleApp`**: The interactive execution environment. Simulates all 5 user roles, automated database seeding, ticket progression, and CSV ingestion directly in the terminal.
> - **`HvacConnect.Api` (Phase 2 Ready)**: A lightweight REST API layer that will wrap the exact same core services with JWT controllers and connect to our HTML/CSS/Vanilla JS frontend with zero backend rework."*

---

### Step 3: Single-Schema Database Architecture
> *"Rather than fragmenting tables into separate schemas, all domain tables are unified in the `Staging` schema with explicit relational boundaries and referential integrity:*
> 
> 1. **`users`**: Role-based accounts for all 5 roles with PBKDF2 hashed passwords, lockout counters, and tenant scopes.
> 2. **`vendors`**: Acquired contractors, tracking two-phase onboarding readiness.
> 3. **`canonical_customers` & `brand_customer_accounts`**: Decouples the unified holding-group customer identity from the vendor's local legacy account numbers.
> 4. **`site_locations` & `site_location_histories`**: Physical facilities with an immutable audit ledger recording every change of address or primary contractor.
> 5. **`asset_equipment`**: High-value commercial equipment (RTUs, Chillers, Boilers, Walk-in Coolers, Refrigeration Racks) with warranty tracking.
> 6. **`service_tickets` & `ticket_comments`**: Active tickets with live SLA calculations and immutable comment threads where work notes and asset history are captured.
> 7. **`import_jobs`, `import_rows`, `duplicate_candidates`**: The multi-step ingestion staging pipeline."*

---

### Step 4: Core Algorithmic Engines Implemented
> *"We implemented two mission-critical, pure business logic engines:*
> 
> 1. **SLA Calculator (`SlaCalculator.cs`)**:
>    - Dynamically evaluates SLA health based on ticket priority:
>      - **Critical**: 2h response / 4h resolution
>      - **High**: 4h response / 8h resolution
>      - **Medium**: 8h response / 24h resolution
>      - **Low**: 24h response / 48h resolution
>    - Calculates real-time state: `WithinSLA`, `AtRisk` (when $\le 25\%$ or $< 2\text{ hours}$ remain), and `Breached`. Freezes the clock upon ticket resolution.
> 
> 2. **Duplicate Matching Engine (`DuplicateMatchingEngine.cs`)**:
>    - Normalizes customer names by stripping corporate suffixes (`LLC`, `Inc`, `Corp`, `Group`, `Services`).
>    - Executes **Word Boundary Prefix Matching** and **Trigram Similarity ($\ge 0.75$)**.
>    - Cross-references corporate email domains (filtering out free-mail providers like Gmail/Yahoo) and phone numbers to score matches as **High Confidence (90)** or **Medium Confidence (70)** for human review."*

---

### Step 5: What Can Be Demonstrated Right Now
> *"When we run the application, it automatically seeds realistic commercial HVAC data and offers:*
> - **Role Switching**: Instantly switch between SuperAdmin, VendorAdmin, Dispatcher, Field Tech, and Facility Manager.
> - **Live SLA Watchdog**: View the color-coded ticket queue (`[WithinSLA]`, `[AtRisk]`, `[Breached]`).
> - **Ticket Lifecycle Control**: Enforce valid transitions (`Open` $\rightarrow$ `Assigned` $\rightarrow$ `InProgress` $\rightarrow$ `Resolved` with mandatory diagnostic notes $\rightarrow$ `Closed`).
> - **Diagnostic Comment Threads**: Add internal or customer-facing work notes that compile the complete historical service record of each asset.
> - **Duplicate Review**: Review detected customer duplicates and apply decisions (`[M]erge`, `[C]reate New`, `[S]kip`)."*

