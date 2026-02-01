using KonSanad.Core.Entity;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using KonSanad.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services;

public class VolunteerServices : IVolunteerService
{
    private readonly IUnitOfWork _unitOfWork;

    public VolunteerServices(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task StartMissionAsync(int volunteerId, int missionId)
    {
     /*   var existing = await _unitOfWork
         .Repository<VolunteerMission, int>()
         .FindAllAsync(x => x.MissionId == missionId);

        if (existing != null)
            throw new Exception("Mission already started");*/

        var MissionVolunteer = new MissionVolunteer
        {
            VolunteerId = volunteerId,
            MissionId = missionId,
            CreatedAt = DateTime.UtcNow,
            MissionStatus = MissionStatus.InProgress
        };

        await _unitOfWork.Repository<MissionVolunteer, int>()
            .AddAsync(MissionVolunteer);

        await _unitOfWork.Complete();
    }


}