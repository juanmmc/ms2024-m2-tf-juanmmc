using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;
using MediatR;

namespace LogisticsAndDeliveries.Application.Packages.SetDeliveryOrder
{
    public class SetDeliveryOrderHandler : IRequestHandler<SetDeliveryOrderCommand, Result<bool>>
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetDeliveryOrderHandler(IPackageRepository packageRepository, IUnitOfWork unitOfWork)
        {
            _packageRepository = packageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(SetDeliveryOrderCommand request, CancellationToken cancellationToken)
        {
            var package = await _packageRepository.GetByIdAsync(request.PackageId);
            if (package == null)
                return Result<bool>.ValidationFailure(PackageErrors.PackageNotFound);
            try
            {
                package.SetDeliveryOrder(request.DeliveryOrder);
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
