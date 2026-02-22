using LogisticsAndDeliveries.Application;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Drivers;
using LogisticsAndDeliveries.Domain.Packages;
using LogisticsAndDeliveries.Infrastructure.Persistence;
using LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel;
using LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel;
using LogisticsAndDeliveries.Infrastructure.Persistence.Repositories;
using LogisticsAndDeliveries.Infrastructure.Messaging;
using LogisticsAndDeliveries.Infrastructure.Messaging.Consumers;
using LogisticsAndDeliveries.Infrastructure.Messaging.Outbox;
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

            services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddHostedService<PackageDispatchCreatedConsumer>();
            services.AddHostedService<OutboxPublisherService>();

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

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS outbox_message (
                        id uuid NOT NULL,
                        eventname text NOT NULL,
                        type text NOT NULL,
                        content text NOT NULL,
                        occurredonutc timestamp with time zone NOT NULL,
                        processedonutc timestamp with time zone NULL,
                        error text NULL,
                        CONSTRAINT PK_outbox_message PRIMARY KEY (id)
                    );");

                await context.Database.ExecuteSqlRawAsync(@"
                    CREATE INDEX IF NOT EXISTS IX_outbox_message_processedOnUtc_occurredOnUtc
                    ON outbox_message (processedonutc, occurredonutc);");

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

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
