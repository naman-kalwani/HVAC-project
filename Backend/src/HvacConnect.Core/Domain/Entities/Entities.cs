using HvacConnect.Core.Domain.Enums;

namespace HvacConnect.Core.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int? CustomerId { get; set; }
    public CanonicalCustomer? Customer { get; set; }

    public string? EmployeeCode { get; set; }
    public string? Specialisation { get; set; }
    public TechnicianAvailability AvailabilityStatus { get; set; } = TechnicianAvailability.Available;

    public bool IsActive { get; set; } = true;
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEndAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class Vendor
{
    public int Id { get; set; }
    public string VendorCode { get; set; } = string.Empty; // AAA-NNN
    public string BrandName { get; set; } = string.Empty;
    public string? LegalEntityName { get; set; }
    public string Region { get; set; } = string.Empty;

    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhone { get; set; }

    public VendorStatus Status { get; set; } = VendorStatus.PendingOnboarding;
    public OnboardingPhase OnboardingPhase { get; set; } = OnboardingPhase.Discovery;

    public DateTime? Phase1ApprovedAt { get; set; }
    public int? Phase1ApprovedByUserId { get; set; }

    public DateOnly? CutoverDate { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int? CompletedByUserId { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<BrandCustomerAccount> BrandCustomerAccounts { get; set; } = new List<BrandCustomerAccount>();
    public ICollection<VendorOnboardingChecklist> Checklists { get; set; } = new List<VendorOnboardingChecklist>();
}

public class CanonicalCustomer
{
    public int Id { get; set; }
    public string CanonicalCode { get; set; } = string.Empty; // CUST-00128
    public string CustomerName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Commercial;

    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactEmail { get; set; }
    public string? PrimaryContactPhone { get; set; }
    public string? BillingAddress { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;
    public bool IsSelfRegistered { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<BrandCustomerAccount> BrandAccounts { get; set; } = new List<BrandCustomerAccount>();
    public ICollection<SiteLocation> Sites { get; set; } = new List<SiteLocation>();
    public ICollection<User> EndUsers { get; set; } = new List<User>();
}

public class BrandCustomerAccount
{
    public int Id { get; set; }
    public int CanonicalCustomerId { get; set; }
    public CanonicalCustomer? CanonicalCustomer { get; set; }

    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string LocalAccountNumber { get; set; } = string.Empty;
    public string LocalCustomerName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? BillingAddress { get; set; }

    public string AccountStatus { get; set; } = "Active";
    public string? SourceSystem { get; set; }
    public string? SourceSystemKey { get; set; }
    public string? RowHash { get; set; }

    public int? FirstImportJobId { get; set; }
    public int? LastImportJobId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class SiteLocation
{
    public int Id { get; set; }
    public string SiteCode { get; set; } = string.Empty; // LOC-00812
    public int CanonicalCustomerId { get; set; }
    public CanonicalCustomer? Customer { get; set; }

    public int? PrimaryVendorId { get; set; }
    public Vendor? PrimaryVendor { get; set; }

    public int? ParentSiteId { get; set; }
    public SiteLocation? ParentSite { get; set; }

    public string SiteName { get; set; } = string.Empty;
    public string SiteType { get; set; } = "Commercial";

    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public string TimeZone { get; set; } = "America/New_York";
    public string? OperatingHours { get; set; }
    public string? AccessInstructions { get; set; }

    public string? SiteContactName { get; set; }
    public string? SiteContactPhone { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetEquipment> Assets { get; set; } = new List<AssetEquipment>();
    public ICollection<SiteLocationHistory> History { get; set; } = new List<SiteLocationHistory>();
}

public class SiteLocationHistory
{
    public long Id { get; set; }
    public int SiteLocationId { get; set; }
    public SiteLocation? SiteLocation { get; set; }

    public int? ChangedByUserId { get; set; }
    public string ChangeType { get; set; } = string.Empty; // AddressChange, OwnershipTransfer, VendorReassignment, Deactivated
    public string? PreviousValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? ReasonNotes { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}

public class AssetEquipment
{
    public int Id { get; set; }
    public string AssetTag { get; set; } = string.Empty; // RTU-00182, CH-00410, etc.
    public int SiteLocationId { get; set; }
    public SiteLocation? SiteLocation { get; set; }

    public AssetCategory Category { get; set; }
    public string Manufacturer { get; set; } = string.Empty;
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? RefrigerantType { get; set; }

    public DateOnly? InstallDate { get; set; }
    public DateOnly? WarrantyEndDate { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;

    public int? SourceVendorId { get; set; }
    public string? LegacyAssetId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ServiceTicket> Tickets { get; set; } = new List<ServiceTicket>();
}

public class ServiceTicket
{
    public int Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty; // TKT-2408
    public int CanonicalCustomerId { get; set; }
    public CanonicalCustomer? Customer { get; set; }

    public int SiteLocationId { get; set; }
    public SiteLocation? SiteLocation { get; set; }

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public int AssetEquipmentId { get; set; }
    public AssetEquipment? Asset { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public int? RaisedByUserId { get; set; }
    public User? RaisedByUser { get; set; }

    public int? AssignedTechnicianId { get; set; }
    public User? AssignedTechnician { get; set; }

    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? TargetResponseAt { get; set; }
    public DateTime? TargetResolutionAt { get; set; }

    public SLAState SLAState { get; set; } = SLAState.WithinSLA;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionNotes { get; set; }

    public uint ConcurrencyToken { get; set; } // mapped to xmin

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
}

public class TicketComment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public ServiceTicket? Ticket { get; set; }

    public int AuthorUserId { get; set; }
    public User? AuthorUser { get; set; }

    public CommentType CommentType { get; set; } = CommentType.UserComment;
    public string CommentText { get; set; } = string.Empty;
    public bool IsInternalOnly { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ImportJob
{
    public int Id { get; set; }
    public string JobCode { get; set; } = string.Empty; // IMP-204
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string EntityType { get; set; } = "Customers"; // Customers, Locations, Assets
    public string? BatchDescription { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public int UploadedByUserId { get; set; }

    public short CurrentStage { get; set; } = 1; // 1 to 5
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int WarningRows { get; set; }
    public int ErrorRows { get; set; }
    public int DuplicateRows { get; set; }
    public int UpdatedRows { get; set; }
    public int SkippedRows { get; set; }

    public ImportJobStatus Status { get; set; } = ImportJobStatus.Uploaded;
    public string? MappingConfigJson { get; set; }
    public string? FailureReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<ImportRow> Rows { get; set; } = new List<ImportRow>();
    public ICollection<DuplicateCandidate> DuplicateCandidates { get; set; } = new List<DuplicateCandidate>();
}

public class ImportRow
{
    public long Id { get; set; }
    public int ImportJobId { get; set; }
    public ImportJob? ImportJob { get; set; }

    public int RowNumber { get; set; }
    public string RawDataJson { get; set; } = "{}";
    public string? MappedDataJson { get; set; }
    public string? NormalizedName { get; set; }
    public string? RowHash { get; set; }

    public string ValidationStatus { get; set; } = "Valid"; // Valid, Warning, Error
    public string? ValidationIssuesJson { get; set; }

    public string? MatchOutcome { get; set; } // New, PossibleDuplicate, ExistingChanged, ExistingUnchanged
    public int? ResolvedCanonicalCustomerId { get; set; }
    public bool IsProcessed { get; set; }
}

public class DuplicateCandidate
{
    public int Id { get; set; }
    public string DuplicateCode { get; set; } = string.Empty; // DUP-001
    public int ImportJobId { get; set; }
    public ImportJob? ImportJob { get; set; }

    public long ImportRowId { get; set; }
    public ImportRow? ImportRow { get; set; }

    public string EntityType { get; set; } = "Customer";
    public int CandidateVendorId { get; set; }

    public string CandidateName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AccountNumber { get; set; }
    public string? Address { get; set; }

    public int ExistingCanonicalCustomerId { get; set; }
    public CanonicalCustomer? ExistingCanonicalCustomer { get; set; }

    public short ConfidenceScore { get; set; } // 90 or 70
    public DuplicateConfidenceLevel ConfidenceLevel { get; set; } = DuplicateConfidenceLevel.Medium;
    public string MatchReason { get; set; } = string.Empty;

    public DuplicateDecision Decision { get; set; } = DuplicateDecision.PendingReview;
    public string? ReviewNotes { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class VendorOnboardingChecklist
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string? TaskCode { get; set; }
    public string Phase { get; set; } = "Phase 1";
    public string TaskDescription { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCompleted { get; set; }

    public int? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class VendorMappingTemplate
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public string EntityType { get; set; } = "Customers";
    public string TemplateName { get; set; } = string.Empty;
    public string MappingConfigJson { get; set; } = "{}";

    public int CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

