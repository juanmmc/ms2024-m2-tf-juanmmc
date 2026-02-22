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
            var existingPackage = await _packageRepository.GetByIdAsync(request.Id, readOnly: true);
            if (existingPackage is not null)
            {
                return Result.Success(existingPackage.Id);
            }

            Package package;
            try
            {
                package = new Package(request.Id, request.Number, request.PatientId, request.PatientName, request.PatientPhone, request.DeliveryAddress, request.DeliveryLatitude, request.DeliveryLongitude, request.DeliveryDate, request.DriverId);
            }
            catch (DomainException ex)
            {
                return Result<Guid>.ValidationFailure(ex.Error);
            }

            // Persistir el agregado
            await _packageRepository.AddAsync(package);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(package.Id);
        }
    }
}
