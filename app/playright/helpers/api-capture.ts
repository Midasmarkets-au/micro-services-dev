/**
 * ApiCapture — intercepts all XHR / fetch responses on a Playwright Page
 * and stores them keyed by a normalised route pattern.
 *
 * Usage:
 *   const cap = new ApiCapture(page);
 *   cap.attach();
 *   await page.goto('/wallet');
 *   const walletRes = cap.get('/api/v1/wallet/primary');
 */
import { Page, Response } from '@playwright/test';

export interface CapturedResponse {
  url: string;
  normalizedPath: string;
  status: number;
  body: unknown;
  headers: Record<string, string>;
  timestamp: number;
}

// Dynamic path segments to strip: UUIDs, numeric IDs, dates
const DYNAMIC_SEGMENT = /^([0-9a-f]{8}-[0-9a-f-]{27}|[0-9]+|[0-9]{4}-[0-9]{2}-[0-9]{2})$/i;

function normalizePath(rawUrl: string): string {
  try {
    const u = new URL(rawUrl);
    const segments = u.pathname.split('/').map((seg) =>
      DYNAMIC_SEGMENT.test(seg) ? ':id' : seg
    );
    return segments.join('/');
  } catch {
    return rawUrl;
  }
}

export class ApiCapture {
  private readonly page: Page;
  private readonly store = new Map<string, CapturedResponse[]>();
  private readonly handler: (res: Response) => void;

  constructor(page: Page) {
    this.page = page;
    this.handler = this.onResponse.bind(this);
  }

  attach(): void {
    this.page.on('response', this.handler);
  }

  detach(): void {
    this.page.off('response', this.handler);
  }

  clear(): void {
    this.store.clear();
  }

  private async onResponse(res: Response): Promise<void> {
    const url = res.url();

    // Only capture API calls (skip static assets, next-image, etc.)
    if (!this.isApiUrl(url)) return;

    let body: unknown = null;
    try {
      const ct = res.headers()['content-type'] ?? '';
      if (ct.includes('application/json')) {
        body = await res.json();
      }
    } catch {
      // binary / non-JSON — ignore body
    }

    const entry: CapturedResponse = {
      url,
      normalizedPath: normalizePath(url),
      status: res.status(),
      body,
      headers: res.headers(),
      timestamp: Date.now(),
    };

    const key = entry.normalizedPath;
    const existing = this.store.get(key) ?? [];
    existing.push(entry);
    this.store.set(key, existing);
  }

  private isApiUrl(url: string): boolean {
    if (url.includes('/_next/') || url.includes('/node_modules/')) return false;
    if (/\.(js|css|png|jpg|svg|ico|woff2?|ttf)(\?.*)?$/.test(url)) return false;
    return url.includes('/api/') || url.includes('/connect/') || url.includes('/v1/');
  }

  /** Return the latest captured response for a normalised path. */
  get(normalizedPath: string): CapturedResponse | undefined {
    const entries = this.store.get(normalizedPath);
    return entries?.at(-1);
  }

  /** Return all responses whose normalised path starts with a prefix. */
  getByPrefix(prefix: string): CapturedResponse[] {
    const results: CapturedResponse[] = [];
    for (const [key, entries] of this.store.entries()) {
      if (key.startsWith(prefix)) {
        results.push(...entries);
      }
    }
    return results;
  }

  /** Return a snapshot of all captured paths (for debugging). */
  capturedPaths(): string[] {
    return Array.from(this.store.keys());
  }

  /** Return the full store as a plain object (for serialisation / diff). */
  snapshot(): Record<string, CapturedResponse[]> {
    return Object.fromEntries(this.store.entries());
  }

  /**
   * Wait until at least one response matching `pathOrPattern` has been captured.
   * Useful when navigation triggers an async API call.
   */
  async waitFor(pathOrPattern: string | RegExp, timeoutMs = 10_000): Promise<CapturedResponse> {
    const deadline = Date.now() + timeoutMs;
    while (Date.now() < deadline) {
      for (const [key, entries] of this.store.entries()) {
        const match =
          typeof pathOrPattern === 'string'
            ? key === pathOrPattern || key.includes(pathOrPattern)
            : pathOrPattern.test(key);
        if (match && entries.length > 0) return entries.at(-1)!;
      }
      await new Promise((r) => setTimeout(r, 100));
    }
    throw new Error(`[ApiCapture] Timeout: no response matched "${pathOrPattern}" within ${timeoutMs}ms.\nCaptured: ${this.capturedPaths().join(', ') || '(none)'}`);
  }
}
