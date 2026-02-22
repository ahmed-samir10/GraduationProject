using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Entity
{
    public enum VolunteerMissionStatus
    {
        Applied = 0,     // قدم طلب
        Approved = 1,    // المانجر وافق
        Rejected = 2,    // اترفض
        Submitted = 3    // رفع التقرير
    }
}
