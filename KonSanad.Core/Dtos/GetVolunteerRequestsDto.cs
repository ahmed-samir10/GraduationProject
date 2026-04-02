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
public class VolunteerRequestDetailDto
{
    // ── معلومات الطلب ──
    public RefType RequestType { get; set; }
    public int ReferenceId { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── بيانات المتطوع كاملة ──
    public int VolunteerId { get; set; }
    public string VolunteerName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
   
    public int TotalMissions { get; set; }
    public int TotalCampaigns { get; set; }

    // ── بيانات المهمة أو الكامبين كاملة ──
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }


    // Mission-specific (null لو Campaign)
    public VolunteerMissionStatus? MissionStatus { get; set; }


    // Campaign-specific (null لو Mission)
    public RequestedStatus? CampaignJoinStatus { get; set; }
    public int? CampaignVolunteersCount { get; set; }
}