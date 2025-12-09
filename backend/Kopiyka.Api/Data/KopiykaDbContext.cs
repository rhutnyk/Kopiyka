using Kopiyka.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Kopiyka.Api.Data;

public class KopiykaDbContext : DbContext
{
    public KopiykaDbContext(DbContextOptions<KopiykaDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<RecurringTemplate> RecurringTemplates => Set<RecurringTemplate>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Invite> Invites => Set<Invite>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        StampAuditFields();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureHouseholds(modelBuilder);
        ConfigureMemberships(modelBuilder);
        ConfigureAccounts(modelBuilder);
        ConfigureCategories(modelBuilder);
        ConfigureTransactions(modelBuilder);
        ConfigureRecurringTemplates(modelBuilder);
        ConfigureBudgets(modelBuilder);
        ConfigureInvites(modelBuilder);
    }

    private void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        });
    }

    private void ConfigureHouseholds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasIndex(h => h.Name);
        });
    }

    private void ConfigureMemberships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasIndex(m => new { m.HouseholdId, m.UserId }).IsUnique();

            entity.HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Household)
                .WithMany(h => h.Memberships)
                .HasForeignKey(m => m.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureAccounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(a => new { a.HouseholdId, a.Name }).IsUnique();

            entity.HasOne(a => a.Household)
                .WithMany(h => h.Accounts)
                .HasForeignKey(a => a.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(c => new { c.HouseholdId, c.Name }).IsUnique();
            entity.HasIndex(c => c.ParentCategoryId);

            entity.HasOne(c => c.Household)
                .WithMany(h => h.Categories)
                .HasForeignKey(c => c.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Subcategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureTransactions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasIndex(t => t.HouseholdId);
            entity.HasIndex(t => t.AccountId);
            entity.HasIndex(t => t.CategoryId);

            entity.HasOne(t => t.Household)
                .WithMany(h => h.Transactions)
                .HasForeignKey(t => t.HouseholdId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureRecurringTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecurringTemplate>(entity =>
        {
            entity.HasIndex(r => r.HouseholdId);
            entity.HasIndex(r => r.AccountId);
            entity.HasIndex(r => r.CategoryId);

            entity.HasOne(r => r.Household)
                .WithMany(h => h.RecurringTemplates)
                .HasForeignKey(r => r.HouseholdId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Account)
                .WithMany(a => a.RecurringTemplates)
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Category)
                .WithMany(c => c.RecurringTemplates)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureBudgets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasIndex(b => b.HouseholdId);
            entity.HasIndex(b => b.CategoryId);
            entity.HasIndex(b => new { b.HouseholdId, b.CategoryId, b.PeriodStart, b.PeriodEnd }).IsUnique();

            entity.HasOne(b => b.Household)
                .WithMany(h => h.Budgets)
                .HasForeignKey(b => b.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureInvites(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invite>(entity =>
        {
            entity.HasIndex(i => i.HouseholdId);
            entity.HasIndex(i => i.Token).IsUnique();
            entity.HasIndex(i => i.Email);

            entity.HasOne(i => i.Household)
                .WithMany(h => h.Invites)
                .HasForeignKey(i => i.HouseholdId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void StampAuditFields()
    {
        var utcNow = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
