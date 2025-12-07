using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.MarkDeliveryFailed
{
    public record MarkDeliveryFailedCommand(Guid PackageId) : IRequest<Result<bool>>;
}
