using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Kopiyka.Api.Functions;

public class HealthFunction
{
    private readonly ILogger<HealthFunction> _logger;

    public HealthFunction(ILogger<HealthFunction> logger)
    {
        _logger = logger;
    }

    [Function("Health")]
    public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/health")] HttpRequestData req)
    {
        _logger.LogInformation("Health check invoked");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString("{\"status\":\"ok\"}");

        return response;
    }
}
