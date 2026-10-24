# Income statistics, user roles, finished-visit fix, printing and local deployment

> **For agentic workers:** REQUIRED SUB-SKILL: superpowers:executing-plans. Tasks run in order; each ends with tests, a browser check and a commit.

**Goal:** the doctor sees income per day, month, year and per patient; only the doctor (admin) can open statistics; a finished visit cannot be restarted; the patient file can be printed; the app runs on one clinic machine for at least two people.

**Architecture:** income is computed by one new API endpoint from the price already stored on each visit. Login uses ASP.NET Core cookie authentication (part of the framework, no new package) with two roles, Admin and Assistant; the API enforces the roles and the Angular app only hides what a role cannot use. For deployment the API also serves the built Angular app, so there is one process, one address and no cross-origin cookie problems.

**Tech stack:** ASP.NET Core 8, Dapper, SQLite, QuestPDF, Angular 16, DevExtreme 23.1.

## Findings that shape the plan

- **F5 "Prochain RDV" is the latest visit, not the next one** (found 2026-10-24). The API sets `Patients.NextAppointment` to the patient's latest visit date, finished ones included, in `VisitRepository.CreateAsync` and `VisitsService` (update and delete). It should be the earliest planned visit from now on, or empty. To fix with Task 2d's backend step, computed in one place and covered by a unit test.
- **F6 Creating a visit returns `id: 0`** instead of the new id; prescriptions (Task 4b) will need the real id.

- **F1 Finished visits come back to life.** The booking popup offers "Démarer" for a finished visit, and "Modifier" always saves `status = "PLA"` (`rdv.component.ts`, `updateRdvForm`), so editing a finished visit silently reopens it.
- **F2 Income data exists.** Every visit has `Price` (DA), entered in the booking form. Only one visit has a price today.
- **F3 No authentication at all.** Every API endpoint is open to anyone on the network.
- **F4 Deployment blockers.** `Program.cs` re-adds `appsettings.json` after environment variables (no per-machine config), web fonts load from Google (fail without internet), and the app needs the .NET 8 runtime installed.

## Decisions (defaults in bold; say if you want otherwise)

- **D14 What counts as income:** **the price of finished visits only**; planned visits show separately as "prévu".
- **D15 Creating accounts:** **a command run once at installation** (`Clinic.Api create-user --role admin ...`), plus an admin page to add the assistant and reset passwords. No self sign-up.
- **D16 What the assistant can do:** **everything except statistics and user management** (agenda, patients, visits, documents, PDF export).
- **D17 Network:** **HTTP on the clinic network only**, with the server's firewall closed to the internet. HTTPS with a self-signed certificate is possible but every browser shows a warning first.
- **D19 Prescriptions belong to a visit:** **yes**, written from the visit details (a prescription without a visit would need its own place in the app).
- **D20 Who writes prescriptions:** **the doctor (Admin) only**; the assistant views and prints.
- **D21 DevExtreme licence:** DevExtreme is commercial; from 24.2 each install needs a licence key or pages show an evaluation banner. **Use the clinic's DevExtreme licence key** if there is one; otherwise stay on 23.1 (the last version without the key check) or plan a move to a free component library, which is a much larger change.
- **D22 .NET version:** **move to .NET 10 (LTS)**; .NET 8 support ends in November 2026.
- **D18 Server machine:** **a small Windows or Linux PC that stays on** (mini PC or Raspberry Pi 5 with SSD), as discussed earlier.

## Task 1: A finished visit cannot be restarted (F1)

- Booking popup: for a finished visit show "Voir la visite" instead of "Démarer", hide "Modifier".
- `updateRdvForm` keeps the visit's current status instead of forcing `PLA`.
- Visit details open read-only for a finished visit: notes not editable, no upload or delete, no "Enregistrer"/"Terminer"; the gallery, viewer and PDF export stay.
- API: `PUT /visits/{id}` refuses to change a finished visit (409 "Cette visite est terminée.").
- **Check:** finish a visit, reopen it from the calendar and the patient page: read-only, no way to restart; a PUT on it returns 409.

## Task 1 status: done

Finished visits are read-only in the UI and refused by the API (409); "Modifier" no longer resets the status or erases the notes and visit type.

## Task 2a: Billing type chosen at booking (added 2026-10-24)

- Each visit gets `BillingType`: `PAID` (Payant), `PROMO` (Promotion, reduced price) or `FREE` (Gratuit, price forced to 0 by the API).
- Existing databases get the column on startup (`ALTER TABLE ... ADD COLUMN ... DEFAULT 'PAID'`), so every existing visit counts as paid.
- Booking forms show the choice next to the price; the calendar block marks free and promoted visits.
- Choosing the billing type becomes admin-only in Task 3; until then everyone sees it.
- **Check:** a free booking is stored with price 0 even if a price was typed; old visits read back as `PAID`.

## Task 2b: Book from the patient page (added 2026-10-24)

- "Nouveau rendez-vous" on the patient page opens the booking form for that patient (same free-slot lists and billing choice), without going through the calendar.
- **Check:** book from the patient page, the visit appears in the timeline and on the calendar.

## Task 2: Income statistics (D14) - done 2026-10-24

- Totals also split by billing type: paid income, promoted income, and the number of free visits.

- API `GET /v1/api/statistics/income?from=&to=` returns totals by day, by month, by year and by patient for finished visits, plus the planned total, all in DA. Day boundaries in the clinic's local time.
- Statistiques page: period picker (this month by default, presets: today, this week, this month, this year), four total badges (période, aujourd'hui, ce mois, cette année), a bar chart by day or month, a table "Revenus par patient" sorted by amount with visit count, each row linking to the patient page.
- Keep the existing "Avancement des visites" charts below.
- **Check:** unit test for the grouping (a visit at 23:30 local belongs to its own day); API totals match a hand count on test data; page renders with 0 DA when nothing is finished.

## Task 2c: Patients table as a DevExtreme master-detail grid (added 2026-10-24) - done

Following DevExtreme's "Advanced Master-Detail View" demo.

- The patients list becomes a regular `dx-data-grid` with real columns instead of the custom row template: sorting on every column (click a header, Shift-click for several), the grid's own search panel replacing the separate search box, column resizing and a row count.
- Expanding a row shows the patient's detail inside the grid: a tab "Visites" (date, status, billing, price) and a tab "Documents" (thumbnails), with "Ouvrir le dossier" going to the patient page.
- Search covers nom and prénom (today only the first name is searched).
- **Check:** sort by each column, search by last name, expand a row and see its visits and documents, open the patient page from it.

## Task 2d: Upgrade every package, backend first, then frontend (added 2026-10-24)

Done before Task 3 so accounts, settings, prescriptions and deployment are built on current versions. One commit per step, each with all tests and both builds green; if a step breaks, it is fixed or reverted before the next one.

**Backend (versions checked 2026-10-24):**
1. Target `net10.0` (current LTS) in every project; the clinic machine then needs no roll-forward setting.
2. Replace the preview `Microsoft.AspNetCore.Http.Features 6.0.0-preview.4` and the `C:\Program Files\dotnet\...` DLL references in `Clinic.Core.csproj` with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`.
3. Dapper 2.0.123 -> 2.1.89, Microsoft.Data.Sqlite 6.0.0 -> 10.0.13, QuestPDF 2024.3.6 -> 2026.9.1 (re-check the Community licence terms and re-export a patient file to compare), Newtonsoft.Json 13.0.3 -> 13.0.4 or removed if nothing uses it, Swashbuckle 6.2.3 -> 10.3.1.
4. Tests: Microsoft.NET.Test.Sdk 17.14.1 -> 18.11.0, xunit 2 -> xunit.v3 with xunit.runner.visualstudio 4.0.1.
- **Check:** `dotnet test`, the API starts on a copy of the database, every page loads, a patient PDF export looks the same.

**Frontend:**
1. Delete the unused `*.module.ts` files and `@angular/flex-layout` (abandoned at 15.0.0-beta.42); remove the inert `fx*` attributes.
2. Angular 16 -> 22 with `ng update`, one major at a time (17, 18, 19, 20, 21, 22): build, unit tests and a browser pass after each. TypeScript, zone.js and rxjs follow what each Angular version requires (not TypeScript 7.0, which Angular may not support yet).
3. When the Angular version in use drops Karma, move the 21 unit tests to the runner it ships with (Vitest).
4. DevExtreme 23.1.4 -> 26.1.6 (D21), then re-run `themes/recolor-dx-accent.mjs` and re-check the calendar, grid, gallery, popups and charts against screenshots taken before the upgrade.
- **Check:** all unit tests, production build, and the main flows in the browser: book from the calendar, the patient page and the grid; finish a visit; upload and view photos; export a PDF; edit a patient; statistics.

## Task 3: Accounts and roles (F3, D15, D16)

- SQLite table `Users (Id, UserName, PasswordHash, Role, DisplayName)` added to `Database.sql` (`CREATE TABLE IF NOT EXISTS`).
- Passwords hashed with PBKDF2 from the .NET standard library (`Rfc2898DeriveBytes.Pbkdf2`, SHA-256, random salt, 100 000 iterations). No new package.
- Cookie authentication: `POST /auth/login`, `POST /auth/logout`, `GET /auth/me`; HttpOnly, SameSite=Strict cookie, 12-hour sliding session; 5 failed logins lock the account for 5 minutes.
- Every API endpoint requires a signed-in user; statistics and user management require Admin.
- CLI: `Clinic.Api create-user --username dr.amrane --role Admin --name "Dr Amrane L."` (asks for the password).
- Angular: login page, route guard, the Statistiques tab and route only for Admin, the user's name and "Se déconnecter" in the header, an "Utilisateurs" admin page (add assistant, reset password).
- Dev: Angular dev-server proxy for `/v1/api` so the cookie is same-origin in development as in production.
- **Check:** API tests in a new `Clinic.Api.Tests` project (xUnit + WebApplicationFactory): anonymous gets 401, assistant gets 403 on statistics, admin gets 200, lockout after 5 failures; browser: assistant never sees Statistiques.

## Task 3b: Clinic settings panel for the admin (added 2026-10-24)

- **Settings:** practice name, doctor's name and specialty (French and Arabic), address, phone, opening hours, default consultation price (DA), Google Maps link.
- **Storage:** a `Settings` table (one row per key) seeded on first start with today's values from `clinic-info.ts` and `ClinicLetterhead.cs`; those two files stop holding the details.
- **API:** `GET /v1/api/settings` for any signed-in user, `PUT /v1/api/settings` for the admin only, with validation (phone digits, positive price, Maps link must be an https Google Maps address).
- **Frontend:** an admin-only "Paramètres" page with the form and a live preview of the header; header, footer, contact page and "Voir sur la carte" read the settings; the booking forms prefill the price with the default consultation price for "Payant".
- **PDF:** the letterhead reads the same settings, so a change shows on the next export.
- **Check:** change the phone number as admin, it updates in the header, contact page and the next PDF; the assistant gets 403 on PUT and does not see the page.

## Task 4: Printing

- "Imprimer le dossier" next to "Exporter le dossier PDF": opens the same PDF in a new browser tab, where the browser's own print button prints it. No separate print layout.
- **Check:** the tab opens with the PDF for the right patient.

## Task 4b: Prescriptions (ordonnances) (added 2026-10-24)

A patient has any number of prescriptions; each one is written during a visit, printed on the clinic letterhead, and kept with the patient's documents.

- **Data:** table `Prescriptions (Id, VisitId, CreatedAt, Notes)` and `PrescriptionLines (Id, PrescriptionId, Position, Medication, Dosage, Duration, Instructions)`. A prescription belongs to a visit (D19), so it shows up in that visit and in the patient's file.
- **Writing it:** an "Ordonnance" section in the visit details: add, reorder or remove lines (médicament, posologie, durée, consignes), with suggestions from medications already prescribed before.
- **Output:** a PDF ordonnance generated by QuestPDF on A5, with the drawn letterhead, the patient's name and age, the date, the numbered lines and a signature area. On "Enregistrer l'ordonnance" the PDF is saved as a document of the visit, so it appears in the visit gallery, the patient page "Tous les documents" and the exported patient file.
- **Printing and image:** "Imprimer" opens the PDF in a new tab (browser print, as in Task 4). A PNG copy is not generated unless asked: the PDF prints and shares the same way and stays sharp.
- **Who:** only the doctor (Admin) writes and signs prescriptions (D20); the assistant can view and print them.
- **Corrections:** a saved prescription is not edited in place; "Nouvelle version" creates a new one and marks the old one replaced, so the printed history stays true.
- **Check:** unit test for line numbering and the A5 layout fitting 8 lines on one page; create two prescriptions for one visit, both appear as PDFs in the patient documents and open correctly.

## Task 5: Local deployment for the clinic (F4, D17, D18)

- Replace the `C:\Program Files\dotnet\...` DLL references in `Clinic.Core.csproj` with `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (they only resolve on a Windows machine with that exact runtime).
- Remove the duplicate `AddJsonFile` in `Program.cs` so each machine can set its data folder by environment variables or `appsettings.Production.json`.
- The API serves the Angular build (`wwwroot`) with a fallback to `index.html`; the frontend calls `/v1/api` on its own address.
- Fonts served from the app itself (download the Exo, Roboto and Noto Naskh Arabic files into `assets/fonts`), so it works without internet.
- Publish as one self-contained folder (no .NET install on the clinic machine) for `win-x64` and `linux-arm64`.
- Run as a service that starts with the machine: a Windows service, or a systemd unit on Linux; listens on port 5000 of the local network.
- Nightly backup of `Database.db` and the `Documents` folder (scheduled task / cron) to a USB disk, keeping 30 days.
- `INSTALL.md` rewritten: install, create the two accounts, open `http://<machine-name>:5000` from the second PC, restore from backup.
- **Check:** publish, run the published folder on this machine with production settings, sign in from a second browser profile as the assistant while the admin is signed in.

## Review focus

1. Two people booking the same slot at the same time: the API overlap check (already in place) must still refuse the second one after auth is added.
2. Cookie expiry in the middle of a visit: the next save shows "Session expirée, reconnectez-vous" and returns to login instead of losing the notes silently.
3. Price left empty or 0: counts as 0 DA, never breaks totals.
4. A finished visit's documents stay viewable and exportable for both roles.
5. Restoring a backup on a fresh machine brings back patients, visits and photos.
