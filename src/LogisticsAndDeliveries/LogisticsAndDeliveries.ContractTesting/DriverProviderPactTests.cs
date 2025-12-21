namespace LogisticsAndDeliveries.ContractTesting;

using PactNet;
using PactNet.Verifier;
using Xunit;
using Xunit.Abstractions;

public class DriverProviderPactTests
{
    private readonly ITestOutputHelper _output;

    public DriverProviderPactTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EnsureDriverApiHonoursPactWithConsumer()
    {
        var apiUrl = new Uri("http://localhost");

        var pactFile = Path.Combine(
            "..", "..", "..", "..", "pacts",
            "DriverConsumer-LogisticsAndDeliveries.WebApi.json");

        _output.WriteLine($"Looking for pact file at: {Path.GetFullPath(pactFile)}");
        _output.WriteLine($"Verifying against API at: {apiUrl}");

        // Verificar si el archivo existe
        if (!File.Exists(pactFile))
        {
            throw new FileNotFoundException($"Archivo pact no encontrado: {pactFile}");
        }

        // Act & Assert
        var verifier = new PactVerifier("LogisticsAndDeliveries.WebApi", new PactVerifierConfig
        {
            Outputters = new[] { new XUnitOutput(_output) },
            LogLevel = PactLogLevel.Debug
        });

        verifier
            .WithHttpEndpoint(apiUrl)
            .WithFileSource(new FileInfo(pactFile))
            .WithProviderStateUrl(new Uri(apiUrl, "/provider-states"))
            .Verify();
    }
}
