using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Repository.Entities;
using System.Text.Json;

namespace KonSanad.APIS.Helpers
{
    public class ImageUrlsResolver : IValueResolver<NewProductRequestDto, HelpOrder, string?>
    {
        public string? Resolve(NewProductRequestDto source, HelpOrder destination, string destMember, ResolutionContext context)
        {
            if (source == null) return null;
            if (string.IsNullOrWhiteSpace(source.ImageUrl)) return null;

            // If frontend may send a comma separated list in ImageUrl, normalize to array.
            if (source.ImageUrl.Contains(','))
            {
                var parts = source.ImageUrl
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                return JsonSerializer.Serialize(parts);
            }

            return JsonSerializer.Serialize(new[] { source.ImageUrl });
        }
    }
}
