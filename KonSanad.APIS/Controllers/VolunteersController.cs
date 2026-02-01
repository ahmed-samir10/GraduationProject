
namespace KonSanad.APIS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VolunteersController : Controller
{
    private readonly IVolunteerService _volunteerService;
    private readonly IUnitOfWork _unitOfWork;

    public VolunteersController(IVolunteerService volunteerService , IUnitOfWork unitOfWork)
    {
        _volunteerService = volunteerService;
        _unitOfWork = unitOfWork;
    }


    [Authorize]
    [HttpPut("{id}/start")]
    public async Task<IActionResult> StartTask(int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


        var Vol = await _unitOfWork
 .Repository<Volunteer, int>()
 .FirstOrDefaultAsync(b => b.AppUserId == userId);

        if (Vol == null)
            return BadRequest(new { Error = "No Donor found for this user" });

        int VolID = Vol.Id;
        await _volunteerService.StartMissionAsync(VolID , id);
        return Ok(new { message = "تم بدء المهمة بنجاح" });
    }


}