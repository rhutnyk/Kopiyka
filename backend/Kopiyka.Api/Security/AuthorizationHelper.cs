using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace Kopiyka.Api.Security;

internal static class AuthorizationHelper
{
    private static readonly string[] RoleHierarchy = ["viewer", "editor", "owner"];

    public static bool TryEnsureRole(HttpRequestData request, string requiredRole, out HttpResponseData? errorResponse)
    {
        var providedRole = GetRoleFromHeader(request);
        var providedRank = Array.IndexOf(RoleHierarchy, providedRole);
        var requiredRank = Array.IndexOf(RoleHierarchy, requiredRole);

        if (providedRank >= requiredRank && requiredRank >= 0)
        {
            errorResponse = null;
            return true;
        }

        errorResponse = request.CreateResponse(HttpStatusCode.Forbidden);
        errorResponse.WriteString($"Role '{requiredRole}' required for this operation.");
        return false;
    }

    private static string GetRoleFromHeader(HttpRequestData request)
    {
        if (request.Headers.TryGetValues("x-user-role", out var values))
        {
            var role = values.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(role))
            {
                role = role.ToLowerInvariant();
                if (RoleHierarchy.Contains(role))
                {
                    return role;
                }
            }
        }

        return "owner"; // Default for local development until auth is wired up.
    }
}
