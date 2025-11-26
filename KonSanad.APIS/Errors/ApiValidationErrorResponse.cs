
namespace KonSanad.Core.Dtos
{
    public class ApiValidationErrorResponse : ApiResponse
    {
        public IEnumerable<string>? Errors { get; set; }

        public ApiValidationErrorResponse() : base(400, "One or more validation errors occurred")
        {
        }
    }
}
