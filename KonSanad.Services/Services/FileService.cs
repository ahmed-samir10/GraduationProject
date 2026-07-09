
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
                throw new ArgumentException(nameof(file));

            Console.WriteLine($"WebRootPath = {_env.WebRootPath}");

            var uploadsFolder = Path.Combine(_env.WebRootPath, folder);

            Console.WriteLine($"UploadsFolder = {uploadsFolder}");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            Console.WriteLine($"Saving To = {filePath}");

            await using var fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            Console.WriteLine($"Exists = {System.IO.File.Exists(filePath)}");

            return $"{folder}/{fileName}";
        }
        public async Task DeleteFileAsync(string relativePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            await Task.CompletedTask;
        }
    }
}
