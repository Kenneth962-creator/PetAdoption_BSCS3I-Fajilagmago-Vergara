using System;
using System.ComponentModel.DataAnnotations;

namespace PetAdoptionManagementSystem.Models.Domain
{
    public enum PetStatus
    {
        Available,
        UnderReview,
        Adopted
    }

    public class Pet
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Species { get; set; }

        [MaxLength(100)]
        public string? Breed { get; set; }

        public decimal AgeYears { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? HealthStatus { get; set; }

        public bool IsVaccinated { get; set; }

        public string? PhotoPath { get; set; }

        public decimal AdoptionFee { get; set; }

        public PetStatus Status { get; set; } = PetStatus.Available;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}