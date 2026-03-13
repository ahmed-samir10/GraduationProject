using System;
using System.Text.Json.Serialization;
using KonSanad.Core.Converters;
using KonSanad.Core.Entity.Enums;

namespace KonSanad.Core.Dtos
{
    public class MissionDto
    {
        public int Id { get; set; }

        // Card fields (Title should come from Beneficiary.FullName when available)
        public string Title { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Mission fields
        public string? Description { get; set; }
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateTime? StartDate { get; set; }

        // Status comes from mission.Status (BaseEntity.Status) if present
        public MissionStatus MissionStatus { get; set; }

        // UI helper
        [JsonIgnore]
        public string StartDateFormatted =>
            StartDate.HasValue ? StartDate.Value.ToString("yyyy-MM-dd") : string.Empty;
    }
}
