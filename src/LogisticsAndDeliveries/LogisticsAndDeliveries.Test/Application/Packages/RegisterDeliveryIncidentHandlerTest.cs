using LogisticsAndDeliveries.Application.Packages.RegisterDeliveryIncident;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Packages;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Packages;

public class RegisterDeliveryIncidentHandlerTest
{
    private readonly Mock<IPackageRepository> _packageRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RegisterDeliveryIncidentHandler _handler;

    public RegisterDeliveryIncidentHandlerTest()
    {
        _packageRepositoryMock = new Mock<IPackageRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RegisterDeliveryIncidentHandler(_packageRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_PackageNotFound_ReturnsValidationFailure()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var command = new RegisterDeliveryIncidentCommand(
            packageId,
            IncidentType.PatientAbsent,
            "Cliente no estaba en casa"
        );

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
    public async Task Handle_ValidIncident_RegistersSuccessfully()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = CreatePackageInFailedStatus(packageId);

        var command = new RegisterDeliveryIncidentCommand(
            packageId,
            IncidentType.IncorrectAddress,
            "Dirección incorrecta"
        );

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
        Assert.Equal(IncidentType.IncorrectAddress, package.IncidentType);
        Assert.Equal("Dirección incorrecta", package.IncidentDescription);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ReturnsDomainError()
    {
        // Arrange
        var packageId = Guid.NewGuid();
        var package = CreatePackageInPendingStatus(packageId);

        var command = new RegisterDeliveryIncidentCommand(
            packageId,
            IncidentType.Other,
            "Algún problema"
        );

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

    private Package CreatePackageInFailedStatus(Guid packageId)
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
        package.MarkDeliveryFailed();

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
