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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
