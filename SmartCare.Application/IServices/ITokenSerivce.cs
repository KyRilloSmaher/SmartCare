
using SmartCare.Domain.Entities;
using System.Security.Claims;


namespace SmartCare.Domain.Interfaces.IServices
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT access token containing the provided claims.
        /// </summary>
        /// <param name="claims">The user claims to embed in the JWT.</param>
        /// <returns>A serialized JWT access token string.</returns>
        string GenerateAccessToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Generates a secure random refresh token string.
        /// </summary>
        /// <returns>A base64-encoded refresh token.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Calculates the expiration time for a refresh token.
        /// </summary>
        /// <returns>A DateTime in UTC representing the refresh token expiry time.</returns>
        DateTime GetRefreshTokenExpiryTime();

        /// <summary>
        /// Extracts user claims (NameIdentifier, Email, Name) from the specified user entity.
        /// </summary>
        /// <param name="user">The application user to extract claims from.</param>
        /// <returns>A list of claims representing the user identity.</returns>
        IEnumerable<Claim> GetClaims(Client user);

        /// <summary>
        /// Extracts a ClaimsPrincipal from an expired access token.
        /// Used for validating refresh token requests.
        /// </summary>
        /// <param name="token">The expired JWT token.</param>
        /// <returns>A ClaimsPrincipal representing the token’s claims, or null if invalid.</returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}

