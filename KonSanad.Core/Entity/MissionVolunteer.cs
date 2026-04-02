using KonSanad.Core.Entity.Enums;
using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
    [Table("MissionVolunteers")]
    public class MissionVolunteers : BaseEntity<int>
    {

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;
        public String ResultMission { get; set; } = string.Empty;
        public String Notes { get; set; } = string.Empty;
        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;
        public VolunteerMissionStatus MissionStatus { get; set; }

        public string? RejectReason { get; set; }
    }
}
