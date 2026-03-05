using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.IServices
{
    public interface IRedisCacheService
    {
        Task<T?> GetDataAsync<T>(string key, string tagName);

        Task SetDataAsync<T>(string key, T value, string tagName, TimeSpan? expiration = null);

        Task RemoveKeyAsync(string key, string tagName);
        Task DeleteKeysByTag(string tagName);
    }
}
