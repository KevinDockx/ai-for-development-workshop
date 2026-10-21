using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ClaimHub.Api.Tests;

public class CustomerEndpointTests
{
    [Fact]
    public async Task Get_Customers_Returns_Ok()
    {
        var workFolder = Path.Combine(Path.GetTempPath(), "claimhub-api-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workFolder);

        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings__ClaimHub"] = $"Data Source={Path.Combine(workFolder, "claimhub-test.db")};Pooling=False",
            ["DocumentStorage__RootPath"] = Path.Combine(workFolder, "documents")
        };

        foreach (var (key, value) in settings)
        {
            Environment.SetEnvironmentVariable(key, value);
        }

        try
        {
            using var factory = new WebApplicationFactory<Program>();
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/customers");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            foreach (var key in settings.Keys)
            {
                Environment.SetEnvironmentVariable(key, null);
            }

            try
            {
                Directory.Delete(workFolder, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}
