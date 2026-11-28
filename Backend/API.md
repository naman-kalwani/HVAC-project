# HVAC Connect HTTP API

Run the service from the repository root:

```powershell
dotnet run --project Backend/src/HvacConnect.Api
```

The API listens on `http://localhost:5000` by default. Configure `ConnectionStrings:DefaultConnection` or `ConnectionStrings__DefaultConnection` for PostgreSQL. The frontend client reads `window.HVAC_API_URL` before `Frontend/js/api.js` loads; its default is `http://localhost:5000/api`.

## Routes

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/health` | Liveness |
| POST | `/api/auth/login` | `{ "email": "...", "password": "..." }`; verifies the stored password hash and returns a user DTO |
| POST | `/api/auth/signup` | `{ "name": "...", "email": "...", "password": "..." }`; creates an active End User account and returns a user DTO |
| GET | `/api/auth/me/{id}` | Load public user profile |
| GET, POST | `/api/vendors` | Vendor list and create |
| GET | `/api/vendors/{id}` | Vendor details and checklist |
| GET, POST | `/api/customers` | Canonical customer list and create |
| GET, POST | `/api/locations` | Site list and create |
| GET, POST | `/api/assets` | Equipment list and create |
| GET, POST | `/api/tickets` | Ticket list and create |
| GET | `/api/tickets/{id}` | Ticket with comments |
| PATCH | `/api/tickets/{id}` | Update status, priority, technician assignment, or resolution notes |
| POST | `/api/tickets/{id}/comments` | Add ticket comment |
| GET | `/api/technicians` | Technician roster |
| GET | `/api/imports` | Import history |
| GET | `/api/duplicates` | Pending duplicate review queue |
| POST | `/api/duplicates/{id}/decision` | Save duplicate review decision |
| GET | `/api/audit` | Recent site history events |
| GET | `/api/dashboard/summary` | Aggregate dashboard counters |

Enums are accepted as their C# enum names (for example `"High"`, `"InProgress"`, and `"Chiller"`). Create and update endpoints return the persisted entity or DTO; invalid relationships and duplicate keys are reported by ASP.NET's standard error response.

The API currently provides login verification but does not issue access tokens or enforce authorization policies. Treat it as a development integration host; before exposing it beyond a trusted environment, add token/session authentication, role and tenant authorization, request validation, and production secrets management.
