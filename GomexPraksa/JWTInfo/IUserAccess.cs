using Microsoft.Extensions.Configuration.UserSecrets;
using Models.AuthenticationDtos;

namespace GomexPraksa.JWTInfo
{
    public interface IUserAccess
    {
        Task<AccesDto> GetCurrentUserAccessAsync();
    }
}
