# Architecture Overview

## Goals
- Keep hosting costs at or near zero by using Azure Static Web Apps + Azure Functions on the free tier.
- Maintain a clean separation between React UI and the Azure Functions API while keeping a single repository for easier CI/CD.
- Prefer managed services (Azure SQL, Static Web Apps-managed Functions, managed identity) to reduce ops overhead.

## High-level layout
- `frontend/`: React + TypeScript, built with Vite. Deployed as the Static Web App front end.
- `backend/`: Azure Functions (.NET 8 isolated worker) for API endpoints. Can be deployed as the Static Web App API or as a standalone Functions App if needed.
- `docs/`: Living documentation (architecture notes, feature plans).

## Hosting/deployment plan
- **Azure Static Web Apps (SWA)** will serve the React build output and host the Functions API in the `/api` route space.
  - Build steps: `npm install` + `npm run build` for the front end; `dotnet build` for the API project.
  - Routing: front end handles client-side routing; SWA fallback to `index.html` except for `/api/*`.
- **Azure Functions** in .NET 8 isolated mode
  - Local development: `func start` with the Functions Core Tools, or `dotnet run` inside the API project.
  - Deployment: SWA workflow publishes the compiled API to Azure Functions automatically; separate Functions App remains an option if we outgrow SWA limits.

## Data/storage (initial assumptions)
- **Primary DB:** Azure SQL Database free/Developer tier for relational data (households, users, categories, transactions, budgets, forecasts).
- **Secrets/config:** Local development via `local.settings.json` (Functions) and `.env` (front end). In Azure, prefer SWA-managed secrets or Key Vault via managed identity.
- **Files/receipts:** Azure Storage blob container (or Supabase/S3-compatible) once we need uploads.

## API shape (placeholder)
- All APIs will be versioned under `/api/v1/...` to allow future-breaking changes.
- Authentication: Azure Static Web Apps built-in auth providers or Azure AD B2C; JWT passed to Functions for authorization.
- Validation: Minimal APIs inside Functions with request DTOs + FluentValidation (or DataAnnotations) to keep contracts consistent.

## Environments
- **Local:** Vite dev server + Functions host (`frontend` and `backend/Kopiyka.Api`).
- **Preview:** SWA preview environments from PRs.
- **Production:** SWA production slot with Azure SQL/Storage; DB migrations run via GitHub Actions step before deployment.

## Immediate next steps
1. Define API surface (auth, households, categories, transactions, budgets, forecasts) and map to Functions endpoints.
2. Configure local dev scripts: concurrently run Vite dev server and Functions host; stub env files.
3. Add CI (lint, build, format) and deployment workflow for SWA.
