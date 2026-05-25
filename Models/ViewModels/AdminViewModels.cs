using Microsoft.AspNetCore.Http;
using PetAdoptionManagementSystem.Models.Domain;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetAdoptionManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalPets { get; set; }
        public int AvailablePets { get; set; }
        public int AdoptedPets { get; set; }
        public int PendingApplications { get; set; }
        public int TotalMembers { get; set; }

        public Dictionary<string, int> PetsBySpecies { get; set; } = new();
        public Dictionary<string, int> ApplicationsByStatus { get; set; } = new();

        public List<AdoptionApplication> RecentApplications { get; set; } = new();
    }

    public class PetFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Species { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Breed { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal AgeYears { get; set; }

        [Required]
        public string Gender { get; set; } = "Unknown";

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? HealthStatus { get; set; }

        public bool IsVaccinated { get; set; }

        public IFormFile? Photo { get; set; }

        public string? ExistingPhotoPath { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal AdoptionFee { get; set; }

        public PetStatus Status { get; set; } = PetStatus.Available;
    }

    public class ReviewApplicationViewModel
    {
        public int ApplicationId { get; set; }
        
        [MaxLength(500)]
        public string? AdminRemarks { get; set; }

        public ApplicationStatus NewStatus { get; set; }
    }
}