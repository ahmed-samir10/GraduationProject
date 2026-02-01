using Microsoft.AspNetCore.Http;

namespace KonSanad.APIS.DTOs;

public class ReportSubmittedDto
{
    public IFormFile? File { get; set; }
    public int MissionId { get; set; }
    // optional: change mission status or additional notes
    public string? Notes { get; set; }
}

