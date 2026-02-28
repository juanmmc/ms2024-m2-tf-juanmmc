using LogisticsAndDeliveries.Application.Drivers.Dto;

namespace LogisticsAndDeliveries.Application.Packages.DriverSelection
{
    public interface IDriverSelectionService
    {
        Task<DriverDto?> SelectAsync(
            ICollection<DriverDto> drivers,
            DriverSelectionCriteria criteria,
            CancellationToken cancellationToken);
    }
}