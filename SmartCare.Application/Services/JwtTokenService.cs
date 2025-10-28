using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace SmartCare.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<Client> _userManager;

        public TokenService(JwtSettings jwt, UserManager<Client> userManager)
        {
            _jwtSettings = jwt;
            _userManager = userManager;
        }

        // Generate Access Token (JWT)
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.AccessTokenLifetimeHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generate Refresh Token (random string)
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // Calculate refresh token expiry
        public DateTime GetRefreshTokenExpiryTime()
        {
            return DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifetimeDays);
        }

        // Build claims for the JWT
        public async Task<IEnumerable<Claim>> GetClaimsAsync(Client user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var securityStamp = await _userManager.GetSecurityStampAsync(user);
            var authClaims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                                    new Claim("security_stamp", securityStamp ?? "")
                                };

            // Add role claims
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            return authClaims;
        }

        // Extract principal from an expired JWT (used during refresh)
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var tokenParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false // ignore expiration
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
    }
}
