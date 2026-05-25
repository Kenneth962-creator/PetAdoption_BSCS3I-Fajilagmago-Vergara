using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Models.Domain;

namespace PetAdoptionManagementSystem.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Pet> Pets { get; set; }
    public DbSet<AdoptionApplication> AdoptionApplications { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<RecoveryCode> RecoveryCodes { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<SiteSetting>().HasIndex(s => s.Key).IsUnique();
        
        // Prevent cascade deletion to avoid accidental deletion of pets/users when an application is deleted 
        builder.Entity<AdoptionApplication>()
            .HasOne(a => a.Pet)
            .WithMany()
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AdoptionApplication>()
            .HasOne(a => a.Applicant)
            .WithMany()
            .HasForeignKey(a => a.ApplicantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
