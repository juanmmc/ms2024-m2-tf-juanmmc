using LogisticsAndDeliveries.Application.Packages.MarkDeliveryCompleted;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Packages;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Packages;

public class MarkDeliveryCompletedHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkDeliveryCompletedHandler _handler;

    public MarkDeliveryCompletedHandlerTest()
    {
        _packageRepositoryMock = new Mock<IPackageRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new MarkDeliveryCompletedHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPackage_MarksCompletedSuccessfully()
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

        // Preparar el paquete para completar
        package.SetDeliveryOrder(1);
        package.MarkDeliveryInTransit();

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var command = new MarkDeliveryCompletedCommand(packageId, "Foto de evidencia");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(DeliveryStatus.Completed, package.DeliveryStatus);
        Assert.Equal("Foto de evidencia", package.DeliveryEvidence);
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

        var command = new MarkDeliveryCompletedCommand(packageId, "Foto de evidencia");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _packageRepositoryMock.Verify(x => x.GetByIdAsync(packageId, false), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyEvidence_ReturnsFailure()
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

        package.SetDeliveryOrder(1);
        package.MarkDeliveryInTransit();

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        var command = new MarkDeliveryCompletedCommand(packageId, "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        _packageRepositoryMock.Verify(x => x.GetByIdAsync(packageId, false), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
