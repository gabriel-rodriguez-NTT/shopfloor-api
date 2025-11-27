using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace ShopfloorAssistant.EntityFrameworkCore
{
    public class ShopfloorAssistantDbContextFactory : IDesignTimeDbContextFactory<ShopfloorAssistantDbContext>
    {
        public ShopfloorAssistantDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
                    "..",
                    "ShopfloorAssistant.Host"
                );
            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ShopfloorAssistantDbContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new ShopfloorAssistantDbContext(optionsBuilder.Options);
        }
    }


    public static class MigrationExtensions
    {
        public static IHost ApplyMigrations(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ShopfloorAssistantDbContext>();
                db.Database.Migrate();
            }

            return host;
        }
    }


}
