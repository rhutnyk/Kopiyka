# Contributing to Kopiyka

Thanks for your interest in helping out! This guide outlines how to work with the project locally and what checks run in CI.

## Local development
- Install prerequisites: Node.js 18+, npm, and the .NET 8 SDK.
- Frontend: `cd frontend && npm install` to bootstrap dependencies.
- Backend: `cd backend/Kopiyka.Api && dotnet restore` to restore NuGet packages.

Common dev workflows are available from the repo root:

- `npm run dev:frontend` — start the Vite dev server for the React app.
- `npm run dev:api` — run the Azure Functions host with `dotnet watch`.
- `npm run dev` — run both frontend and API concurrently.

## Code quality
- Lint frontend code: `cd frontend && npm run lint`.
- Format frontend code: `cd frontend && npm run format` (uses Prettier).
- Build frontend assets: `cd frontend && npm run build`.
- Build backend: `cd backend/Kopiyka.Api && dotnet build --configuration Release`.
- Run backend tests (when present): `dotnet test` from the repo root or target a specific test project.

## Continuous Integration
GitHub Actions run on pushes and pull requests:
- **Frontend CI** installs dependencies, lints, and builds the React app.
- **Backend CI** restores and builds the Azure Functions project. Test projects are executed automatically when present.
