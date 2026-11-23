using HvacConnect.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HvacConnect.Infrastructure.Data;

public class HvacDbContext : DbContext
{
    public const string SchemaName = "Staging";

    public HvacDbContext(DbContextOptions<HvacDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<CanonicalCustomer> CanonicalCustomers => Set<CanonicalCustomer>();
    public DbSet<BrandCustomerAccount> BrandCustomerAccounts => Set<BrandCustomerAccount>();
    public DbSet<SiteLocation> SiteLocations => Set<SiteLocation>();
    public DbSet<SiteLocationHistory> SiteLocationHistories => Set<SiteLocationHistory>();
    public DbSet<AssetEquipment> AssetEquipment => Set<AssetEquipment>();
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<ImportRow> ImportRows => Set<ImportRow>();
    public DbSet<DuplicateCandidate> DuplicateCandidates => Set<DuplicateCandidate>();
    public DbSet<VendorOnboardingChecklist> VendorOnboardingChecklists => Set<VendorOnboardingChecklist>();
    public DbSet<VendorMappingTemplate> VendorMappingTemplates => Set<VendorMappingTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Change the application schema here; migrations capture this value when generated.
        modelBuilder.HasDefaultSchema(SchemaName);

        // Enable PostgreSQL extensions
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.HasPostgresExtension("pg_trgm");

        // Business Sequences
        modelBuilder.HasSequence<int>("seq_customer_code").StartsAt(100).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_site_code").StartsAt(100).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_asset_tag").StartsAt(100).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_ticket_number").StartsAt(2400).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_import_job").StartsAt(200).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_duplicate_code").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("seq_vendor_code").StartsAt(1).IncrementsBy(1);

        // Configure Users
        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users");
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).HasColumnType("citext").IsRequired();
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);
            b.Property(u => u.AvailabilityStatus).HasConversion<string>().HasMaxLength(20);

            b.HasOne(u => u.Vendor)
                .WithMany(v => v.Users)
                .HasForeignKey(u => u.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(u => u.Customer)
                .WithMany(c => c.EndUsers)
                .HasForeignKey(u => u.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Vendors
        modelBuilder.Entity<Vendor>(b =>
        {
            b.ToTable("vendors");
            b.HasKey(v => v.Id);
            b.Property(v => v.VendorCode).HasMaxLength(20).IsRequired();
            b.HasIndex(v => v.VendorCode).IsUnique();
            b.Property(v => v.BrandName).HasMaxLength(150).IsRequired();
            b.Property(v => v.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(v => v.OnboardingPhase).HasConversion<string>().HasMaxLength(30);
        });

        // Configure CanonicalCustomers
        modelBuilder.Entity<CanonicalCustomer>(b =>
        {
            b.ToTable("canonical_customers");
            b.HasKey(c => c.Id);
            b.Property(c => c.CanonicalCode).HasMaxLength(30).IsRequired();
            b.HasIndex(c => c.CanonicalCode).IsUnique();
            b.Property(c => c.CustomerName).HasMaxLength(200).IsRequired();
            b.Property(c => c.NormalizedName).HasMaxLength(200).IsRequired();
            b.HasIndex(c => c.NormalizedName).HasMethod("gin").HasOperators("gin_trgm_ops");
            b.Property(c => c.CustomerType).HasConversion<string>().HasMaxLength(50);
            b.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        });

        // Configure BrandCustomerAccounts
        modelBuilder.Entity<BrandCustomerAccount>(b =>
        {
            b.ToTable("brand_customer_accounts");
            b.HasKey(bca => bca.Id);
            b.HasIndex(bca => new { bca.VendorId, bca.LocalAccountNumber }).IsUnique();

            b.HasOne(bca => bca.CanonicalCustomer)
                .WithMany(c => c.BrandAccounts)
                .HasForeignKey(bca => bca.CanonicalCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(bca => bca.Vendor)
                .WithMany(v => v.BrandCustomerAccounts)
                .HasForeignKey(bca => bca.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SiteLocations
        modelBuilder.Entity<SiteLocation>(b =>
        {
            b.ToTable("site_locations");
            b.HasKey(s => s.Id);
            b.Property(s => s.SiteCode).HasMaxLength(30).IsRequired();
            b.HasIndex(s => s.SiteCode).IsUnique();

            b.HasOne(s => s.Customer)
                .WithMany(c => c.Sites)
                .HasForeignKey(s => s.CanonicalCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(s => s.PrimaryVendor)
                .WithMany()
                .HasForeignKey(s => s.PrimaryVendorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(s => s.ParentSite)
                .WithMany()
                .HasForeignKey(s => s.ParentSiteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SiteLocationHistories (immutable audit)
        modelBuilder.Entity<SiteLocationHistory>(b =>
        {
            b.ToTable("site_location_histories");
            b.HasKey(h => h.Id);

            b.HasOne(h => h.SiteLocation)
                .WithMany(s => s.History)
                .HasForeignKey(h => h.SiteLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure AssetEquipment
        modelBuilder.Entity<AssetEquipment>(b =>
        {
            b.ToTable("asset_equipment");
            b.HasKey(a => a.Id);
            b.Property(a => a.AssetTag).HasMaxLength(30).IsRequired();
            b.HasIndex(a => a.AssetTag).IsUnique();
            b.Property(a => a.Category).HasConversion<string>().HasMaxLength(50);
            b.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);

            b.HasIndex(a => new { a.SourceVendorId, a.LegacyAssetId })
                .IsUnique()
                .HasFilter("legacy_asset_id IS NOT NULL");

            b.HasOne(a => a.SiteLocation)
                .WithMany(s => s.Assets)
                .HasForeignKey(a => a.SiteLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ServiceTickets
        modelBuilder.Entity<ServiceTicket>(b =>
        {
            b.ToTable("service_tickets");
            b.HasKey(t => t.Id);
            b.Property(t => t.TicketNumber).HasMaxLength(30).IsRequired();
            b.HasIndex(t => t.TicketNumber).IsUnique();
            b.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
            b.Property(t => t.Status).HasConversion<string>().HasMaxLength(30);
            b.Property(t => t.SLAState).HasConversion<string>().HasMaxLength(20);

            // Optimistic concurrency using PostgreSQL xmin
            b.Property(t => t.ConcurrencyToken).IsRowVersion();

            b.HasIndex(t => new { t.VendorId, t.Status });
            b.HasIndex(t => new { t.OwnerCoordinatorId, t.Status })
                .HasDatabaseName("ix_service_tickets_owner_coordinator_status");
            b.HasIndex(t => new { t.OwnerCoordinatorId, t.Status })
                .HasDatabaseName("ix_service_tickets_unclaimed_open_queue")
                .HasFilter("owner_coordinator_id IS NULL AND status = 'Open'");
            b.HasIndex(t => t.TargetResolutionAt);
            b.HasIndex(t => t.SLAState);

            b.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CanonicalCustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.SiteLocation)
                .WithMany()
                .HasForeignKey(t => t.SiteLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Asset)
                .WithMany(a => a.Tickets)
                .HasForeignKey(t => t.AssetEquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.Vendor)
                .WithMany()
                .HasForeignKey(t => t.VendorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.RaisedByUser)
                .WithMany()
                .HasForeignKey(t => t.RaisedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.OwnerCoordinator)
                .WithMany()
                .HasForeignKey(t => t.OwnerCoordinatorId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(t => t.AssignedTechnician)
                .WithMany()
                .HasForeignKey(t => t.AssignedTechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.AssignedByUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure TicketComments
        modelBuilder.Entity<TicketComment>(b =>
        {
            b.ToTable("ticket_comments");
            b.HasKey(tc => tc.Id);
            b.Property(tc => tc.CommentType).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(tc => new { tc.TicketId, tc.CreatedAt });

            b.HasOne(tc => tc.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TicketId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(tc => tc.AuthorUser)
                .WithMany()
                .HasForeignKey(tc => tc.AuthorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ImportJobs
        modelBuilder.Entity<ImportJob>(b =>
        {
            b.ToTable("import_jobs");
            b.HasKey(j => j.Id);
            b.Property(j => j.JobCode).HasMaxLength(30).IsRequired();
            b.HasIndex(j => j.JobCode).IsUnique();
            b.Property(j => j.Status).HasConversion<string>().HasMaxLength(30);

            b.HasOne(j => j.Vendor)
                .WithMany()
                .HasForeignKey(j => j.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ImportRows
        modelBuilder.Entity<ImportRow>(b =>
        {
            b.ToTable("import_rows");
            b.HasKey(r => r.Id);
            b.HasIndex(r => new { r.ImportJobId, r.RowNumber }).IsUnique();

            b.HasOne(r => r.ImportJob)
                .WithMany(j => j.Rows)
                .HasForeignKey(r => r.ImportJobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure DuplicateCandidates
        modelBuilder.Entity<DuplicateCandidate>(b =>
        {
            b.ToTable("duplicate_candidates");
            b.HasKey(d => d.Id);
            b.Property(d => d.DuplicateCode).HasMaxLength(20).IsRequired();
            b.HasIndex(d => d.DuplicateCode).IsUnique();
            b.Property(d => d.ConfidenceLevel).HasConversion<string>().HasMaxLength(20);
            b.Property(d => d.Decision).HasConversion<string>().HasMaxLength(30);

            // Partial index for pending reviews
            b.HasIndex(d => new { d.Decision, d.ConfidenceScore })
                .HasFilter("decision = 'PendingReview'");

            b.HasOne(d => d.ImportJob)
                .WithMany(j => j.DuplicateCandidates)
                .HasForeignKey(d => d.ImportJobId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(d => d.ImportRow)
                .WithMany()
                .HasForeignKey(d => d.ImportRowId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(d => d.ExistingCanonicalCustomer)
                .WithMany()
                .HasForeignKey(d => d.ExistingCanonicalCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure VendorOnboardingChecklists
        modelBuilder.Entity<VendorOnboardingChecklist>(b =>
        {
            b.ToTable("vendor_onboarding_checklists");
            b.HasKey(c => c.Id);
            b.Property(c => c.TaskCode).HasMaxLength(50);

            b.HasOne(c => c.Vendor)
                .WithMany(v => v.Checklists)
                .HasForeignKey(c => c.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure VendorMappingTemplates
        modelBuilder.Entity<VendorMappingTemplate>(b =>
        {
            b.ToTable("vendor_mapping_templates");
            b.HasKey(m => m.Id);
            b.HasIndex(m => new { m.VendorId, m.EntityType, m.TemplateName }).IsUnique();

            b.HasOne(m => m.Vendor)
                .WithMany()
                .HasForeignKey(m => m.VendorId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
