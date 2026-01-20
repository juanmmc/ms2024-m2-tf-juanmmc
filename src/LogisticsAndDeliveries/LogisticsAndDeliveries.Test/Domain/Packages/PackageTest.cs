using LogisticsAndDeliveries.Core.Results;
using LogisticsAndDeliveries.Domain.Packages;

namespace LogisticsAndDeliveries.Test.Domain.Packages;

public class PackageTest
{
    private static Package CreateValidPackage(
            double latitude = 10,
            double longitude = 20,
            DateOnly? deliveryDate = null)
    {
        var id = Guid.NewGuid();
        var number = "PKG-001";
        var patientId = Guid.NewGuid();
        var patientName = "Juan Miguel";
        var patientPhone = "72193153";
        var deliveryAddress = "Urb. Palmas del Norte";
        var deliveryLat = latitude;
        var deliveryLon = longitude;
        var date = deliveryDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var driverId = Guid.NewGuid();

        return new Package(id, number, patientId, patientName, patientPhone, deliveryAddress, deliveryLat, deliveryLon, date, driverId);
    }

    [Fact]
    public void Constructor_WithValidArguments_SetsInitialState()
    {
        // Arrange
        var pkg = CreateValidPackage();

        // Act
        // (no action - constructor already executed in Arrange)

        // Assert
        Assert.Equal("PKG-001", pkg.Number);
        Assert.NotEqual(Guid.Empty, pkg.PatientId);
        Assert.Equal("Juan Miguel", pkg.PatientName);
        Assert.Equal("72193153", pkg.PatientPhone);
        Assert.Equal("Urb. Palmas del Norte", pkg.DeliveryAddress);
        Assert.Equal(10, pkg.DeliveryLatitude);
        Assert.Equal(20, pkg.DeliveryLongitude);
        Assert.Equal(0, pkg.DeliveryOrder);
        Assert.Equal(DeliveryStatus.Pending, pkg.DeliveryStatus);
        Assert.Null(pkg.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidNumber_ThrowsDomainException(string invalid)
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, invalid, patientId, "n", "p", "a", 0, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.NumberIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidPatientName_ThrowsDomainException(string invalid)
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", patientId, invalid, "p", "a", 0, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.PatientNameIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidPatientPhone_ThrowsDomainException(string invalid)
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", patientId, "name", invalid, "a", 0, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.PatientPhoneIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_InvalidDeliveryAddress_ThrowsDomainException(string invalid)
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", patientId, "name", "phone", invalid, 0, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.DeliveryAddressIsRequired().Code, ex.Error.Code);
    }

    [Fact]
    public void Constructor_WithEmptyPatientId_ThrowsDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", Guid.Empty, "n", "p", "a", 0, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.PatientIdIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Constructor_InvalidDeliveryLatitude_ThrowsDomainException(double invalidLat)
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", Guid.NewGuid(), "n", "p", "a", invalidLat, 0, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.InvalidDeliveryLatitude().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Constructor_InvalidDeliveryLongitude_ThrowsDomainException(double invalidLon)
    {
        // Arrange
        var id = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", Guid.NewGuid(), "n", "p", "a", 0, invalidLon, date, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.InvalidDeliveryLongitude().Code, ex.Error.Code);
    }

    [Fact]
    public void Constructor_DeliveryDateInPast_ThrowsDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var past = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        // Act
        Action act = () => new Package(id, "n", Guid.NewGuid(), "n", "p", "a", 0, 0, past, Guid.NewGuid());

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.InvalidDeliveryDate().Code, ex.Error.Code);
    }

    [Fact]
    public void Constructor_WithEmptyDriverId_ThrowsDomainException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        Action act = () => new Package(id, "n", patientId, "name", "phone", "address", 0, 0, date, Guid.Empty);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(PackageErrors.DriverIdIsRequired().Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Constructor_LatLongOnBoundary_AreAllowed(double lat, double lon)
    {
        // Arrange
        var pkg = CreateValidPackage(lat, lon, DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        // (no action)

        // Assert
        Assert.Equal(lat, pkg.DeliveryLatitude);
        Assert.Equal(lon, pkg.DeliveryLongitude);
    }

    [Fact]
    public void SetDeliveryOrder_WithValidOrder_SetsOrderAndUpdatedAt()
    {
        // Arrange
        var pkg = CreateValidPackage();
        var before = DateTime.UtcNow;

        // Act
        pkg.SetDeliveryOrder(1);
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(1, pkg.DeliveryOrder);
        Assert.NotNull(pkg.UpdatedAt);
        Assert.True(pkg.UpdatedAt.Value >= before && pkg.UpdatedAt.Value <= after);
        Assert.Equal(DateTimeKind.Utc, pkg.UpdatedAt.Value.Kind);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetDeliveryOrder_InvalidOrder_ThrowsDomainException(int invalid)
    {
        // Arrange
        var pkg = CreateValidPackage();

        // Act
        Action act = () => pkg.SetDeliveryOrder(invalid);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.InvalidOrderValue.Code, ex.Error.Code);
    }

    [Fact]
    public void MarkDeliveryInTransit_WithValidState_ChangesStatusAndUpdatedAt()
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        var before = DateTime.UtcNow;

        // Act
        pkg.MarkDeliveryInTransit();
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(DeliveryStatus.InTransit, pkg.DeliveryStatus);
        Assert.NotNull(pkg.UpdatedAt);
        Assert.True(pkg.UpdatedAt.Value >= before && pkg.UpdatedAt.Value <= after);
    }

    [Fact]
    public void MarkDeliveryInTransit_InvalidOrder_ThrowsDomainException()
    {
        // Arrange
        var pkg = CreateValidPackage(); // DeliveryOrder == 0

        // Act
        Action act = () => pkg.MarkDeliveryInTransit();

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.InvalidOrderValue.Code, ex.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MarkDeliveryCompleted_RequiresEvidence_ThrowsWhenMissing(string? evidence)
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();

        // Act
        Action act = () => pkg.MarkDeliveryCompleted(evidence!);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.DeliveryEvidenceIsRequired.Code, ex.Error.Code);
    }

    [Fact]
    public void MarkDeliveryCompleted_FromWrongStatus_ThrowsDomainException()
    {
        // Arrange
        var pkg = CreateValidPackage();

        // Act
        Action act = () => pkg.MarkDeliveryCompleted("evid");

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.InvalidStatusTransition.Code, ex.Error.Code);
    }

    [Fact]
    public void MarkDeliveryCompleted_SucceedsFromInTransit()
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();
        var before = DateTime.UtcNow;

        // Act
        pkg.MarkDeliveryCompleted("photo.jpg");
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(DeliveryStatus.Completed, pkg.DeliveryStatus);
        Assert.Equal("photo.jpg", pkg.DeliveryEvidence);
        Assert.NotNull(pkg.UpdatedAt);
        Assert.True(pkg.UpdatedAt.Value >= before && pkg.UpdatedAt.Value <= after);
    }

    [Fact]
    public void MarkDeliveryFailed_OnlyFromInTransit_SetsFailed()
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();
        var before = DateTime.UtcNow;

        // Act
        pkg.MarkDeliveryFailed();
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(DeliveryStatus.Failed, pkg.DeliveryStatus);
        Assert.NotNull(pkg.UpdatedAt);
        Assert.True(pkg.UpdatedAt.Value >= before && pkg.UpdatedAt.Value <= after);
    }

    [Fact]
    public void CancelDelivery_CannotCancelCompleted_ThrowsDomainException()
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();
        pkg.MarkDeliveryCompleted("evid");

        // Act
        Action act = () => pkg.CancelDelivery();

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.CannotCancelCompletedDelivery.Code, ex.Error.Code);
    }

    [Fact]
    public void CancelDelivery_FromPendingOrFailed_SetsCancelled()
    {
        // Arrange / Act / Assert
        // From Pending
        var p1 = CreateValidPackage();

        // Act
        p1.CancelDelivery();

        // Assert
        Assert.Equal(DeliveryStatus.Cancelled, p1.DeliveryStatus);
        Assert.NotNull(p1.UpdatedAt);

        // From Failed
        var p2 = CreateValidPackage();
        p2.SetDeliveryOrder(1);
        p2.MarkDeliveryInTransit();
        p2.MarkDeliveryFailed();

        // Act
        p2.CancelDelivery();

        // Assert
        Assert.Equal(DeliveryStatus.Cancelled, p2.DeliveryStatus);
        Assert.NotNull(p2.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegisterDeliveryIncident_RequiresDescription_ThrowsWhenMissing(string? desc)
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();
        pkg.MarkDeliveryFailed();

        // Act
        Action act = () => pkg.RegisterDeliveryIncident(IncidentType.Other, desc!);

        // Assert
        var ex = Assert.Throws<DomainException>(act);
        Assert.Equal(DeliveryErrors.IncidentDescriptionIsRequired.Code, ex.Error.Code);
    }

    [Fact]
    public void RegisterDeliveryIncident_OnlyWhenFailed_Succeeds()
    {
        // Arrange
        var pkg = CreateValidPackage();
        pkg.SetDeliveryOrder(1);
        pkg.MarkDeliveryInTransit();
        pkg.MarkDeliveryFailed();
        var before = DateTime.UtcNow;
        var incidentDescription = "Wrong street";

        // Act
        pkg.RegisterDeliveryIncident(IncidentType.IncorrectAddress, incidentDescription);
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(IncidentType.IncorrectAddress, pkg.IncidentType);
        Assert.Equal(incidentDescription, pkg.IncidentDescription);
        Assert.NotNull(pkg.UpdatedAt);
        Assert.True(pkg.UpdatedAt.Value >= before && pkg.UpdatedAt.Value <= after);
    }


}
