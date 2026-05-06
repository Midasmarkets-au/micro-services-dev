/**
 * ApiDiff — deep-diffs two API response bodies and reports semantic mismatches.
 *
 * Design decisions:
 *  - Keys listed in IGNORE_KEYS (IDs, timestamps, request-trace fields) are
 *    stripped before comparison so infra-noise doesn't fail business logic tests.
 *  - Numeric values are compared within a configurable tolerance to handle
 *    floating-point rounding between platforms.
 *  - Array items are compared by *position* by default; pass `arrayKey` to
 *    compare by a stable key field instead (e.g. `id`, `accountNumber`).
 *  - The result object is JSON-serialisable — attach it to Playwright test
 *    annotations via `testInfo.attach()`.
 */

// Fields to strip before comparison (env-configurable)
const DEFAULT_IGNORE_KEYS = new Set(
  (process.env.API_DIFF_IGNORE_FIELDS ?? 'id,createdAt,updatedAt,timestamp,requestId,traceId,correlationId')
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean)
);

export interface DiffResult {
  matched: boolean;
  differences: DiffEntry[];
  summary: string;
}

export interface DiffEntry {
  path: string;
  nextValue: unknown;
  vueValue: unknown;
  kind: 'missing_in_next' | 'missing_in_vue' | 'type_mismatch' | 'value_mismatch';
}

export interface DiffOptions {
  /** Extra keys to ignore beyond the defaults. */
  ignoreKeys?: Set<string>;
  /** Numeric tolerance (absolute). Default: 0. */
  numericTolerance?: number;
  /** For arrays of objects — key field to use for alignment. */
  arrayKey?: string;
  /** Maximum number of differences to report before stopping. Default: 50. */
  maxDiffs?: number;
}

export function diffApiResponses(
  nextBody: unknown,
  vueBody: unknown,
  opts: DiffOptions = {}
): DiffResult {
  const ignoreKeys = new Set([...DEFAULT_IGNORE_KEYS, ...(opts.ignoreKeys ?? [])]);
  const differences: DiffEntry[] = [];
  const maxDiffs = opts.maxDiffs ?? 50;

  function walk(a: unknown, b: unknown, pathParts: string[]): void {
    if (differences.length >= maxDiffs) return;

    const path = pathParts.join('') || '/';

    // Both null/undefined
    if (a == null && b == null) return;

    // One is null/undefined
    if (a == null || b == null) {
      differences.push({
        path,
        nextValue: a,
        vueValue: b,
        kind: a == null ? 'missing_in_next' : 'missing_in_vue',
      });
      return;
    }

    const typeA = typeof a;
    const typeB = typeof b;

    // Type mismatch
    if (typeA !== typeB) {
      differences.push({ path, nextValue: a, vueValue: b, kind: 'type_mismatch' });
      return;
    }

    // Numbers
    if (typeA === 'number') {
      const tol = opts.numericTolerance ?? 0;
      if (Math.abs((a as number) - (b as number)) > tol) {
        differences.push({ path, nextValue: a, vueValue: b, kind: 'value_mismatch' });
      }
      return;
    }

    // Primitives
    if (typeA !== 'object') {
      if (a !== b) {
        differences.push({ path, nextValue: a, vueValue: b, kind: 'value_mismatch' });
      }
      return;
    }

    // Arrays
    if (Array.isArray(a) && Array.isArray(b)) {
      const aArr = a as unknown[];
      const bArr = b as unknown[];

      if (opts.arrayKey) {
        const key = opts.arrayKey;
        const bMap = new Map<unknown, unknown>(
          bArr.map((item) => [(item as Record<string, unknown>)[key], item])
        );
        for (const aItem of aArr) {
          const keyVal = (aItem as Record<string, unknown>)[key];
          const bItem = bMap.get(keyVal);
          if (bItem === undefined) {
            differences.push({
              path: `${path}[${key}=${keyVal}]`,
              nextValue: aItem,
              vueValue: undefined,
              kind: 'missing_in_vue',
            });
          } else {
            walk(aItem, bItem, [`${path}[${key}=${keyVal}]`]);
          }
        }
      } else {
        const maxLen = Math.max(aArr.length, bArr.length);
        for (let i = 0; i < maxLen; i++) {
          walk(aArr[i], bArr[i], [...pathParts.slice(0, -1), `[${i}]`]);
        }
      }
      return;
    }

    // Plain objects
    const aObj = a as Record<string, unknown>;
    const bObj = b as Record<string, unknown>;
    const allKeys = new Set([...Object.keys(aObj), ...Object.keys(bObj)]);

    for (const k of allKeys) {
      if (ignoreKeys.has(k)) continue;
      walk(aObj[k], bObj[k], [...pathParts, pathParts.length === 0 ? k : `.${k}`]);
    }
  }

  walk(nextBody, vueBody, []);

  const matched = differences.length === 0;
  const summary = matched
    ? 'API responses match'
    : `${differences.length} difference(s) found:\n` +
      differences
        .slice(0, 10)
        .map(
          (d) =>
            `  [${d.kind}] ${d.path}  next=${JSON.stringify(d.nextValue)}  vue=${JSON.stringify(d.vueValue)}`
        )
        .join('\n') +
      (differences.length > 10 ? `\n  ...and ${differences.length - 10} more` : '');

  return { matched, differences, summary };
}

/**
 * Convenience: assert two captured responses match and attach the diff report
 * to the Playwright test info on failure.
 */
import { expect, TestInfo } from '@playwright/test';
import { CapturedResponse } from './api-capture';

export async function assertApiMatch(
  nextRes: CapturedResponse | undefined,
  vueRes: CapturedResponse | undefined,
  label: string,
  testInfo: TestInfo,
  opts?: DiffOptions
): Promise<void> {
  // Both missing — warn but don't fail
  if (!nextRes && !vueRes) {
    console.warn(`[api-diff] "${label}" — no captured response in either app`);
    return;
  }

  if (!nextRes) {
    throw new Error(`[api-diff] "${label}" — response captured in Vue3 but NOT in Next.js`);
  }
  if (!vueRes) {
    throw new Error(`[api-diff] "${label}" — response captured in Next.js but NOT in Vue3`);
  }

  // HTTP status must match
  expect(nextRes.status, `${label}: HTTP status mismatch`).toBe(vueRes.status);

  const result = diffApiResponses(nextRes.body, vueRes.body, opts);

  if (!result.matched) {
    await testInfo.attach(`api-diff: ${label}`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify({ differences: result.differences }, null, 2)),
    });
    throw new Error(`[api-diff] "${label}" mismatch:\n${result.summary}`);
  }
}
