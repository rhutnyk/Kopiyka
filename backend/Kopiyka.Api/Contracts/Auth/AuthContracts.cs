namespace Kopiyka.Api.Contracts.Auth;

public record MembershipSummary(Guid HouseholdId, string HouseholdName, string Role);

public record CurrentUserProfile(Guid Id, string Email, string DisplayName, IReadOnlyList<MembershipSummary> Memberships);
