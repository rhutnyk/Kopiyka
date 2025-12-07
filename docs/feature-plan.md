# Feature Plan (Draft)

This document captures the initial backlog for Kopiyka. Items are ordered by implementation phases; each bullet is a concrete, testable outcome.

## Phase 1 — Foundations
- **Auth & identity**
  - Azure Static Web Apps auth provider integration (email/OAuth) with JWT passed to Functions.
  - User profile table in Azure SQL (user id, display name, email, created/lastLogin timestamps).
  - Household membership model: roles = Owner, Editor, Viewer; membership rows scoped by household.
- **Households**
  - Create household (name, currency, start month, optional invite link token).
  - Invite flow: owner can generate shareable link and revoke it; joining requires auth + acceptance of role.
  - Household switcher in UI header, persisted in local storage.
- **Categories**
  - CRUD for income/expense categories with color + emoji/icon and optional parent category.
  - Category order per household to control UI display; soft-delete with “restore” option.
- **Accounts**
  - Support cash, checking, credit, savings accounts per household.
  - Balances tracked as derived from transactions (no manual edits except opening balance).
- **Transactions**
  - CRUD for dated transactions: account, amount, category, merchant/payee, memo, receipt URL.
  - Recurring templates: frequency (weekly, monthly, quarterly), start/end, next occurrence; “generate upcoming” job.
  - CSV import: map columns, preview, detect duplicates by date/amount/merchant.

## Phase 2 — Budgeting & envelopes
- **Budgets**
  - Monthly envelope allocations per category with optional rollover rules (all/none/cap).
  - Planned vs actual view for the month; quick adjust via inline edits.
  - Rollover job runs on month close and seeds next month’s envelopes.
- **Goals & sinking funds**
  - Goal entity with target amount/date; contributes to monthly funding suggestions.
  - Progress meter showing funded vs target across accounts tagged to the goal.
- **Notifications**
  - Email digest: weekly summary (spend vs budget, upcoming recurring items, goals progress).
  - Alert rules: transaction over threshold, budget burn rate alert (e.g., >80% used mid-month).

## Phase 3 — Forecasting & insights
- **Cash flow forecast**
  - Combine recurring templates + historical averages to project 3/6/12-month balances per account.
  - Scenario inputs: one-off planned expense/income, toggling specific recurring items.
- **Reports & exports**
  - Category trends chart (last 12 months), income vs expenses, account balance history.
  - Export transactions/budgets to CSV; audit trail of changes for compliance.
- **Collaboration & permissions**
  - Activity feed for household events (invite accepted, category added, budget adjusted).
  - Fine-grained permissions: Editors cannot delete accounts; Viewers read-only.

## Non-functional requirements
- **Performance**: Fast client loads via code-splitting; API P95 < 300ms for cached reads.
- **Reliability**: Idempotent Functions for imports and recurring generation; retries with poison queue for failures.
- **Security**: All endpoints require auth; role checks enforced per household; input validation across all DTOs.
- **Observability**: Structured logging + correlation ids; basic metrics (requests, errors) shipped to Application Insights.

## Upcoming technical tasks
- Define DB schema (users, households, memberships, accounts, categories, budgets, transactions, recurring, invites).
- Design API surface (Functions endpoints) mapped to the schema and feature phases.
- Add shared contracts package for request/response DTOs between front end and API.
- Introduce linting/formatting (ESLint + Prettier) and CI for build + tests.
- Add dev experience scripts: `npm run dev:frontend`, `dotnet watch` for Functions, and combined runner with `concurrently`.
