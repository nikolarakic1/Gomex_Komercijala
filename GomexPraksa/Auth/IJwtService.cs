using GomexPraksa.ApplicationUserSecurity;

namespace GomexPraksa.Auth
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
