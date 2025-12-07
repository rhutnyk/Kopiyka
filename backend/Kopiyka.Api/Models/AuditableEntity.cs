using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public abstract class AuditableEntity
{
    [Key]
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
}
