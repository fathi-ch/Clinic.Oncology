# Booking, Calendar, Nav Fixes and Visual Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bookings show on the calendar at the time they were booked, the booking form keeps end time after start time, the nav bar always highlights the current page, the patients page lists patients on open, then a production-quality visual pass.

**Architecture:** Part 1 fixes four verified bugs with small, local changes (one backend mapper, one new pure time helper, Angular `routerLinkActive` in place of manual DOM class toggling). Part 2 applies a design token set through `styles.css` and per-view SCSS, overriding DevExtreme's light theme rather than replacing it.

**Tech Stack:** Angular 16 (NgModule), DevExtreme 23.1.4, Karma/Jasmine, ASP.NET Core net8.0 + Dapper + SQLite.

**Spec:** User request in session, 2026-10-23. Verified findings below are the spec for Part 1. Part 2 tokens need user sign-off (see Decisions) before Tasks 5–7 start.

## Verified findings (reproduced in a browser on 2026-10-23)

- **F1 Bookings shift one hour earlier, first slot vanishes.** The form sends UTC (`"2026-10-27T07:00:00.000Z"` for 08:00 local). SQLite stores `2026-10-27 07:00:00` with no zone. The API returns `"2026-10-27T07:00:00"` with no `Z`, so the browser reads it as local 07:00. A booking at 08:00 renders at 07:00. A booking at 07:00 renders at 06:00, before the view's `startDayHour` of 7, so it never appears (API returned 3 visits for the week, scheduler rendered 2).
- **F2 End time ignores start time.** Changing start to 08:00 leaves end at 09:30, and the end list starts at 00:00.
- **F3 Nav highlight is wrong.** `/` shows nothing selected and a blank page. Browser Back leaves the previous tab highlighted. The hover "slider" renders at `left: 320px` while the hovered item is at 1040px. "Assitante" and "Parametres" have no route and do nothing on click.
- **F4 Patients page looks disconnected.** The list is empty until the user types, because `GetAllPateintList()` is commented out in `ngOnInit`. On open the page also calls `GET /v1/api/Patients/undefined/visits` (400) because `patientsDetail.component.ts:89` reacts to the initial empty `Pateint` in `ServiceCmnObject.patientDetail`.

## Global Constraints

- No new npm or NuGet dependencies. Fonts load from Google Fonts via `<link>` in `index.html`.
- DevExtreme stays at 23.1.4. Angular stays at 16.
- UI copy stays French.
- No database schema change. Existing visits in production DBs were stored as UTC clock time, and Task 2 must make them display correctly without a migration.
- Working hours for booking: 07:00 to 22:00, 15-minute steps (matches `dxi-view` `startDayHour`/`endDayHour` and the form's `[interval]="15"`).
- Work on branch `feature/booking-nav-fixes`, created from `feature/frontend-flatten`.

## Local run (used by verification steps)

```bash
mkdir -p /tmp/clinic-dev/data/Documents
cat > /tmp/clinic-dev/appsettings.json <<'EOF'
{ "AllowedHosts": "*", "DatabaseConfigurations": { "DatabasePath": "/tmp/clinic-dev/data", "DatabaseName": "Database.db", "DocumentsPath": "/tmp/clinic-dev/data/Documents", "DatabaseConfigFilePath": "Data/SqlLiteScripts/Database.sql" } }
EOF
dotnet build Clinic.Oncology.sln
cd src/BackEnd/Clinic.Api && DOTNET_ROLL_FORWARD=Major dotnet bin/Debug/net8.0/Clinic.Api.dll --contentRoot /tmp/clinic-dev --urls http://localhost:7017
# second shell
cd src/FrontEnd/Clinic.Web && npx ng serve
```

`--contentRoot` is needed because `Program.cs:12` re-adds `appsettings.json` after env vars, so env overrides are ignored. `DOTNET_ROLL_FORWARD=Major` is needed only on machines without the .NET 8 runtime.

Unit tests: `cd src/FrontEnd/Clinic.Web && CHROME_BIN=/usr/bin/google-chrome npx ng test --watch=false --browsers=ChromeHeadless`

## Review Focus

1. A booking at 07:00, the first slot, appears in the 07:00 row (the original invisible case). Pinned in Task 2 Step 5.
2. Opening an existing visit from the calendar keeps its own date and times in the form, not 07:00 on 1900-01-01. Pinned in Task 1 Step 7.
3. Start 21:45 gives end 22:00, never past the calendar end. Pinned in Task 1 Step 1.
4. Dragging a visit to another slot keeps the new time after "Recharger" (the update path goes through the same mapper). Pinned in Task 2 Step 5.
5. Loading or refreshing `/patients` directly highlights "Patients". Pinned in Task 3 Step 1.

## Decisions for the user

- **D5** Show times in the browser's own timezone and remove `timeZone="Africa/Algiers"` from the scheduler. The booking form already uses the browser's zone, so keeping both caused the mismatch seen in testing. In Algeria both are the same. Recommended.
- **D6** Hide "Assitante" and "Parametres" until those pages exist, instead of showing dead links. Recommended.
- **D7** Approve the Part 2 design tokens below before Tasks 5–7.

---

## Part 1: Fixes

### Task 1: Booking end time follows start time

**Files:**
- Create: `src/FrontEnd/Clinic.Web/views/rdv/booking-time.ts`
- Create: `src/FrontEnd/Clinic.Web/views/rdv/booking-time.spec.ts`
- Modify: `src/FrontEnd/Clinic.Web/views/rdv/rdv.component.ts` (`newPatientPopUp`, `onPopUpRdvOpen`, `onAppointmentFormOpening`, `newRdv`, `updateRdvForm`)
- Modify: `src/FrontEnd/Clinic.Web/views/rdv/rdv.component.html:135-144` (the `#startRdv` and `#endRdv` date boxes)
- Modify: `src/FrontEnd/Clinic.Web/app.component.spec.ts`

**Interfaces:**
- Produces: `SLOT_MINUTES = 15`, `withTimeOf(day: Date, time: Date): Date`, `endAfterStart(start: Date): Date`, `bookingBounds(day: Date): { startMin: Date; startMax: Date; endMax: Date }` exported from `booking-time.ts`.

- [ ] **Step 1: Make the existing suite runnable, then write the failing helper spec**

`app.component.spec.ts` fails today because `AppConfig.settings` is undefined and the template has unknown elements. In `beforeEach`, set `AppConfig.settings = { apiServer: { clnqApi: '' } } as IAppConfig`, add `schemas: [NO_ERRORS_SCHEMA]`, and delete the two Angular starter tests (`title` and `.content span`). Keep `should create the app`.

```ts
describe('booking-time', () => {
  const day = new Date(2026, 9, 27);
  it('withTimeOf puts the time on the booking day', () => {
    expect(withTimeOf(day, new Date(1900, 0, 1, 8, 30))).toEqual(new Date(2026, 9, 27, 8, 30));
  });
  it('endAfterStart adds one slot', () => {
    expect(endAfterStart(new Date(2026, 9, 27, 8, 0))).toEqual(new Date(2026, 9, 27, 8, 15));
  });
  it('endAfterStart from the last start slot ends at 22:00', () => {
    expect(endAfterStart(new Date(2026, 9, 27, 21, 45))).toEqual(new Date(2026, 9, 27, 22, 0));
  });
  it('bookingBounds covers working hours', () => {
    expect(bookingBounds(day)).toEqual({
      startMin: new Date(2026, 9, 27, 7, 0),
      startMax: new Date(2026, 9, 27, 21, 45),
      endMax: new Date(2026, 9, 27, 22, 0),
    });
  });
});
```

- [ ] **Step 2: Run tests, expect the helper spec to fail**

Run: unit test command with `--include views/rdv/booking-time.spec.ts`
Expected: FAIL, cannot find module `./booking-time`.

- [ ] **Step 3: Implement `booking-time.ts`**

Pure functions on local `Date`. No Angular imports.

- [ ] **Step 4: Run the full suite**

Expected: 5 specs, 0 failures (1 app + 4 helper).

- [ ] **Step 5: Wire the form**

In `rdv.component.ts` add `startMin`, `startMax`, `endMin`, `endMax: Date` properties and `onStartTimeChanged(e: { value: Date | null })`. On a non-null value set `endRdv.value = endAfterStart(e.value)` and `endMin = endAfterStart(e.value)`.

Every place that sets `startRdv.value` (`newPatientPopUp`, `onPopUpRdvOpen`, `onAppointmentFormOpening`, both branches) must first normalize start to the booking day with `withTimeOf`, then set the bounds from `bookingBounds(day)` and `endMin = endAfterStart(start)`. Replace the duplicated `setHours/setMinutes` blocks in `newRdv` and `updateRdvForm` with `withTimeOf(this.dateRdv.value, ...)`.

In `newRdv` and `updateRdvForm`, if end is not after start, call `notify("L'heure de fin doit être après l'heure de début.", 'error', 3000)` from `devextreme/ui/notify` and return without saving.

In the template bind `[min]="startMin" [max]="startMax" (onValueChanged)="onStartTimeChanged($event)"` on `#startRdv` and `[min]="endMin" [max]="endMax"` on `#endRdv`.

- [ ] **Step 6: Browser check of F2**

With the app running, double-click Tue 09:00, set start to 08:00.
Expected: end shows 08:15. The end list's first item is 08:15 and the last is 22:00. The start list runs 07:00 to 21:45.

- [ ] **Step 7: Browser check of Review Focus 2**

Click an existing visit on the calendar.
Expected: the form shows that visit's date, start and end, and the end list starts one slot after its start.

- [ ] **Step 8: Commit**

```bash
git add src/FrontEnd/Clinic.Web/views/rdv src/FrontEnd/Clinic.Web/app.component.spec.ts
git commit -m "fix(booking): end time follows start time and stays in working hours"
```

### Task 2: Bookings appear at the booked time

**Files:**
- Modify: `src/BackEnd/Clinic.Core/Mappers/VisitMapper.cs` (the three `Visit` → `VisitResponse` overloads)
- Modify: `src/FrontEnd/Clinic.Web/views/rdv/rdv.component.html:54` (remove `timeZone="Africa/Algiers"`, per D5)

**Interfaces:**
- Produces: API `startTime`/`endTime` on every visit response serialize with a trailing `Z`.

- [ ] **Step 1: Capture the failing behavior**

Run (API up, a patient with id 1 exists):
```bash
curl -s -X POST localhost:7017/v1/api/visits -H 'Content-Type: application/json' -d '{"patientId":1,"startTime":"2026-10-28T06:00:00.000Z","endTime":"2026-10-28T06:15:00.000Z","price":0,"status":"PLA","visitType":"RDV","description":"","weight":0,"height":0,"visitDateTitel":""}'
curl -s "localhost:7017/v1/api/visits/Sat%20Oct%2024%202026/Fri%20Oct%2030%202026" | grep -o '"startTime":"[^"]*"'
```
Expected now: `"startTime":"2026-10-28T06:00:00"` (no `Z`).

- [ ] **Step 2: Add `private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);` in `VisitMapper` and apply it to `StartTime` and `EndTime` in the three overloads that take `Visit`**

The `VisitDto` overloads already carry `Kind=Utc` from JSON binding and stay as they are.

- [ ] **Step 3: Rebuild, restart the API, rerun Step 1's GET**

Expected: `"startTime":"2026-10-28T06:00:00Z"`.

- [ ] **Step 4: Remove `timeZone="Africa/Algiers"` from `<dx-scheduler>`**

- [ ] **Step 5: Browser check of F1 and Review Focus 1 and 4**

Book 07:00 and 08:00 through the form on the same day.
Expected: they render in the 07:00 and 08:00 rows. The number of `.dx-scheduler-appointment` elements equals the number of visits the range endpoint returns. Drag the 08:00 visit to 10:00, click "Recharger".
Expected: it stays at 10:00.

- [ ] **Step 6: Commit**

```bash
git add src/BackEnd/Clinic.Core/Mappers/VisitMapper.cs src/FrontEnd/Clinic.Web/views/rdv/rdv.component.html
git commit -m "fix(visits): return visit times as UTC so the calendar shows them at the booked time"
```

### Task 3: Nav bar follows the router

**Files:**
- Modify: `src/FrontEnd/Clinic.Web/components/header/header.component.html`, `header.component.ts`, `header.component.scss`
- Modify: `src/FrontEnd/Clinic.Web/app-routing.module.ts`
- Modify: `src/FrontEnd/Clinic.Web/app.component.ts` (delete `lastElement`, `ngAfterViewInit` and `navigateTo`, which target `imgNavigation_*` ids that do not exist)
- Create: `src/FrontEnd/Clinic.Web/components/header/header.component.spec.ts`

- [ ] **Step 1: Write the failing spec**

```ts
// TestBed: declarations [HeaderComponent],
// imports [RouterTestingModule.withRoutes([{ path: 'accueil', children: [] }, { path: 'patients', children: [] }])]
it('highlights the link for the current route', fakeAsync(() => {
  router.navigateByUrl('/patients'); tick(); fixture.detectChanges();
  const active = fixture.nativeElement.querySelectorAll('a.active');
  expect(active.length).toBe(1);
  expect(active[0].textContent.trim()).toBe('Patients');
}));
it('moves the highlight on back navigation', fakeAsync(() => {
  router.navigateByUrl('/patients'); tick();
  router.navigateByUrl('/accueil'); tick(); fixture.detectChanges();
  expect(fixture.nativeElement.querySelector('a.active').textContent.trim()).toBe('Accueil');
}));
```

- [ ] **Step 2: Run it, expect FAIL** (the current header only sets classes on click).

- [ ] **Step 3: Replace manual highlighting**

Links become `<a routerLink="/accueil" routerLinkActive="active">`. Delete `setMenuIndex`, `navigateTo`, `lastElement` and the `ngOnInit` body from `header.component.ts`. Delete `li.slider` and all `nth-child` slider rules from the SCSS. Keep a single `a.active` underline rule. Remove the "Assitante" and "Parametres" items (D6). Add `{ path: '', redirectTo: 'accueil', pathMatch: 'full' }` and `{ path: '**', redirectTo: 'accueil' }` to `app-routing.module.ts`.

- [ ] **Step 4: Run the full suite, expect all pass**

- [ ] **Step 5: Browser check of F3 and Review Focus 5**

Open `/`: lands on `/accueil` with "Accueil" active. Open `/patients` directly and refresh: "Patients" active. Click Accueil, press Back: "Patients" active. Hover any item: no block appears away from the item.

- [ ] **Step 6: Commit**

```bash
git add src/FrontEnd/Clinic.Web/components/header src/FrontEnd/Clinic.Web/app-routing.module.ts src/FrontEnd/Clinic.Web/app.component.ts
git commit -m "fix(nav): drive the active tab from the router"
```

### Task 4: Patients page lists patients on open

**Files:**
- Modify: `src/FrontEnd/Clinic.Web/views/patients/patients.component.ts:74-78` and `searchPatient`
- Modify: `src/FrontEnd/Clinic.Web/views/patientsDatails/patientsDetail.component.ts:89`

- [ ] **Step 1: In `ngOnInit` call `this.GetAllPateintList()`. In `searchPatient`, replace `this.LstPatients = []` with `this.GetAllPateintList()`.**

- [ ] **Step 2: In the `patientDetail` subscription in `patientsDetail.component.ts`, return early when `pateint.id` is undefined.**

- [ ] **Step 3: Browser check of F4**

Open `/patients`.
Expected: the grid lists every patient from `GET /v1/api/patients`, the network log has no `undefined` request, and clearing the search box restores the full list.

- [ ] **Step 4: Commit**

```bash
git add src/FrontEnd/Clinic.Web/views/patients src/FrontEnd/Clinic.Web/views/patientsDatails
git commit -m "fix(patients): list all patients on open and skip the empty detail request"
```

---

## Part 2: Visual redesign (starts after D7)

**Subject:** the reception desk app of an Algerian medical practice. The doctor and assistant keep it open all day on a desktop. Its main job is to read the week's agenda and book or change a visit in a few seconds. Its second job is finding a patient.

**Tokens:**

| Token | Value | Use |
|---|---|---|
| `--ink` | `#2A2433` | text |
| `--paper` | `#F7F6F9` | app background, cool grey, not cream |
| `--panel` | `#FFFFFF` | calendar, grid, popups |
| `--line` | `#E3DFE9` | 1px separators |
| `--plum` | `#6E3A8C` | brand, active tab, current-time line, primary button. Darker than today's `#9b59b6` so white text on it passes WCAG AA |
| `--booked` | `#2F64A8` | planned visit (status `PLA`), replaces `#78b6d9` |
| `--done` | `#2E7D72` | completed visit, replaces `#60a69f` |

**Type:** Atkinson Hyperlegible Next, one family, weights 400 and 600, scale 13 / 15 / 18 / 24 px, 15px base for a dense desk UI. It was drawn for legibility at small sizes, which suits patient names and times read at a glance. Check that it has tabular figures for times. If not, keep it anyway; times are short enough that this matters little.

**Layout:**

```
┌───────────────────────────────────────────────────────────────┐
│ (D) Cabinet    Accueil  Patients                ven. 23 octobre │ 56px bar, white, 1px line
├───────────────────────────────────────────────────────────────┤
│ 24 – 30 octobre   ‹ ›  Aujourd'hui      Semaine | Jour   + Nouveau rdv │
│ ┌──────────── calendar, full width, fills remaining height ──┐ │
│ │ 07:00 │ ██ Benali S. 08:00–08:15                            │ │
│ └────────────────────────────────────────────────────────────┘ │
│ Avancement des visites (charts), below the fold               │
└───────────────────────────────────────────────────────────────┘
```

Left-aligned throughout. The calendar is the one memorable element: solid status-colored blocks with the patient name in 600 and the time range under it, and a plum current-time line. Everything around it stays quiet.

**Review against defaults:** a first draft wrapped each section in a shadowed rounded card, which is the generic SaaS card kit and what `box-shadow` on `#form-container` does today. Revised to flat white panels on `--paper` separated by `--line`. Grid headers stay sentence case, with no all-caps labels.

### Task 5: Tokens, font and DevExtreme overrides

**Files:** `src/FrontEnd/Clinic.Web/index.html`, `src/FrontEnd/Clinic.Web/styles.css`

- [ ] Add the Google Fonts `<link>` for Atkinson Hyperlegible Next 400 and 600. Define the tokens on `:root` in `styles.css`. Set `body` font, color and background. Override DevExtreme for `.dx-widget` font-family, `.dx-button` primary color, `.dx-scheduler-header-panel-current-time-cell`, `.dx-scheduler-date-time-indicator` and `.dx-datagrid-headers`.
- [ ] Verify: `npx ng build` passes the `anyComponentStyle` budget. Screenshot `/accueil` and `/patients` at 1440×900 and 768×1024. Body text computes to Atkinson Hyperlegible Next.
- [ ] Commit `style: add design tokens and font`.

### Task 6: Header and home page

**Files:** `components/header/*`, `main/accueil.component.html`, `main/accueil.component.scss`, `views/rdv/rdv.component.html`, `views/rdv/rdv.component.scss`

- [ ] Header per the layout: logo, product name "Cabinet" (D7 covers the copy), tabs, today's date on the right. Replace the purple toolbar above the calendar with one row: the scheduler's own navigator, view switcher and a primary "Nouveau rdv" button (the hidden `display:none` "Nouveau" button becomes this). Appointment template shows name and `HH:mm–HH:mm`, colored by `--booked`/`--done`.
- [ ] Verify: screenshots at both widths, visible keyboard focus on tabs and buttons, the Task 3 header spec still passes.
- [ ] Commit `style: redesign header and agenda`.

### Task 7: Patients page and booking popup

**Files:** `views/patients/patients.component.html`, `patients.component.scss`, `views/rdv/rdv.component.html` (popup), `views/rdv/rdv.component.scss`

- [ ] Search box and actions on one row above a flat grid. Fix the breadcrumb typo "Pateints" to "Patients". Popup fields on a 2-column grid with labels above inputs, and the primary action on the right ("Ajouter le rdv", "Enregistrer"). The empty grid shows "Aucun patient. Ajoutez-en un avec « Nouveau »."
- [ ] Verify: screenshots at both widths. The Task 1 browser checks still pass.
- [ ] Commit `style: redesign patients page and booking form`.
