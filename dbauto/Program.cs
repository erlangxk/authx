using System.Reflection;
using EvolveDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;


namespace dbauto;

public class Program
{
    class EvolveHostedService : IHostedService
    {
        private readonly ILogger _logger;

        const string ConnectionString = "Host=localhost;Database=postgres;Username=postgres;Password=mysecretpassword";

        public EvolveHostedService(ILogger<EvolveHostedService> logger)
        {
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Host StartAsync has been called.");
            try
            {
                using var ds =NpgsqlDataSource.Create(ConnectionString);
                using var cnx = ds.OpenConnection();
                _logger.LogInformation("connectionString is {connectionString}", ConnectionString);
                var evolve = new Evolve(cnx, msg => _logger.LogInformation("evolve logging: {msg}", msg))
                {
                    EmbeddedResourceAssemblies = new[] { Assembly.GetExecutingAssembly() }
                };
                evolve.Migrate();
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Database migration failed.", ex);
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Host StopAsync has been called.");
            return Task.CompletedTask;
        }
    }

    public static void Main(string[] args)
    {
        CreateHost(args).Run();
    }

    private static IHost CreateHost(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args);
        host.ConfigureServices((_, services) =>
            services.AddHostedService<EvolveHostedService>());
        return host.Build();
    }
}