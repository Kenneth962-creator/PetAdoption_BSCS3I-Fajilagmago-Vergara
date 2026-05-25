using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetAdoptionManagementSystem.Models.Domain
{
    public class OtpCode
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [Required]
        public string CodeHash { get; set; } = string.Empty;

        public string? Purpose { get; set; } // Registration / TwoFactor

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public int Attempts { get; set; } = 0;

        public DateTime? LastSentAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}