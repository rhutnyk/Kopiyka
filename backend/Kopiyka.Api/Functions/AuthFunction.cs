using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Auth;
using Kopiyka.Api.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Functions;

public class AuthFunction
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    [Function("SignUp")]
    public async Task<HttpResponseData> SignUp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/auth/sign-up")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<SignUpRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password)
            || string.IsNullOrWhiteSpace(payload.DisplayName))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email, password, and display name are required.");
        }

        try
        {
            var (user, token) = InMemoryIdentityStore.SignUp(payload.Email, payload.Password, payload.DisplayName);
            return CreateSessionResponse(req, user, token, HttpStatusCode.Created);
        }
        catch (InvalidOperationException ex)
        {
            return CreateErrorResponse(req, HttpStatusCode.Conflict, ex.Message);
        }
    }

    [Function("SignIn")]
    public async Task<HttpResponseData> SignIn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/auth/sign-in")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<SignInRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email and password are required.");
        }

        try
        {
            var (user, token) = InMemoryIdentityStore.SignIn(payload.Email, payload.Password);
            return CreateSessionResponse(req, user, token);
        }
        catch (UnauthorizedAccessException ex)
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, ex.Message);
        }
    }

    [Function("GoogleSignIn")]
    public async Task<HttpResponseData> GoogleSignIn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/v1/auth/google")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<GoogleSignInRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "A Google email address is required.");
        }

        var displayName = string.IsNullOrWhiteSpace(payload.DisplayName)
            ? payload.Email.Split('@')[0]
            : payload.DisplayName;

        var (user, token) = InMemoryIdentityStore.SignInWithGoogle(payload.Email, displayName);
        return CreateSessionResponse(req, user, token);
    }

    [Function("GetCurrentUser")]
    public async Task<HttpResponseData> GetCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/v1/auth/me")] HttpRequestData req)
    {
        if (!TryReadBearer(req, out var token))
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Missing access token.");
        }

        if (!InMemoryIdentityStore.TryGetUser(token, out var user) || user is null)
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid or expired session.");
        }

        var profile = new CurrentUserProfile(
            user.Id,
            user.Email,
            user.DisplayName,
            InMemoryIdentityStore.GetMembershipsForUser(user));

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(profile, SerializerOptions);
        return response;
    }

    private static bool TryReadBearer(HttpRequestData req, out string token)
    {
        token = string.Empty;

        if (!req.Headers.TryGetValues("Authorization", out var values))
        {
            return false;
        }

        var header = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(header))
        {
            return false;
        }

        const string prefix = "Bearer ";
        if (!header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = header[prefix.Length..].Trim();
        return !string.IsNullOrWhiteSpace(token);
    }

    private static HttpResponseData CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(JsonSerializer.Serialize(new { error = message }, SerializerOptions));
        return response;
    }

    private static HttpResponseData CreateSessionResponse(
        HttpRequestData req,
        InMemoryUser user,
        string token,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var memberships = InMemoryIdentityStore.GetMembershipsForUser(user);
        var session = new AuthSession(user.Id, user.Email, user.DisplayName, token, memberships);

        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(JsonSerializer.Serialize(session, SerializerOptions));
        return response;
    }
}
