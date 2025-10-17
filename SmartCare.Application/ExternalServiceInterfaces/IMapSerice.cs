using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Application.ExternalServiceInterfaces
{
    public interface IMapService
    {
        public float CalculateDistanceKm(float lat1, float lon1, float lat2, float lon2);
        public  Task<(double lat, double lng)?> GeocodeAsync(string address);
        private double ToRadians(double angle) => Math.PI * angle / 180;
    }
}
