using Exemplo.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using System.IO;

namespace CRM.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExemploDbContext>
    {
        public ExemploDbContext CreateDbContext(string[] args)
        {
            // Ajuste o nome da pasta conforme está na sua solução
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../CRM.Api");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ExemploDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ExemploDbContext(optionsBuilder.Options);
        }
    }
}
