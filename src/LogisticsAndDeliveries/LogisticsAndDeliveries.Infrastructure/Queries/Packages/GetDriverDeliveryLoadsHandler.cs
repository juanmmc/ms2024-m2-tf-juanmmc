using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogisticsAndDeliveries.Application.Packages.GetDriverDeliveryLoads;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAndDeliveries.Infrastructure.Queries.Packages
{
    internal sealed class GetDriverDeliveryLoadsHandler : IRequestHandler<GetDriverDeliveryLoadsQuery, Result<ICollection<DriverDeliveryLoadDto>>>
    {
        private readonly PersistenceDbContext _dbContext;

        public GetDriverDeliveryLoadsHandler(PersistenceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<ICollection<DriverDeliveryLoadDto>>> Handle(GetDriverDeliveryLoadsQuery request, CancellationToken cancellationToken)
        {
            var driverLoads = await _dbContext.Package
                .Where(package => package.DeliveryDate == request.DeliveryDate)
                .GroupBy(package => package.DriverId)
                .Select(group => new DriverDeliveryLoadDto
                {
                    DriverId = group.Key,
                    PackagesCount = group.Count()
                })
                .ToListAsync(cancellationToken);

            return driverLoads;
        }
    }
}
