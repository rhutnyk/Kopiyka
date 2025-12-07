using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Accounts;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Functions;

public class AccountsFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("GetAccounts")]
    public async Task<HttpResponseData> GetAccounts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/households/{householdId:guid}/accounts")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var accounts = new List<AccountResponse>
        {
            new(Guid.Parse("00000000-0000-0000-0000-000000000010"), "Checking", "cash", "USD", 1200m),
            new(Guid.Parse("00000000-0000-0000-0000-000000000011"), "Credit Card", "liability", "USD", -300m)
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(accounts, JsonOptions);
        return response;
    }

    [Function("CreateAccount")]
    public async Task<HttpResponseData> CreateAccount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/households/{householdId:guid}/accounts")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<CreateAccountRequest>(req.Body, JsonOptions);
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

        var created = new AccountResponse(Guid.NewGuid(), request.Name, request.Type, request.Currency, 0);
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created, JsonOptions);
        return response;
    }

    [Function("UpdateAccount")]
    public async Task<HttpResponseData> UpdateAccount(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/households/{householdId:guid}/accounts/{accountId:guid}")] HttpRequestData req,
        Guid householdId,
        Guid accountId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<UpdateAccountRequest>(req.Body, JsonOptions);
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

        var updated = new AccountResponse(accountId, request.Name, request.Type, "USD", 1200m);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(updated, JsonOptions);
        return response;
    }
}
