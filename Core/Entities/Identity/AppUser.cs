using Microsoft.AspNetCore.Identity;

namespace Core.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public bool IsAdmin { get; set; }
        public bool IsEmployee { get; set; }
    }
}