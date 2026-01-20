using LogisticsAndDeliveries.Application.Drivers.CreateDriver;
using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Drivers;
using Moq;

namespace LogisticsAndDeliveries.Test.Application.Drivers;

public class CreateDriverHandlerTest
{
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateDriverHandler _handler;

    public CreateDriverHandlerTest()
    {
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateDriverHandler(_driverRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesDriverSuccessfully()
    {
        // Arrange
        var command = new CreateDriverCommand(Guid.NewGuid(), "Carlos Mendoza");

        _driverRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Driver>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Id, result.Value);
        _driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateDriverCommand(Guid.NewGuid(), "");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        _driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullName_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateDriverCommand(Guid.NewGuid(), null!);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        _driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhitespaceName_ThrowsDomainException()
    {
        // Arrange
        var command = new CreateDriverCommand(Guid.NewGuid(), "   ");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(async () =>
        {
            await _handler.Handle(command, CancellationToken.None);
        });

        _driverRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Driver>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
