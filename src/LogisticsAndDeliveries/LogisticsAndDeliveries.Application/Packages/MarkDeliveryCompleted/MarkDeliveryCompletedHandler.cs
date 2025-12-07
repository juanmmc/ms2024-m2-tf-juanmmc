using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.MarkDeliveryCompleted
{
    internal class MarkDeliveryCompletedHandler : IRequestHandler<MarkDeliveryCompletedCommand, Result<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MarkDeliveryCompletedHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
        {
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(MarkDeliveryCompletedCommand request, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);

            if (package is null)
            {
                return Result<bool>.ValidationFailure(PackageErrors.PackageNotFound);
            }

            try
            {
                package.MarkDeliveryCompleted(request.DeliveryEvidence);
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
