using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.SetDeliveryOrder
{
    public record SetDeliveryOrderCommand(Guid PackageId, int DeliveryOrder) : IRequest<Result<bool>>;
}
