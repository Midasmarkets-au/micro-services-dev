/**
 * VisualHelper — screenshot + snapshot comparison helpers.
 *
 * Each named region captures an element or viewport crop and compares it
 * against a baseline stored under snapshots/<project>/<test-title>/<name>.png
 *
 * First run (no baseline): generates baselines automatically.
 * Subsequent runs: pixel-diffs against baseline; fails if drift > threshold.
 */
import { Page, TestInfo, Locator, expect } from '@playwright/test';
import path from 'path';

const THRESHOLD = parseFloat(process.env.VISUAL_THRESHOLD ?? '0.05');

export interface ScreenshotRegion {
  /** CSS selector or Playwright Locator for the element to capture. */
  selector: string | Locator;
  /** Human-readable name used as the snapshot filename key. */
  name: string;
  /** Optional clip rect (overrides element bounds). */
  clip?: { x: number; y: number; width: number; height: number };
  /** Per-region threshold override. */
  threshold?: number;
  /** Mask dynamic content inside the region (e.g. currency values). */
  maskSelectors?: string[];
}

export class VisualHelper {
  constructor(
    private readonly page: Page,
    private readonly testInfo: TestInfo
  ) {}

  /**
   * Capture a full-page screenshot and compare to baseline.
   */
  async assertFullPage(name: string, threshold = THRESHOLD): Promise<void> {
    await this.page.waitForLoadState('networkidle');
    await expect(this.page).toHaveScreenshot(this.snapshotName(`full-${name}`), {
      fullPage: true,
      maxDiffPixelRatio: threshold,
      animations: 'disabled',
    });
  }

  /**
   * Capture a specific page region by selector and compare to baseline.
   */
  async assertRegion(region: ScreenshotRegion): Promise<void> {
    const loc = this.resolveLocator(region.selector);

    // Wait for element to be visible
    await loc.waitFor({ state: 'visible', timeout: 10_000 });

    const masks = (region.maskSelectors ?? []).map((sel) => this.page.locator(sel));

    await expect(loc).toHaveScreenshot(this.snapshotName(region.name), {
      maxDiffPixelRatio: region.threshold ?? THRESHOLD,
      animations: 'disabled',
      mask: masks,
    });
  }

  /**
   * Capture multiple regions in one call.
   */
  async assertRegions(regions: ScreenshotRegion[]): Promise<void> {
    for (const region of regions) {
      await this.assertRegion(region);
    }
  }

  /**
   * Take a raw screenshot Buffer without asserting (for manual inspection).
   */
  async capture(selector?: string | Locator): Promise<Buffer> {
    if (selector) {
      return this.resolveLocator(selector).screenshot({ animations: 'disabled' });
    }
    return this.page.screenshot({ fullPage: true, animations: 'disabled' });
  }

  /**
   * Attach a screenshot to the Playwright report without asserting.
   * Useful for annotating reports with before/after states.
   */
  async attach(name: string, selector?: string | Locator): Promise<void> {
    const buf = await this.capture(selector);
    await this.testInfo.attach(name, { body: buf, contentType: 'image/png' });
  }

  // ─── Private helpers ──────────────────────────────────────────────────────

  private resolveLocator(selector: string | Locator): Locator {
    // Always resolve to the first match to avoid strict-mode violations
    // when a selector matches multiple elements (e.g. repeated wallet cards).
    const loc = typeof selector === 'string' ? this.page.locator(selector) : selector;
    return loc.first();
  }

  private snapshotName(name: string): string[] {
    // Playwright toHaveScreenshot accepts an array that maps to a sub-path
    // under snapshotDir: [project, sanitised-title, name.png]
    const projectName = this.testInfo.project.name;
    const title = this.testInfo.title.replace(/[^a-z0-9-]/gi, '_').toLowerCase();
    return [projectName, title, `${name}.png`];
  }
}

// ─── Cross-app visual diff utility ───────────────────────────────────────────

/**
 * Compare two raw screenshot Buffers and return a similarity ratio [0, 1].
 * Uses a simple pixel-count heuristic (no external lib required).
 * For accurate per-pixel diffs, use Playwright's built-in toHaveScreenshot instead.
 */
export function screenshotSimilarity(a: Buffer, b: Buffer): number {
  if (a.length !== b.length) return 0;
  let same = 0;
  for (let i = 0; i < a.length; i++) {
    if (a[i] === b[i]) same++;
  }
  return same / a.length;
}

/**
 * Key UI regions to capture for each comparable page.
 * Tests import these constants to keep selectors in one place.
 */
export const REGIONS = {
  // ─── Common ────────────────────────────────────────────────────────────
  NAV: 'nav, [data-testid="nav"], .sidebar, aside',
  HEADER: 'header, [data-testid="header"]',

  // ─── Wallet ────────────────────────────────────────────────────────────
  // Next.js: data-testid="wallet-banner"  Vue3: div.wallet-card
  WALLET_BANNER: '[data-testid="wallet-banner"], .wallet-card',
  // Next.js: table  Vue3: table inside #monthlyTable
  WALLET_TABLE: '#monthlyTable table, [data-testid="transaction-table"], table',
  // Next.js: data-testid="wallet-tabs" > button  Vue3: div.tabs-nav
  WALLET_TABS: '[data-testid="wallet-tabs"], div.tabs-nav',

  // ─── Account ───────────────────────────────────────────────────────────
  ACCOUNT_LIST:   '[data-testid="account-list"], .account-list, .account-cards',
  ACCOUNT_DETAIL: '[data-testid="account-detail"], .account-detail',

  // ─── Dashboard ─────────────────────────────────────────────────────────
  DASHBOARD_STATS: '[data-testid="stats"], .stat-cards, .dashboard-stats',
  DASHBOARD_CHART: '[data-testid="chart"], .chart-wrapper, canvas',

  // ─── Auth ──────────────────────────────────────────────────────────────
  LOGIN_FORM: 'form, [data-testid="login-form"], .sign-in-form',
} as const;
