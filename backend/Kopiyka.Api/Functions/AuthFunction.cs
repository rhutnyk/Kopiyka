using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Auth;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Functions;

public class AuthFunction
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Function("GetCurrentUser")]
    public async Task<HttpResponseData> GetCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/auth/me")] HttpRequestData req)
    {
        if (!AuthorizationHelper.TryEnsureRole(req, "viewer", out var unauthorized))
        {
            return unauthorized!;
        }

        var memberships = new List<MembershipSummary>
        {
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                "Demo Household",
                "owner"),
            new(
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                "Shared Budget",
                "viewer")
        };

        var profile = new CurrentUserProfile(
            Guid.Parse("00000000-0000-0000-0000-000000000100"),
            "demo@example.com",
            "Demo User",
            memberships);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(profile, JsonOptions);
        return response;
    }
}
