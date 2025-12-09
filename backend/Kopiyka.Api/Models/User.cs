using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class User : AuditableEntity
{
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
}
