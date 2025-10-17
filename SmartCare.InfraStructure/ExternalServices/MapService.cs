using SmartCare.Application.ExternalServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.InfraStructure.ExternalServices
{
    public class MapService : IMapService
    {
        private readonly HttpClient _httpClient;

        public MapService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SmartCareApp/1.0 (kirolos7maher@gmail.com)");
        }

        public async Task<(double lat, double lng)?> GeocodeAsync(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            var json = await _httpClient.GetStringAsync(url);

            var results = System.Text.Json.JsonSerializer.Deserialize<NominatimResult[]>(json);
            if (results != null && results.Length > 0)
            {
                return (double.Parse(results[0].lat), double.Parse(results[0].lon));
            }
            return null;
        }

        private class NominatimResult
        {
            public string lat { get; set; }
            public string lon { get; set; }
        }
        public float CalculateDistanceKm(float lat1, float lon1, float lat2, float lon2)
        {
            const float R = 6371; // Earth radius in KM
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            float c = (float)(2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
            return R * c;
        }
        private static double ToRadians(double angle) => Math.PI * angle / 180;
    }
}
