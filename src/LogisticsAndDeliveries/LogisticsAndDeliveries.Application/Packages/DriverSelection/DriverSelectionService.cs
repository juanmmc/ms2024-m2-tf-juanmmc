using LogisticsAndDeliveries.Application.Drivers.Dto;
using LogisticsAndDeliveries.Application.Packages.GetDriverDeliveryLoads;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LogisticsAndDeliveries.Application.Packages.DriverSelection
{
    internal sealed class DriverSelectionService : IDriverSelectionService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DriverSelectionService> _logger;

        public DriverSelectionService(
            IMediator mediator,
            ILogger<DriverSelectionService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<DriverDto?> SelectAsync(
            ICollection<DriverDto> drivers,
            DriverSelectionCriteria criteria,
            CancellationToken cancellationToken)
        {
            if (drivers.Count == 0)
            {
                return null;
            }

            return criteria.Strategy switch
            {
                DriverSelectionStrategy.LeastPackagesOnDate => await SelectDriverWithLeastPackagesAsync(
                    drivers,
                    criteria.DeliveryDate,
                    cancellationToken) ?? SelectDriverByDistance(drivers, criteria),
                _ => SelectDriverByDistance(drivers, criteria)
            };
        }

        private async Task<DriverDto?> SelectDriverWithLeastPackagesAsync(
            ICollection<DriverDto> drivers,
            DateOnly deliveryDate,
            CancellationToken cancellationToken)
        {
            var driverLoadsResult = await _mediator.Send(new GetDriverDeliveryLoadsQuery(deliveryDate), cancellationToken);

            if (driverLoadsResult.IsFailure)
            {
                _logger.LogWarning(
                    "No fue posible obtener la carga de paquetes para la fecha {DeliveryDate}. Se usará la estrategia por proximidad. Codigo: {Code} - {Message}",
                    deliveryDate,
                    driverLoadsResult.Error.Code,
                    driverLoadsResult.Error.Description);

                return null;
            }

            var loadLookup = driverLoadsResult.Value.ToDictionary(load => load.DriverId, load => load.PackagesCount);

            return drivers
                .Select(driver => new
                {
                    Driver = driver,
                    Packages = loadLookup.TryGetValue(driver.Id, out var count) ? count : 0
                })
                .OrderBy(entry => entry.Packages)
                .ThenBy(entry => entry.Driver.Name)
                .ThenBy(entry => entry.Driver.Id)
                .Select(entry => entry.Driver)
                .FirstOrDefault();
        }

        private static DriverDto? SelectDriverByDistance(
            ICollection<DriverDto> drivers,
            DriverSelectionCriteria criteria)
        {
            return drivers
                .Where(driver => driver.Latitude.HasValue && driver.Longitude.HasValue)
                .OrderBy(driver => GetDistanceInKm(
                    criteria.DeliveryLatitude,
                    criteria.DeliveryLongitude,
                    driver.Latitude!.Value,
                    driver.Longitude!.Value))
                .ThenBy(driver => driver.Name)
                .FirstOrDefault()
                ?? drivers
                    .OrderBy(driver => driver.Name)
                    .FirstOrDefault();
        }

        private static double GetDistanceInKm(double originLatitude, double originLongitude, double targetLatitude, double targetLongitude)
        {
            const double earthRadiusKm = 6371;

            var deltaLatitude = ToRadians(targetLatitude - originLatitude);
            var deltaLongitude = ToRadians(targetLongitude - originLongitude);

            var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                    Math.Cos(ToRadians(originLatitude)) * Math.Cos(ToRadians(targetLatitude)) *
                    Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double ToRadians(double value)
        {
            return value * Math.PI / 180;
        }
    }
}