using LogisticsAndDeliveries.Application.Drivers.UpdateDriverLocation;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Drivers;

public class UpdateDriverLocationHandlerTest
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly UpdateDriverLocationHandler _handler;

    public UpdateDriverLocationHandlerTest()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new UpdateDriverLocationHandler(_driverRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidLocation_UpdatesLocationSuccessfully()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        var command = new UpdateDriverLocationCommand(driverId, -77.0428, -12.0464);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        _driverRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(-77.0428, driver.Latitude);
        Assert.Equal(-12.0464, driver.Longitude);
        Assert.NotNull(driver.LastLocationUpdate);
        _driverRepositoryMock.Verify(x => x.GetByIdAsync(driverId, false), Times.Once);
        _driverRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DriverNotFound_ReturnsFailure()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var command = new UpdateDriverLocationCommand(driverId, -77.0428, -12.0464);

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync((Driver?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(DriverErrors.DriverNotFound.Code, result.Error.Code);
        _driverRepositoryMock.Verify(x => x.GetByIdAsync(driverId, false), Times.Once);
        _driverRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidLatitude_ThrowsDomainException()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        var command = new UpdateDriverLocationCommand(driverId, -97.0428, 100.0); // Latitud inválida

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        _driverRepositoryMock.Verify(x => x.GetByIdAsync(driverId, false), Times.Once);
        _driverRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidLongitude_ThrowsDomainException()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        var command = new UpdateDriverLocationCommand(driverId, -200.0, -12.0464); // Longitud inválida

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        _driverRepositoryMock.Verify(x => x.GetByIdAsync(driverId, false), Times.Once);
        _driverRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BoundaryLatitudeValues_UpdatesSuccessfully()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        var command = new UpdateDriverLocationCommand(driverId, 90.0, 0.0); // Latitud máxima válida

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        _driverRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(90.0, driver.Latitude);
        Assert.Equal(0.0, driver.Longitude);
    }

    [Fact]
    public async Task Handle_BoundaryLongitudeValues_UpdatesSuccessfully()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var driver = new Driver(driverId, "Carlos Mendoza");
        var command = new UpdateDriverLocationCommand(driverId, 0.0, 180.0); // Longitud máxima válida

        _driverRepositoryMock
            .Setup(x => x.GetByIdAsync(driverId, false))
            .ReturnsAsync(driver);

        _driverRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Driver>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.0, driver.Latitude);
        Assert.Equal(180.0, driver.Longitude);
    }
}
