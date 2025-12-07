using System.ComponentModel.DataAnnotations;

namespace Kopiyka.Api.Models;

public class Household : AuditableEntity
{
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Membership> Memberships { get; set; } = new List<Membership>();

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<RecurringTemplate> RecurringTemplates { get; set; } = new List<RecurringTemplate>();

    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public ICollection<Invite> Invites { get; set; } = new List<Invite>();
}
