using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace KonSanad.Core.IRepositery;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string folder = "uploads");
    Task DeleteFileAsync(string relativePath);
}