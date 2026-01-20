using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.MarkDeliveryInTransit
{
    public class MarkDeliveryInTransitHandler : IRequestHandler<MarkDeliveryInTransitCommand, Result<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkDeliveryInTransitHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
        {
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(MarkDeliveryInTransitCommand request, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);

            if (package is null)
            {
                return Result<bool>.ValidationFailure(PackageErrors.PackageNotFound);
            }

            try
            {
                package.MarkDeliveryInTransit();
            }
            catch (DomainException ex)
            {
                return Result<bool>.ValidationFailure(ex.Error);
            }

            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
    }
}
