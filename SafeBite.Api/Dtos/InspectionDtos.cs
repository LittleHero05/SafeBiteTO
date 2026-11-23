using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Dtos
{
    public class InspectionReadDto
    {
        public int InspectionId { get; set; }
        public int EstablishmentId { get; set; }
        public string EstablishmentName { get; set; } = string.Empty;   // flattened from Establishment.Name
        public DateOnly InspectionDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int InfractionCount { get; set; }
        public int CrucialCount { get; set; }
        public int SignificantCount { get; set; }
        public int MinorCount { get; set; }
        public string InfractionSummary { get; set; } = string.Empty;
        public string? Observation { get; set; }
    }

    public class InspectionCreateDto
    {
        [Required] public int EstablishmentId { get; set; }
        [Required] public DateOnly InspectionDate { get; set; }
        [Required, RegularExpression("^(Pass|Conditional Pass|Closed)$",
            ErrorMessage = "Status must be Pass, Conditional Pass or Closed.")]
        public string Status { get; set; } = string.Empty;
        [Range(0, 100)] public int InfractionCount { get; set; }
        [Range(0, 100)] public int CrucialCount { get; set; }
        [Range(0, 100)] public int SignificantCount { get; set; }
        [Range(0, 100)] public int MinorCount { get; set; }
        [Required, MaxLength(500)] public string InfractionSummary { get; set; } = string.Empty;
        [MaxLength(500)] public string? Observation { get; set; }
    }

    public class InspectionUpdateDto
    {
        [Required] public DateOnly InspectionDate { get; set; }
        [Required, RegularExpression("^(Pass|Conditional Pass|Closed)$",
            ErrorMessage = "Status must be Pass, Conditional Pass or Closed.")]
        public string Status { get; set; } = string.Empty;
        [Range(0, 100)] public int InfractionCount { get; set; }
        [Range(0, 100)] public int CrucialCount { get; set; }
        [Range(0, 100)] public int SignificantCount { get; set; }
        [Range(0, 100)] public int MinorCount { get; set; }
        [Required, MaxLength(500)] public string InfractionSummary { get; set; } = string.Empty;
        [MaxLength(500)] public string? Observation { get; set; }
    }
}