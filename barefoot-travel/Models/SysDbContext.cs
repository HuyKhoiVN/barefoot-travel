using barefoot_travel.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;

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

    public virtual DbSet<HomePageFeaturedTour> HomePageFeaturedTours { get; set; }

    public virtual DbSet<HomePageSection> HomePageSections { get; set; }

    public virtual DbSet<HomePageSectionCategory> HomePageSectionCategories { get; set; }

    public virtual DbSet<HomePageSectionTour> HomePageSectionTours { get; set; }

    public virtual DbSet<HomePageSelectedTour> HomePageSelectedTours { get; set; }

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

    public virtual DbSet<TourStatusHistory> TourStatusHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(CustomQuery).GetMethod(nameof(CustomQuery.ToCustomString))).HasTranslation(
                e =>
                {
                    return new SqlFunctionExpression(functionName: "format", arguments: new[]{
                                 e.First(),
                                 new SqlFragmentExpression("'dd/MM/yyyy HH:mm:ss'")
                            }, nullable: true, new List<bool>(), type: typeof(string), typeMapping: new StringTypeMapping("", DbType.String));
                });

        modelBuilder.HasDbFunction(typeof(CustomQuery).GetMethod(nameof(CustomQuery.ToDateString))).HasTranslation(
            e =>
            {
                return new SqlFunctionExpression(functionName: "format", arguments: new[]{
                                 e.First(),
                                 new SqlFragmentExpression("'dd/MM/yyyy'")
                        }, nullable: true, new List<bool>(), type: typeof(string), typeMapping: new StringTypeMapping("", DbType.String));
            });
        modelBuilder.UseCollation("Latin1_General_CI_AS");
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC0780E3BA5A");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E46452D927").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC07C0F9F35D");

            entity.ToTable("Booking");

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
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BookingS__3214EC077F0934BD");

            entity.ToTable("BookingStatus");

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
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC0748447EA8");

            entity.ToTable("Category");

            entity.HasIndex(e => new { e.ShowInDailyTours, e.DailyTourOrder }, "IX_Category_DailyTours");

            entity.HasIndex(e => new { e.HomepageTitle, e.Active, e.HomepageOrder }, "IX_Category_HomepageDisplay");

            entity.HasIndex(e => e.Slug, "IX_Category_Slug")
                .IsUnique()
                .HasFilter("([Slug] IS NOT NULL)");

            entity.HasIndex(e => new { e.ShowInWaysToTravel, e.WaysToTravelOrder }, "IX_Category_WaysToTravel");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CategoryName).HasMaxLength(255);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DailyTourBadge).HasMaxLength(100);
            entity.Property(e => e.DailyTourCardClass).HasMaxLength(100);
            entity.Property(e => e.DailyTourDescription).HasMaxLength(500);
            entity.Property(e => e.DailyTourImageUrl).HasMaxLength(500);
            entity.Property(e => e.Enable).HasDefaultValue(true);
            entity.Property(e => e.HomepageTitle).HasMaxLength(200);
            entity.Property(e => e.ShowInDailyTours).HasDefaultValue(false);
            entity.Property(e => e.ShowInWaysToTravel).HasDefaultValue(false);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            entity.Property(e => e.WaysToTravelImage1).HasMaxLength(500);
            entity.Property(e => e.WaysToTravelImage2).HasMaxLength(500);
        });

        modelBuilder.Entity<CompanyInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CompanyI__3214EC07144A07C7");

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

        modelBuilder.Entity<HomePageFeaturedTour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomePage__3214EC07F9A7D438");

            entity.HasIndex(e => new { e.DisplayOrder, e.Active }, "IX_FeaturedTour_DisplayOrder");

            entity.HasIndex(e => e.TourId, "IX_FeaturedTour_TourId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CardClass).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.Tour).WithMany(p => p.HomePageFeaturedTours)
                .HasForeignKey(d => d.TourId)
                .HasConstraintName("FK_FeaturedTour_Tour");
        });

        modelBuilder.Entity<HomePageSection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomePage__3214EC07CCA10FE2");

            entity.ToTable("HomePageSection");

            entity.HasIndex(e => e.DisplayOrder, "IX_HomePageSection_DisplayOrder");

            entity.HasIndex(e => e.IsActive, "IX_HomePageSection_IsActive");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.BadgeText).HasMaxLength(100);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomClass).HasMaxLength(255);
            entity.Property(e => e.HomepageTitle).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LayoutStyle)
                .HasMaxLength(50)
                .HasDefaultValue("grid");
            entity.Property(e => e.MaxItems).HasDefaultValue(8);
            entity.Property(e => e.SectionName).HasMaxLength(255);
            entity.Property(e => e.SelectionMode)
                .HasMaxLength(20)
                .HasDefaultValue("auto");
            entity.Property(e => e.SpotlightImageUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.PrimaryCategory).WithMany(p => p.HomePageSections)
                .HasForeignKey(d => d.PrimaryCategoryId)
                .HasConstraintName("FK_HomePageSection_PrimaryCategory");
        });

        modelBuilder.Entity<HomePageSectionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomePage__3214EC07D5039116");

            entity.ToTable("HomePageSectionCategory");

            entity.HasIndex(e => e.CategoryId, "IX_HomePageSectionCategory_CategoryId");

            entity.HasIndex(e => e.SectionId, "IX_HomePageSectionCategory_SectionId");

            entity.HasIndex(e => new { e.SectionId, e.CategoryId }, "UK_HomePageSectionCategory").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.HomePageSectionCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomePageSectionCategory_Category");

            entity.HasOne(d => d.Section).WithMany(p => p.HomePageSectionCategories)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_HomePageSectionCategory_Section");
        });

        modelBuilder.Entity<HomePageSectionTour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomePage__3214EC073DF9A60F");

            entity.ToTable("HomePageSectionTour");

            entity.HasIndex(e => e.SectionId, "IX_HomePageSectionTour_SectionId");

            entity.HasIndex(e => e.TourId, "IX_HomePageSectionTour_TourId");

            entity.HasIndex(e => new { e.SectionId, e.TourId }, "UK_HomePageSectionTour").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(255);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.Section).WithMany(p => p.HomePageSectionTours)
                .HasForeignKey(d => d.SectionId)
                .HasConstraintName("FK_HomePageSectionTour_Section");

            entity.HasOne(d => d.Tour).WithMany(p => p.HomePageSectionTours)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomePageSectionTour_Tour");
        });

        modelBuilder.Entity<HomePageSelectedTour>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HomePage__3214EC079020713A");

            entity.ToTable("HomePageSelectedTour");

            entity.HasIndex(e => new { e.CategoryId, e.DisplayOrder }, "IX_HomePageSelectedTour_CategoryOrder");

            entity.HasIndex(e => e.TourId, "IX_HomePageSelectedTour_TourId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.HomePageSelectedTours)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomePageSelectedTour_Category");

            entity.HasOne(d => d.Tour).WithMany(p => p.HomePageSelectedTours)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HomePageSelectedTour_Tour");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC0733DD13B7");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.PermissionKey, "UQ__Permissi__8884ABD4246C797F").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Policy__3214EC074808FE2F");

            entity.ToTable("Policy");

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
            entity.HasKey(e => e.Id).HasName("PK__PriceTyp__3214EC079D1B1B1C");

            entity.ToTable("PriceType");

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
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC073FE87E0C");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B61609FEFF9F9").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC070BC434D3");

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
            entity.HasKey(e => e.Id).HasName("PK__Tours__3214EC075316BC9A");

            entity.HasIndex(e => e.Slug, "IX_Tour_Slug")
                .IsUnique()
                .HasFilter("([Slug] IS NOT NULL)");

            entity.HasIndex(e => e.Status, "IX_Tour_Status").HasFilter("([Active]=(1))");

            entity.HasIndex(e => new { e.Status, e.Active }, "IX_Tour_Status_Active");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.MapLink)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.PricePerPerson).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("draft");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourCate__3214EC0718B989E3");

            entity.ToTable("TourCategory");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourImag__3214EC072F983203");

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
            entity.HasKey(e => e.Id).HasName("PK__TourPoli__3214EC07BBF7F7E3");

            entity.ToTable("TourPolicy");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourPric__3214EC07E2F0A77E");

            entity.ToTable("TourPrice");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<TourStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourStat__3214EC075485286C");

            entity.ToTable("TourStatusHistory");

            entity.HasIndex(e => e.ChangedTime, "IX_TourStatusHistory_ChangedTime").IsDescending();

            entity.HasIndex(e => e.TourId, "IX_TourStatusHistory_TourId");

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.ChangedTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatus).HasMaxLength(50);
            entity.Property(e => e.OldStatus).HasMaxLength(50);
            entity.Property(e => e.Reason).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
