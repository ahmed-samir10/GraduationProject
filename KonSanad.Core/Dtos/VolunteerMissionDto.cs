using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class VolunteerMissionDto
    {
        public int Id { get; set; }
        public int MissionId { get; set; }
        public string Name { get; set; }
        public VolunteerMissionStatus MissionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
       
        public string  Description { get; set; }
        public string? ResultMission { get; set; }
        public string? Notes { get; set; }


        // ✅ New — list of supporting file URLs for the beneficiary's mission submission
        public List<string> BeneficiaryFiles { get; set; } = new List<string>();
    }
}
