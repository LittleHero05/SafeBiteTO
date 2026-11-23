using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Models
{
    public class Inspection
    {
        public int InspectionId { get; set; }

        public int EstablishmentId { get; set; }          // foreign key
        public Establishment Establishment { get; set; } = null!;

        public DateOnly InspectionDate { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = string.Empty;   // Pass, Conditional Pass, Closed

        public int InfractionCount { get; set; }
        public int CrucialCount { get; set; }
        public int SignificantCount { get; set; }
        public int MinorCount { get; set; }

        [Required, MaxLength(500)]
        public string InfractionSummary { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Observation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation: one inspection can have owner responses
        public ICollection<OwnerResponse> OwnerResponses { get; set; } = new List<OwnerResponse>();
    }
}