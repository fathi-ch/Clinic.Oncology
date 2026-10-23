# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

Medical clinic management app (patients, visits/appointments, visit documents). Two independent parts:

- `src/BackEnd/` — ASP.NET Core Web API, **net8.0** (INSTALL.md still says .NET 6; it is out of date). Projects: `Clinic.Api` (host, controllers, business services) → `Clinic.Core` (models, DTOs, Dapper repositories, SQLite).
- `src/FrontEnd/Clinic.Web/` — Angular 16 SPA using DevExtreme. Not part of the .sln. Flat layout: there is no `src/app/`. `angular.json` uses `sourceRoot: ""`, so `main.ts`, `app.module.ts`, `views/`, `services/` and `assets/` all sit next to `package.json`.

There are no tests yet (`tests/` only holds a placeholder README).

## Commands

Backend (from repo root):

```bash
dotnet build Clinic.Oncology.sln
dotnet run --project src/BackEnd/Clinic.Api        # http://localhost:7017, Swagger at /swagger
dotnet publish src/BackEnd/Clinic.Api -c Release -o ./publish   # deployed to IIS per INSTALL.md
```

Ignore `src/BackEnd/Clinic.Api/Clinic.Api.sln` — a stale solution containing only the API project; use the root `Clinic.Oncology.sln`.

Frontend (from `src/FrontEnd/Clinic.Web`):

```bash
npm install
npm start          # ng serve --o, http://localhost:4200
npm run build
npm test           # Karma/Jasmine; single spec: npx ng test --include app.component.spec.ts
```

## Backend architecture

- **No EF, no migrations.** Schema lives in `Clinic.Core/Data/SqlLiteScripts/Database.sql` (`CREATE TABLE IF NOT EXISTS`), copied to output and executed on every startup by `DatabaseInitializer` (called from `Program.cs`). Schema changes = edit that script; existing DBs won't get altered columns.
- Data access is raw SQL via Dapper, built with `StringBuilder` in `Clinic.Core/Services/*Repository.cs`. Each call opens a fresh connection from `ISqliteDbConnectionFactory`. Inserts use `SELECT last_insert_rowid();`.
- `DatabaseInitializer` registers `SqLiteGuidTypeHandler` globally with Dapper.
- All services/repositories are registered as **singletons** in `Program.cs` — keep them stateless.
- Layering is inconsistent: `PatientsController` and `VisitsController` go through `BusinessService/` (`PatientService`, `VisitsService`), but `DocumentsController` calls `IDocumentRepository` directly, and some repositories depend on other repositories (`VisitRepository` → `DocumentRepository`, `PatientRepository`).
- Cascading deletes are done in code, not the DB: `PatientService.DeleteByIdAsync` deletes each visit's documents, then visits, then the patient.
- Mapping is hand-written extension methods in `Clinic.Core/Mappers` (`Models` ↔ `Dto` (input) ↔ `Contracts/*Response` (output)).
- Routes are `/v1/api/{patients|visits|documents}`. `GET /v1/api/patients/{id}/documents/export` builds a PDF with QuestPDF (Community license) in `PatientReportRepository`.

### Documents (files on disk)

- Uploaded as JSON with a base64 data-URL in `PatientDocumentDto.File64` (the part after the comma is decoded), not multipart.
- Saved by `FileRepository` to `{DocumentsPath}/{visitId}/{visitId}_{yyyyMMdd_HHmmss}_{ms}{ext}`; only the file name goes in the `Documents` table. Readers recover the folder from the prefix before the first `_` (`DatabaseConfigurations.GetFullSaveFolderPathForDocuments`), so don't change the naming scheme without updating both sides.
- Every document response re-reads the file and returns it base64-encoded (`PatientDocumentMapper`), so listing documents loads all files into memory.

### Configuration

`appsettings.json` → `DatabaseConfigurations` (`DatabasePath`, `DatabaseName`, `DocumentsPath`, `DatabaseConfigFilePath`). Defaults are Windows paths (`C:\Projects\Data`); on Linux override with env vars, e.g. `DatabaseConfigurations__DatabasePath=/some/dir DatabaseConfigurations__DocumentsPath=/some/dir/Documents DatabaseConfigurations__DatabaseConfigFilePath=Data/SqlLiteScripts/Database.sql`. CORS allows any origin.

## Frontend architecture

- NgModule-based (not standalone). All views are declared eagerly in `app.module.ts`; routes in `app-routing.module.ts` (`accueil`, `patients`, `patientsDetail`, `rdv`, `rdvDetails`). The per-view `*routing.module.ts` / `*.module.ts` files are mostly unused leftovers.
- UI language is French: `rdv` = appointment/visit, `piecejointe` = attachment, `accueil` = home. Note the existing misspellings used as real identifiers (`environmont/` folder, `Pateint` model, `patientsDatails/` folder) — match them, don't "fix" them piecemeal.
- API base URL comes from runtime config, not Angular environments: `AppConfig.load()` runs in `APP_INITIALIZER` and fetches `assets/config/config.dev.json` (dev, points to `http://localhost:7017/v1/api`) or `assets/config/config.json` (prod, `environment.production`). Services read `AppConfig.settings.apiServer.clnqApi`.
- Cross-component state (selected patient/visit, attachments list, spinner, popup flags) is shared through `BehaviorSubject`s in `services/ServiceCmnObject.ts`.
