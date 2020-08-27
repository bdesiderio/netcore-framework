using Framework.Authentication.ServiceContracts;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.JWT.Services
{
    public class JWTSessionPersistanceService : ISessionPersistanceService
    {
        private readonly IOptions<JwtConfiguration> _jwtConfiguration;

        public JWTSessionPersistanceService(IOptions<JwtConfiguration> jwtConfiguration)
        {
            _jwtConfiguration = jwtConfiguration;
        }

        public async Task<object> PersistSession(IApplicationUser applicationUser, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, applicationUser.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Value.JwtSecret));

            var token = new JwtSecurityToken(
                issuer: _jwtConfiguration.Value.JwtValidUser,
                audience: _jwtConfiguration.Value.JwtValidAudience,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return await Task.FromResult(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
            });
        }
    }
}
