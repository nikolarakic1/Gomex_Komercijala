using GomexPraksa.Auth;
using Microsoft.EntityFrameworkCore;
using Models.AuthenticationDtos;
using System.Security.Claims;

namespace GomexPraksa.JWTInfo
{
    public class UserAccess : IUserAccess
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly AuthDbContext _auth;

        public UserAccess(
            IHttpContextAccessor httpContext,
            AuthDbContext auth)
        {
            _httpContext = httpContext;
            _auth = auth;
        }

        public async Task<AccesDto> GetCurrentUserAccessAsync()
        {
            var currentUser = _httpContext.HttpContext?.User;

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException();
            }

            var userId = currentUser.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException();
            }

            var isSef = currentUser.IsInRole(
                "SefMenadzera"
            );

            if (isSef)
            {
                return new AccesDto
                {
                    UserId = userId,
                    CanViewAllCategories = true,
                    KategorijaIds = []
                };
            }

            var kategorije = await _auth.UserKategorije
                .Where(uk => uk.UserId == userId)
                .Select(uk => uk.KategorijaId)
                .ToListAsync();

            return new AccesDto
            {
                UserId = userId,
                CanViewAllCategories = false,
                KategorijaIds = kategorije
            };
        }
    }
}