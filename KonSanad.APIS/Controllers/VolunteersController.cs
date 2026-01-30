
namespace KonSanad.APIS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VolunteersController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}