using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kopiyka.Api.Models;

public class Budget : AuditableEntity
{
    public Guid HouseholdId { get; set; }

    public Guid CategoryId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LimitAmount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public Household? Household { get; set; }

    public Category? Category { get; set; }
}
