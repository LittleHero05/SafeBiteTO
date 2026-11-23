using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Models
{
    public class OwnerResponse
    {
        public int OwnerResponseId { get; set; }

        public int InspectionId { get; set; }             // foreign key
        public Inspection Inspection { get; set; } = null!;

        [Required, MaxLength(200)]
        public string OwnerName { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}