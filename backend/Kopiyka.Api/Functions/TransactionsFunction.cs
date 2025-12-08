using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Transactions;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Functions;

public class TransactionsFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("GetTransactions")]
    public async Task<HttpResponseData> GetTransactions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/households/{householdId:guid}/transactions")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var transactions = new List<TransactionResponse>
        {
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000030"),
                Guid.Parse("00000000-0000-0000-0000-000000000010"),
                Guid.Parse("00000000-0000-0000-0000-000000000021"),
                "Groceries",
                120.55m,
                "USD",
                DateTime.UtcNow.AddDays(-2)),
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000031"),
                Guid.Parse("00000000-0000-0000-0000-000000000010"),
                null,
                "Paycheck",
                2400m,
                "USD",
                DateTime.UtcNow.AddDays(-5))
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(transactions);
        return response;
    }

    [Function("CreateTransaction")]
    public async Task<HttpResponseData> CreateTransaction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/households/{householdId:guid}/transactions")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<CreateTransactionRequest>(req.Body, JsonOptions);
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
            await invalid.WriteAsJsonAsync(new { errors });
            return invalid;
        }

        var created = new TransactionResponse(
            Guid.NewGuid(),
            request.AccountId,
            request.CategoryId,
            request.Description,
            request.Amount,
            request.Currency,
            request.OccurredAt);

        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created);
        return response;
    }

    [Function("UpdateTransaction")]
    public async Task<HttpResponseData> UpdateTransaction(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/households/{householdId:guid}/transactions/{transactionId:guid}")] HttpRequestData req,
        Guid householdId,
        Guid transactionId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<UpdateTransactionRequest>(req.Body, JsonOptions);
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
            await invalid.WriteAsJsonAsync(new { errors });
            return invalid;
        }

        var updated = new TransactionResponse(
            transactionId,
            Guid.Parse("00000000-0000-0000-0000-000000000010"),
            request.CategoryId,
            request.Description,
            request.Amount,
            "USD",
            request.OccurredAt);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(updated);
        return response;
    }
}
