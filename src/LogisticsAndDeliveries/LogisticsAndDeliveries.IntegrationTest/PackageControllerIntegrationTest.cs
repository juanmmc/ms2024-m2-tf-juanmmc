using FluentAssertions;
using LogisticsAndDeliveries.Application.Drivers.Dto;
using LogisticsAndDeliveries.Application.Packages.Dto;
using LogisticsAndDeliveries.IntegrationTest.Factories;
using System.Net;
using System.Net.Http.Json;

namespace LogisticsAndDeliveries.IntegrationTest;

public class PackageControllerIntegrationTest
{
    private readonly HttpClient _httpClient;

    public PackageControllerIntegrationTest()
    {
        this._httpClient = HttpClientFactory.CreateClient();
    }

    [Fact]
    public async Task CreatePackage_CreateValidPackage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var number = "PKG-01";
        var patientId = Guid.NewGuid();
        var patientName = "Ana Perez";
        var driverId = await GetRandomDriverIdAsync();
        var payload = new
        {
            id,
            number,
            patientId,
            patientName,
            patientPhone = "77341234",
            deliveryAddress = "B/Trompillo C/Alcaya #581",
            deliveryLatitude = -16.5000,
            deliveryLongitude = -68.1500,
            deliveryDate = DateOnly.FromDateTime(DateTime.UtcNow),
            driverId
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/Package/createPackage", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().Be(id);
    }

    [Fact]
    public async Task GetPackage_AfterCreate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var number = "PKG-02";
        var patientId = Guid.NewGuid();
        var patientName = "Luis Gomez";
        var patientPhone = "77987654";
        var deliveryAddress = "Av. San Martin #123";
        var deliveryLatitude = -16.5000;
        var deliveryLongitude = -68.1500;
        var deliveryDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var driverId = await GetRandomDriverIdAsync();
        var createPayload = new
        {
            id,
            number,
            patientId,
            patientName,
            patientPhone,
            deliveryAddress,
            deliveryLatitude,
            deliveryLongitude,
            deliveryDate,
            driverId
        };
        var createResp = await _httpClient.PostAsJsonAsync("/api/Package/createPackage", createPayload);
        createResp.EnsureSuccessStatusCode();

        // Act
        var getResp = await _httpClient.GetAsync($"/api/Package/getPackage?packageId={id}");

        // Assert
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResp.Content.ReadFromJsonAsync<PackageDto>();
        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.Number.Should().Be(number);
        dto.PatientId.Should().Be(patientId);
        dto.PatientName.Should().Be(patientName);
    }

    private async Task<Guid> GetRandomDriverIdAsync()
    {
        var getResp= await _httpClient.GetAsync(("/api/Driver/getDrivers"));
        getResp.EnsureSuccessStatusCode();
        var dtos = await getResp.Content.ReadFromJsonAsync<List<DriverDto>>();
        if (dtos == null || dtos.Count == 0)
        {
            throw new InvalidOperationException("No drivers available for testing.");
        }
        var randomIndex = Random.Shared.Next(dtos.Count);
        return dtos[randomIndex].Id;
    }
}
