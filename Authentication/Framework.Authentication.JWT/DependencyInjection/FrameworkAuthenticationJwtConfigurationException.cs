using Framework.Authentication.JWT.Services;
using Framework.Authentication.ServiceContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT.DependencyInjection
{
    public static class FrameworkAuthenticationJwtConfigurationException
    {
        public static IServiceCollection AddFrameworkAuthenticationJwtConfiguration(this IServiceCollection services, Action<JwtConfiguration> jwt)
        {
            services.Configure(jwt);
            services.AddTransient<ISessionPersistanceService, JWTSessionPersistanceService>();
            return services;
        }
    }
}
