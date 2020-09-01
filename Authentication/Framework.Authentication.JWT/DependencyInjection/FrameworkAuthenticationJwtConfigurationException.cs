using Framework.Authentication.JWT.Services;
using Framework.Authentication.ServiceContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT.DependencyInjection
{
    public static class FrameworkAuthenticationJwtConfigurationException
    {
        public static IServiceCollection AddFrameworkAuthenticationJwtConfiguration(this IServiceCollection services, string jwtSecret, Action<JwtConfiguration> jwt)
        {
            services.Configure(jwt);
            services.AddHttpContextAccessor();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            services.AddTransient<ISessionPersistanceService, JWTSessionPersistanceService>();
            return services;
        }
    }
}
