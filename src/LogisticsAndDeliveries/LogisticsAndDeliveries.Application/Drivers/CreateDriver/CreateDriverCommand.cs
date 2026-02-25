using LogisticsAndDeliveries.Core.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogisticsAndDeliveries.Application.Drivers.CreateDriver
{
    public record CreateDriverCommand(string Name) : IRequest<Result<Guid>>;
}
