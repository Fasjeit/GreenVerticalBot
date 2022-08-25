using System.Security.Claims;

namespace GreenVerticalBot.Authorization
{
    internal class BotClaim : Claim
    {
        public BotClaim(string type, string value, string valueType, string issuer, string originalIssuer) :
            base(type, value, valueType, issuer, originalIssuer)
        { }
    }
}