# playrighttest — Next.js vs Vue3 Cross-App Comparison Suite

Playwright test framework that runs the **same user flows** against both the **Next.js (React)** and **Vue3** client portals simultaneously and verifies three dimensions of parity:

| Dimension | Mechanism |
|---|---|
| **功能一致性** (Functional) | Playwright `expect` assertions on UI elements, navigation, modals, form validation |
| **数据一致性** (Data) | Intercept every API response → deep-diff two JSON payloads → fail on semantic mismatches |
| **视觉一致性** (Visual) | Key-region `toHaveScreenshot` baselines per project; dynamic values masked |

---

## Project layout

```
playrighttest/
├── playwright.config.ts   # Two projects: "next" and "vue"
├── .env                   # Runtime config (copy from .env.example)
├── fixtures/
│   ├── auth.setup.ts      # Login once, save storageState for both apps
│   └── index.ts           # Extended fixtures: appName, capture, visual
├── helpers/
│   ├── api-capture.ts     # Intercept XHR/fetch → keyed response store
│   ├── api-diff.ts        # Deep-diff two JSON bodies, ignore noise fields
│   └── visual.ts          # VisualHelper + REGIONS constants
├── tests/
│   ├── compare.ts         # (util) Open both apps concurrently in one test
│   ├── 01-auth/           login.spec.ts
│   ├── 02-dashboard/      dashboard.spec.ts
│   ├── 03-wallet/         wallet.spec.ts
│   └── 04-account/        account.spec.ts
├── .auth/                 # Saved auth state (gitignored)
└── snapshots/             # Visual baselines (committed to git)
```

---

## Quick start

```bash
# 1. Install
cd app/playrighttest
npm install
npx playwright install chromium

# 2. Configure
cp .env.example .env
# Edit .env — set NEXT_BASE_URL, VUE_BASE_URL, TEST_EMAIL, TEST_PASSWORD

# 3. Generate auth state (run once, or when session expires)
npm run setup

# 4. Run all comparison tests
npm test

# 5. View HTML report
npm run test:report
```

---

## Running individual suites

```bash
npm run test:auth       # Login form, error handling, token shape
npm run test:dashboard  # Dashboard widgets, nav links, API data
npm run test:wallet     # Balance banner, tabs, withdrawal modal, API diff
npm run test:account    # Account list, open-account modal, detail navigation
```

## Running a single project (app)

```bash
npx playwright test --project=next    # Only Next.js
npx playwright test --project=vue     # Only Vue3
```

## UI mode (interactive)

```bash
npm run test:ui
```

---

## How it works

### 功能一致性 — Functional

Each spec asserts that *both* apps expose the same UI interactions:
- Form fields present and usable
- Modals open/close correctly
- Navigation redirects to expected URLs
- Error states render for invalid input

### 数据一致性 — API Diff

`ApiCapture` attaches a `response` listener to the Playwright `Page` and stores every JSON API response in a `Map<normalizedPath, CapturedResponse[]>`.

After navigation, tests call `capture.get('/api/v1/wallet/primary')` (or `getByPrefix`), extract the body, and run `diffApiResponses(nextBody, vueBody)`.

The differ:
- Strips noise keys (`id`, `createdAt`, `updatedAt`, `timestamp`, …) — configurable via `API_DIFF_IGNORE_FIELDS` env var
- Handles nested objects, arrays (position or keyed alignment)
- Reports `missing_in_next | missing_in_vue | type_mismatch | value_mismatch`
- Attaches the diff JSON to the Playwright HTML report

### 视觉一致性 — Visual

`VisualHelper.assertRegion()` wraps Playwright's `toHaveScreenshot()` with:
- Per-project snapshot directories: `snapshots/next/...` vs `snapshots/vue/...`
- Dynamic content masking (balance numbers, timestamps)
- Configurable threshold via `VISUAL_THRESHOLD` env var (default `0.05`)

**First run** generates baselines. Commit them to git.  
**Subsequent runs** pixel-diff against the baseline; failures produce annotated diff images in the HTML report.

To update baselines after intentional UI changes:

```bash
npx playwright test --update-snapshots
```

---

## Adding a new page test

1. Create `tests/05-<page>/<page>.spec.ts`
2. Import `test, expect` from `../../fixtures`
3. Structure three `describe` blocks: functional / API data / visual
4. Add new selectors to `REGIONS` in `helpers/visual.ts` if needed

---

## Environment variables

| Variable | Default | Description |
|---|---|---|
| `NEXT_BASE_URL` | `http://localhost:3000` | Next.js dev server |
| `VUE_BASE_URL` | `http://client.localhost:8084` | Vue3 dev server |
| `TEST_EMAIL` | `test@example.com` | Login email |
| `TEST_PASSWORD` | `TestPassword123!` | Login password |
| `VISUAL_THRESHOLD` | `0.05` | Max pixel diff ratio (0–1) |
| `API_DIFF_IGNORE_FIELDS` | `id,createdAt,…` | Comma-separated keys to skip in diff |
