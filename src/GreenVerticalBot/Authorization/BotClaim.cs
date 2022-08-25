using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GreenVerticalBot.Authorization
{
    internal class BotClaim : Claim
    {
        public BotClaim(string type, string value, string valueType, string issuer, string originalIssuer) :
            base(type, value, valueType, issuer, originalIssuer)
        { }
    }
}
