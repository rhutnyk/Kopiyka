using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Households;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Kopiyka.Api.Functions;

public class HouseholdsFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<HouseholdsFunction> _logger;

    public HouseholdsFunction(ILogger<HouseholdsFunction> logger)
    {
        _logger = logger;
    }

    [Function("GetHouseholds")]
    public async Task<HttpResponseData> GetHouseholds(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/households")] HttpRequestData req)
    {
        _logger.LogInformation("Listing households for request {TraceId}", req.FunctionContext.InvocationId);

        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var households = new List<HouseholdResponse>
        {
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "Demo Household",
                "USD",
                DateTime.UtcNow.AddDays(-10))
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(households, JsonOptions);
        return response;
    }

    [Function("CreateHousehold")]
    public async Task<HttpResponseData> CreateHousehold(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/households")] HttpRequestData req)
    {
        _logger.LogInformation("Creating household for request {TraceId}", req.FunctionContext.InvocationId);

        if (!AuthorizationHelper.TryEnsureRole(req, "owner", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<CreateHouseholdRequest>(req.Body, JsonOptions);
        if (request is null)
        {
            var missing = req.CreateResponse(HttpStatusCode.BadRequest);
            await missing.WriteStringAsync("Invalid payload.");
            return missing;
        }

        var errors = request.Validate();
        if (errors.Count > 0)
        {
            var invalid = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalid.WriteAsJsonAsync(new { errors }, JsonOptions);
            return invalid;
        }

        var created = new HouseholdResponse(Guid.NewGuid(), request.Name, request.DefaultCurrency, DateTime.UtcNow);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created, JsonOptions);
        return response;
    }

    [Function("GetHouseholdDetails")]
    public async Task<HttpResponseData> GetHouseholdDetails(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/households/{householdId:guid}")] HttpRequestData req,
        Guid householdId)
    {
        _logger.LogInformation("Fetching household {HouseholdId}", householdId);

        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        var household = new HouseholdResponse(householdId, "Demo Household", "USD", DateTime.UtcNow.AddDays(-10));
        await response.WriteAsJsonAsync(household, JsonOptions);
        return response;
    }

    [Function("UpdateHousehold")]
    public async Task<HttpResponseData> UpdateHousehold(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/households/{householdId:guid}")] HttpRequestData req,
        Guid householdId)
    {
        _logger.LogInformation("Updating household {HouseholdId}", householdId);

        if (!AuthorizationHelper.TryEnsureRole(req, "owner", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<UpdateHouseholdRequest>(req.Body, JsonOptions);
        if (request is null)
        {
            var missing = req.CreateResponse(HttpStatusCode.BadRequest);
            await missing.WriteStringAsync("Invalid payload.");
            return missing;
        }

        var errors = request.Validate();
        if (errors.Count > 0)
        {
            var invalid = req.CreateResponse(HttpStatusCode.BadRequest);
            await invalid.WriteAsJsonAsync(new { errors }, JsonOptions);
            return invalid;
        }

        var updated = new HouseholdResponse(householdId, request.Name, "USD", DateTime.UtcNow.AddDays(-10));

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(updated, JsonOptions);
        return response;
    }
}
