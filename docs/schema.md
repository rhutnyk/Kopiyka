# Data schema overview

The API now uses Entity Framework Core to manage persistence. The initial migration creates the following entities and relationships:

- **Users**: global users with email + display name. Unique email, soft-delete + audit timestamps.
- **Households**: container for finances. Soft-delete + audit timestamps.
- **Memberships**: joins users to households with a role. Unique per (household, user).
- **Accounts**: financial accounts scoped to a household with type/currency. Unique name per household.
- **Categories**: income/expense categories per household with optional parent for hierarchy. Unique name per household.
- **Transactions**: monetary entries tied to a household, account, and optional category with description, amount, currency, timestamp.
- **RecurringTemplates**: recurring transaction plans linked to household, account, optional category with frequency and next occurrence.
- **Budgets**: spending limits per household/category for a date window.
- **Invites**: tokens emailed to future members, scoped to a household, with expiry/accept timestamps.

## Relationships

- A **Household** has many **Memberships**, **Accounts**, **Categories**, **Transactions**, **RecurringTemplates**, **Budgets**, and **Invites**.
- A **User** can have many **Memberships** (and therefore belong to many households).
- **Accounts** and **Categories** are required for **Transactions** and **RecurringTemplates** (categories optional, default to `SET NULL`).
- **Categories** can be nested via `ParentCategoryId`.
- **Budgets** link a household to a category for a specific period.
- **Invites** belong to a household and use unique tokens for acceptance.

## Indexing and scoping

- Household scoping indexes exist on `HouseholdId` for accounts, categories, transactions, recurring templates, budgets, memberships, and invites.
- Unique constraints include user emails, household/member pairs, household account names, household category names, and unique invite tokens.
- Additional indexes cover category parents, budget period uniqueness per category, and transaction/account/category lookups.

All entities include `CreatedAt`, `UpdatedAt`, and `IsDeleted` fields for auditing and soft-deletion support.
