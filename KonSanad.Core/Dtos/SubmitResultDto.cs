using System.Collections.Generic;

namespace KonSanad.Core.Dtos
{
    public class SubmitResultDto
    {
        public bool Success { get; set; }
        public HelpOrderDto? HelpOrder { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
