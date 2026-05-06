/**
 * Extended Playwright fixtures used across all comparison tests.
 *
 * Provides:
 *  - `appName`  — current project name ('next' | 'vue')
 *  - `baseURL`  — resolved base URL for the current project
 *  - `capture`  — ApiCapture instance already attached to the current page
 *  - `visual`   — VisualHelper bound to the current page
 */
import { test as base, Page } from '@playwright/test';
import { ApiCapture } from '../helpers/api-capture';
import { VisualHelper } from '../helpers/visual';
import { NEXT_BASE_URL, VUE_BASE_URL } from '../playwright.config';

export type AppName = 'next' | 'vue';

type CompareFixtures = {
  appName: AppName;
  appBaseURL: string;
  capture: ApiCapture;
  visual: VisualHelper;
};

export const test = base.extend<CompareFixtures>({
  // Derive which app this test run belongs to from the project name
  appName: async ({}, use, testInfo) => {
    const name = testInfo.project.name as AppName;
    await use(name);
  },

  appBaseURL: async ({ appName }, use) => {
    await use(appName === 'next' ? NEXT_BASE_URL : VUE_BASE_URL);
  },

  // Attach API capture to the page before the test
  capture: async ({ page }, use) => {
    const cap = new ApiCapture(page);
    cap.attach();
    await use(cap);
    cap.detach();
  },

  // Visual helper — wraps page.screenshot + toMatchSnapshot
  visual: async ({ page }, use, testInfo) => {
    const helper = new VisualHelper(page, testInfo);
    await use(helper);
  },
});

export { expect } from '@playwright/test';
