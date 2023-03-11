using System.Reflection;
using EvolveDb;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace dbauto;

public class Program
{
    class EvolveHostedService : IHostedService
    {
        private readonly ILogger _logger;

        const string DBFile = "/Users/simonking/Microsoft/authx/authx.sqlite";

        private SqliteConnection? _cnx;

        public EvolveHostedService(ILogger<EvolveHostedService> logger)
        {
            this._logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("1. StartAsync has been called.");
            var connectionString = $"Data Source={DBFile};";
            _cnx = new SqliteConnection(connectionString);
            try
            {
                _logger.LogInformation("connectionString is {connectionString}", connectionString);
                var evolve = new Evolve(_cnx, msg => _logger.LogInformation("evolve logging: {msg}", msg))
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
            _logger.LogInformation("4. StopAsync has been called.");
            _cnx?.Close();
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