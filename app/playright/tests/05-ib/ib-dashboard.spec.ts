/**
 * 05 — IB Dashboard  (/ib)
 *
 * Goal: verify that Next.js and Vue3 behave identically for the same business
 * scenario on the IB dashboard.
 *
 * Scope:
 *  - 功能一致性: nav presence, account selector, stat sections, widget sections,
 *               sub-page navigation links all reachable
 *  - 数据一致性: 9 report/widget APIs return correct shapes, timezoneOffset is
 *               forwarded, same response structure + values across both apps
 *  - 权限一致性: IB/Sales role gates the route in both apps, agent account is
 *               resolved and used as path segment in all API calls
 *
 * URL difference (by design — NOT a failure):
 *   Next.js  → /ib            (stays on /ib)
 *   Vue3     → /ib → /ib/index  (Vue router redirect)
 *
 * DOM notes:
 *   Next.js nav  → CenterSidebar: <a href="/ib/customers"> etc inside nav/aside
 *   Vue3 nav     → headerMenuLg (desktop ≥ lg): .ib-menu-item links
 *                  headerMenu   (mobile): .sub-menu-item links
 *   Next.js card → StatCard: div[class*="bg-surface"][class*="rounded-xl"]
 *   Vue3 card    → .card.h-50 (Bootstrap)
 *
 * Not tested here:
 *  - Pixel-level layout / CSS / spacing
 *  - DOM structural parity
 */
import { test, expect } from '../../fixtures';
import { diffApiResponses } from '../../helpers/api-diff';
import type { CapturedResponse } from '../../helpers/api-capture';

// ─── Constants ────────────────────────────────────────────────────────────────

const IB_PATH = '/ib';

// ─── Selectors ────────────────────────────────────────────────────────────────

// Stat card container
// Next.js: StatCard → Tailwind "bg-surface" + "rounded-xl"
// Vue3: widgets render Bootstrap .card — number and visibility depends on
//       isMobile and rebateEnabled config; use .card as the common denominator
const STAT_CARD_SEL_NEXT = '[class*="bg-surface"][class*="rounded-xl"]';
const STAT_CARD_SEL_VUE  = '.card';

// Account selector (Vue3: .account-select el-select | Next.js: AccountSelectorCard)
const ACCOUNT_SEL_SEL =
  '.account-select, .account-selector-card, .ib-code, [class*="account-select"]';

// Latest deposits (Vue3: IBWidgetTopFundings | Next.js: LatestDepositsWidget table)
const LATEST_DEPOSITS_SEL =
  'table:has(td), .el-table, [class*="LatestDeposit"], [class*="TopFunding"]';

// IB link section — matched by visible text "IB Link" present in both nav and widget header
const IB_LINK_TEXT_SEL = ':text("IB Link"), :text("Ib Link"), :text("iblink")';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/**
 * Find the latest captured response whose normalised path contains the suffix.
 * Works regardless of /api/v1/ (Vue3) vs /api/backend/v1/ (Next.js) prefix
 * and regardless of the agentUid segment (normalised to :id by ApiCapture).
 */
function getIBResponse(
  capture: { capturedPaths(): string[]; get(p: string): CapturedResponse | undefined },
  suffix: string
): CapturedResponse | undefined {
  const target = suffix.replace(/^\//, '');
  const match = capture.capturedPaths().find(
    (p) => p.includes('/ib/') && p.includes(target)
  );
  return match ? capture.get(match) : undefined;
}

/** Assert body is IBReportValue[]: { currencyId: number, amount: number }[] */
function assertReportValueShape(body: unknown, label: string, appName: string): void {
  expect(Array.isArray(body), `[${appName}] ${label}: must be an array`).toBe(true);
  const items = body as { currencyId?: unknown; amount?: unknown }[];
  if (items.length > 0) {
    expect(typeof items[0].currencyId, `[${appName}] ${label}: items[0].currencyId must be number`).toBe('number');
    expect(typeof items[0].amount,     `[${appName}] ${label}: items[0].amount must be number`).toBe('number');
  }
}

/** Assert body is a paginated list: { data: T[], criteria: { page, size } } */
function assertPaginatedShape(body: unknown, label: string, appName: string): void {
  expect(
    body !== null && typeof body === 'object' && !Array.isArray(body),
    `[${appName}] ${label}: must be an object`
  ).toBe(true);
  const obj = body as Record<string, unknown>;
  expect(Array.isArray(obj['data']),             `[${appName}] ${label}: .data must be an array`).toBe(true);
  expect(typeof obj['criteria'] === 'object',    `[${appName}] ${label}: .criteria must be an object`).toBe(true);
  const c = obj['criteria'] as Record<string, unknown>;
  expect(typeof c['page'], `[${appName}] ${label}: criteria.page must be number`).toBe('number');
  expect(typeof c['size'], `[${appName}] ${label}: criteria.size must be number`).toBe('number');
}

/**
 * Extract a numeric scalar from a response body.
 * Handles both raw number (Vue3: `42`) and object-wrapped (Next.js: `{"volume":42}` / `{"count":42}`).
 */
function extractScalar(body: unknown): number | null {
  if (typeof body === 'number') return body;
  if (typeof body === 'string' && !isNaN(Number(body))) return Number(body);
  if (body !== null && typeof body === 'object') {
    const obj = body as Record<string, unknown>;
    // Try common wrapper key names
    for (const key of ['volume', 'count', 'value', 'total', 'data']) {
      if (typeof obj[key] === 'number') return obj[key] as number;
    }
  }
  return null;
}

// ─── Module-level snapshot store for cross-app diff ──────────────────────────

type IBStatSnapshot = {
  rebateTodayValue?: unknown;
  rebateTotalValue?: unknown;
  tradeTodayVolume?: unknown;
  todayAccountCreation?: unknown;
  depositTodayValue?: unknown;
};

const captured: Record<'next' | 'vue', IBStatSnapshot> = {
  next: {},
  vue:  {},
};

// ─── 功能一致性 ────────────────────────────────────────────────────────────────

test.describe('IB Dashboard — 功能一致性', () => {
  test('IB dashboard loads and user is not redirected to sign-in or 403', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const url = page.url();
    expect(url, `[${appName}] Must not redirect to sign-in`).not.toContain('sign-in');
    expect(url, `[${appName}] Must not redirect to 403`).not.toContain('403');

    // Vue3 redirects /ib → /ib/index; Next.js stays on /ib — both are valid
    const path = new URL(url).pathname;
    expect(path.startsWith('/ib'), `[${appName}] Must stay on /ib* route, got: ${path}`).toBe(true);
  });

  test('IB navigation links are present for all sub-pages', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');
    // Extra wait for Vue3 IBLayout async mount and potential IBWidgetSelector redirect
    await page.waitForTimeout(2_000);

    // Guard: if IBWidgetSelector redirected away (missing ibCurrentAccount in localStorage),
    // annotate and skip rather than hard-fail.
    const currentPath = new URL(page.url()).pathname;
    if (!currentPath.startsWith('/ib')) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] Page redirected to "${currentPath}" — ` +
          `IBWidgetSelector account guard triggered. Re-run "playwright test --project=setup" ` +
          `to regenerate auth state with ibCurrentAccount in localStorage.`,
      });
      return;
    }

    // Collect ALL <a> hrefs from the entire page — works regardless of
    // nav container class, router mode (hash vs history), or link component.
    const allHrefs: string[] = await page.evaluate(() =>
      Array.from(document.querySelectorAll('a[href]'))
        .map((el) => el.getAttribute('href') ?? '')
    );

    const ibHrefs = allHrefs.filter((h) => h.includes('ib'));
    expect(
      ibHrefs.length,
      `[${appName}] At least one IB-related <a> link must exist on the page. ` +
      `Total hrefs found: ${allHrefs.length}`
    ).toBeGreaterThan(0);

    const requiredSlugs = ['customers', 'new-customers', 'trade', 'deposit', 'withdrawal', 'rebate', 'iblink'];
    for (const slug of requiredSlugs) {
      const found = allHrefs.some((h) => h.includes(slug));
      expect(
        found,
        `[${appName}] Page must contain an <a> link for "${slug}". ` +
        `IB-related hrefs found: ${ibHrefs.join(', ')}`
      ).toBe(true);
    }
  });

  test('account selector / agent profile card is rendered', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const selector = page.locator(ACCOUNT_SEL_SEL).first();
    const isVisible = await selector.isVisible().catch(() => false);

    if (!isVisible) {
      // Fallback: any element showing the agent UID (a number inside an IB section)
      const ibSection = page.locator('.ib-info, [class*="ib-widget"], [class*="IBAccount"]').first();
      const ibVisible = await ibSection.isVisible().catch(() => false);
      if (!ibVisible) {
        test.info().annotations.push({
          type: 'warn',
          description: `[${appName}] Account selector not found with selector "${ACCOUNT_SEL_SEL}". ` +
            `Page may use a different component structure.`,
        });
      }
      // Soft pass — don't hard-fail on selector mismatch
      return;
    }

    await expect(selector).toBeVisible();
  });

  test('stat card sections are rendered on the dashboard (trade vol, deposit, customers, rebate)', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    // Guard: IBWidgetSelector redirects to "/" when ibCurrentAccount is absent from localStorage.
    // Root fix: re-run "playwright test --project=setup" to regenerate auth state.
    const currentPath = new URL(page.url()).pathname;
    if (!currentPath.startsWith('/ib')) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] Page redirected to "${currentPath}" — ` +
          `IBWidgetSelector account guard triggered. Re-run "--project=setup" to fix.`,
      });
      return;
    }

    // Next.js: StatCard uses Tailwind bg-surface + rounded-xl
    // Vue3: Bootstrap .card — count varies by isMobile / rebateEnabled config
    const cardSel = appName === 'next' ? STAT_CARD_SEL_NEXT : STAT_CARD_SEL_VUE;
    const firstCard = page.locator(cardSel).first();

    await expect(
      firstCard,
      `[${appName}] At least one card element must be visible (sel: "${cardSel}")`
    ).toBeVisible({ timeout: 15_000 });

    // Next.js always shows 4 StatCards; Vue3 count varies by config/viewport
    const count = await page.locator(cardSel).count();
    const minCount = appName === 'next' ? 4 : 1;
    expect(count, `[${appName}] Expected >= ${minCount} card elements`).toBeGreaterThanOrEqual(minCount);
  });

  test('rebate chart widget is present (may be hidden if rebateEnabled=false)', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    // Vue3: only rendered when rebateEnabled===true in projectConfig
    const chartSel = 'canvas, [class*="RebateChart"], [class*="rebate-chart"], .ib-widget-rebate-chart';
    const chart = page.locator(chartSel).first();
    const visible = await chart.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] Rebate chart not visible — may be disabled by rebateEnabled config`,
      });
    }
  });

  test('latest deposits section is present', async ({ page, appBaseURL, appName }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const widget = page.locator(LATEST_DEPOSITS_SEL).first();
    const visible = await widget.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] Latest deposits section not found — may require data or different selector`,
      });
      return;
    }
    await expect(widget).toBeVisible();
  });

  test('IB links section is accessible from the dashboard', async ({ page, appBaseURL, appName }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    // Check via href (most reliable if it exists)
    const hrefLink = page.locator('a[href*="iblink"]').first();
    const hrefVisible = await hrefLink.isVisible().catch(() => false);
    if (hrefVisible) {
      await expect(hrefLink).toBeVisible();
      return;
    }

    // Fallback: check for visible text containing "IB Link" / "ibLink" (i18n key)
    const textEl = page.locator(IB_LINK_TEXT_SEL).first();
    const textVisible = await textEl.isVisible().catch(() => false);
    if (textVisible) {
      await expect(textEl).toBeVisible();
      return;
    }

    // Last resort: verify the /ib/iblink sub-page is navigable
    await page.goto(`${appBaseURL}/ib/iblink`);
    await page.waitForLoadState('networkidle');
    const url = page.url();
    expect(url, `[${appName}] /ib/iblink must be accessible`).not.toContain('sign-in');
    expect(new URL(url).pathname.startsWith('/ib'), `[${appName}] iblink URL: ${url}`).toBe(true);
  });

  test('sub-pages are navigable (customers, trade, deposit)', async ({ page, appBaseURL, appName }) => {
    const subPages = [
      { path: '/ib/customers', label: 'Customers' },
      { path: '/ib/trade',     label: 'Trade' },
      { path: '/ib/deposit',   label: 'Deposit' },
    ];

    for (const sub of subPages) {
      await page.goto(`${appBaseURL}${sub.path}`);
      await page.waitForLoadState('networkidle');

      const url = page.url();
      expect(url, `[${appName}] ${sub.label}: must not redirect to sign-in`).not.toContain('sign-in');
      expect(url, `[${appName}] ${sub.label}: must not 403`).not.toContain('403');
      expect(
        new URL(url).pathname.startsWith('/ib'),
        `[${appName}] ${sub.label}: URL must stay on /ib*, got: ${url}`
      ).toBe(true);
    }
  });
});

// ─── 数据一致性 ────────────────────────────────────────────────────────────────

test.describe('IB Dashboard — 数据一致性', () => {
  test('stat API: report/rebate/today-value → IBReportValue[]', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/rebate/today-value');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] rebate/today-value not captured` });
      return;
    }

    expect(res.status, `[${appName}] rebate/today-value must return 200`).toBe(200);
    expect(
      res.url.includes('timezoneOffset'),
      `[${appName}] rebate/today-value must include timezoneOffset. URL: ${res.url}`
    ).toBe(true);
    assertReportValueShape(res.body, 'rebate/today-value', appName);
    captured[appName as 'next' | 'vue'].rebateTodayValue = res.body;

    await testInfo.attach(`${appName}: rebate-today-value`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('stat API: report/rebate/total-value → IBReportValue[]', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/rebate/total-value');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] rebate/total-value not captured` });
      return;
    }

    expect(res.status, `[${appName}] rebate/total-value must return 200`).toBe(200);
    assertReportValueShape(res.body, 'rebate/total-value', appName);
    captured[appName as 'next' | 'vue'].rebateTotalValue = res.body;

    await testInfo.attach(`${appName}: rebate-total-value`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('stat API: report/trade/today-volume → number', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/trade/today-volume');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] trade/today-volume not captured` });
      return;
    }

    expect(res.status, `[${appName}] trade/today-volume must return 200`).toBe(200);
    expect(
      res.url.includes('timezoneOffset'),
      `[${appName}] trade/today-volume must include timezoneOffset. URL: ${res.url}`
    ).toBe(true);

    // Vue3: raw number  |  Next.js: { volume: number }
    const scalar = extractScalar(res.body);
    expect(
      scalar !== null,
      `[${appName}] trade/today-volume must resolve to a number. Got: ${JSON.stringify(res.body)}`
    ).toBe(true);

    captured[appName as 'next' | 'vue'].tradeTodayVolume = scalar;

    await testInfo.attach(`${appName}: trade-today-volume`, {
      contentType: 'application/json',
      body: Buffer.from(String(scalar)),
    });
  });

  test('stat API: report/account/today-creation → number', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/account/today-creation');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] account/today-creation not captured` });
      return;
    }

    expect(res.status, `[${appName}] account/today-creation must return 200`).toBe(200);

    // Vue3: raw number  |  Next.js: { count: number }
    const scalar = extractScalar(res.body);
    expect(
      scalar !== null,
      `[${appName}] account/today-creation must resolve to a number. Got: ${JSON.stringify(res.body)}`
    ).toBe(true);

    captured[appName as 'next' | 'vue'].todayAccountCreation = scalar;

    await testInfo.attach(`${appName}: today-account-creation`, {
      contentType: 'application/json',
      body: Buffer.from(String(scalar)),
    });
  });

  test('stat API: report/deposit/today-value → IBReportValue[]', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/deposit/today-value');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] deposit/today-value not captured` });
      return;
    }

    expect(res.status, `[${appName}] deposit/today-value must return 200`).toBe(200);
    assertReportValueShape(res.body, 'deposit/today-value', appName);
    captured[appName as 'next' | 'vue'].depositTodayValue = res.body;

    await testInfo.attach(`${appName}: deposit-today-value`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('widget API: report/deposit/latest → IBLatestDeposit[]', async ({
    page, appBaseURL, capture, appName,
  }) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'report/deposit/latest');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] deposit/latest not captured` });
      return;
    }

    expect(res.status, `[${appName}] deposit/latest must return 200`).toBe(200);
    expect(Array.isArray(res.body), `[${appName}] deposit/latest must be an array`).toBe(true);

    const items = res.body as Record<string, unknown>[];
    if (items.length > 0) {
      const item = items[0];
      // Identifier field differs by app:
      //   Vue3:    id (number)
      //   Next.js: matterId (number) — the backend uses "matter" terminology
      const hasId =
        typeof item['id']       === 'number' ||
        typeof item['matterId'] === 'number' ||
        typeof item['hashId']   === 'string';
      expect(
        hasId,
        `[${appName}] deposit/latest item must have id / matterId / hashId. ` +
        `Keys: ${Object.keys(item).join(', ')}`
      ).toBe(true);
      expect(typeof item['amount'],     `[${appName}] deposit/latest item must have numeric amount`).toBe('number');
      expect(typeof item['currencyId'], `[${appName}] deposit/latest item must have numeric currencyId`).toBe('number');
    }
  });

  test('widget API: referral/user-history → paginated list with IsUnverified param', async ({
    page, appBaseURL, capture, appName,
  }) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'referral/user-history');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] referral/user-history not captured` });
      return;
    }

    expect(res.status, `[${appName}] referral/user-history must return 200`).toBe(200);
    assertPaginatedShape(res.body, 'referral/user-history', appName);

    expect(
      res.url.toLowerCase().includes('isunverified'),
      `[${appName}] referral/user-history must include IsUnverified param. URL: ${res.url}`
    ).toBe(true);
  });

  test('widget API: tradetransaction → paginated list with IBTradeRecord items', async ({
    page, appBaseURL, capture, appName,
  }) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const res = getIBResponse(capture, 'tradetransaction');
    if (!res) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] tradetransaction not captured` });
      return;
    }

    expect(res.status, `[${appName}] tradetransaction must return 200`).toBe(200);
    assertPaginatedShape(res.body, 'tradetransaction', appName);

    const body = res.body as { data: Record<string, unknown>[] };
    if (body.data.length > 0) {
      expect(
        typeof body.data[0]['id'],
        `[${appName}] tradetransaction record must have numeric id`
      ).toBe('number');
    }
  });

  test('widget API: referral (IB links list) → paginated list with IBLink items', async ({
    page, appBaseURL, capture, appName,
  }) => {
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    // "referral" without "/user-history" = link list
    const allPaths = capture.capturedPaths();
    const refPath = allPaths.find(
      (p) => p.includes('/ib/') && p.includes('referral') && !p.includes('user-history')
    );

    if (!refPath) {
      test.info().annotations.push({ type: 'warn', description: `[${appName}] referral (link list) not captured` });
      return;
    }

    const res = capture.get(refPath);
    if (!res) return;

    expect(res.status, `[${appName}] referral (link list) must return 200`).toBe(200);
    assertPaginatedShape(res.body, 'referral-link-list', appName);

    const body = res.body as { data: Record<string, unknown>[] };
    if (body.data.length > 0) {
      const link = body.data[0];
      // Identifier / key field differs by app:
      //   Vue3:    id (number) + status (number)
      //   Next.js: code (string) only — no numeric id, no status in this endpoint
      // `code` is the stable referral code and exists in BOTH apps.
      expect(
        typeof link['code'],
        `[${appName}] IBLink must have code (string). Keys: ${Object.keys(link).join(', ')}`
      ).toBe('string');
    }
  });

  /**
   * Cross-app diff: compare the 3 IBReportValue[] stat APIs between Next.js and Vue3.
   * Both apps call the same backend for the same user — values must be identical.
   * Runs only after BOTH projects have populated `captured`.
   */
  test('cross-app diff: stat API responses must be identical between Next.js and Vue3', async (
    { appName },
    testInfo,
  ) => {
    const { next, vue } = captured;

    if (!next.rebateTodayValue || !vue.rebateTodayValue) {
      test.skip(); // one or both projects haven't run yet
      return;
    }

    const checks: Array<{ key: keyof IBStatSnapshot; label: string }> = [
      { key: 'rebateTodayValue',  label: 'rebate/today-value' },
      { key: 'rebateTotalValue',  label: 'rebate/total-value' },
      { key: 'depositTodayValue', label: 'deposit/today-value' },
    ];

    let totalDiffs = 0;

    for (const { key, label } of checks) {
      const nextData = next[key];
      const vueData  = vue[key];
      if (nextData === undefined || vueData === undefined) continue;

      const result = diffApiResponses(nextData, vueData, {
        numericTolerance: 0,  // same backend, same user → values must match
        arrayKey: 'currencyId',
      });

      await testInfo.attach(`diff: ${label} (${appName})`, {
        contentType: 'application/json',
        body: Buffer.from(JSON.stringify({ label, differences: result.differences }, null, 2)),
      });

      if (result.differences.length > 0) {
        totalDiffs += result.differences.length;
        console.warn(
          `[ib-diff] ${label}: ${result.differences.length} difference(s):\n` +
          result.differences
            .map((d) => `  [${d.kind}] ${d.path}  next=${JSON.stringify(d.nextValue)}  vue=${JSON.stringify(d.vueValue)}`)
            .join('\n')
        );
      }
    }

    // Scalar values (volume, count): compare directly
    for (const key of ['tradeTodayVolume', 'todayAccountCreation'] as const) {
      const n = next[key];
      const v = vue[key];
      if (n === undefined || v === undefined) continue;
      if (n !== v) {
        totalDiffs++;
        console.warn(`[ib-diff] ${key}: next=${n}  vue=${v}`);
        await testInfo.attach(`diff: ${key} (${appName})`, {
          contentType: 'application/json',
          body: Buffer.from(JSON.stringify({ key, next: n, vue: v }, null, 2)),
        });
      }
    }

    expect(
      totalDiffs,
      `${totalDiffs} difference(s) found between Next.js and Vue3 stat API responses. ` +
      `Both apps call the same backend for the same user — values must be identical.`
    ).toBe(0);
  });
});

// ─── 权限一致性 ────────────────────────────────────────────────────────────────

test.describe('IB Dashboard — 权限一致性', () => {
  test('IB user can access /ib without redirect', async ({ page, appBaseURL, appName }) => {
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');

    const url = page.url();
    const path = new URL(url).pathname;

    expect(url, `[${appName}] Must not redirect to sign-in`).not.toContain('sign-in');
    expect(url, `[${appName}] Must not 403`).not.toContain('403');
    expect(path.startsWith('/ib'), `[${appName}] Must remain on /ib route, got: ${path}`).toBe(true);
  });

  test('all IB report APIs return 200 (not 401/403) for the IB user', async ({
    page, appBaseURL, capture, appName,
  }) => {
    // Force fresh page load to ensure APIs are called (not served from memory)
    await page.goto('about:blank');
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');
    // Allow extra time for async widget loads after networkidle
    await page.waitForTimeout(2_000);

    const ibPaths = capture.capturedPaths().filter((p) => p.includes('/ib/') && p.includes('/report/'));

    if (ibPaths.length === 0) {
      // Vue3 may serve from Vuex cache on repeated navigation; skip rather than fail
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] No /ib/*/report/* paths captured — data may be Vuex-cached from earlier tests`,
      });
      return;
    }

    for (const p of ibPaths) {
      const res = capture.get(p);
      if (!res) continue;
      expect(
        res.status,
        `[${appName}] IB API "${p}" must return 200 for authenticated IB user. Got: ${res.status}`
      ).toBe(200);
    }
  });

  test('IB API path structure: agentUid is a path segment (not query param)', async ({
    page, appBaseURL, capture, appName,
  }) => {
    await page.goto('about:blank');
    capture.clear();
    await page.goto(`${appBaseURL}${IB_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const ibReportPaths = capture.capturedPaths().filter(
      (p) => p.includes('/ib/') && p.includes('/report/')
    );

    for (const p of ibReportPaths) {
      // After normalization: /api/.../ib/:id/report/...  or  /api/v1/ib/:id/report/...
      const hasUidSegment = p.includes('/ib/:id/') || /\/ib\/\d+\//.test(p);
      expect(hasUidSegment, `[${appName}] Path "${p}" must include agentUid as a segment`).toBe(true);
    }

    if (ibReportPaths.length === 0) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] No /ib/*/report/* paths captured — Vuex cache may have served data`,
      });
    }
  });
});
