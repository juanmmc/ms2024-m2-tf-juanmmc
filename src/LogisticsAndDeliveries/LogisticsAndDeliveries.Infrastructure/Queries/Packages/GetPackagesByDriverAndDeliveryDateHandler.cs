using LogisticsAndDeliveries.Application.Packages.Dto;
using LogisticsAndDeliveries.Application.Packages.GetPackagesByDriverAndDeliveryDate;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LogisticsAndDeliveries.Infrastructure.Queries.Packages
{
    internal class GetPackagesByDriverAndDeliveryDateHandler : IRequestHandler<GetPackagesByDriverAndDeliveryDateQuery, Result<ICollection<PackageDto>>>
    {
        private readonly PersistenceDbContext _dbContext;

        public GetPackagesByDriverAndDeliveryDateHandler(PersistenceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<ICollection<PackageDto>>> Handle(GetPackagesByDriverAndDeliveryDateQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Package
                .Where(package => package.DriverId == request.DriverId && package.DeliveryDate == request.DeliveryDate)
                .OrderBy(package => package.DeliveryOrder)
                .Select(package => new PackageDto
                {
                    Id = package.Id,
                    DriverId = package.DriverId,
                    Number = package.Number,
                    PatientId = package.PatientId,
                    PatientName = package.PatientName,
                    PatientPhone = package.PatientPhone,
                    DeliveryAddress = package.DeliveryAddress,
                    DeliveryLatitude = package.DeliveryLatitude,
                    DeliveryLongitude = package.DeliveryLongitude,
                    DeliveryDate = package.DeliveryDate,
                    DeliveryEvidence = package.DeliveryEvidence,
                    DeliveryOrder = package.DeliveryOrder,
                    DeliveryStatus = package.DeliveryStatus,
                    IncidentType = package.IncidentType,
                    IncidentDescription = package.IncidentDescription,
                    UpdatedAt = package.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
