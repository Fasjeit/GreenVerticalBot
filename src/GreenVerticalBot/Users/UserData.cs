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
                return (List<Invite>)invites;
            }
            set
            {
                this[nameof(this.Invites)] = value;
            }
        }
    }
}