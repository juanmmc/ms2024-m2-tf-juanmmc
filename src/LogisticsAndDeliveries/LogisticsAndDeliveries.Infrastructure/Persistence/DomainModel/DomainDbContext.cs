using LogisticsAndDeliveries.Domain.Packages;
using LogisticsAndDeliveries.Domain.Drivers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel
{
    public class DomainDbContext : DbContext
    {
        public DbSet<Package> Package { get; set; }
        public DbSet<Driver> Driver { get; set; }

        public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<Core.Abstractions.DomainEvent>();
        }
    }
}
