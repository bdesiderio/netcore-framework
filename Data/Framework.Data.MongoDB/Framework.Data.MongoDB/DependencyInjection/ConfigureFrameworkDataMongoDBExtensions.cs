using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace Framework.Data.MongoDB.DependencyInjection
{
    public static class ConfigureFrameworkDataMongoDBExtensions
    {
        public static IServiceCollection AddConfigureFrameworkDataMongoDB(this IServiceCollection services, Action<MongoDbSettings> action)
        {
            services.Configure(action);

            services.AddSingleton<IMongoDbSettings>(serviceProvider =>
                serviceProvider.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            return services;
        }
    }
}
