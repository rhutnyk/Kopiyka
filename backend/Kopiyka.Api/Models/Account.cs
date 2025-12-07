using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class Account : AuditableEntity
{
    public Guid HouseholdId { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "checking";

    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public Household? Household { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<RecurringTemplate> RecurringTemplates { get; set; } = new List<RecurringTemplate>();
}
