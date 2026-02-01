using KonSanad.Repository.Entities;
using KonSanad.Services.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{ 
    [Table("VolunteerMissions")]
    public class VolunteerMission: BaseEntity<int>
    {
        
        public int Ahmed { get; set; }
        public int VolunteerId { get; set; }
            public Volunteer Volunteer { get; set; } = null!;

            public int MissionId { get; set; }
            public Mission Mission { get; set; } = null!;
            public MissionStatus MissionStatus { get; set; }


            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public string? ResultMission { get; set; }
        
    }
}
