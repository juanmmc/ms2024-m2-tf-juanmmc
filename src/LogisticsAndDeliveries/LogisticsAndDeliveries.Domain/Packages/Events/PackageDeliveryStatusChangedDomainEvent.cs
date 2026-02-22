using LogisticsAndDeliveries.Core.Abstractions;

namespace LogisticsAndDeliveries.Domain.Packages.Events
{
    public sealed record PackageDeliveryStatusChangedDomainEvent(
        Guid PackageId,
        Guid DriverId,
        string Number,
        string DeliveryStatus,
        string? IncidentType,
        string? IncidentDescription,
        string? DeliveryEvidence,
        DateTime? UpdatedAt) : DomainEvent;
}
