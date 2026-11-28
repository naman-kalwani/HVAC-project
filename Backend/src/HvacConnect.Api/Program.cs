using HvacConnect.Core.Domain.Entities;
using HvacConnect.Core.Domain.Enums;
using HvacConnect.Core.Services;
using HvacConnect.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=127.0.0.1;Port=5433;Database=hvac;Username=admin;Password=admin123";
builder.Services.AddDbContext<HvacDbContext>(o => o.UseNpgsql(connection,
    x => x.MigrationsHistoryTable("__EFMigrationsHistory", HvacDbContext.SchemaName)).UseSnakeCaseNamingConvention());
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HVAC Connect API v1");
    options.RoutePrefix = "swagger";
});
app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "HvacConnect.Api" }));

app.MapPost("/api/auth/login", async (LoginRequest request, HvacDbContext db) => {
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password)) return Results.Unauthorized();
    var email = request.Email.Trim().ToLowerInvariant();
    var user = await db.Users.Include(x => x.Vendor).Include(x => x.Customer).SingleOrDefaultAsync(x => x.Email == email);
    if (user is null || !user.IsActive || !PasswordService.VerifyPassword(request.Password, user.PasswordHash)) return Results.Unauthorized();
    user.LastLoginAt = DateTime.UtcNow; await db.SaveChangesAsync();
    return Results.Ok(new { user = UserDto(user) });
});
app.MapPost("/api/auth/signup", async (SignupRequest request, HvacDbContext db) => {
    var name = request.Name?.Trim() ?? "";
    var email = request.Email?.Trim().ToLowerInvariant() ?? "";
    var pieces = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    if (pieces.Length == 0 || string.IsNullOrWhiteSpace(email) || !email.Contains('@') || string.IsNullOrEmpty(request.Password) || request.Password.Length < 8)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Enter a name, valid email, and password with at least 8 characters."] });
    if (await db.Users.AnyAsync(x => x.Email == email))
        return Results.Conflict(new { message = "An account with this email already exists." });
    var user = new User {
        FirstName = pieces[0], LastName = pieces.Length > 1 ? pieces[1] : "",
        Email = email, PasswordHash = PasswordService.HashPassword(request.Password),
        Role = UserRole.EndUser, IsActive = true
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/auth/me/{user.Id}", new { user = UserDto(user) });
});
app.MapGet("/api/auth/me/{id:int}", async (int id, HvacDbContext db) => { var u=await db.Users.Include(x=>x.Vendor).Include(x=>x.Customer).FirstOrDefaultAsync(x=>x.Id==id && x.IsActive); return u is null ? Results.NotFound() : Results.Ok(UserDto(u)); });

// Read endpoints return compact DTOs, avoiding EF navigation cycles and keeping the wire format stable.
app.MapGet("/api/vendors", async (HvacDbContext db) => Results.Ok(await db.Vendors.OrderBy(x=>x.BrandName).Select(x=>new { x.Id,x.VendorCode,x.BrandName,x.Region,x.Status,x.OnboardingPhase,x.PrimaryContactName,x.PrimaryContactEmail,x.PrimaryContactPhone,x.IsActive }).ToListAsync()));
app.MapPost("/api/vendors", async (VendorInput x,HvacDbContext db)=> { var v=new Vendor { VendorCode=x.VendorCode,BrandName=x.BrandName,Region=x.Region,LegalEntityName=x.LegalEntityName,PrimaryContactName=x.PrimaryContactName,PrimaryContactEmail=x.PrimaryContactEmail,PrimaryContactPhone=x.PrimaryContactPhone }; db.Vendors.Add(v); await db.SaveChangesAsync(); return Results.Created($"/api/vendors/{v.Id}",v); });
app.MapGet("/api/vendors/{id:int}", async (int id,HvacDbContext db)=> { var v=await db.Vendors.Include(x=>x.Checklists).FirstOrDefaultAsync(x=>x.Id==id); return v is null?Results.NotFound():Results.Ok(new {v.Id,v.VendorCode,v.BrandName,v.Region,v.Status,v.OnboardingPhase,v.Checklists}); });
app.MapGet("/api/customers", async (HvacDbContext db)=>Results.Ok(await db.CanonicalCustomers.OrderBy(x=>x.CustomerName).Select(x=>new {x.Id,x.CanonicalCode,x.CustomerName,x.CustomerType,x.PrimaryContactName,x.PrimaryContactEmail,x.PrimaryContactPhone,x.BillingAddress,x.Status}).ToListAsync()));
app.MapPost("/api/customers",async(CustomerInput x,HvacDbContext db)=>{var c=new CanonicalCustomer {CanonicalCode=x.CanonicalCode,CustomerName=x.CustomerName,NormalizedName=x.CustomerName.Trim().ToUpper(),CustomerType=x.CustomerType,PrimaryContactName=x.PrimaryContactName,PrimaryContactEmail=x.PrimaryContactEmail,PrimaryContactPhone=x.PrimaryContactPhone,BillingAddress=x.BillingAddress}; db.CanonicalCustomers.Add(c); await db.SaveChangesAsync(); return Results.Created($"/api/customers/{c.Id}",c);});
app.MapGet("/api/locations",async(HvacDbContext db)=>Results.Ok(await db.SiteLocations.Include(x=>x.Customer).Include(x=>x.PrimaryVendor).Select(x=>new {x.Id,x.SiteCode,x.SiteName,x.SiteType,x.CanonicalCustomerId,customerName=x.Customer!.CustomerName,x.PrimaryVendorId,vendorName=x.PrimaryVendor==null?null:x.PrimaryVendor.BrandName,x.AddressLine,x.City,x.State,x.PostalCode,x.AccessInstructions,x.IsActive}).ToListAsync()));
app.MapPost("/api/locations",async(LocationInput x,HvacDbContext db)=>{var s=new SiteLocation {SiteCode=x.SiteCode,CanonicalCustomerId=x.CanonicalCustomerId,PrimaryVendorId=x.PrimaryVendorId,SiteName=x.SiteName,AddressLine=x.AddressLine,City=x.City,State=x.State,PostalCode=x.PostalCode,SiteContactName=x.SiteContactName,SiteContactPhone=x.SiteContactPhone,AccessInstructions=x.AccessInstructions}; db.SiteLocations.Add(s); await db.SaveChangesAsync(); return Results.Created($"/api/locations/{s.Id}",s);});
app.MapGet("/api/assets",async(HvacDbContext db)=>Results.Ok(await db.AssetEquipment.Include(x=>x.SiteLocation).ThenInclude(x=>x!.Customer).Select(x=>new {x.Id,x.AssetTag,x.Category,x.Manufacturer,x.ModelNumber,x.SerialNumber,x.Status,x.SiteLocationId,locationName=x.SiteLocation!.SiteName,customerName=x.SiteLocation.Customer!.CustomerName,x.InstallDate,x.WarrantyEndDate}).ToListAsync()));
app.MapPost("/api/assets",async(AssetInput x,HvacDbContext db)=>{var a=new AssetEquipment {AssetTag=x.AssetTag,SiteLocationId=x.SiteLocationId,Category=x.Category,Manufacturer=x.Manufacturer,ModelNumber=x.ModelNumber,SerialNumber=x.SerialNumber,RefrigerantType=x.RefrigerantType,InstallDate=x.InstallDate,WarrantyEndDate=x.WarrantyEndDate}; db.AssetEquipment.Add(a); await db.SaveChangesAsync(); return Results.Created($"/api/assets/{a.Id}",a);});
app.MapGet("/api/tickets",async(HvacDbContext db)=>Results.Ok(await db.ServiceTickets.Include(x=>x.Customer).Include(x=>x.SiteLocation).Include(x=>x.Asset).Include(x=>x.Vendor).Include(x=>x.AssignedTechnician).OrderByDescending(x=>x.CreatedAt).Select(x=>new {x.Id,x.TicketNumber,x.Title,x.Description,x.Priority,x.Status,x.SLAState,x.CanonicalCustomerId,customerName=x.Customer!.CustomerName,x.SiteLocationId,locationName=x.SiteLocation!.SiteName,x.AssetEquipmentId,assetTag=x.Asset!.AssetTag,x.VendorId,vendorName=x.Vendor==null?null:x.Vendor.BrandName,x.AssignedTechnicianId,technicianName=x.AssignedTechnician==null?null:x.AssignedTechnician.FirstName+" "+x.AssignedTechnician.LastName,x.TargetResolutionAt,x.CreatedAt}).ToListAsync()));
app.MapGet("/api/tickets/{id:int}",async(int id,HvacDbContext db)=>{var t=await db.ServiceTickets.Include(x=>x.Comments).ThenInclude(x=>x.AuthorUser).FirstOrDefaultAsync(x=>x.Id==id); return t is null?Results.NotFound():Results.Ok(t);});
app.MapPost("/api/tickets",async(TicketInput x,HvacDbContext db)=>{var t=new ServiceTicket {TicketNumber=x.TicketNumber,CanonicalCustomerId=x.CanonicalCustomerId,SiteLocationId=x.SiteLocationId,AssetEquipmentId=x.AssetEquipmentId,VendorId=x.VendorId,Title=x.Title,Description=x.Description,Priority=x.Priority,RaisedByUserId=x.RaisedByUserId}; db.ServiceTickets.Add(t); await db.SaveChangesAsync(); return Results.Created($"/api/tickets/{t.Id}",t);});
app.MapPatch("/api/tickets/{id:int}",async(int id,TicketUpdate x,HvacDbContext db)=>{var t=await db.ServiceTickets.FindAsync(id); if(t is null)return Results.NotFound(); if(x.Status.HasValue)t.Status=x.Status.Value; if(x.Priority.HasValue)t.Priority=x.Priority.Value; if(x.AssignedTechnicianId.HasValue){t.AssignedTechnicianId=x.AssignedTechnicianId;t.AssignedAt=DateTime.UtcNow;} if(x.ResolutionNotes is not null)t.ResolutionNotes=x.ResolutionNotes; t.UpdatedAt=DateTime.UtcNow; await db.SaveChangesAsync(); return Results.Ok(t);});
app.MapPost("/api/tickets/{id:int}/comments",async(int id,CommentInput x,HvacDbContext db)=>{if(!await db.ServiceTickets.AnyAsync(t=>t.Id==id))return Results.NotFound(); var c=new TicketComment {TicketId=id,AuthorUserId=x.AuthorUserId,CommentText=x.CommentText,IsInternalOnly=x.IsInternalOnly}; db.TicketComments.Add(c); await db.SaveChangesAsync(); return Results.Created($"/api/tickets/{id}/comments/{c.Id}",c);});
app.MapGet("/api/technicians",async(HvacDbContext db)=>Results.Ok(await db.Users.Where(x=>x.Role==UserRole.Technician).Include(x=>x.Vendor).Select(x=>new {x.Id,x.EmployeeCode,x.FirstName,x.LastName,x.Email,x.PhoneNumber,x.Specialisation,x.AvailabilityStatus,x.VendorId,vendorName=x.Vendor==null?null:x.Vendor.BrandName,x.IsActive}).ToListAsync()));
app.MapGet("/api/imports",async(HvacDbContext db)=>Results.Ok(await db.ImportJobs.Include(x=>x.Vendor).OrderByDescending(x=>x.CreatedAt).Select(x=>new {x.Id,x.JobCode,x.EntityType,x.Status,x.TotalRows,x.ValidRows,x.WarningRows,x.ErrorRows,x.DuplicateRows,x.CreatedAt,vendorName=x.Vendor!.BrandName}).ToListAsync()));
app.MapGet("/api/duplicates",async(HvacDbContext db)=>Results.Ok(await db.DuplicateCandidates.Include(x=>x.ExistingCanonicalCustomer).Where(x=>x.Decision==DuplicateDecision.PendingReview).Select(x=>new {x.Id,x.DuplicateCode,x.CandidateName,x.AccountNumber,x.Email,x.Phone,x.ConfidenceScore,x.ConfidenceLevel,x.MatchReason,existingCustomer=x.ExistingCanonicalCustomer!.CustomerName,x.Decision}).ToListAsync()));
app.MapPost("/api/duplicates/{id:int}/decision",async(int id,DuplicateDecisionInput x,HvacDbContext db)=>{var d=await db.DuplicateCandidates.FindAsync(id); if(d is null)return Results.NotFound(); d.Decision=x.Decision;d.ReviewNotes=x.Notes;d.ReviewedByUserId=x.UserId;d.ReviewedAt=DateTime.UtcNow;await db.SaveChangesAsync();return Results.Ok(d);});
app.MapGet("/api/audit",async(HvacDbContext db)=>Results.Ok(await db.SiteLocationHistories.OrderByDescending(x=>x.EffectiveDate).Take(200).Select(x=>new {x.Id,x.SiteLocationId,x.ChangeType,x.ReasonNotes,x.EffectiveDate,x.ChangedByUserId}).ToListAsync()));
app.MapGet("/api/dashboard/summary",async(HvacDbContext db)=>Results.Ok(new {vendors=await db.Vendors.CountAsync(),customers=await db.CanonicalCustomers.CountAsync(),locations=await db.SiteLocations.CountAsync(),assets=await db.AssetEquipment.CountAsync(),openTickets=await db.ServiceTickets.CountAsync(x=>x.Status==TicketStatus.Open||x.Status==TicketStatus.Assigned||x.Status==TicketStatus.InProgress),slaBreaches=await db.ServiceTickets.CountAsync(x=>x.SLAState==SLAState.Breached),pendingDuplicates=await db.DuplicateCandidates.CountAsync(x=>x.Decision==DuplicateDecision.PendingReview)}));
app.Run();

static object UserDto(User u)=>new {u.Id,email=u.Email,name=$"{u.FirstName} {u.LastName}",u.FirstName,u.LastName,role=u.Role.ToString(),u.VendorId,vendorName=u.Vendor?.BrandName,u.CustomerId,customerName=u.Customer?.CustomerName,u.EmployeeCode,u.Specialisation};
record LoginRequest(string Email,string Password);
record SignupRequest(string Name,string Email,string Password);
record VendorInput(string VendorCode,string BrandName,string Region,string? LegalEntityName,string? PrimaryContactName,string? PrimaryContactEmail,string? PrimaryContactPhone);
record CustomerInput(string CanonicalCode,string CustomerName,CustomerType CustomerType,string? PrimaryContactName,string? PrimaryContactEmail,string? PrimaryContactPhone,string? BillingAddress);
record LocationInput(string SiteCode,int CanonicalCustomerId,int? PrimaryVendorId,string SiteName,string AddressLine,string City,string State,string PostalCode,string? SiteContactName,string? SiteContactPhone,string? AccessInstructions);
record AssetInput(string AssetTag,int SiteLocationId,AssetCategory Category,string Manufacturer,string? ModelNumber,string? SerialNumber,string? RefrigerantType,DateOnly? InstallDate,DateOnly? WarrantyEndDate);
record TicketInput(string TicketNumber,int CanonicalCustomerId,int SiteLocationId,int AssetEquipmentId,int? VendorId,string Title,string? Description,TicketPriority Priority,int? RaisedByUserId);
record TicketUpdate(TicketStatus? Status,TicketPriority? Priority,int? AssignedTechnicianId,string? ResolutionNotes);
record CommentInput(int AuthorUserId,string CommentText,bool IsInternalOnly);
record DuplicateDecisionInput(DuplicateDecision Decision,string? Notes,int? UserId);
