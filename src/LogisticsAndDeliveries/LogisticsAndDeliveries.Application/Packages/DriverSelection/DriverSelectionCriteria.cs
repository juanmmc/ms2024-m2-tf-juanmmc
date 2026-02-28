namespace LogisticsAndDeliveries.Application.Packages.DriverSelection
{
    public record DriverSelectionCriteria(
        DateOnly DeliveryDate,
        double DeliveryLatitude,
        double DeliveryLongitude,
        DriverSelectionStrategy Strategy);
}