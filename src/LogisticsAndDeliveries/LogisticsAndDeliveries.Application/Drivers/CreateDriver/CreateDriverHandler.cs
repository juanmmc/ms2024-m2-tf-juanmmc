using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;
using MediatR;

namespace LogisticsAndDeliveries.Application.Drivers.CreateDriver
{
    public class CreateDriverHandler : IRequestHandler<CreateDriverCommand, Result<Guid>>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDriverHandler(IDriverRepository deliveryRepository, IUnitOfWork unitOfWork)
        {
            _driverRepository = deliveryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            // Crear el agregado de dominio
            var driver = new Driver(request.Id, request.Name);

            // Persistir el agregado
            await _driverRepository.AddAsync(driver);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(driver.Id);
        }
    }
}
