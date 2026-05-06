/**
 * compare.ts — Shared utilities for running the same scenario in both apps
 * and immediately diffing the results.
 *
 * Use this when you want a single test that opens both apps concurrently
 * and compares them side-by-side (requires `--project=next` AND
 * `--project=vue` to run in the same worker — which Playwright doesn't
 * support natively).  For that reason we provide a helper that accepts two
 * already-navigated Page instances (obtained from two browser contexts) so
 * tests can drive both sides themselves.
 *
 * Example (inside a custom setup file or a specialised test):
 *
 *   import { comparePage } from '../compare';
 *   const { nextCap, vueCap } = await comparePage(browser, '/wallet', testInfo);
 *   await assertApiMatch(nextCap.get('/api/v1/wallet/primary'), vueCap.get('/api/v1/wallet/primary'), 'wallet', testInfo);
 */
import { Browser, BrowserContext, Page, TestInfo } from '@playwright/test';
import { ApiCapture } from '../helpers/api-capture';
import { NEXT_AUTH_FILE, VUE_AUTH_FILE, NEXT_BASE_URL, VUE_BASE_URL } from '../playwright.config';

export interface CompareSession {
  nextPage: Page;
  vuePage: Page;
  nextCap: ApiCapture;
  vueCap: ApiCapture;
  nextCtx: BrowserContext;
  vueCtx: BrowserContext;
  close: () => Promise<void>;
}

/**
 * Open both apps at `path` concurrently using saved auth states.
 * The caller must call `session.close()` after the test.
 */
export async function comparePage(
  browser: Browser,
  path: string,
  _testInfo: TestInfo
): Promise<CompareSession> {
  const [nextCtx, vueCtx] = await Promise.all([
    browser.newContext({ storageState: NEXT_AUTH_FILE, baseURL: NEXT_BASE_URL, viewport: { width: 1440, height: 900 } }),
    browser.newContext({ storageState: VUE_AUTH_FILE,  baseURL: VUE_BASE_URL,  viewport: { width: 1440, height: 900 } }),
  ]);

  const [nextPage, vuePage] = await Promise.all([nextCtx.newPage(), vueCtx.newPage()]);

  const nextCap = new ApiCapture(nextPage);
  const vueCap  = new ApiCapture(vuePage);
  nextCap.attach();
  vueCap.attach();

  await Promise.all([
    nextPage.goto(`${NEXT_BASE_URL}${path}`).then(() => nextPage.waitForLoadState('networkidle')),
    vuePage.goto(`${VUE_BASE_URL}${path}`).then(() => vuePage.waitForLoadState('networkidle')),
  ]);

  return {
    nextPage,
    vuePage,
    nextCap,
    vueCap,
    nextCtx,
    vueCtx,
    close: async () => {
      nextCap.detach();
      vueCap.detach();
      await Promise.all([nextCtx.close(), vueCtx.close()]);
    },
  };
}
