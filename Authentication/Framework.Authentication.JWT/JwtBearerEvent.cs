using Framework.Authentication.JWT.Services;
using Framework.Authentication.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.AspNetCore.JWT
{
    public class JwtBearerEvent<TKey> : JwtBearerEvents
    {
        private readonly JWTSessionPersistanceService<TKey> _jwtSessionPersistanceService;

        public JwtBearerEvent(ISessionPersistanceService<TKey> jwtSessionPersistanceService)
        {
            _jwtSessionPersistanceService = jwtSessionPersistanceService as JWTSessionPersistanceService<TKey>;
        }

        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            return base.AuthenticationFailed(context);
        }

        public override async Task MessageReceived(MessageReceivedContext context)
        {
            await base.MessageReceived(context);

            if (context.Request.Cookies.ContainsKey("X-Access-Token"))
            {
                context.Token = context.Request.Cookies["X-Access-Token"];

                var exp = new JwtSecurityTokenHandler().ReadJwtToken(context.Token).Payload.Exp;

                if (exp.HasValue && DateTime.UnixEpoch.ToLocalTime().AddSeconds(exp.Value) <= DateTime.Now)
                {
                    await _jwtSessionPersistanceService.RefreshSession();
                    context.Token = GetCookieValueFromResponse(context.Response, "X-Access-Token");
                }
                //context.Request.Headers.Add("Authorization", $"bearer {context.Token}");
                //_jwtSessionPersistanceService.RefreshSession()
            }
        }

        private string GetCookieValueFromResponse(HttpResponse response, string cookieName)
        {
            foreach (var headers in response.Headers.Values)
                foreach (var header in headers)
                    if (header.StartsWith($"{cookieName}="))
                    {
                        var p1 = header.IndexOf('=');
                        var p2 = header.IndexOf(';');
                        return header.Substring(p1 + 1, p2 - p1 - 1);
                    }
            return null;
        }
    }
}
