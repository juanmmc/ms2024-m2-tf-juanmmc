using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.CreatePackage
{
    public class CreatePackageHandler : IRequestHandler<CreatePackageCommand, Result<Guid>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePackageHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
        {
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
        {
            // Crear el agregado de dominio
            var package = new Package(request.Id, request.Number, request.PatientId, request.PatientName, request.PatientPhone, request.DeliveryAddress, request.DeliveryLatitude, request.DeliveryLongitude, request.DeliveryDate, request.DriverId);

            // Persistir el agregado
            await _packageRepository.AddAsync(package);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(package.Id);
        }
    }
}
