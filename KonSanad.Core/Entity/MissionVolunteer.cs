using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
    [Table("VolunteerMissions")]
    public class MissionVolunteers : BaseEntity<int>
    {

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;
        public MissionStatus MissionStatus { get; set; }
    }
}
