using LogisticsAndDeliveries.Application.Packages.MarkDeliveryFailed;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Packages;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Packages;

public class MarkDeliveryFailedHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly MarkDeliveryFailedHandler _handler;

    public MarkDeliveryFailedHandlerTest()
    {
        _packageRepositoryMock = new Mock<IPackageRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new MarkDeliveryFailedHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_PackageNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var command = new MarkDeliveryFailedCommand(packageId);

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync((Package?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PackageErrors.PackageNotFound.Code, result.Error.Code);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidPackage_MarksAsFailedSuccessfully()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = CreatePackageInTransit(packageId);

        var command = new MarkDeliveryFailedCommand(packageId);

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(DeliveryStatus.Failed, package.DeliveryStatus);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ReturnsDomainError()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = CreatePackageInPendingStatus(packageId);

        var command = new MarkDeliveryFailedCommand(packageId);

        _packageRepositoryMock
            .Setup(x => x.GetByIdAsync(packageId, false))
            .ReturnsAsync(package);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private Package CreatePackageInTransit(Guid packageId)
    {
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
            Guid.NewGuid());

        package.SetDeliveryOrder(1);
        package.MarkDeliveryInTransit();

        return package;
    }

    private Package CreatePackageInPendingStatus(Guid packageId)
    {
        return new Package(
            packageId,
            "PKG-001",
            Guid.NewGuid(),
            "Juan Miguel",
            "72193153",
            "Urb. Palmas del Norte",
            10.5,
            20.5,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Guid.NewGuid());
    }
}
