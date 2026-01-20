using LogisticsAndDeliveries.Application.Drivers.Dto;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;
using MediatR;

namespace LogisticsAndDeliveries.Application.Drivers.GetDriver
{
    public class GetDriverHandler : IRequestHandler<GetDriverQuery, Result<DriverDto>>
    {
        private readonly IDriverRepository _driverRepository;

        public GetDriverHandler(IDriverRepository driverRepository)
        {
            _driverRepository = driverRepository;
        }

        public async Task<Result<DriverDto>> Handle(GetDriverQuery request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId);
            if (driver is null)
            {
                return Result<DriverDto>.ValidationFailure(DriverErrors.DriverNotFound);
            }
            var driverDto = new DriverDto
            {
                Id = driver.Id,
                Name = driver.Name,
                Latitude = driver.Latitude,
                Longitude = driver.Longitude,
                LastLocationUpdate = driver.LastLocationUpdate
            };
            return Result<DriverDto>.Success(driverDto);
        }
    }
}
