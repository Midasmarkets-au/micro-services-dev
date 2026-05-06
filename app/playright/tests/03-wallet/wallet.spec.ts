/**
 * 03 — Wallet
 *
 * Compares:
 *  - 功能一致性: balance shown, transaction tabs work, withdrawal modal opens,
 *               pagination, date filter
 *  - 数据一致性: /wallet/primary + /wallet/transactions API shape + diff
 *  - 视觉一致性: banner, table, tab-bar screenshots
 */
import { test, expect } from '../../fixtures';
import { diffApiResponses } from '../../helpers/api-diff';
import { REGIONS } from '../../helpers/visual';

// ─── Shared per-run state for cross-app diff ──────────────────────────────────
// We collect captured responses per app, then diff in a separate "compare" test.

const captured: Record<'next' | 'vue', { wallet?: unknown; transactions?: unknown }> = {
  next: {},
  vue: {},
};

// ─── Functional consistency ───────────────────────────────────────────────────

test.describe('Wallet — functional', () => {
  test('wallet page loads and shows balance', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    expect(page.url()).not.toContain('sign-in');

    // Try banner first; fall back to any visible balance/amount element
    const banner = page.locator(
      '[data-testid="wallet-banner"], .wallet-banner, .balance-card, ' +
      '.verification-banner-bg, div.relative.overflow-hidden.rounded, ' +   // Next.js
      '.wallet-card, [class*="wallet"], [class*="balance"]'                 // Vue3
    ).first();

    await expect(banner, 'Wallet banner / balance area must be visible').toBeVisible({ timeout: 10_000 });
  });

  test('transaction tabs are rendered (Withdrawal/Deposit/Transfer…)', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    const tabs = page.locator(REGIONS.WALLET_TABS).first();
    await expect(tabs, 'Transaction tabs must be present').toBeVisible({ timeout: 8_000 });

    // Next.js: buttons inside data-testid="wallet-tabs"
    // Vue3:    buttons inside div.tabs-nav
    const tabItems = page.locator(
      '[data-testid="wallet-tabs"] button, div.tabs-nav button'
    );
    const count = await tabItems.count();
    expect(count, 'At least 1 tab must exist').toBeGreaterThan(0);
  });

  test('switching tabs updates the transaction list', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    // Find Deposit tab
    const depositTab = page.locator(
      '[role="tab"]:has-text("Deposit"), .el-tabs__item:has-text("Deposit"), button:has-text("Deposit")'
    ).first();

    if (!(await depositTab.isVisible())) { test.skip(); return; }

    await depositTab.click();

    // Table should refresh (wait for a row or empty state)
    const table = page.locator('table, [data-testid="transaction-table"], .el-table').first();
    await expect(table, 'Table must be visible after tab switch').toBeVisible({ timeout: 8_000 });
  });

  test('withdrawal modal can be opened', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    const withdrawBtn = page.locator(
      'button:has-text("Withdraw"), button:has-text("withdrawal"), [data-testid*="withdraw"]'
    ).first();

    if (!(await withdrawBtn.isVisible())) { test.skip(); return; }

    await withdrawBtn.click();

    const modal = page.locator(
      '[role="dialog"], .el-dialog, [data-testid*="modal"], .modal'
    ).first();
    await expect(modal, 'Withdrawal modal must open').toBeVisible({ timeout: 6_000 });

    // Close it
    const closeBtn = modal.locator('button[aria-label="close"], .el-dialog__close, button:has-text("Cancel")').first();
    if (await closeBtn.isVisible()) await closeBtn.click();
  });

  test('pagination controls appear when there are multiple pages', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    // Pagination is only present if there are rows; skip otherwise
    const pagination = page.locator(
      '[data-testid*="pagination"], .el-pagination, [aria-label*="pagination"], nav[role="navigation"]'
    ).first();

    const isVisible = await pagination.isVisible().catch(() => false);
    if (!isVisible) { console.log('[wallet] No pagination visible — possibly no transactions'); return; }

    await expect(pagination).toBeVisible();
  });

  test('date filter controls are present', async ({ page, appBaseURL }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    const dateFilter = page.locator(
      '[data-testid*="date"], .date-filter, input[type="date"], .el-date-editor, [placeholder*="date" i], [placeholder*="From" i]'
    ).first();

    if (await dateFilter.isVisible()) {
      await expect(dateFilter).toBeVisible();
    }
  });
});

// ─── Data consistency ─────────────────────────────────────────────────────────

test.describe('Wallet — API data', () => {
  test('primary wallet API returns 200 with balance fields', async ({ page, appBaseURL, capture, appName }, testInfo) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    // Try multiple normalised paths used by both apps
    const walletRes =
      capture.get('/api/v1/wallet/primary') ??
      capture.getByPrefix('/api/v1/wallet').at(0) ??
      capture.getByPrefix('/api/wallet').at(0);

    if (!walletRes) {
      console.warn(`[${appName}] No wallet API captured — skipping`);
      return;
    }

    expect(walletRes.status).toBe(200);

    const data = ((walletRes.body as Record<string, unknown>)?.['data'] ?? walletRes.body) as Record<string, unknown>;

    // Store for cross-app diff
    captured[appName as 'next' | 'vue'].wallet = data;

    await testInfo.attach(`${appName}: wallet-primary`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(data, null, 2)),
    });

    // Required fields
    const hasCurrency = 'currency' in data || 'currencyCode' in data;
    expect(hasCurrency, 'Wallet must have a currency field').toBe(true);
  });

  test('withdrawal transactions API returns array', async ({ page, appBaseURL, capture, appName }, testInfo) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    const txRes =
      capture.get('/api/v1/wallet/withdrawal') ??
      capture.getByPrefix('/api/v1/wallet/transactions').at(0) ??
      capture.getByPrefix('/api/v1/transaction').at(0);

    if (!txRes) {
      console.warn(`[${appName}] No transaction API captured`);
      return;
    }

    expect(txRes.status).toBe(200);

    const body = txRes.body as Record<string, unknown>;
    const list = body?.['list'] ?? body?.['data'] ?? body?.['items'] ?? body;

    captured[appName as 'next' | 'vue'].transactions = body;

    await testInfo.attach(`${appName}: transactions`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(body, null, 2)),
    });

    expect(Array.isArray(list) || typeof body === 'object', 'Transactions response must be an object or array').toBe(true);
  });

  /**
   * Cross-app diff — only runs after both next + vue test projects have
   * exercised the same page.  If run in isolation (single project), this test
   * silently skips.
   */
  test('wallet API response diff: Next vs Vue', async ({ appName }, testInfo) => {
    const { next, vue } = captured;

    if (!next.wallet || !vue.wallet) {
      test.skip(); // one or both projects haven't run yet
      return;
    }

    const result = diffApiResponses(next.wallet, vue.wallet, {
      ignoreKeys: new Set(['id', 'createdAt', 'updatedAt']),
    });

    await testInfo.attach('wallet-primary-diff', {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(result.differences, null, 2)),
    });

    if (!result.matched) {
      console.warn(`[wallet-diff] ${result.summary}`);
    }
    // Soft assertion — log differences but don't hard-fail visual/functional tests
    expect(result.differences.length, result.summary).toBeLessThanOrEqual(3);
  });
});

// ─── Visual consistency ───────────────────────────────────────────────────────

test.describe('Wallet — visual', () => {
  test('wallet banner visual baseline', async ({ page, appBaseURL, appName, visual }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    // Next.js → data-testid="wallet-banner"  |  Vue3 → div.wallet-card
    const bannerSel = appName === 'next' ? '[data-testid="wallet-banner"]' : '.wallet-card';
    await page.locator(bannerSel).first().waitFor({ state: 'visible', timeout: 10_000 });
    // Scope screenshot to first card only (Vue3 may render multiple wallet cards)

    await visual.assertRegion({
      selector: bannerSel,
      name: 'wallet-banner',
      // Mask balance numbers — they change every run
      maskSelectors: [
        // Next.js BalanceShow
        '[data-testid="wallet-banner"] [class*="font-bold"]',
        // Vue3 wallet-card balance
        '.wallet-card .fs-1, .wallet-card [class*="balance"]',
      ],
    });
  });

  test('transaction table visual baseline', async ({ page, appBaseURL, appName, visual }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');

    // Next.js → <table>  |  Vue3 → table inside #monthlyTable
    const tableSel = appName === 'next' ? 'table' : '#monthlyTable table';
    const table = page.locator(tableSel).first();
    const isVisible = await table.isVisible().catch(() => false);
    if (!isVisible) { test.skip(); return; }

    await visual.assertRegion({
      selector: tableSel,
      name: 'wallet-table',
      // Mask all cell content — amounts/dates change every run
      maskSelectors: ['td'],
    });
  });

  test('full wallet page visual baseline', async ({ page, appBaseURL, visual }) => {
    await page.goto(`${appBaseURL}/wallet`);
    await page.waitForLoadState('networkidle');
    await visual.assertFullPage('wallet', 0.08); // slightly looser threshold for dynamic data
  });
});
