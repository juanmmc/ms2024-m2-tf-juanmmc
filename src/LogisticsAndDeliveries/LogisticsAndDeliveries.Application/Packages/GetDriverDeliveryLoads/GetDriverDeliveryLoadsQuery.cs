using System;
using System.Collections.Generic;
using LogisticsAndDeliveries.Core.Results;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.GetDriverDeliveryLoads
{
    public record GetDriverDeliveryLoadsQuery(DateOnly DeliveryDate) : IRequest<Result<ICollection<DriverDeliveryLoadDto>>>;
}
