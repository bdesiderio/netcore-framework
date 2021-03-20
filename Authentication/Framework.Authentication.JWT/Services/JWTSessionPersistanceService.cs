using Framework.Authentication.ServiceContracts;
using Framework.SharedKernel.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.JWT.Services
{
    public abstract class JWTSessionPersistanceService<TApplicationUserKey> : ISessionPersistanceService<TApplicationUserKey>
    {
        private readonly IOptions<JwtConfiguration> _jwtConfiguration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JWTSessionPersistanceService(IOptions<JwtConfiguration> jwtConfiguration,
            IHttpContextAccessor httpContextAccessor)
        {
            _jwtConfiguration = jwtConfiguration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IUserSession<TApplicationUserKey>> GetUserSession()
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

            var sessionUser = new SessionUser<TApplicationUserKey>();

            sessionUser.Id = GetIdFromClaim(claimsPrincipal.Claims.FirstOrDefault(x => String.Equals(x.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase)).Value);
            sessionUser.UserName = claimsPrincipal.Claims.FirstOrDefault(x => String.Equals(x.Type, ClaimTypes.Name, StringComparison.OrdinalIgnoreCase)).Value;
            sessionUser.Roles = claimsPrincipal.Claims.Where(x => String.Equals(x.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)).Select(x => x.Value).ToList();

            await MapClaimToSessionUser(claimsPrincipal.Claims, sessionUser);

            return await Task.FromResult(sessionUser);
        }

        public abstract TApplicationUserKey GetIdFromClaim(string claimId);

        public abstract Task SaveRefreshToken(string username, string token, string refreshToken);
        public abstract Task<string> GetRefreshToken(string username, string token);

        public virtual async Task MapClaimToSessionUser(IEnumerable<Claim> claims, SessionUser<TApplicationUserKey> sessionUser)
            => await Task.CompletedTask;

        public async Task<IPersistSessionClientResponse> PersistSession(IApplicationUser<TApplicationUserKey> applicationUser, IEnumerable<string> roles, bool persist)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, applicationUser.UserName),
                    //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            await SetToken(applicationUser.UserName, authClaims, persist);

            return null;
        }

        private async Task SetToken(string username, IEnumerable<Claim> claims, bool persist)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Value.JwtSecret));

            var token = new JwtSecurityToken(
                issuer: _jwtConfiguration.Value.JwtValidUser,
                audience: _jwtConfiguration.Value.JwtValidAudience,
                expires: DateTime.Now.AddSeconds(_jwtConfiguration.Value.TokenExpirationSeconds),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            var refreshToken = GenerateRefreshToken();

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
            
            await SaveRefreshToken(username, tokenValue, refreshToken);


            var expire = persist ? (DateTime?)DateTime.Now.AddYears(1) : null;

            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Token-Expiration", token.ValidTo.ToString(), new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = expire });
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Access-Token", tokenValue, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = expire });
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Refresh-Token", refreshToken, new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = expire });
            _httpContextAccessor.HttpContext.Response.Cookies.Append("X-Token-Persist", persist.ToString(), new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict, Expires = expire });
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Refresca el Token en base a lo que venga en el Header de Authorizacion y en el Header custom X-RefreshToken
        /// </summary>
        /// <returns></returns>
        public async Task RefreshSession()
        {
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Access-Token", out string tokenValue);
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Refresh-Token", out string refreshToken);
            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("X-Token-Persist", out string persistValue);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(tokenValue);
            var username = token.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            if (await GetRefreshToken(username, tokenValue) != refreshToken)
            {
                return;
            }

            bool.TryParse(persistValue, out bool persist);

            await SetToken(username, token.Claims
                .Where(x => x.Type == ClaimTypes.NameIdentifier ||
                x.Type == ClaimTypes.Name ||
                x.Type == ClaimTypes.Role), persist);
        }

        public async Task LeaveSession()
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Token-Expiration", new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Access-Token", new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Refresh-Token", new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("X-Refresh-Persist", new CookieOptions() { HttpOnly = true, SameSite = SameSiteMode.Strict });
        }
    }
}
