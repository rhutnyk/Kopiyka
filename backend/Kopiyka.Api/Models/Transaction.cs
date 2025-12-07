using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kopiyka.Api.Models;

public class Transaction : AuditableEntity
{
    public Guid HouseholdId { get; set; }

    public Guid AccountId { get; set; }

    public Guid? CategoryId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTimeOffset TransactedAt { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Household? Household { get; set; }

    public Account? Account { get; set; }

    public Category? Category { get; set; }
}
