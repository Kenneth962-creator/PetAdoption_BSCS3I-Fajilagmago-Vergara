using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetAdoptionManagementSystem.Models.Domain
{
    public enum ApplicationStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class AdoptionApplication
    {
        public int Id { get; set; }

        public int PetId { get; set; }
        [ForeignKey(nameof(PetId))]
        public Pet? Pet { get; set; }

        [Required]
        public string ApplicantId { get; set; } = string.Empty;
        [ForeignKey(nameof(ApplicantId))]
        public ApplicationUser? Applicant { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        public string? LivingSituation { get; set; }

        public bool HasOtherPets { get; set; }

        public bool HasChildren { get; set; }

        public string? EmploymentStatus { get; set; }

        public bool AgreedToTerms { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        [MaxLength(500)]
        public string? AdminRemarks { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
    }
}