using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Repository.Entities;
using System.Text.Json;

namespace KonSanad.APIS.Helpers
{
    public class ImageUrlsResolver : IValueResolver<OrderItem, OrderItemDto, string>
    {
        private readonly IConfiguration _configuration;
        public ImageUrlsResolver(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public string Resolve(OrderItem source, OrderItemDto destination, string destMember, ResolutionContext context)
        {

            if (!string.IsNullOrEmpty(source.ImageUrl))
                return $"{source.ImageUrl}";
            return null;

        }
    }
}
