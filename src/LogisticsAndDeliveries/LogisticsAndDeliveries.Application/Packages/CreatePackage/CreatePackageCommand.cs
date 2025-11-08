using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.CreatePackage
{
    public record CreatePackageCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public Guid PatientId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string PatientPhone { get; init; } = string.Empty;
        public string DeliveryAddress { get; init; } = string.Empty;
        public double DeliveryLatitude { get; init; }
        public double DeliveryLongitude { get; init; }
        public DateOnly ScheduledDate { get; init; }
        public Guid DriverId { get; init; }
    }
}
