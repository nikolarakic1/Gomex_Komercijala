using GomexPraksa.ApplicationUserSecurity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GomexPraksa.Auth
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config
                ?? throw new ArgumentNullException(nameof(config));
        }

        public string GenerateToken(
            ApplicationUser user,
            IList<string> roles)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(roles);

            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException(
                    "Jwt:Key nije podešen u appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException(
                    "Jwt:Issuer nije podešen u appsettings.json.");

            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException(
                    "Jwt:Audience nije podešen u appsettings.json.");

            var expireMinutes =
                _config.GetValue<int?>("Jwt:ExpireMinutes") ?? 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(
                    ClaimTypes.Name,
                    user.UserName ?? user.Email ?? user.Id
                )
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(
                    new Claim(ClaimTypes.Email, user.Email)
                );
            }

            claims.AddRange(
                roles
                    .Where(role => !string.IsNullOrWhiteSpace(role))
                    .Select(role =>
                        new Claim(ClaimTypes.Role, role!)
                    )
            );

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}