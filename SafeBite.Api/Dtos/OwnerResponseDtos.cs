using System.ComponentModel.DataAnnotations;

namespace SafeBite.Api.Dtos
{
    public class OwnerResponseReadDto
    {
        public int OwnerResponseId { get; set; }
        public int InspectionId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OwnerResponseCreateDto
    {
        [Required] public int InspectionId { get; set; }
        [Required, MaxLength(200)] public string OwnerName { get; set; } = string.Empty;
        [Required, MaxLength(1000)] public string Message { get; set; } = string.Empty;
    }

    public class OwnerResponseUpdateDto
    {
        [Required, MaxLength(200)] public string OwnerName { get; set; } = string.Empty;
        [Required, MaxLength(1000)] public string Message { get; set; } = string.Empty;
    }
}