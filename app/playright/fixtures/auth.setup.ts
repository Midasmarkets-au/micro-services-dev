/**
 * Auth setup — runs once before all test projects.
 * Performs login for both Next.js and Vue3 apps and saves storageState.
 *
 * Run via: npx playwright test --project=setup
 */
import { test as setup, expect } from '@playwright/test';
import path from 'path';
import fs from 'fs';
import { NEXT_BASE_URL, VUE_BASE_URL, NEXT_AUTH_FILE, VUE_AUTH_FILE } from '../playwright.config';

const EMAIL    = process.env.TEST_EMAIL    ?? 'test@example.com';
const PASSWORD = process.env.TEST_PASSWORD ?? 'TestPassword123!';

// ─── Helpers ─────────────────────────────────────────────────────────────────

function ensureAuthDir() {
  const dir = path.dirname(NEXT_AUTH_FILE);
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
}

// ─── Next.js login ───────────────────────────────────────────────────────────

setup('authenticate: Next.js', async ({ page }) => {
  ensureAuthDir();

  await page.goto(`${NEXT_BASE_URL}/sign-in`);
  await page.waitForLoadState('networkidle');

  // Fill credentials — Next app uses standard HTML inputs
  await page.locator('input[type="email"], input[name="email"], input[placeholder*="email" i]')
    .first()
    .fill(EMAIL);

  await page.locator('input[type="password"]')
    .first()
    .fill(PASSWORD);

  await page.locator('button[type="submit"]').first().click();

  // Wait until redirected away from /sign-in
  await page.waitForURL((url) => !url.pathname.includes('sign-in'), { timeout: 15_000 });

  // Verify we are authenticated (dashboard or protected page loaded)
  await expect(page).not.toHaveURL(/sign-in/);

  await page.context().storageState({ path: NEXT_AUTH_FILE });
  console.log(`[setup] Next.js auth state saved → ${NEXT_AUTH_FILE}`);
});

// ─── Vue3 login ──────────────────────────────────────────────────────────────

setup('authenticate: Vue3', async ({ page }) => {
  ensureAuthDir();

  await page.goto(`${VUE_BASE_URL}/sign-in`);
  await page.waitForLoadState('networkidle');

  // Vue3 app uses el-input components wrapping native inputs
  await page.locator('input[type="email"], input[name="email"], input[placeholder*="email" i], input[placeholder*="Email" ]')
    .first()
    .fill(EMAIL);

  await page.locator('input[type="password"]')
    .first()
    .fill(PASSWORD);

  await page.locator('button[type="submit"], .btn-sign-in, button:has-text("Sign In"), button:has-text("Login")')
    .first()
    .click();

  await page.waitForURL((url) => !url.pathname.includes('sign-in'), { timeout: 15_000 });
  await expect(page).not.toHaveURL(/sign-in/);

  // IBSelector.vue mounts in the toolbar and asynchronously calls queryAccounts
  // to initialise ibCurrentAccount in localStorage.  We must wait for all
  // in-flight network requests to settle before saving storageState, otherwise
  // ibCurrentAccount will be missing and IBWidgetSelector will redirect to "/".
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(1_000); // small buffer for Vuex → localStorage write

  await page.context().storageState({ path: VUE_AUTH_FILE });
  console.log(`[setup] Vue3 auth state saved → ${VUE_AUTH_FILE}`);
});
