using LogisticsAndDeliveries.Application.Packages.GetPackage;
using LogisticsAndDeliveries.Domain.Packages;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Packages;

public class GetPackageHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly GetPackageHandler _handler;

    public GetPackageHandlerTest()
    {
        _packageRepositoryMock = new Mock<IPackageRepository>();
        _handler = new GetPackageHandler(_packageRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_PackageNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var query = new GetPackageQuery(packageId);

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync((Package?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PackageErrors.PackageNotFound.Code, result.Error.Code);
        Assert.Equal(PackageErrors.PackageNotFound.Description, result.Error.Description);
    }

    [Fact]
    public async Task Handle_PackageExists_ReturnsSuccessWithDto()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var deliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var package = new Package(
            packageId,
            "PKG-001",
            patientId,
            "Juan Miguel",
            "72193153",
            "Urb. Palmas del Norte",
            10.5,
            20.5,
            deliveryDate,
            driverId);

        var query = new GetPackageQuery(packageId);

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(packageId, result.Value.Id);
        Assert.Equal("PKG-001", result.Value.Number);
        Assert.Equal(patientId, result.Value.PatientId);
        Assert.Equal("Juan Miguel", result.Value.PatientName);
        Assert.Equal("72193153", result.Value.PatientPhone);
        Assert.Equal("Urb. Palmas del Norte", result.Value.DeliveryAddress);
        Assert.Equal(10.5, result.Value.DeliveryLatitude);
        Assert.Equal(20.5, result.Value.DeliveryLongitude);
        Assert.Equal(deliveryDate, result.Value.DeliveryDate);
        Assert.Equal(driverId, result.Value.DriverId);
        Assert.Equal("Pending", result.Value.DeliveryStatus);
        Assert.Equal(0, result.Value.DeliveryOrder);
    }
}
