using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Kopiyka.Api.Contracts.Auth;
using Kopiyka.Api.Data;
using Kopiyka.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;

namespace Kopiyka.Api.Functions;

public class AuthFunction
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConcurrentDictionary<string, Guid> ActiveTokens = new();

    private readonly KopiykaDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthFunction(KopiykaDbContext dbContext, IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [Function("SignUp")]
    public async Task<HttpResponseData> SignUp(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/auth/sign-up")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<SignUpRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password)
            || string.IsNullOrWhiteSpace(payload.DisplayName))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email, password, and display name are required.");
        }

        try
        {
            var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
            var displayName = payload.DisplayName.Trim();

            if (await _dbContext.Users.AsNoTracking().AnyAsync(u => u.Email == normalizedEmail))
            {
                return CreateErrorResponse(req, HttpStatusCode.Conflict, "An account with that email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = displayName
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, payload.Password);

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var token = IssueToken(user.Id);
            return CreateSessionResponse(req, user, token, HttpStatusCode.Created);
        }
        catch (InvalidOperationException ex)
        {
            return CreateErrorResponse(req, HttpStatusCode.Conflict, ex.Message);
        }
    }

    [Function("SignIn")]
    public async Task<HttpResponseData> SignIn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/auth/sign-in")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<SignInRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Password))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "Email and password are required.");
        }

        var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user is null)
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "We couldn't find an account with that email.");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "This account uses Google sign-in.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, payload.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Incorrect password.");
        }

        var token = IssueToken(user.Id);
        return CreateSessionResponse(req, user, token);
    }

    [Function("GoogleSignIn")]
    public async Task<HttpResponseData> GoogleSignIn(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/auth/google")] HttpRequestData req)
    {
        var payload = await JsonSerializer.DeserializeAsync<GoogleSignInRequest>(req.Body, SerializerOptions);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Email))
        {
            return CreateErrorResponse(req, HttpStatusCode.BadRequest, "A Google email address is required.");
        }

        var displayName = string.IsNullOrWhiteSpace(payload.DisplayName)
            ? payload.Email.Split('@')[0]
            : payload.DisplayName;

        var normalizedEmail = payload.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                DisplayName = displayName,
                PasswordHash = string.Empty
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        var token = IssueToken(user.Id);
        return CreateSessionResponse(req, user, token);
    }

    [Function("GetCurrentUser")]
    public async Task<HttpResponseData> GetCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/auth/me")] HttpRequestData req)
    {
        if (!TryReadBearer(req, out var token))
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Missing access token.");
        }

        if (!ActiveTokens.TryGetValue(token, out var userId))
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid or expired session.");
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return CreateErrorResponse(req, HttpStatusCode.Unauthorized, "Invalid or expired session.");
        }

        var profile = new CurrentUserProfile(user.Id, user.Email, user.DisplayName, Array.Empty<MembershipSummary>());

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(JsonSerializer.Serialize(profile, SerializerOptions));
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
        User user,
        string token,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var session = new AuthSession(user.Id, user.Email, user.DisplayName, token, Array.Empty<MembershipSummary>());

        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");
        response.WriteString(JsonSerializer.Serialize(session, SerializerOptions));
        return response;
    }

    private static string IssueToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        ActiveTokens[token] = userId;
        return token;
    }
}
