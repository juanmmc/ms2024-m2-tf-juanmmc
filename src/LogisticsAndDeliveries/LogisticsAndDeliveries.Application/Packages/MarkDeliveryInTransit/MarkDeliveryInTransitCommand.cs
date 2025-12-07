using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.MarkDeliveryInTransit
{
    public record MarkDeliveryInTransitCommand(Guid PackageId) : IRequest<Result<bool>>;
}
