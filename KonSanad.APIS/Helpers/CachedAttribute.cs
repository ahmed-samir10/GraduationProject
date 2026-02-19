using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace KonSanad.APIS.Helpers
{
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveInSeconds;

        public CachedAttribute(int timeToLiveInSeconds)
        {
            _timeToLiveInSeconds = timeToLiveInSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICashServices>();

            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            var cachedResponse = await cacheService.GetCashAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedResponse))
            {
                var contentResult = new ContentResult()
                {
                    Content = cachedResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }

            var executedEndPointContext = await next(); // move to execute endpoint

            if (executedEndPointContext.Result is OkObjectResult okObjectResult)
                await cacheService.SetCashAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveInSeconds));

        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}"); // baseUrl/Products?pageSize=10&pageIndex=1

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
                keyBuilder.Append($"|{key}-{value}");

            return keyBuilder.ToString();
        }
    }
}
