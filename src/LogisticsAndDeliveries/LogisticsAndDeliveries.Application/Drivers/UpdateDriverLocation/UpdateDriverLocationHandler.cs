using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;
using MediatR;

namespace LogisticsAndDeliveries.Application.Drivers.UpdateDriverLocation
{
    public class UpdateDriverLocationHandler : IRequestHandler<UpdateDriverLocationCommand, Result<bool>>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDriverLocationHandler(IDriverRepository driverRepository, IUnitOfWork unitOfWork)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId);

            if (driver == null)
            {
                return Result<bool>.ValidationFailure(DriverErrors.DriverNotFound);
            }

            driver.UpdateLocation(request.Longitude, request.Latitude);

            await _driverRepository.UpdateAsync(driver);
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}
