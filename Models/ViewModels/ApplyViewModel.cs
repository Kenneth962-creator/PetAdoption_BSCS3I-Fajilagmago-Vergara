using System.ComponentModel.DataAnnotations;

namespace PetAdoptionManagementSystem.Models.ViewModels
{
    public class ApplyViewModel
    {
        public int PetId { get; set; }
        public string? PetName { get; set; }

        [Required, Display(Name = "Why do you want to adopt this pet?")]
        public string Reason { get; set; } = string.Empty;

        [Required, Display(Name = "Living Situation (e.g., Own, Rent, House, Apartment)")]
        public string LivingSituation { get; set; } = string.Empty;

        [Required, Display(Name = "Employment Status")]
        public string EmploymentStatus { get; set; } = string.Empty;

        [Display(Name = "Do you have other pets?")]
        public bool HasOtherPets { get; set; }

        [Display(Name = "Do you have children under 12?")]
        public bool HasChildren { get; set; }

        [Required]
        [PetAdoptionManagementSystem.Models.Validation.MustBeTrue]
        [Display(Name = "I agree to provide a safe, loving home for this pet and allow follow-up visits.")]
        public bool AgreedToTerms { get; set; }
    }
}