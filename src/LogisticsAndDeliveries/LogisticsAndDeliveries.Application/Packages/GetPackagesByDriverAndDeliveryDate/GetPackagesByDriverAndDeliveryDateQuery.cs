using LogisticsAndDeliveries.Application.Packages.Dto;
using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.GetPackagesByDriverAndDeliveryDate
{
    public record GetPackagesByDriverAndDeliveryDateQuery(Guid DriverId, DateOnly DeliveryDate) : IRequest<Result<ICollection<PackageDto>>>;
}
