using FluentAssertions;
using LogisticsAndDeliveries.Application.Drivers.Dto;
using LogisticsAndDeliveries.IntegrationTest.Factories;
using System.Net;
using System.Net.Http.Json;

namespace LogisticsAndDeliveries.IntegrationTest;

public class DriverControllerIntegrationTest
{
    private readonly HttpClient _httpClient;

    public DriverControllerIntegrationTest()
    {
        this._httpClient = HttpClientFactory.CreateClient();
    }

    [Fact]
    public async Task CreateDriver_CreateValidDriver()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payload = new
        {
            id = id,
            name = "Eduardo Avaroa"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/Driver/createDriver", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var returnedId = await response.Content.ReadFromJsonAsync<Guid>();
        returnedId.Should().Be(id);
    }

    [Fact]
    public async Task GetDriver_AfterCreate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Simón Bolivar";
        var createPayload = new { id = id, name = name };
        var createResp = await _httpClient.PostAsJsonAsync("/api/Driver/createDriver", createPayload);
        createResp.EnsureSuccessStatusCode();

        // Act
        var getResp = await _httpClient.GetAsync($"/api/Driver/getDriver?driverId={id}");

        // Assert
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await getResp.Content.ReadFromJsonAsync<DriverDto>();
        dto.Should().NotBeNull();
        dto.Id.Should().Be(id);
        dto.Name.Should().Be(name);
    }

    [Fact]
    public async Task UpdateDriverLocation_ExistingDriver()
    {
        // Arrange
        var id = Guid.NewGuid();
        var createPayload = new { id = id, name = "Antonio José de Sucre" };
        var createResp = await _httpClient.PostAsJsonAsync("/api/Driver/createDriver", createPayload);
        createResp.EnsureSuccessStatusCode();

        // Act
        var updatePayload = new
        {
            DriverId = id,
            Latitude = 40.4168,
            Longitude = -3.7038
        };
        var updateResp = await _httpClient.PostAsJsonAsync("/api/Driver/updateDriverLocation", updatePayload);

        // Assert
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

}