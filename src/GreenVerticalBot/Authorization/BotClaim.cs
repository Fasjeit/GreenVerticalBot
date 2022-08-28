using System.Security.Claims;

namespace GreenVerticalBot.Authorization
{
    internal class BotClaim : Claim, IEquatable<BotClaim>
    {
        public BotClaim(string type, string value, string valueType, string issuer, string originalIssuer) :
            base(type, value, valueType, issuer, originalIssuer)
        { }

        public bool Equals(BotClaim? other)
        {
            return this.Value == other?.Value;
        }
    }

    internal static class ClaimsExtensions
    {
        public static bool HasRole(this List<BotClaim> claims, UserRole role)
        {
            return claims.Any(
                c => c.Type == ClaimTypes.Role &&
                c.Value == role.ToString());
        }

        public static bool HasAllRolles(this List<BotClaim> claims, List<UserRole> requredRoles)
        {
            return requredRoles.All(
                rr => 
                    claims.Any(
                        c => 
                            c.Type == ClaimTypes.Role &&
                            c.Value == rr.ToString()));
        }
    }
}