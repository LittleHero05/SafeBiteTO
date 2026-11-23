using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Models
{
    public class Establishment
    {
        public int EstablishmentId { get; set; }

        [Required, MaxLength(50)]
        public string DineSafeId { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Street { get; set; } = string.Empty;

        [MaxLength(7)]
        public string? PostalCode { get; set; }

        [MaxLength(15)]
        public string? Phone { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Audit fields: stored in the database, hidden from clients by the DTOs
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation: one establishment has many inspections
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}