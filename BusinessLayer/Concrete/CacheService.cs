using BusinessLayer.Abstract;
using StackExchange.Redis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BusinessLayer.Concrete
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _cache;

        public CacheService(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
            _cache = redisConnection.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cache.StringGetAsync(key);
            if (value.HasValue)
            {
                return JsonSerializer.Deserialize<T>(value!);
            }
            return default;
        }

        public async Task SetAsync<T>(string key, T value, int expirationMinutes = 60)
        {
            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.StringSetAsync(key, serializedValue, System.TimeSpan.FromMinutes(expirationMinutes));
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.KeyDeleteAsync(key);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var endpoints = _redisConnection.GetEndPoints();
            var server = _redisConnection.GetServer(endpoints.First());

            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Any())
            {
                await _cache.KeyDeleteAsync(keys);
            }
        }
    }
}