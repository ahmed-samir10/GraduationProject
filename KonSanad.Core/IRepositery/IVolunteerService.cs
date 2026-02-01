using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery;

public interface IVolunteerService
{

    Task StartMissionAsync(int volunteerId, int missionId);


}