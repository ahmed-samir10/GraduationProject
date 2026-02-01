using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Repository.Entities;

namespace KonSanad.Core.Entity
{
    public class MissionVolunteer : BaseEntity<int>
    {

        public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; } = null!;

        public int MissionId { get; set; }
        public Mission Mission { get; set; } = null!;
        public MissionStatus MissionStatus { get; set; }
    }
}
