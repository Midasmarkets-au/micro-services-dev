import { defineConfig, devices } from '@playwright/test';
import path from 'path';
import * as dotenv from 'fs';

// Load .env if present (walk-up not needed here since we run from this dir)
const envPath = path.resolve(__dirname, '.env');
if (dotenv.existsSync(envPath)) {
  require('fs').readFileSync(envPath, 'utf-8')
    .split('\n')
    .filter((line: string) => line && !line.startsWith('#'))
    .forEach((line: string) => {
      const [key, ...rest] = line.split('=');
      if (key && rest.length) process.env[key.trim()] = rest.join('=').trim();
    });
}

const NEXT_BASE_URL = process.env.NEXT_BASE_URL ?? 'http://localhost:3000';
const VUE_BASE_URL  = process.env.VUE_BASE_URL  ?? 'http://client.localhost:8084';

export { NEXT_BASE_URL, VUE_BASE_URL };

/**
 * Auth state files — created by setup project, reused by tests.
 */
export const NEXT_AUTH_FILE = path.join(__dirname, '.auth/next.json');
export const VUE_AUTH_FILE  = path.join(__dirname, '.auth/vue.json');

export default defineConfig({
  testDir: '.',  // scan both tests/ and fixtures/ so setup is found
  fullyParallel: false,  // comparison tests need sequential ordering
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: process.env.CI ? 1 : 2,

  reporter: [
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['json', { outputFile: 'playwright-report/results.json' }],
    ['list'],
  ],

  use: {
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    // Expand viewport so visual snapshots are consistent
    viewport: { width: 1440, height: 900 },
    // Capture console errors for debugging
    ignoreHTTPSErrors: true,
  },

  snapshotDir: './snapshots',

  projects: [
    // ─── Auth setup (must run before all other projects) ──────────────────
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },

    // ─── Next.js (React) ──────────────────────────────────────────────────
    {
      name: 'next',
      testMatch: /[/\\]tests[/\\].*\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: NEXT_BASE_URL,
        storageState: NEXT_AUTH_FILE,
        // Tag all requests so api-capture can attribute them
        extraHTTPHeaders: { 'x-test-app': 'next' },
      },
      dependencies: ['setup'],
    },

    // ─── Vue3 ─────────────────────────────────────────────────────────────
    {
      name: 'vue',
      testMatch: /[/\\]tests[/\\].*\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        baseURL: VUE_BASE_URL,
        storageState: VUE_AUTH_FILE,
        extraHTTPHeaders: { 'x-test-app': 'vue' },
      },
      dependencies: ['setup'],
    },
  ],
});
