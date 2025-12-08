using System.Security.Cryptography;
using System.Text;
using Kopiyka.Api.Contracts.Auth;

namespace Kopiyka.Api.Security;

internal record InMemoryUser(Guid Id, string Email, string DisplayName, string PasswordHash, string Provider);

internal static class InMemoryIdentityStore
{
    private static readonly object SyncRoot = new();
    private static readonly List<InMemoryUser> Users =
    [
        new(
            Guid.Parse("00000000-0000-0000-0000-000000000100"),
            "demo@example.com",
            "Demo User",
            Hash("Password123!"),
            "password")
    ];

    private static readonly Dictionary<string, Guid> ActiveTokens = new();

    public static (InMemoryUser User, string Token) SignUp(string email, string password, string displayName)
    {
        lock (SyncRoot)
        {
            if (Users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("An account with that email already exists.");
            }

            var user = new InMemoryUser(Guid.NewGuid(), email.Trim(), displayName.Trim(), Hash(password), "password");
            Users.Add(user);
            return IssueToken(user);
        }
    }

    public static (InMemoryUser User, string Token) SignIn(string email, string password)
    {
        lock (SyncRoot)
        {
            var user = Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                throw new UnauthorizedAccessException("We couldn't find an account with that email.");
            }

            if (!string.Equals(user.PasswordHash, Hash(password), StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Incorrect password.");
            }

            return IssueToken(user);
        }
    }

    public static (InMemoryUser User, string Token) SignInWithGoogle(string email, string displayName)
    {
        lock (SyncRoot)
        {
            var user = Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                user = new InMemoryUser(Guid.NewGuid(), email.Trim(), displayName.Trim(), string.Empty, "google");
                Users.Add(user);
            }

            return IssueToken(user);
        }
    }

    public static bool TryGetUser(string token, out InMemoryUser? user)
    {
        lock (SyncRoot)
        {
            if (ActiveTokens.TryGetValue(token, out var userId))
            {
                user = Users.FirstOrDefault(u => u.Id == userId);
                return user != null;
            }
        }

        user = null;
        return false;
    }

    public static IReadOnlyList<MembershipSummary> GetMembershipsForUser(InMemoryUser user)
    {
        return new List<MembershipSummary>
        {
            new(user.Id, $"{user.DisplayName}'s Household", "owner"),
            new(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Shared Budget", "viewer")
        };
    }

    private static (InMemoryUser User, string Token) IssueToken(InMemoryUser user)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        ActiveTokens[token] = user.Id;
        return (user, token);
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
