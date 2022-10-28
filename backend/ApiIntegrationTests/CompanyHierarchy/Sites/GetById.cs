using System.Net;
using FastEndpoints;
using Xunit;
using Xunit.Priority;
using Endpoint = Api.Endpoints.Sites.GetById;

namespace ApiIntegrationTests.CompanyHierarchy.Sites;

[Collection("Sequential")]
[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class GetById : IClassFixture<Setup>
{
    private readonly HttpClient _client;

    public GetById(Setup setup)
    {
        _client = setup.Client;
    }
    
    [Fact]
    public async Task CanGetById()
    {
        Endpoint.Req req = new()
        {
            Id = 1
        };

        var (response, result) = await _client.GETAsync<Endpoint, Endpoint.Req, Endpoint.Res>(req);
        
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.OPUs.Count());
    }
}