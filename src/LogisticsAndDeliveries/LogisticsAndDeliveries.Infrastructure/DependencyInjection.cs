using LogisticsAndDeliveries.Application;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Deliveries;
using LogisticsAndDeliveries.Domain.Drivers;
using LogisticsAndDeliveries.Domain.Packages;
using LogisticsAndDeliveries.Infrastructure.Persistence;
using LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel;
using LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel;
using LogisticsAndDeliveries.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LogisticsAndDeliveries.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplication()
                .AddPersistence(configuration);

            // Register MediatR handlers defined in the Infrastructure assembly (e.g., query handlers)
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            return services;
        }

        /// <summary>
        /// Aplica las migraciones pendientes de la base de datos
        /// </summary>
        public static async Task ApplyMigrationsAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

            try
            {
                // Obtener el contexto de persistencia que contiene las migraciones
                var context = serviceProvider.GetRequiredService<PersistenceDbContext>();

                // Aplicar migraciones pendientes
                await context.Database.MigrateAsync();

                logger.LogInformation("Migraciones aplicadas exitosamente.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al aplicar migraciones: {Message}", ex.Message);
                throw;
            }
        }

        private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConnectionString = configuration.GetConnectionString("LogisticsAndDeliveriesDatabase");

            services.AddDbContext<PersistenceDbContext>(context => context.UseNpgsql(dbConnectionString));
            services.AddDbContext<DomainDbContext>(context => context.UseNpgsql(dbConnectionString));

            // Repositories
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IDeliveryRepository, DeliveryRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
