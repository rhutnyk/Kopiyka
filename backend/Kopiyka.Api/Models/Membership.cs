using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class Membership : AuditableEntity
{
    public Guid UserId { get; set; }

    public Guid HouseholdId { get; set; }

    [MaxLength(50)]
    public string Role { get; set; } = "member";

    public User? User { get; set; }

    public Household? Household { get; set; }
}
