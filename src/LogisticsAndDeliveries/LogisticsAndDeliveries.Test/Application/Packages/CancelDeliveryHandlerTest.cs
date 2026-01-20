using LogisticsAndDeliveries.Application.Packages.CancelDelivery;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Packages;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Packages;

public class CancelDeliveryHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelDeliveryHandler _handler;

    public CancelDeliveryHandlerTest()
    {
        _packageRepositoryMock = new Mock<IPackageRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CancelDeliveryHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPackage_CancelsDeliverySuccessfully()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new Package(
            packageId,
            "PKG-001",
            Guid.NewGuid(),
            "Juan Miguel",
            "72193153",
            "Urb. Palmas del Norte",
            10.5,
            20.5,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Guid.NewGuid()
        );

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var command = new CancelDeliveryCommand(packageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(DeliveryStatus.Cancelled, package.DeliveryStatus);
        _packageRepositoryMock.Verify(x => x.GetByIdAsync(packageId, false), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PackageNotFound_ReturnsFailure()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync((Package?)null);

        var command = new CancelDeliveryCommand(packageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _packageRepositoryMock.Verify(x => x.GetByIdAsync(packageId, false), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CompletedPackage_ReturnsFailure()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = new Package(
            packageId,
            "PKG-001",
            Guid.NewGuid(),
            "Juan Miguel",
            "72193153",
            "Urb. Palmas del Norte",
            10.5,
            20.5,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Guid.NewGuid()
        );

        // Completar la entrega
        package.SetDeliveryOrder(1);
        package.MarkDeliveryInTransit();
        package.MarkDeliveryCompleted("Evidencia");

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        var command = new CancelDeliveryCommand(packageId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _packageRepositoryMock.Verify(x => x.GetByIdAsync(packageId, false), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
