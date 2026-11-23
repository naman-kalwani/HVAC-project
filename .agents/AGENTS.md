# HVAC Connect — Development Context

> **Last updated:** 23 November 2026
> **Status:** Phase 1 (Console Backend & Database Layer) Functional & Tested | Transitioning to Phase 2 (Web API)  
> **Conversation ID:** a8777a27-494f-4e34-b30b-f9c4068e25d3

---

## Project Overview

We are building **HVAC Connect**, a **Contractor Integration and Service Operations Platform for Commercial HVAC and Refrigeration Groups** (for roll-up and acquisition-led companies such as CoolSys, Service Logic, PremiStar, or Legence).

The platform addresses two core challenges:
1. **Acquisition Data Consolidation (Two-Phase Onboarding)**: Acquired contractors keep using their existing software during **Phase 1 (Coexist & Import)**. Customer, site, asset, and contract records are imported, mapped, and deduplicated into a canonical model without operational disruption. Transition to **Phase 2 (Cutover)** occurs in stages based on checklist readiness.
2. **Service Operations & SLA Watchdog**: Active management of commercial service tickets, technician assignments, SLA countdowns (Within SLA, At Risk, Breached), technician comment threads, and facility manager self-service.

**Tech Stack**: Pure HTML/CSS/Vanilla JS frontend with .NET 8 (LTS) backend, Entity Framework Core with Npgsql, and PostgreSQL database using a unified **`Staging` schema** with migrations.

---

## Confirmed Roles (5)

Based directly on the wireframe workspaces:
1. **Super Admin (Parent Company)**: Group-wide oversight, contractor onboarding & checklist verification, canonical customer/asset registry, CSV data import wizard, duplicate review queue. *Does not create daily operational tickets.*
2. **Vendor Admin (Acquired Contractor)**: Scoped contractor dashboard, customer locations serviced, local technician roster, contractor tickets, SLA monitoring. Cannot view or access other contractors' data.
3. **Service Coordinator (Dispatcher)**: Dispatch queue, real-time SLA countdowns, smart technician assignment based on equipment specialization and active workload, escalation follow-ups.
4. **Technician (Field Worker)**: Assigned tickets, status progression (`Assigned` $\rightarrow$ `InProgress` $\rightarrow$ `Resolved`), diagnostic comments, mandatory closure work notes.
5. **End User (Facility Manager / Customer)**: Self-service registration, registered assets, service requests raising and tracking, location equipment history.

---

## What Has Been Completed (Phase 1)

1. **Solution Architecture (.NET 8 LTS & Clean Architecture)**:
   - `HvacConnect.Core`: Domain models (13 entities), Enums, DTOs, pure `SlaCalculator`, `DuplicateMatchingEngine` (trigram + corporate domain matching), and `PasswordService` (PBKDF2 salted hash).
   - `HvacConnect.Infrastructure`: EF Core `HvacDbContext` targeting PostgreSQL in a **single `Staging` schema**, configured with sequences, GIN trigram indexes, immutable change tracking, and row-level concurrency tokens.
   - `HvacConnect.ConsoleApp`: Interactive CLI terminal application exercising the full database and service layer without needing HTTP or frontend.

2. **Core Fixes & Functional Refinements (Nov 19, 2026)**:
   - **Super Admin Scope Restriction**: Super Admin can view all tickets across the enterprise but cannot raise operational tickets (too low-level for parent company leadership).
   - **Ticket Comment Types by Role**: Comments now properly distinguish between author roles and comment types (`StatusChange` for Super/Vendor Admin, `Assignment` for Coordinator, `Resolution` for Tech, `UserComment` for Customer).
   - **Vendor Ticket Visibility**: When a Vendor Admin raises a ticket, it automatically binds to their `VendorId` and immediately displays in their scoped queue.
   - **Multi-Tenant Contractor Isolation**: Vendor Admins can only view and manage their own contractor profile and local technicians; other vendors remain completely hidden.
   - **Smart Technician Assignment Module**: Recommends and ranks technicians based on equipment category match (RTU $\rightarrow$ HVAC, Chillers $\rightarrow$ Chiller, Coolers/Racks $\rightarrow$ Commercial Refrigeration) and active ticket workload.
   - **Visible Color-Coded Error Handling**: Structured, user-friendly error banners (`[ERROR]`, `[WARNING]`, `✓`) throughout all console commands.
   - **Asset Registration with Asset ID**: Added equipment registration wizard prompting for explicit Asset ID / Tag (e.g. `RTU-00190`), category, OEM brand, serial number, and location linkage.
   - **Customer Self-Registration**: Public sign-up workflow for End Users (Facility Managers), automatically creating an active customer account with a linked canonical profile.

---

## What Remains (Phase 2 & Phase 3)

1. **Phase 2: ASP.NET Core REST API (`HvacConnect.Api`)**:
   - Thin ASP.NET Core controllers exposing the exact same services from `HvacConnect.Core`.
   - JWT authentication (15-minute access token + 7-day rotating refresh tokens) with role and tenant-scoping claims (`VendorId`, `CustomerId`).
   - Swagger / OpenAPI documentation configured with JWT Bearer authentication.
   - Background service (`SlaRefreshBackgroundService`) updating open ticket SLA states in DB every 60 seconds.
   - Ingestion endpoints for multipart file upload (`.csv`, `.xlsx`).

2. **Phase 3: Frontend Integration & Testing**:
   - Replace mock data calls in Vanilla JS frontend (`Frontend/js/api.js` / `router.js`) with live `fetch` calls to the REST API.
   - Attach JWT Bearer tokens to requests and handle HTTP error codes (401 redirect to login, 403, 404, 500 error screens).
   - End-to-end testing across all 5 roles.

---

## Equipment Supported (MVP)
1. **Rooftop Units (RTUs)**
2. **Chillers**
3. **Boilers**
4. **Walk-in Coolers / Freezers**
5. **Refrigeration Racks**

---

## Ticket Lifecycle & Comments
- **Statuses**: `Open` $\rightarrow$ `Assigned` $\rightarrow$ `InProgress` $\rightarrow$ `Resolved` (with mandatory notes) $\rightarrow$ `Closed`.
- **SLA Thresholds**:
  - Critical: 2h response / 4h resolution
  - High: 4h response / 8h resolution
  - Medium: 8h response / 24h resolution
  - Low: 24h response / 48h resolution
  - `AtRisk`: $\le 25\%$ resolution window or $< 2\text{ hours}$ remaining.

---

## Change Log

| Date | Change |
|------|--------|
| 23 Nov 2026 | Replaced the ConsoleApp persona picker with email/password sign-in and customer sign-up. Seeded demo users now use Indian names, mobile numbers, and addresses. Added an EF Core initial migration for the `Staging` schema; `HvacDbContext.SchemaName` is the schema setting. Startup reset is controlled by `Database:ResetOnStartup` in appsettings (enabled by default) or `--reset-db[=true|false]`. Removed the unused frontend quick-role login helper. |
| 23 Nov 2026 | Expanded agency-scoped coordinator access across staff, customer, location, asset, and ticket workflows. Coordinators now see and manage only their own agency records, with cross-agency Super Admin visibility retained. Updated the quick persona login menu to include both Super Admins, all three Vendor Admins, agency coordinators, technicians, and facility manager personas. |
| 23 Nov 2026 | Implemented Coordinator Shared Dispatch Queue & Ticket Ownership architecture: Added `OwnerCoordinatorId`, `ClaimedAt`, `AssignedByUserId`, and `AssignedAt` to `ServiceTicket` with partial index for unclaimed queue (`WHERE owner_coordinator_id IS NULL AND status = 'Open'`) and composite index `(owner_coordinator_id, status)`. Implemented atomic concurrency claiming (`ExecuteUpdateAsync` with 409 Conflict handling) and Take-Over flow with required audit comment. Updated ticket creation to start with `VendorId = null` and `OwnerCoordinatorId = null`, auto-binding `VendorId` to technician's vendor upon assignment. Added queue filters (All / Unclaimed / My Claimed) on coordinator screen. |
| 23 Nov 2026 | Seeded comprehensive multi-agency dataset in `DatabaseSeeder.cs` (3 distinct acquired agencies: Pacific Climate, Metro Mechanical, Tri-State HVAC; multiple users per role for all 5 roles; multiple customers, facilities, assets, and tickets). Configured strict tenant isolation: Super Admin views group-wide data; Vendor Admin and agency Service Coordinators view strictly their agency's tickets, staff, customers, locations, and equipment; End Users view only their own customer facilities and requests. Updated quick persona login menu in `Program.Authentication.cs`. |
| 23 Nov 2026 | Modularized ConsoleApp architecture into focused partial classes (`Program.Authentication.cs`, `Program.Tickets.cs`, `Program.Staff.cs`, `Program.CustomerAssets.cs`, `Program.DataOnboarding.cs`, `Program.ConsoleHelpers.cs`). Added comprehensive Data Ingestion Pipeline (`VendorImportPipeline.cs`, `ImportFileReader.cs`) supporting both CSV and Excel (.xlsx) parsing, automated column alias mapping, canonical trigram customer matching, and site/asset staging. Integrated end-to-end multi-entity test datasets. |
| 19 Nov 2026 | Enforced strict 10-digit phone number validation (`^\d{10}$`), added comprehensive Technician Job Schedule view (accessible by Super Admin, Vendor Admin, Service Coordinator, and Field Technicians), and implemented clean box-drawing ASCII table formatting (`TableFormatter.cs`) across all 10 console data lists (Tickets, Contractors, Staff, Registry, Locations, Assets, Checklists, Schedules). |
| 19 Nov 2026 | Implemented 8 core functional refinements in ConsoleApp: Super Admin ticket creation restriction, role-specific comment types, vendor ticket scoping & isolation, smart technician assignment module, structured error messaging, asset creation with Asset ID, customer self-registration, and pinned to .NET 8 LTS. Updated AGENTS.md. |
| 19 Nov 2026 | Implemented strict RBAC role-specific workspace menus & gatekeeper (blocking cross-role execution with 403 Forbidden), along with production-grade InputValidator engine (regex for password, email, phone, asset tags, and length constraints). Verified 0 warnings 0 errors. |
| 19 Nov 2026 | Enhanced authentication security (mandatory PBKDF2 password verification + 5-attempt lockout for custom logins) and expanded role capabilities: Super Admin (add vendor, technician, dispatcher; separate vendor list from staff list), Vendor Admin (dedicated technician roster, ticket assignment shortcut, add technician), Service Coordinator (technician roster access & ticket assignment), Technician (daily job schedule view with date filtering), and End User (add new facility/location wizard with validation). |
| 17 Nov 2026 | Downgraded and built backend on .NET 8 SDK; initial single-schema PostgreSQL architecture; generated ER diagram and setup guide. |
| 10 Nov 2026 | Pivoted project from generic CRM to HVAC Connect. Confirmed 5 roles, 3-sprint delivery structure, simplified rule-based matching, and updated ticket lifecycle with technician comments. |
