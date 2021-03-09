using Framework.Authentication.AspNetCore.JWT;
using Framework.Authentication.JWT.Services;
using Framework.Authentication.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Authentication.JWT.DependencyInjection
{
    public static class FrameworkAuthenticationJwtCookieConfigurationExtensions
    {
        public static IServiceCollection AddFrameworkAuthenticationJwtConfiguration<TKey, TSessionPersistantService>(this IServiceCollection services, string jwtSecret,
            Action<JwtConfiguration> jwt)
            where TSessionPersistantService : ISessionPersistanceService<TKey>
        {
            services.Configure(jwt);
            services.AddHttpContextAccessor();

            services.AddAuthentication(i =>
            {
                i.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                i.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                i.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                i.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
            })
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuer = false,
                       ValidateAudience = false,
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                       ValidateLifetime = true,
                       ClockSkew = TimeSpan.Zero
                   };
                   options.SaveToken = true;
                   options.EventsType = typeof(JwtBearerEvent<TKey>);
               })
               .AddCookie(options =>
               {
                   options.Cookie.SameSite = SameSiteMode.Strict;
                   options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                   options.Cookie.IsEssential = true;
               });

            services.AddTransient(typeof(ISessionPersistanceService<TKey>), typeof(TSessionPersistantService));
            services.AddTransient<JwtBearerEvent<TKey>>();

            return services;
        }
    }
}
