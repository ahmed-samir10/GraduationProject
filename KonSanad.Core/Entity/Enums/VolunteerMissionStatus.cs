using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity.Enums
{
    public enum VolunteerMissionStatus
    {
        New = 1,
        InReview = 4,
        WaitingForReport = 5,
        Completed = 6,
        Rejected = 7

    }
    public enum RefType
    {
        Campaign = 1,
        Mission = 2

    }
}

