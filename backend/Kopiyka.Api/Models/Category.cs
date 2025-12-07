using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class Category : AuditableEntity
{
    public Guid HouseholdId { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = "expense";

    public Household? Household { get; set; }

    public Category? ParentCategory { get; set; }

    public ICollection<Category> Subcategories { get; set; } = new List<Category>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<RecurringTemplate> RecurringTemplates { get; set; } = new List<RecurringTemplate>();

    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
