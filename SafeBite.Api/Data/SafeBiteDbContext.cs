using Microsoft.EntityFrameworkCore;
using SafeBite.Api.Models;

namespace SafeBite.Api.Data
{
    public class SafeBiteDbContext : DbContext
    {
        public SafeBiteDbContext(DbContextOptions<SafeBiteDbContext> options) : base(options) { }

        public DbSet<Establishment> Establishments => Set<Establishment>();
        public DbSet<Inspection> Inspections => Set<Inspection>();
        public DbSet<OwnerResponse> OwnerResponses => Set<OwnerResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Each DineSafe establishment appears only once
            modelBuilder.Entity<Establishment>()
                .HasIndex(e => e.DineSafeId)
                .IsUnique();

            // Deleting an establishment deletes its inspections,
            // and deleting an inspection deletes its owner responses
            modelBuilder.Entity<Inspection>()
                .HasOne(i => i.Establishment)
                .WithMany(e => e.Inspections)
                .HasForeignKey(i => i.EstablishmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OwnerResponse>()
                .HasOne(r => r.Inspection)
                .WithMany(i => i.OwnerResponses)
                .HasForeignKey(r => r.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}