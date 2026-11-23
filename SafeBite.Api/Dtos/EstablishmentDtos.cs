using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Dtos
{
    public class EstablishmentReadDto
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
        [Required, MaxLength(50)] public string DineSafeId { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Street { get; set; } = string.Empty;
        [MaxLength(7)] public string? PostalCode { get; set; }
        [MaxLength(15)] public string? Phone { get; set; }
        [Range(-90, 90)] public double Latitude { get; set; }
        [Range(-180, 180)] public double Longitude { get; set; }
    }

    public class EstablishmentUpdateDto
    {
        [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Street { get; set; } = string.Empty;
        [MaxLength(7)] public string? PostalCode { get; set; }
        [MaxLength(15)] public string? Phone { get; set; }
        [Range(-90, 90)] public double Latitude { get; set; }
        [Range(-180, 180)] public double Longitude { get; set; }
    }

    // The SafeBite badge: a flattened summary of one establishment + its inspections
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
        public string Source { get; set; } = "Toronto Public Health DineSafe";
    }
}