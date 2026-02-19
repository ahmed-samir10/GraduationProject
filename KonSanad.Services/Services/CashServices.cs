using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class CashServices : ICashServices
    {
        private readonly IDatabase _Database;

        public CashServices(IConnectionMultiplexer redis)
        {
            _Database = redis.GetDatabase();
        }
        public async Task<string> GetCashAsync(string Key)
        {
            var data = await _Database.StringGetAsync(Key);
            if (data.IsNullOrEmpty)
            {
                return null;

            }
            return data.ToString();

        }
        public async Task SetCashAsync(string key, object response, TimeSpan exp)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            if (response == null)
                throw new ArgumentNullException(nameof(response));

            // تحويل الكائن إلى JSON
            var json = JsonSerializer.Serialize(response);

            // تخزين في Redis مع Expiration
            await _Database.StringSetAsync(key, json, exp);
        }

    }
}
