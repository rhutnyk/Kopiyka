# API surface (draft)

All functions are hosted under the Azure Functions isolated worker. Routes are versioned under `/api/v1/...` so Static Web Apps can proxy them directly from the front end.

## Health
- **GET `/api/v1/health`**
  - Response: `{ "status": "ok", "timestamp": "2024-01-01T00:00:00Z", "version": "v1" }`

## Auth metadata
- **GET `/api/v1/auth/me`**
  - Returns the signed-in user profile and household memberships used by the front end to bootstrap state.
  - Response example:
    ```json
    {
      "id": "00000000-0000-0000-0000-000000000100",
      "email": "demo@example.com",
      "displayName": "Demo User",
      "memberships": [
        {
          "householdId": "00000000-0000-0000-0000-000000000001",
          "householdName": "Demo Household",
          "role": "owner"
        }
      ]
    }
    ```

## Households
- **GET `/api/v1/households`** — list households the user can view (requires `viewer` role or higher).
- **POST `/api/v1/households`** — create a new household (requires `owner`).
  - Request:
    ```json
    { "name": "My Household", "defaultCurrency": "USD" }
    ```
- **GET `/api/v1/households/{householdId}`** — get details (requires `viewer`).
- **PUT `/api/v1/households/{householdId}`** — rename a household (requires `owner`).
  - Request:
    ```json
    { "name": "Renamed Household" }
    ```
  - Response objects include `{ id, name, defaultCurrency, createdAt }`.

## Accounts
- **GET `/api/v1/households/{householdId}/accounts`** — list accounts (requires `viewer`).
- **POST `/api/v1/households/{householdId}/accounts`** — add an account (requires `editor`).
  - Request:
    ```json
    { "name": "Checking", "type": "cash", "currency": "USD" }
    ```
- **PUT `/api/v1/households/{householdId}/accounts/{accountId}`** — update name/type (requires `editor`).
  - Request:
    ```json
    { "name": "Joint Checking", "type": "cash" }
    ```
  - Responses return `{ id, name, type, currency, balance }`.

## Categories
- **GET `/api/v1/households/{householdId}/categories`** — list categories (requires `viewer`).
- **POST `/api/v1/households/{householdId}/categories`** — create a category (requires `editor`).
  - Request:
    ```json
    { "name": "Groceries", "type": "expense", "parentCategoryId": null }
    ```
- **PUT `/api/v1/households/{householdId}/categories/{categoryId}`** — update a category (requires `editor`).
  - Request mirrors the create payload.
  - Responses return `{ id, name, type, parentCategoryId }`.

## Transactions
- **GET `/api/v1/households/{householdId}/transactions`** — list recent transactions (requires `viewer`).
- **POST `/api/v1/households/{householdId}/transactions`** — create a transaction (requires `editor`).
  - Request:
    ```json
    {
      "accountId": "00000000-0000-0000-0000-000000000010",
      "categoryId": "00000000-0000-0000-0000-000000000021",
      "description": "Groceries",
      "amount": 120.55,
      "currency": "USD",
      "occurredAt": "2024-01-10T18:25:43Z"
    }
    ```
- **PUT `/api/v1/households/{householdId}/transactions/{transactionId}`** — edit a transaction (requires `editor`).
  - Request follows the create shape without the account id.
  - Responses return `{ id, accountId, categoryId, description, amount, currency, occurredAt }`.

## Authorization model
- Role hierarchy: `owner > editor > viewer`. Endpoints use the `x-user-role` header in local development to simulate access (defaults to `owner`). When real auth is connected, the Functions middleware should populate the role and membership context to enforce these checks.
- Validation hooks on each request payload produce `400` responses with an `errors` array when fields are missing or malformed.
