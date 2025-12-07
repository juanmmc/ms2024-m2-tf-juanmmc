using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.MarkDeliveryCompleted
{
    public record MarkDeliveryCompletedCommand(
        Guid PackageId,
        string DeliveryEvidence
    ) : IRequest<Result<bool>>;
}
