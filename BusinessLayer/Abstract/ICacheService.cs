using System.Threading.Tasks;

namespace BusinessLayer.Abstract
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, int expirationMinutes = 60);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }
}