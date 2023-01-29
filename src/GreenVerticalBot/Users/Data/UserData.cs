using GreenVerticalBot.EntityFramework.Entities.Tasks;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace GreenVerticalBot.Users.Data
{
    internal class UserData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual TaskType Type { get; protected set; } = TaskType.NoType;

        public List<Invite> Invites { get; set; } = new List<Invite>();       
    }
}