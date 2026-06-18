using System.Net;
using FluentAssertions;
using Xunit;

namespace StrikeballApi.WebApi.Tests.IntegrationTests.Airsofters;

public class GetAirsoftersListTests : IntegrationTestBase
{
    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList()
    {
        var response = await Client.GetAsync("/api/airsofters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }
}
