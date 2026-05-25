using System.ComponentModel.DataAnnotations;

namespace PetAdoptionManagementSystem.Models.Domain
{
    public class SiteSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        public string? Value { get; set; }
    }
}