using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Helpers
{
    internal class ConvertIFormFile
    {

        public async Task<string> ConvertIFormFileToBase64Async(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms); // انسخ محتوى الملف للـ memory stream
            byte[] fileBytes = ms.ToArray(); // حولها لـ byte array

            string base64String = Convert.ToBase64String(fileBytes); // حولها لـ Base64 string
            return base64String;
        }
    }
}
