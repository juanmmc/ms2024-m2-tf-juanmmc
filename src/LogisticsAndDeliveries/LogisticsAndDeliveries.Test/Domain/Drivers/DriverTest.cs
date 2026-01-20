using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;

namespace LogisticsAndDeliveries.Test.Domain.Drivers;

public class DriverTest
{
    [Fact]
    public void Constructor_WithValidIdAndName_SetsProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Juan Miguel";

        // Act
        var driver = new Driver(id, name);

        // Assert
        Assert.Equal(id, driver.Id);
        Assert.Equal(name, driver.Name);
        Assert.Null(driver.Longitude);
        Assert.Null(driver.Latitude);
        Assert.Null(driver.LastLocationUpdate);
        Assert.Empty(driver.DomainEvents);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Nombre";

        // Act
        Action act = () => new Driver(id, name);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsDomainException(string name)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () => new Driver(id, name);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DriverErrors.NameIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(-74.005974, 40.712776)]
    [InlineData(0.0, 0.0)]
    [InlineData(179.9999, -89.9999)]
    public void UpdateLocation_SetsCoordinatesAndTimestamp(double longitude, double latitude)
    {
        // Arrange
        var driver = new Driver(Guid.NewGuid(), "x");
        var before = DateTime.UtcNow;

        // Act
        driver.UpdateLocation(longitude, latitude);

        // Assert
        var after = DateTime.UtcNow;
        Assert.Equal(longitude, driver.Longitude);
        Assert.Equal(latitude, driver.Latitude);
        Assert.NotNull(driver.LastLocationUpdate);
        Assert.True(driver.LastLocationUpdate.Value >= before && driver.LastLocationUpdate.Value <= after);
        Assert.Equal(DateTimeKind.Utc, driver.LastLocationUpdate.Value.Kind);
    }

    [Fact]
    public void UpdateLocation_MultipleCalls_UpdateTimestampAndValues()
    {
        // Arrange
        var driver = new Driver(Guid.NewGuid(), "x");

        // Act
        driver.UpdateLocation(1.0, 1.0);
        var first = driver.LastLocationUpdate;
        Thread.Sleep(10);
        driver.UpdateLocation(2.0, 2.0);

        // Assert
        Assert.True(driver.LastLocationUpdate > first);
        Assert.Equal(2.0, driver.Longitude);
        Assert.Equal(2.0, driver.Latitude);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void UpdateLocation_WithInvalidLatitude_ThrowsDomainException(double invalidLatitude)
    {
        // Arrange
        var driver = new Driver(Guid.NewGuid(), "x");

        // Act
        Action act = () => driver.UpdateLocation(0.0, invalidLatitude);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DriverErrors.InvalidLatitude().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void UpdateLocation_WithInvalidLongitude_ThrowsDomainException(double invalidLongitude)
    {
        // Arrange
        var driver = new Driver(Guid.NewGuid(), "x");

        // Act
        Action act = () => driver.UpdateLocation(invalidLongitude, 0.0);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DriverErrors.InvalidLongitude().Code, ex.Error.Code);
    }
}
