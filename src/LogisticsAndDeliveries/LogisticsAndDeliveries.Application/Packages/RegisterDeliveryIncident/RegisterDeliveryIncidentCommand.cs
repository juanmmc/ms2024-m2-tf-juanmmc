using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.RegisterDeliveryIncident
{
    public record RegisterDeliveryIncidentCommand(
        Guid PackageId,
        IncidentType IncidentType,
        string IncidentDescription
    ) : IRequest<Result<bool>>;
}
