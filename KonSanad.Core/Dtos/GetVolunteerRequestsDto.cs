using KonSanad.Core.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos;

public class GetVolunteerRequestsDto
{
    public int ReferenceId { get; set; }
    public int VolunteerId { get; set; }
    public string VolunteerName { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }

    // "Campaign" or "Mission"
    public RefType RequestType { get; set; }

    // Campaign name OR Mission title
    public string Title { get; set; }

    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}