using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos.ManagerDtos
{
    public class GetVolunteerDto
    {
            public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
         public string Availability { get; set; } = string.Empty;

        public RequestedStatus Status { get; set; } // اتغيرت من int لـ Enum عشان الريسبونس يبعت "Approved" بدل 3

    }
    public class GetJoinVolunteerDto
    {
        public int Id { get; set; }
        public RequestedStatus Status { get; set; }
        public string Email { get; set; } = string.Empty;
       public DateTime JoinDate { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Availability { get; set; } = string.Empty;

    }
}
