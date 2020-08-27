using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Authentication.JWT.DependencyInjection
{
    public static class FrameworkAuthenticationJwtConfigurationException
    {
        public static IServiceCollection AddFrameworkAuthenticationJwtConfiguration(this IServiceCollection services,
            Action jwt)
        {
            
            return services;
        }
    }
}
