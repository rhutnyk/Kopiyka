# Kopiyka

Family budget manager (work-in-progress) powered by React + Azure Functions.

## Repository layout
- `frontend/` — React + TypeScript app built with Vite. Intended for Azure Static Web Apps front end.
- `backend/Kopiyka.Api/` — Azure Functions (.NET 8 isolated worker). Can run locally with the Functions host.
- `docs/` — Architecture notes and feature plan.

## Getting started
### Prerequisites
- Node.js 18+ and npm
- .NET 8 SDK
- (Optional) Azure Functions Core Tools for local API hosting

### Quick start (from the repo root)
- `npm run dev:frontend` — start the Vite dev server for the React app.
- `npm run dev:api` — run the Azure Functions host with `dotnet watch`.
- `npm run dev` — run both services together using `concurrently`.

### Frontend
```bash
cd frontend
npm install
npm run dev
```

### Backend
```bash
cd backend/Kopiyka.Api
# Restore dependencies and run the Functions host
dotnet restore
dotnet run
```
The sample `HealthFunction` responds to `GET /api/v1/health`.

### Documentation
- Architecture overview: [`docs/architecture.md`](docs/architecture.md)
- Feature plan/backlog: [`docs/feature-plan.md`](docs/feature-plan.md)
- Contributor workflow and CI details: [`CONTRIBUTING.md`](CONTRIBUTING.md)

## Deployment target
- Azure Static Web Apps for front end + Functions API, using Azure SQL for data.
