
namespace KonSanad.Services.Services;

public class VolunteerServices : IVolunteerService
{
    // Dependencies injected via constructor injection pattern 
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileService _fileService;

    // Constructor To initialize dependencies 
    public VolunteerServices(IUnitOfWork unitOfWork, IFileService fileService)
    {
        _unitOfWork = unitOfWork;
        _fileService = fileService;
    }


    // ----------------------------------------------------------
    // Method to submit a mission report by a volunteer
    // ----------------------------------------------------------
    public async Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null)
    {
        // Validate volunteer exists
        var volunteer = await _unitOfWork.Repository<Volunteer, int>().FirstOrDefaultAsync(v => v.Id == volunteerId);

        if (volunteer is null)  throw new ArgumentException("Volunteer not found");



        // Validate volunteer-mission relation exists Or not
        var vmRepo = _unitOfWork.Repository<VolunteerMission, int>();
        var volunteerMission = await vmRepo.FirstOrDefaultAsync(vm => vm.VolunteerId == volunteerId && vm.MissionId == missionId);

        if (volunteerMission is null)
            throw new ArgumentException("VolunteerMission not found for given volunteer and mission");



        // Optionally validate mission exists and dates (if needed)
        var mission = await _unitOfWork.Repository<Mission, int>().FirstOrDefaultAsync(m => m.Id == missionId);
        if (mission is null)
            throw new ArgumentException("Mission not found");



        // If mission has start/end dates, you can prevent reporting outside mission window
        if (mission.StartDate.HasValue && mission.EndDate.HasValue)
        {
            var now = DateTime.UtcNow;
            if (now < mission.StartDate.Value || now > mission.EndDate.Value)
            {
                throw new ArgumentException("Cannot submit report outside mission active period");
            }
        }


        // Save file
        var relativeUrl = await _fileService.SaveFileAsync(file, "uploads");


        // Store the result (for now store a single URL).
        // If multiple files are needed consider storing JSON or separate table.
        volunteerMission.ResultMission = relativeUrl;
        if (!string.IsNullOrEmpty(notes))
        {
            volunteerMission.Description = (volunteerMission.Description ?? string.Empty) + "\n" + notes;
        }

        vmRepo.Update(volunteerMission);
        await _unitOfWork.Complete();

        return relativeUrl;
    }
    // ----------------------------------------------------------
    // ----------------------------------------------------------
}
