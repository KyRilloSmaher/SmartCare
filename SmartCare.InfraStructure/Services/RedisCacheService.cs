using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using Newtonsoft.Json.Linq;
using SmartCare.Application.IServices;
using SmartCare.Domain.Constants;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache? _cache;
        private readonly IConnectionMultiplexer _redis;
        private const string TagPrefix = "tag:";


        //private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        //{
        //    PropertyNameCaseInsensitive = true,
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
        //    WriteIndented = false
        //};

        public RedisCacheService(IDistributedCache? cache , IConnectionMultiplexer redis)
        {
            _cache = cache;
            _redis = redis;
        }

        private string BuildFullKey(string key, string tagName) => $"{key}{TagPrefix}{tagName}";

        public async Task<T?> GetDataAsync<T>(string key , string tagName)
        {

            var fullKey = BuildFullKey(key, tagName);

            var cachedData = await _cache.GetStringAsync(fullKey);

            if (cachedData == null || cachedData.Length == 0)
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(cachedData);
            }
            catch
            {
                return default;
            }

        }

        public async Task SetDataAsync<T>(string key, T value, string tagName, TimeSpan? expiration = null)
        {
            if (value == null) return;

            var fullKey = BuildFullKey(key, tagName);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(60)
            };

            var jsonString = JsonSerializer.Serialize<T>(value);

            await _cache.SetStringAsync(fullKey, jsonString, options);
        }

        public async Task RemoveKeyAsync(string key , string tagName)
        {
            var FullKey = BuildFullKey(key, tagName);
            await _cache.RemoveAsync(FullKey);
        }

        public async Task DeleteKeysByTag(string tagName)
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints[0]);
            var pattern = $"*{TagPrefix}{tagName}";

            var keys = server.Keys(pattern: pattern).ToArray();
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key.ToString());
            }
        }
    }
}
