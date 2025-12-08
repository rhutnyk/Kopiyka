namespace Kopiyka.Api.Contracts.Auth;

public record MembershipSummary(Guid HouseholdId, string HouseholdName, string Role);

public record CurrentUserProfile(Guid Id, string Email, string DisplayName, IReadOnlyList<MembershipSummary> Memberships);

public record SignUpRequest(string Email, string Password, string DisplayName);

public record SignInRequest(string Email, string Password);

public record GoogleSignInRequest(string Email, string DisplayName);

public record AuthSession(Guid UserId, string Email, string DisplayName, string AccessToken, IReadOnlyList<MembershipSummary> Memberships);
