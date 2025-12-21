using LogisticsAndDeliveries.Domain.Packages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel.Config
{
    public class PackageConfig : IEntityTypeConfiguration<Package>
    {
        public void Configure(EntityTypeBuilder<Package> builder)
        {
            builder.ToTable("package");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                .HasColumnName("id");

            builder.Property(d => d.DriverId)
                .HasColumnName("driverId");

            builder.Property(p => p.Number)
                .HasColumnName("number")
                .HasMaxLength(100);

            builder.Property(p => p.PatientId)
                .HasColumnName("patientId");

            builder.Property(p => p.PatientName)
                .HasColumnName("patientName")
                .HasMaxLength(200);

            builder.Property(p => p.PatientPhone)
                .HasColumnName("patientPhone")
                .HasMaxLength(15);

            builder.Property(p => p.DeliveryAddress)
                .HasColumnName("deliveryAddress")
                .HasMaxLength(300);

            builder.Property(p => p.DeliveryLatitude)
                .HasColumnName("deliveryLatitude")
                .HasColumnType("double precision");

            builder.Property(p => p.DeliveryLongitude)
                .HasColumnName("deliveryLongitude")
                .HasColumnType("double precision");

            builder.Property(p => p.DeliveryDate)
                .HasColumnName("deliveryDate")
                .HasColumnType("date");

            builder.Property(p => p.DeliveryEvidence)
                .HasColumnName("deliveryEvidence");

            builder.Property(p => p.DeliveryOrder)
                .HasColumnName("deliveryOrder")
                .HasColumnType("integer");

            var statusConverter = new ValueConverter<DeliveryStatus, string>(
                statusConverter => statusConverter.ToString(),
                deliveryStatus => (DeliveryStatus)Enum.Parse(typeof(DeliveryStatus), deliveryStatus)
            );

            builder.Property(p => p.DeliveryStatus)
                .HasConversion(statusConverter)
                .HasColumnName("deliveryStatus");

            var incidentTypeConverter = new ValueConverter<IncidentType, string>(
                incidentTypeConverter => incidentTypeConverter.ToString(),
                incidentType => (IncidentType)Enum.Parse(typeof(IncidentType), incidentType)
            );

            builder.Property(p => p.IncidentType)
                .HasConversion(incidentTypeConverter)
                .HasColumnName("incidentType");

            builder.Property(p => p.IncidentDescription)
                .HasColumnName("incidentDescription");

            builder.Property(d => d.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Ignore("_domainEvents");
            builder.Ignore(x => x.DomainEvents);
        }
    }
}
