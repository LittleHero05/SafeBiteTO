using System.ComponentModel.DataAnnotations;

namespace SafeBite.Client.Models
{
    // These classes mirror the API's DTOs (SafeBite.Api/Dtos).
    // The client never sees the API's entity classes, only the JSON the DTOs produce.
    // The validation attributes are copied too, so jQuery validation catches mistakes
    // in the browser before a request is ever sent.

    // ---------- Establishments ----------
    public class EstablishmentDto
    {
        public int EstablishmentId { get; set; }
        public string DineSafeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class EstablishmentCreateDto
    {
        [Required, MaxLength(50), Display(Name = "DineSafe ID")] public string DineSafeId { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Street { get; set; } = string.Empty;
        [MaxLength(7), Display(Name = "Postal code")] public string? PostalCode { get; set; }
        [MaxLength(15)] public string? Phone { get; set; }
        [Range(-90, 90)] public double Latitude { get; set; } = 43.7853;
        [Range(-180, 180)] public double Longitude { get; set; } = -79.2270;
    }

    public class EstablishmentUpdateDto
    {
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Street { get; set; } = string.Empty;
        [MaxLength(7), Display(Name = "Postal code")] public string? PostalCode { get; set; }
        [MaxLength(15)] public string? Phone { get; set; }
        [Range(-90, 90)] public double Latitude { get; set; }
        [Range(-180, 180)] public double Longitude { get; set; }
    }

    public class BadgeDto
    {
        public int EstablishmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateOnly? LastInspected { get; set; }
        public string? LatestInfraction { get; set; }
        public int CrucialCount { get; set; }
        public int SignificantCount { get; set; }
        public int MinorCount { get; set; }
        public List<string> Last12Months { get; set; } = new();
        public string Source { get; set; } = string.Empty;
    }

    // ---------- Inspections ----------
    public class InspectionDto
    {
        public int InspectionId { get; set; }
        public int EstablishmentId { get; set; }
        public string EstablishmentName { get; set; } = string.Empty;
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
        [Required, Display(Name = "Establishment ID")] public int EstablishmentId { get; set; }
        [Required, Display(Name = "Inspection date")] public DateOnly InspectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required, RegularExpression("^(Pass|Conditional Pass|Closed)$", ErrorMessage = "Status must be Pass, Conditional Pass or Closed.")]
        public string Status { get; set; } = "Pass";
        [Range(0, 100), Display(Name = "Infractions")] public int InfractionCount { get; set; }
        [Range(0, 100), Display(Name = "Crucial")] public int CrucialCount { get; set; }
        [Range(0, 100), Display(Name = "Significant")] public int SignificantCount { get; set; }
        [Range(0, 100), Display(Name = "Minor")] public int MinorCount { get; set; }
        [Required, MaxLength(500), Display(Name = "Infraction summary")] public string InfractionSummary { get; set; } = "No infractions found";
        [MaxLength(500)] public string? Observation { get; set; }
    }

    public class InspectionUpdateDto
    {
        [Required, Display(Name = "Inspection date")] public DateOnly InspectionDate { get; set; }
        [Required, RegularExpression("^(Pass|Conditional Pass|Closed)$", ErrorMessage = "Status must be Pass, Conditional Pass or Closed.")]
        public string Status { get; set; } = string.Empty;
        [Range(0, 100), Display(Name = "Infractions")] public int InfractionCount { get; set; }
        [Range(0, 100), Display(Name = "Crucial")] public int CrucialCount { get; set; }
        [Range(0, 100), Display(Name = "Significant")] public int SignificantCount { get; set; }
        [Range(0, 100), Display(Name = "Minor")] public int MinorCount { get; set; }
        [Required, MaxLength(500), Display(Name = "Infraction summary")] public string InfractionSummary { get; set; } = string.Empty;
        [MaxLength(500)] public string? Observation { get; set; }
    }

    // ---------- Owner responses ----------
    public class OwnerResponseDto
    {
        public int OwnerResponseId { get; set; }
        public int InspectionId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OwnerResponseCreateDto
    {
        [Required, Display(Name = "Inspection ID")] public int InspectionId { get; set; }
        [Required, MaxLength(200), Display(Name = "Owner name")] public string OwnerName { get; set; } = string.Empty;
        [Required, MaxLength(1000)] public string Message { get; set; } = string.Empty;
    }

    public class OwnerResponseUpdateDto
    {
        [Required, MaxLength(200), Display(Name = "Owner name")] public string OwnerName { get; set; } = string.Empty;
        [Required, MaxLength(1000)] public string Message { get; set; } = string.Empty;
    }

    // ---------- PATCH form ----------
    // One small form used by all three PATCH pages: pick a field, type a new value.
    // The controller turns it into a JSON Patch document: [{ "op": "replace", "path": "/phone", "value": "..." }]
    public class PatchFieldForm
    {
        public int Id { get; set; }
        [Required] public string Field { get; set; } = string.Empty;
        [Display(Name = "New value")] public string? Value { get; set; }
    }
}
