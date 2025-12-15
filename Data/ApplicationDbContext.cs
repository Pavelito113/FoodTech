using FoodTech.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodTech.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSet’ы бизнес-сущностей
      public DbSet<ContactRequest> ContactRequests { get; set; } = default!;
        public DbSet<EndProduct> EndProducts { get; set; } = null!;
        public DbSet<Equipment> Equipments { get; set; } = null!;
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; } = null!;
        public DbSet<EquipmentEndProduct> EquipmentEndProducts { get; set; } = null!;
        public DbSet<EquipmentImage> EquipmentImages { get; set; } = null!;
        public DbSet<EquipmentSpec> EquipmentSpecs { get; set; } = null!;
        public DbSet<Industry> Industries { get; set; } = null!;
        public DbSet<Manufacturer> Manufacturers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==== Identity → фиксация таблиц ====
            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");

            // ==== Бизнес-таблицы (singular) ====
            modelBuilder.Entity<ContactRequest>().ToTable("ContactRequest");
            modelBuilder.Entity<EndProduct>().ToTable("EndProduct");
            modelBuilder.Entity<Equipment>().ToTable("Equipment");
            modelBuilder.Entity<EquipmentCategory>().ToTable("EquipmentCategory");
            modelBuilder.Entity<EquipmentEndProduct>().ToTable("EquipmentEndProduct");
            modelBuilder.Entity<EquipmentImage>().ToTable("EquipmentImage");
            modelBuilder.Entity<EquipmentSpec>().ToTable("EquipmentSpec");
            modelBuilder.Entity<Industry>().ToTable("Industry");
            modelBuilder.Entity<Manufacturer>().ToTable("Manufacturer");

            // ==== Композитный ключ (many-to-many) ====
            modelBuilder.Entity<EquipmentEndProduct>()
                .HasKey(ep => new { ep.EquipmentId, ep.EndProductId });

            // ==== EquipmentCategory (self-reference) ====
            modelBuilder.Entity<EquipmentCategory>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==== ContactRequest → Equipment (nullable) ====
            modelBuilder.Entity<ContactRequest>()
                .HasOne(cr => cr.Equipment)
                .WithMany(e => e.ContactRequests)
                .HasForeignKey(cr => cr.EquipmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // ==== Equipment связи ====
            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Industry)
                .WithMany(i => i.Equipments)
                .HasForeignKey(e => e.IndustryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Equipments)
                .HasForeignKey(e => e.EquipmentCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Equipment>()
                .HasOne(e => e.Manufacturer)
                .WithMany(m => m.Equipments)
                .HasForeignKey(e => e.ManufacturerId)
                .OnDelete(DeleteBehavior.SetNull);

            // ==== EquipmentImage → Equipment ====
            modelBuilder.Entity<EquipmentImage>()
                .HasOne(i => i.Equipment)
                .WithMany(e => e.Images)
                .HasForeignKey(i => i.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ==== EquipmentSpec → Equipment ====
            modelBuilder.Entity<EquipmentSpec>()
                .HasOne(s => s.Equipment)
                .WithMany(e => e.Specs)
                .HasForeignKey(s => s.EquipmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
