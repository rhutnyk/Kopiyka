using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class Invite : AuditableEntity
{
    public Guid HouseholdId { get; set; }

    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public Household? Household { get; set; }
}
