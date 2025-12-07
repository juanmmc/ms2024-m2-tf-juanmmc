using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.CancelDelivery
{
    public record CancelDeliveryCommand(Guid PackageId) : IRequest<Result<bool>>;
}
