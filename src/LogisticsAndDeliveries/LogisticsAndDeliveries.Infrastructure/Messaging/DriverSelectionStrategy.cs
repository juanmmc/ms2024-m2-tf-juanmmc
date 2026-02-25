namespace LogisticsAndDeliveries.Infrastructure.Messaging
{
    public enum DriverSelectionStrategy
    {
        NearestToDelivery = 0,
        LeastPackagesOnDate = 1
    }
}
