using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Categories;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Functions;

public class CategoriesFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("GetCategories")]
    public async Task<HttpResponseData> GetCategories(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/households/{householdId:guid}/categories")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var categories = new List<CategoryResponse>
        {
            new(Guid.Parse("00000000-0000-0000-0000-000000000020"), "Income", "income", null),
            new(Guid.Parse("00000000-0000-0000-0000-000000000021"), "Groceries", "expense", null)
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(categories, JsonOptions);
        return response;
    }

    [Function("CreateCategory")]
    public async Task<HttpResponseData> CreateCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/households/{householdId:guid}/categories")] HttpRequestData req,
        Guid householdId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<CreateCategoryRequest>(req.Body, JsonOptions);
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

        var created = new CategoryResponse(Guid.NewGuid(), request.Name, request.Type, request.ParentCategoryId);
        var response = req.CreateResponse(HttpStatusCode.Created);
        await response.WriteAsJsonAsync(created, JsonOptions);
        return response;
    }

    [Function("UpdateCategory")]
    public async Task<HttpResponseData> UpdateCategory(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "api/v1/households/{householdId:guid}/categories/{categoryId:guid}")] HttpRequestData req,
        Guid householdId,
        Guid categoryId)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "editor", out var unauthorized))
        {
            return unauthorized!;
        }

        var request = await JsonSerializer.DeserializeAsync<UpdateCategoryRequest>(req.Body, JsonOptions);
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

        var updated = new CategoryResponse(categoryId, request.Name, request.Type, request.ParentCategoryId);
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(updated, JsonOptions);
        return response;
    }
}
