namespace LogisticsAndDeliveries.ContractTesting;

using PactNet;
using PactNet.Matchers;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

public class DriverConsumerPactTests
{
    private readonly IPactBuilderV4 _pactBuilder;
    private readonly ITestOutputHelper _output;

    public DriverConsumerPactTests(ITestOutputHelper output)
    {
        _output = output;

        var pactConfig = new PactConfig
        {
            PactDir = Path.Combine("..", "..", "..", "..", "pacts"),
            LogLevel = PactLogLevel.Debug,
            Outputters = new[] { new XUnitOutput(_output) }
        };

        _pactBuilder = Pact.V4("DriverConsumer", "LogisticsAndDeliveries.WebApi", pactConfig)
            .WithHttpInteractions();
    }

    [Fact]
    public async Task GetDriver_WhenDriverExists_ReturnsDriverDetails()
    {
        // Arrange
        var driverId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

        _pactBuilder
            .UponReceiving("A GET request to retrieve an existing driver")
            .Given("A driver with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 exists")
            .WithRequest(HttpMethod.Get, "/api/Driver/getDriver")
            .WithQuery("driverId", Match.Type(driverId.ToString()))
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                id = Match.Type(driverId),
                name = Match.Type("Juan Pérez"),
                latitude = Match.Decimal(40.4168),
                longitude = Match.Decimal(-3.7038),
                lastLocationUpdate = Match.Type("2025-12-20T10:30:00Z")
            });

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync($"/api/Driver/getDriver?driverId={driverId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            Assert.Contains("Juan Pérez", content);
            Assert.Contains(driverId.ToString(), content);
        });
    }

    [Fact]
    public async Task GetDriver_WhenDriverDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var nonExistentDriverId = Guid.Parse("00000000-0000-0000-0000-000000000000");

        _pactBuilder
            .UponReceiving("A GET request to retrieve a non-existent driver")
            .Given("No driver with ID 00000000-0000-0000-0000-000000000000 exists")
            .WithRequest(HttpMethod.Get, "/api/Driver/getDriver")
            .WithQuery("driverId", Match.Type(nonExistentDriverId.ToString()))
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(404);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync($"/api/Driver/getDriver?driverId={nonExistentDriverId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }

    [Fact]
    public async Task GetDrivers_ReturnsListOfDrivers()
    {
        // Arrange
        _pactBuilder
            .UponReceiving("A GET request to retrieve all drivers")
            .Given("Multiple drivers exist in the system")
            .WithRequest(HttpMethod.Get, "/api/Driver/getDrivers")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(Match.MinType(
                new
                {
                    id = Match.Type(Guid.NewGuid()),
                    name = Match.Type("Juan Pérez"),
                    latitude = Match.Decimal(40.4168),
                    longitude = Match.Decimal(-3.7038),
                    lastLocationUpdate = Match.Type("2025-12-20T10:30:00Z")
                }, 1));

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var response = await client.GetAsync("/api/Driver/getDrivers");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");

            Assert.Contains("Juan Pérez", content);
        });
    }

    [Fact]
    public async Task CreateDriver_WithValidData_ReturnsCreatedDriver()
    {
        // Arrange
        var driverId = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7");
        var driverName = "Carlos González";

        _pactBuilder
            .UponReceiving("A POST request to create a new driver")
            .Given("Valid driver data is provided")
            .WithRequest(HttpMethod.Post, "/api/Driver/createDriver")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .WithJsonBody(new
            {
                id = driverId,
                name = driverName
            })
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(Match.Type(driverId));

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var requestBody = new
            {
                id = driverId,
                name = driverName
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Driver/createDriver", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");
        });
    }

    [Fact]
    public async Task CreateDriver_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        _pactBuilder
            .UponReceiving("A POST request to create a driver with invalid data")
            .Given("Driver name is empty or invalid")
            .WithRequest(HttpMethod.Post, "/api/Driver/createDriver")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .WithJsonBody(new
            {
                id = Guid.NewGuid(),
                name = ""
            })
            .WillRespond()
            .WithStatus(400);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var requestBody = new
            {
                id = Guid.NewGuid(),
                name = ""
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Driver/createDriver", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    [Fact]
    public async Task UpdateDriverLocation_WithValidCoordinates_ReturnsSuccess()
    {
        // Arrange
        var driverId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        var latitude = 40.4168;
        var longitude = -3.7038;

        _pactBuilder
            .UponReceiving("A POST request to update driver location")
            .Given("A driver with ID 3fa85f64-5717-4562-b3fc-2c963f66afa6 exists")
            .WithRequest(HttpMethod.Post, "/api/Driver/updateDriverLocation")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .WithJsonBody(new
            {
                driverId = driverId,
                latitude = latitude,
                longitude = longitude
            })
            .WillRespond()
            .WithStatus(200)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(Match.Type(true));

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var requestBody = new
            {
                driverId = driverId,
                latitude = latitude,
                longitude = longitude
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Driver/updateDriverLocation", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response: {content}");
        });
    }

    [Fact]
    public async Task UpdateDriverLocation_WhenDriverNotFound_ReturnsNotFound()
    {
        // Arrange
        var nonExistentDriverId = Guid.Parse("00000000-0000-0000-0000-000000000000");

        _pactBuilder
            .UponReceiving("A POST request to update location of non-existent driver")
            .Given("No driver with ID 00000000-0000-0000-0000-000000000000 exists")
            .WithRequest(HttpMethod.Post, "/api/Driver/updateDriverLocation")
            .WithHeader("Content-Type", "application/json")
            .WithHeader("Accept", "application/json")
            .WithJsonBody(new
            {
                driverId = nonExistentDriverId,
                latitude = 40.4168,
                longitude = -3.7038
            })
            .WillRespond()
            .WithStatus(404);

        await _pactBuilder.VerifyAsync(async ctx =>
        {
            // Act
            var client = new HttpClient { BaseAddress = ctx.MockServerUri };
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var requestBody = new
            {
                driverId = nonExistentDriverId,
                latitude = 40.4168,
                longitude = -3.7038
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("/api/Driver/updateDriverLocation", jsonContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        });
    }
}
