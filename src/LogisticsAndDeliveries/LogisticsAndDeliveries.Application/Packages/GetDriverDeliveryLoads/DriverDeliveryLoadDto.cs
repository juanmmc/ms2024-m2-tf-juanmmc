using System;

namespace LogisticsAndDeliveries.Application.Packages.GetDriverDeliveryLoads
{
    public record DriverDeliveryLoadDto
    {
        public Guid DriverId { get; init; }
        public int PackagesCount { get; init; }
    }
}
