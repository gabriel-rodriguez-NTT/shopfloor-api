using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShopfloorAssistant.Core.Repository;

namespace ShopfloorAssistant.EntityFrameworkCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkCoreServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ShopfloorAssistantDbContext>(options =>
                options
                .UseSqlServer(connectionString)
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
                , ServiceLifetime.Transient, ServiceLifetime.Transient)

                ;
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            //services.AddTransient<IThreadRepository, ThreadRepository>();
            services.AddTransient<IThreadRepository, ThreadRepository>();

            return services;
        }
    }
}
