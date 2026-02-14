using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Core.Dtos
{
    public class UpdateMissionReportDto
    {
        public int VolunteerId { get; set; }
        public int MissionId { get; set; }
        public IFormFile? File { get; set; }
        public string? Notes { get; set; }
    }

}
