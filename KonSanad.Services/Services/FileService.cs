
namespace KonSanad.Services.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("file");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            return $"/{folder}/{fileName}";
        }
    }
}
