using LogisticsAndDeliveries.Domain.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel.Config
{
    public class DriverConfig : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.ToTable("driver");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id");

            builder.Property(d => d.Name)
                .HasColumnName("name")
                .HasMaxLength(200);

            builder.Property(d => d.Latitude)
                .HasColumnName("latitude")
                .HasColumnType("double precision");

            builder.Property(d => d.Longitude)
                .HasColumnName("longitude")
                .HasColumnType("double precision");

            builder.Property(d => d.LastLocationUpdate)
                .HasColumnName("lastLocationUpdate");

            builder.Ignore("_domainEvents");
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
