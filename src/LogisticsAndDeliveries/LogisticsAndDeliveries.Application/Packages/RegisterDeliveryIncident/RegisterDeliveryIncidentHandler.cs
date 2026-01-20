using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.RegisterDeliveryIncident
{
    public class RegisterDeliveryIncidentHandler : IRequestHandler<RegisterDeliveryIncidentCommand, Result<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterDeliveryIncidentHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
        {
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(RegisterDeliveryIncidentCommand request, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);

            if (package == null)
                return Result<bool>.ValidationFailure(PackageErrors.PackageNotFound);

            try
            {
                package.RegisterDeliveryIncident(request.IncidentType, request.IncidentDescription);
            }
            catch (DomainException ex)
            {
                return Result<bool>.ValidationFailure(ex.Error);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(true);
        }
    }
}
