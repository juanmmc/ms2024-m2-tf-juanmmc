using LogisticsAndDeliveries.Application.Drivers.GetDriver;
using LogisticsAndDeliveries.Domain.Drivers;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Drivers;

public class GetDriverHandlerTest
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly GetDriverHandler _handler;

    public GetDriverHandlerTest()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _handler = new GetDriverHandler(_driverRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DriverNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var query = new GetDriverQuery(driverId);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync((Driver?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(DriverErrors.DriverNotFound.Code, result.Error.Code);
        Assert.Equal(DriverErrors.DriverNotFound.Description, result.Error.Description);
    }

    [Fact]
    public async Task Handle_DriverExists_ReturnsSuccessWithDto()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        driver.UpdateLocation(-77.0428, -12.0464);

        var query = new GetDriverQuery(driverId);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(driverId, result.Value.Id);
        Assert.Equal("Carlos Mendoza", result.Value.Name);
        Assert.Equal(-77.0428, result.Value.Longitude);
        Assert.Equal(-12.0464, result.Value.Latitude);
        Assert.NotNull(result.Value.LastLocationUpdate);
    }

    [Fact]
    public async Task Handle_DriverWithoutLocation_ReturnsSuccessWithNullCoordinates()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Ana López");

        var query = new GetDriverQuery(driverId);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(driverId, result.Value.Id);
        Assert.Equal("Ana López", result.Value.Name);
        Assert.Null(result.Value.Longitude);
        Assert.Null(result.Value.Latitude);
        Assert.Null(result.Value.LastLocationUpdate);
    }
}
