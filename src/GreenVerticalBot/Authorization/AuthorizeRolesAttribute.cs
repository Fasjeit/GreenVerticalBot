namespace GreenVerticalBot.Authorization
{
    using Microsoft.AspNetCore.Authorization;

    internal class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public UserRole[] RoleArray { get; private set; }

        public AuthorizeRolesAttribute(params UserRole[] roles)
        {
            this.Roles = string.Join(",", roles);
            this.RoleArray = roles;
        }
    }
}