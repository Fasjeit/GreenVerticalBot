using Newtonsoft.Json.Linq;

namespace GreenVerticalBot.Users
{
    internal class UserData : Dictionary<string, object>
    {
        public List<Invite> Invites
        {
            get
            {
                if (!this.TryGetValue(nameof(this.Invites), out var invites) ||
                    invites == null)
                {
                    return new List<Invite>();
                }
                if (invites is JArray invitesArray)
                {
                    invites = invitesArray.ToObject<List<Invite>>();
                }
                return (List<Invite>)invites;
            }
            set
            {
                this[nameof(this.Invites)] = value;
            }
        }
    }
}