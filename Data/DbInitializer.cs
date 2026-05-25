using Microsoft.AspNetCore.Identity;
using PetAdoptionManagementSystem.Models.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace PetAdoptionManagementSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Seed Roles
            var roles = new[] { "Admin", "Member" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@petadopt.local") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@petadopt.local",
                    Email = "admin@petadopt.local",
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@1234");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Seed Settings
            if (!context.SiteSettings.Any())
            {
                context.SiteSettings.AddRange(
                    new SiteSetting { Key = "SiteName", Value = "Happy Paws Rescue" },
                    new SiteSetting { Key = "ContactEmail", Value = "hello@petadopt.local" },
                    new SiteSetting { Key = "AboutUs", Value = "We are a local rescue dedicated to finding forever homes for our furry friends." },
                    new SiteSetting { Key = "MaxOtpAttempts", Value = "5" },
                    new SiteSetting { Key = "OtpExpiryMinutes", Value = "10" }
                );
            }

            // Seed Pets
            if (!context.Pets.Any())
            {
                context.Pets.AddRange(
                    new Pet { Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", AgeYears = 3m, Gender = "Female", Description = "Bella is a sweet and energetic lab looking for an active family.", HealthStatus = "Healthy", IsVaccinated = true, AdoptionFee = 150.00m, Status = PetStatus.Available },
                    new Pet { Name = "Luna", Species = "Cat", Breed = "Domestic Shorthair", AgeYears = 1.5m, Gender = "Female", Description = "Luna loves to cuddle and nap in the sun.", HealthStatus = "Healthy", IsVaccinated = true, AdoptionFee = 75.00m, Status = PetStatus.UnderReview },
                    new Pet { Name = "Charlie", Species = "Dog", Breed = "Beagle Mix", AgeYears = 0.5m, Gender = "Male", Description = "Charlie is a playful puppy who is currently learning his basic commands.", HealthStatus = "Needs booster shots", IsVaccinated = false, AdoptionFee = 200.00m, Status = PetStatus.Available },
                    new Pet { Name = "Milo", Species = "Cat", Breed = "Maine Coon", AgeYears = 5m, Gender = "Male", Description = "Milo is a gentle giant who gets along well with other pets.", HealthStatus = "Healthy", IsVaccinated = true, AdoptionFee = 50.00m, Status = PetStatus.Adopted },
                    new Pet { Name = "Daisy", Species = "Dog", Breed = "Poodle", AgeYears = 7m, Gender = "Female", Description = "Daisy is a calm, senior dog just looking for a quiet place to retire.", HealthStatus = "Mild arthritis", IsVaccinated = true, AdoptionFee = 100.00m, Status = PetStatus.Available }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}