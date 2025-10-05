using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Models;

public partial class SysDbContext : DbContext
{
    public SysDbContext(DbContextOptions<SysDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CompanyInfo> CompanyInfos { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Policy> Policies { get; set; }

    public virtual DbSet<PriceType> PriceTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Tour> Tours { get; set; }

    public virtual DbSet<TourCategory> TourCategories { get; set; }

    public virtual DbSet<TourImage> TourImages { get; set; }

    public virtual DbSet<TourPolicy> TourPolicies { get; set; }

    public virtual DbSet<TourPrice> TourPrices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0782569B4A");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Active, "IX_Account_Active");

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E403771DB2").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Photo)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC073D33053B");

            entity.ToTable("Booking");

            entity.HasIndex(e => e.Active, "IX_Booking_Active");

            entity.HasIndex(e => e.StartDate, "IX_Booking_StartDate");

            entity.HasIndex(e => e.StatusTypeId, "IX_Booking_StatusTypeId");

            entity.HasIndex(e => e.TourId, "IX_Booking_TourId");

            entity.HasIndex(e => e.UserId, "IX_Booking_UserId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.NameCustomer).HasMaxLength(255);
            entity.Property(e => e.PaymentStatus).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingS__3214EC07C52222F3");

            entity.ToTable("BookingStatus");

            entity.HasIndex(e => e.Active, "IX_BookingStatus_Active");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StatusName).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07F66634D3");

            entity.ToTable("Category");

            entity.HasIndex(e => e.Active, "IX_Category_Active");

            entity.HasIndex(e => e.CategoryName, "IX_Category_CategoryName");

            entity.HasIndex(e => e.ParentId, "IX_Category_ParentId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Enable).HasDefaultValue(true);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CompanyI__3214EC071EA18966");

            entity.ToTable("CompanyInfo");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Icon)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC077BB771A7");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.Active, "IX_Permission_Active");

            entity.HasIndex(e => e.PermissionKey, "UQ__Permissi__8884ABD4001192CF").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PermissionKey).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Policy__3214EC07640B9913");

            entity.ToTable("Policy");

            entity.HasIndex(e => e.Active, "IX_Policy_Active");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PolicyType).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<PriceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PriceTyp__3214EC07DBF31274");

            entity.ToTable("PriceType");

            entity.HasIndex(e => e.Active, "IX_PriceType_Active");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PriceTypeName).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0787F5D6AC");

            entity.HasIndex(e => e.Active, "IX_Roles_Active");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61609274EEF7").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(100);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC07B87B79F5");

            entity.ToTable("RolePermission");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<Tour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tours__3214EC07DB98891E");

            entity.HasIndex(e => e.Active, "IX_Tours_Active");

            entity.HasIndex(e => new { e.PricePerPerson, e.Duration }, "IX_Tours_PricePerPerson_Duration");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.MapLink)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PricePerPerson).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourCate__3214EC07246FE5F3");

            entity.ToTable("TourCategory");

            entity.HasIndex(e => new { e.TourId, e.CategoryId }, "IX_TourCategory_TourId_CategoryId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourImag__3214EC07D8D7D6A2");

            entity.HasIndex(e => e.Active, "IX_TourImages_Active");

            entity.HasIndex(e => e.IsBanner, "IX_TourImages_IsBanner");

            entity.HasIndex(e => e.TourId, "IX_TourImages_TourId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourPolicy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourPoli__3214EC0765B4A214");

            entity.ToTable("TourPolicy");

            entity.HasIndex(e => new { e.TourId, e.PolicyId }, "IX_TourPolicy_TourId_PolicyId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourPric__3214EC07790C9F54");

            entity.ToTable("TourPrice");

            entity.HasIndex(e => new { e.TourId, e.PriceTypeId }, "IX_TourPrice_TourId_PriceTypeId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
