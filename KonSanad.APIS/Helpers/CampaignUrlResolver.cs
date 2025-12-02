using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Repository.Entities;

namespace KonSanad.APIS.Helpers
{
    public class CampaignUrlResolver : IValueResolver<Campaign, CampaignDto, string>
    {
        private readonly IConfiguration _configuration;
        public CampaignUrlResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public string Resolve(Campaign source, CampaignDto destination, string destMember, ResolutionContext context)
        {

            if (!string.IsNullOrEmpty(source.PictureUrl))
                return $"{source.PictureUrl}";
            return null;

        }
    }
}
