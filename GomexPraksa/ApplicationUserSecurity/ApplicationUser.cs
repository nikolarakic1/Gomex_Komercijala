using Microsoft.AspNetCore.Identity;

namespace GomexPraksa.ApplicationUserSecurity
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<UserKategorija> Kategorije { get; set; }
            = new List<UserKategorija>();
    }
}
