using Framework.Authentication.ServiceContracts;
using Framework.SharedKernel.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.JWT.Services
{
    public class JWTSessionPersistanceService : ISessionPersistanceService
    {
        private readonly IOptions<JwtConfiguration> _jwtConfiguration;
        private readonly IIdTypeFactory _idTypeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JWTSessionPersistanceService(IOptions<JwtConfiguration> jwtConfiguration,
            IIdTypeFactory idTypeFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _jwtConfiguration = jwtConfiguration;
            _idTypeFactory = idTypeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IUserSession> GetSessionPersistance()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext.User;

            if (claimsPrincipal == null)
            {
                throw new AuthenticationException("Cannot invoke GetSessionPersistance because Current ClaimsPrincipal is not defined");
            }

            if (claimsPrincipal.Identity == null || !claimsPrincipal.Identity.IsAuthenticated)
            {
                throw new AuthenticationException("Cannot invoke GetSessionPersistance because CurrentIdentity is not authenticated");
            }

            if (claimsPrincipal.Claims == null || claimsPrincipal.Claims.Count() == 0)
            {
                throw new AuthenticationException("Cannot invoke GetSessionPersistance because Claims is empty or undefined");
            }

            return await Task.FromResult(new SessionUser
            {
                Id = await _idTypeFactory.GetIdType(claimsPrincipal.Claims.FirstOrDefault(x => String.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)).Value),
                UserName = claimsPrincipal.Claims.FirstOrDefault(x => String.Equals(x.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase)).Value,
                Roles = claimsPrincipal.Claims.Where(x => String.Equals(x.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList(),
            });
        }

        public async Task<IPersistSessionClientResponse> PersistSession(IApplicationUser applicationUser, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
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
                expires: DateTime.Now.AddSeconds(_jwtConfiguration.Value.TokenExpirationSeconds),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return await Task.FromResult(new JwtSessionClientResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
            });
        }
    }
}
