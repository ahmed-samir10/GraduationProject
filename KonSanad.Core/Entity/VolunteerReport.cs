using KonSanad.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
    public class VolunteerReport : BaseEntity<int>
    {
       public int VolunteerId { get; set; }
        public Volunteer Volunteer { get; set; }
        public int BenficieryID { get; set; }
        public Beneficiary Beneficiary { get; set; }

        public string SupportFile { get; set; }

        
    }
}
