using System.Security.Claims;

namespace GreenVerticalBot.Authorization
{
    internal class BotClaim : Claim
    {
        public BotClaim(string type, string value, string valueType, string issuer, string originalIssuer) :
            base(type, value, valueType, issuer, originalIssuer)
        { }
    }

    internal static class ClaimsExtensions
    {
        public static bool HasRole(this List<BotClaim> claims, UserRole role)
        {
            return claims.Any(
                c => c.Type == ClaimTypes.Role &&
                c.Value == role.ToString());
        }
    }
}