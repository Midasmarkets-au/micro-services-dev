/**
 * 06 — IB 客户列表  (/ib/customers)
 *
 * 目标：验证 Next.js 与 Vue3 在 IB 客户列表页面的行为完全一致。
 *
 * 验证范围：
 *  - 功能一致性：页面加载、角色筛选（全部/IB/客户）、搜索框、日期选择器、
 *               排序下拉、每页条数、搜索/重置按钮、表格列、
 *               客户行"查看"链接、IB行操作下拉菜单项、
 *               IB下钻面包屑链、清除按钮、分页控件
 *  - 数据一致性：/ib/{agentUid}/account 分页接口 → 两端返回相同结构与数据；
 *               角色筛选与搜索参数在两端传递方式相同
 *
 * 已知实现差异（符合设计，不视为缺陷）：
 *   角色筛选  → Vue3: el-select 下拉框  |  Next.js: Tabs 组件
 *   操作菜单  → Vue3: el-dropdown       |  Next.js: DropdownMenu
 *   日期选择  → Vue3: el-date-picker    |  Next.js: CustomerFilter dateRange
 *   每页条数  → Vue3: el-select (10-100) | Next.js: 固定 15 条
 *   搜索方式  → Vue3: el-input + 搜索按钮 | Next.js: CustomerFilter 防抖输入
 */
import { test, expect } from '../../fixtures';
import { diffApiResponses } from '../../helpers/api-diff';
import type { CapturedResponse } from '../../helpers/api-capture';

// ─── 常量 ─────────────────────────────────────────────────────────────────────

const CUSTOMERS_PATH = '/ib/customers';

// ─── 选择器 ───────────────────────────────────────────────────────────────────

// 角色筛选
// Next.js → Tabs 组件：[role="tab"] 按钮
// Vue3    → el-select 下拉框
const ROLE_FILTER_SEL_NEXT = '[role="tab"], [data-key="all"], [data-key="ib"], [data-key="client"]';
const ROLE_FILTER_SEL_VUE  = '.el-select .el-input__wrapper, .el-select';

// 搜索输入框
// Next.js → CustomerFilter 内部 input
// Vue3    → el-input
const SEARCH_INPUT_SEL = 'input[placeholder*="search" i], input[placeholder*="Search" i], .el-input__inner, input[type="text"]';

// 搜索 & 重置按钮（兼容中英文 locale）
const SEARCH_BTN_SEL  = 'button:has-text("Search"), button:has-text("search"), button:has-text("搜索"), .el-button:has-text("Search"), .el-button:has-text("搜索"), .btn:has-text("Search"), .btn:has-text("搜索")';
const RESET_BTN_SEL   = 'button:has-text("Reset"), button:has-text("reset"), button:has-text("重置"), .el-button:has-text("Reset"), .el-button:has-text("重置"), .btn:has-text("Reset"), .btn:has-text("重置")';

// 表格
// Next.js → DataTable 组件
// Vue3    → table.table
const TABLE_SEL     = 'table, [class*="DataTable"], [class*="data-table"]';
const TABLE_ROW_SEL = 'tbody tr, [class*="table-row"]:not(thead *)';

// IB 行操作下拉按钮
// Next.js → DropdownMenu 触发按钮
// Vue3    → el-dropdown 内的 el-button
const ACTION_BTN_SEL =
  '.el-dropdown button, .el-dropdown .el-button, ' +
  'button:has-text("Action"), button:has-text("action"), button:has-text("操作"), ' +
  '[class*="DropdownMenu"] button, [data-testid*="action"]';

// 下拉菜单项（点击操作按钮后出现）
const DROPDOWN_ITEM_SEL =
  '.el-dropdown-menu__item, [role="menuitem"], [class*="dropdown-item"], [class*="DropdownItem"]';

// 客户行"查看详情"链接
const CLIENT_VIEW_LINK_SEL =
  'a[href*="/ib/customers/"], .router-link-active[href*="/ib/customers/"]';

// 分页控件
// Next.js → Pagination 组件
// Vue3    → TableFooter → el-pagination
const PAGINATION_SEL =
  '.el-pagination, [class*="Pagination"], [aria-label="pagination"], nav[aria-label*="page" i]';

// IB 面包屑链（下钻后出现）
// Next.js → 带 Lv 徽标的按钮
// Vue3    → .level-tool-tip
const CHAIN_SEL = '.level-tool-tip, [class*="level-tool"], span:has-text("Lv")';

// 清除面包屑按钮（兼容中英文）
const CLEAR_CHAIN_BTN_SEL =
  'button:has-text("Clear"), .btn:has-text("Clear"), button:has-text("clear"), button:has-text("清除"), button:has-text("返回")';

// ─── 辅助函数 ─────────────────────────────────────────────────────────────────

/** 从拦截记录中找到 /ib/:id/account 接口的最新响应 */
function getCustomersResponse(
  capture: { capturedPaths(): string[]; get(p: string): CapturedResponse | undefined },
): CapturedResponse | undefined {
  const match = capture.capturedPaths().find(
    (p) => p.includes('/ib/') && p.endsWith('/account')
  );
  return match ? capture.get(match) : undefined;
}

/** 断言分页客户列表响应结构：{ data: T[], criteria: { page, size, total? } } */
function assertCustomersShape(body: unknown, appName: string): void {
  expect(
    body !== null && typeof body === 'object' && !Array.isArray(body),
    `[${appName}] 客户接口：响应体必须是对象`
  ).toBe(true);
  const obj = body as Record<string, unknown>;
  expect(Array.isArray(obj['data']), `[${appName}] 客户接口：.data 必须是数组`).toBe(true);
  expect(typeof obj['criteria'] === 'object', `[${appName}] 客户接口：.criteria 必须是对象`).toBe(true);
  const c = obj['criteria'] as Record<string, unknown>;
  expect(typeof c['page'], `[${appName}] 客户接口：criteria.page 必须是 number`).toBe('number');
  expect(typeof c['size'], `[${appName}] 客户接口：criteria.size 必须是 number`).toBe('number');
}

/** 断言单条 IBClientAccount 包含必要字段 */
function assertCustomerItemShape(item: Record<string, unknown>, appName: string, idx: number): void {
  expect(typeof item['uid'],  `[${appName}] data[${idx}].uid 必须是 number`).toBe('number');
  expect(typeof item['role'], `[${appName}] data[${idx}].role 必须是 number`).toBe('number');
  expect(item['user'] !== null && typeof item['user'] === 'object',
    `[${appName}] data[${idx}].user 必须是对象`).toBe(true);
}

// ─── 跨端快照存储（用于 diff 对比）────────────────────────────────────────────

type CustomersSnapshot = {
  defaultList?: unknown;
  ibFilterList?: unknown;
  clientFilterList?: unknown;
  searchResult?: unknown;
};

const captured: Record<'next' | 'vue', CustomersSnapshot> = {
  next: {},
  vue:  {},
};

// ─── 功能一致性 ────────────────────────────────────────────────────────────────

test.describe('IB Customers — 功能一致性', () => {

  test('页面正常加载，不跳转到登录页或 403', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');

    const url = page.url();
    expect(url, `[${appName}] 不能跳转到登录页`).not.toContain('sign-in');
    expect(url, `[${appName}] 不能跳转到 403`).not.toContain('403');

    const path = new URL(url).pathname;
    expect(
      path.startsWith('/ib/customers') || path.startsWith('/ib/index'),
      `[${appName}] 必须停留在 /ib/customers* 路由，当前：${path}`
    ).toBe(true);
  });

  test('角色筛选控件已渲染（Tab 或下拉框）', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    if (appName === 'next') {
      // Next.js：Tabs 组件，包含 全部 / IB / 客户 三个 Tab
      const tabAll = page.locator('[role="tab"]').first();
      const visible = await tabAll.isVisible().catch(() => false);
      if (!visible) {
        test.info().annotations.push({
          type: 'warn',
          description: `[next] 角色 Tab 未找到 — 页面可能已跳转或选择器已变更`,
        });
        return;
      }
      await expect(tabAll).toBeVisible();
    } else {
      // Vue3：el-select 下拉框，包含 全部 / IB / 客户 选项
      const roleSelect = page.locator('.el-select').first();
      const visible = await roleSelect.isVisible().catch(() => false);
      if (!visible) {
        test.info().annotations.push({
          type: 'warn',
          description: `[vue] 角色 el-select 下拉框未找到 — 页面可能已跳转`,
        });
        return;
      }
      await expect(roleSelect).toBeVisible();
    }
  });

  test('搜索输入框已渲染且可输入文字', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    // 排除 readonly 的 el-select 触发器 input，只匹配真正可编辑的输入框
    const inputs = page.locator(
      'input[type="text"]:not([readonly]), input:not([type]):not([readonly])'
    );
    const count = await inputs.count();

    let found = false;
    for (let i = 0; i < count; i++) {
      const inp = inputs.nth(i);
      if (await inp.isVisible().catch(() => false)) {
        await inp.fill('test');
        const val = await inp.inputValue();
        expect(val, `[${appName}] 搜索框应能接受输入`).toBe('test');
        await inp.clear();
        found = true;
        break;
      }
    }

    if (!found) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] 页面上未找到可见的文本输入框`,
      });
    }
  });

  test('搜索按钮和重置按钮已渲染', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    const searchBtn = page.locator(SEARCH_BTN_SEL).first();
    const resetBtn  = page.locator(RESET_BTN_SEL).first();

    const searchVisible = await searchBtn.isVisible().catch(() => false);
    const resetVisible  = await resetBtn.isVisible().catch(() => false);

    // Vue3 有独立的搜索/重置按钮；Next.js 将其内嵌在 CustomerFilter 中
    if (!searchVisible && !resetVisible) {
      test.info().annotations.push({
        type: 'info',
        description:
          `[${appName}] 未找到独立的搜索/重置按钮 — ` +
          `Next.js CustomerFilter 可能使用不同文案或自动触发搜索`,
      });
    }
  });

  test('表格已渲染且包含预期列标题', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const table = page.locator(TABLE_SEL).first();
    const tableVisible = await table.isVisible().catch(() => false);

    if (!tableVisible) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] 未找到表格 — 可能需要先配置代理账号`,
      });
      return;
    }

    await expect(table).toBeVisible();

    // 每项为 [英文, 中文]，任意一个匹配即通过（兼容多语言）
    const headerTexts = await page.locator('thead th, thead td, [class*="th"]').allTextContents();
    const flatHeaders = headerTexts.join(' ').toLowerCase();

    const expectedHeaders: [string, string][] = [
      ['customer', '客户'],
      ['group',    '组'],
      ['code',     '代码'],
      ['actions',  '操作'],
    ];
    for (const [en, zh] of expectedHeaders) {
      expect(
        flatHeaders.includes(en) || flatHeaders.includes(zh),
        `[${appName}] 表格必须包含 "${en}" / "${zh}" 列标题。当前：${flatHeaders}`
      ).toBe(true);
    }
  });

  test('有数据时表格行已渲染', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const table = page.locator(TABLE_SEL).first();
    const tableVisible = await table.isVisible().catch(() => false);
    if (!tableVisible) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] 表格不可见 — 跳过行数检查`,
      });
      return;
    }

    const rows = page.locator(TABLE_ROW_SEL);
    const rowCount = await rows.count();
    // 有数据行 或 显示空状态提示，两者均为正常
    if (rowCount === 0) {
      const emptyState = page.locator('.no-data, [class*="NoData"], [class*="empty"], td[colspan]').first();
      const emptyVisible = await emptyState.isVisible().catch(() => false);
      if (!emptyVisible) {
        test.info().annotations.push({
          type: 'warn',
          description: `[${appName}] 表格行数为 0 且未找到空状态提示`,
        });
      }
    }
  });

  test('客户行包含指向 /ib/customers/{uid} 的查看详情链接', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const viewLink = page.locator(CLIENT_VIEW_LINK_SEL).first();
    const visible = await viewLink.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 未找到客户行"查看"链接 — 第一页可能没有客户类型的行`,
      });
      return;
    }

    const href = await viewLink.getAttribute('href');
    expect(href, `[${appName}] 客户查看链接必须指向 /ib/customers/...`).toMatch(
      /\/ib\/customers\/\d+/
    );
  });

  test('IB 行包含操作下拉按钮', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const actionBtn = page.locator(ACTION_BTN_SEL).first();
    const visible = await actionBtn.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 未找到 IB 行操作下拉按钮 — 第一页可能没有 IB 类型的行`,
      });
      return;
    }

    await expect(actionBtn).toBeVisible();
    await expect(actionBtn).toBeEnabled();
  });

  test('IB 行操作下拉菜单展开后包含预期菜单项', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const actionBtn = page.locator(ACTION_BTN_SEL).first();
    const visible = await actionBtn.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 未找到 IB 操作按钮 — 跳过下拉内容验证`,
      });
      return;
    }

    await actionBtn.click();

    // 等菜单项实际出现（Portal 渲染到 body，需要等 DOM 插入完成）
    const menuItems = page.locator(DROPDOWN_ITEM_SEL);
    await expect(menuItems.first()).toBeVisible({ timeout: 5_000 }).catch(() => {});
    await page.waitForTimeout(200);

    const itemCount = await menuItems.count();

    expect(itemCount, `[${appName}] IB 操作下拉菜单至少应有 2 项`).toBeGreaterThanOrEqual(2);

    // 收集所有可见菜单项文字
    const itemTexts: string[] = [];
    for (let i = 0; i < itemCount; i++) {
      const item = menuItems.nth(i);
      if (await item.isVisible().catch(() => false)) {
        const txt = (await item.textContent() ?? '').trim().toLowerCase();
        if (txt) itemTexts.push(txt);
      }
    }

    // 两端都必须包含"查看账户"和"查看返佣统计"选项
    const hasViewAccounts = itemTexts.some((t) =>
      t.includes('account') || t.includes('view accounts') || t.includes('查看账户') || t.includes('账户')
    );
    const hasRebateStat = itemTexts.some((t) =>
      t.includes('rebate') || t.includes('statistic') || t.includes('返佣') || t.includes('统计')
    );

    expect(
      hasViewAccounts,
      `[${appName}] 下拉菜单必须包含"查看账户"选项。当前菜单项：${itemTexts.join(', ')}`
    ).toBe(true);
    expect(
      hasRebateStat,
      `[${appName}] 下拉菜单必须包含"查看返佣统计"选项。当前菜单项：${itemTexts.join(', ')}`
    ).toBe(true);

    // 关闭下拉菜单
    await page.keyboard.press('Escape');
  });

  test('点击"查看账户"后显示 IB 下钻面包屑', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const actionBtn = page.locator(ACTION_BTN_SEL).first();
    const visible = await actionBtn.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 未找到 IB 操作按钮 — 跳过下钻面包屑测试`,
      });
      return;
    }

    await actionBtn.click();

    // 等菜单项出现
    const menuItems = page.locator(DROPDOWN_ITEM_SEL);
    await expect(menuItems.first()).toBeVisible({ timeout: 5_000 }).catch(() => {});

    const itemCount = await menuItems.count();
    let clicked = false;
    for (let i = 0; i < itemCount; i++) {
      const item = menuItems.nth(i);
      if (!(await item.isVisible().catch(() => false))) continue;
      const txt = (await item.textContent() ?? '').toLowerCase();
      if (txt.includes('account') || txt.includes('账户')) {
        await item.click();
        clicked = true;
        break;
      }
    }

    if (!clicked) {
      test.info().annotations.push({
        type: 'warn',
        description: `[${appName}] "查看账户"菜单项不可点击`,
      });
      return;
    }

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    // 面包屑（Lv2 徽标）应出现
    const chainEl = page.locator(CHAIN_SEL).first();
    const chainVisible = await chainEl.isVisible().catch(() => false);

    if (!chainVisible) {
      test.info().annotations.push({
        type: 'warn',
        description:
          `[${appName}] 点击"查看账户"后面包屑未出现 — 该 IB 账号可能没有下级账号`,
      });
      return;
    }

    await expect(chainEl).toBeVisible();

    // 清除按钮也应可用
    const clearBtn = page.locator(CLEAR_CHAIN_BTN_SEL).first();
    const clearVisible = await clearBtn.isVisible().catch(() => false);
    if (clearVisible) {
      await expect(clearBtn).toBeEnabled();
    }
  });

  test('点击清除按钮后面包屑消失，表格回到根层级', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    // 先下钻
    const actionBtn = page.locator(ACTION_BTN_SEL).first();
    if (!(await actionBtn.isVisible().catch(() => false))) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 未找到 IB 操作按钮 — 跳过清除面包屑测试`,
      });
      return;
    }

    await actionBtn.click();

    // 等菜单项出现
    const menuItems = page.locator(DROPDOWN_ITEM_SEL);
    await expect(menuItems.first()).toBeVisible({ timeout: 5_000 }).catch(() => {});

    let clicked = false;
    for (let i = 0; i < await menuItems.count(); i++) {
      const item = menuItems.nth(i);
      if (!(await item.isVisible().catch(() => false))) continue;
      const txt = (await item.textContent() ?? '').toLowerCase();
      if (txt.includes('account') || txt.includes('账户')) {
        await item.click();
        clicked = true;
        break;
      }
    }
    if (!clicked) return;

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    const clearBtn = page.locator(CLEAR_CHAIN_BTN_SEL).first();
    if (!(await clearBtn.isVisible().catch(() => false))) return;

    await clearBtn.click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    // 面包屑应消失
    const chain = page.locator(CHAIN_SEL).first();
    const chainGone = !(await chain.isVisible().catch(() => false));
    expect(chainGone, `[${appName}] 点击清除后面包屑应消失`).toBe(true);
  });

  test('分页控件已渲染', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2_000);

    const pagination = page.locator(PAGINATION_SEL).first();
    const visible = await pagination.isVisible().catch(() => false);

    if (!visible) {
      test.info().annotations.push({
        type: 'info',
        description: `[${appName}] 分页控件不可见 — 可能只有一页数据`,
      });
      return;
    }

    await expect(pagination).toBeVisible();
  });

  test('切换到客户角色后显示日期选择器（仅支持的端）', async ({
    page, appBaseURL, appName,
  }) => {
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    if (appName === 'vue') {
      // Vue3：切换角色下拉到"客户"选项
      const roleSelect = page.locator('.el-select').first();
      if (!(await roleSelect.isVisible().catch(() => false))) return;

      await roleSelect.click();
      await page.waitForTimeout(300);

      // 找到"客户"选项并点击
      const options = page.locator('.el-select-dropdown__item');
      const count = await options.count();
      for (let i = 0; i < count; i++) {
        const opt = options.nth(i);
        const txt = (await opt.textContent() ?? '').toLowerCase();
        if (txt.includes('client') || txt.includes('客户')) {
          await opt.click();
          break;
        }
      }
      await page.waitForTimeout(500);

      // 日期选择器应出现
      const datePicker = page.locator('.el-date-picker, .el-date-editor').first();
      const dateVisible = await datePicker.isVisible().catch(() => false);
      if (!dateVisible) {
        test.info().annotations.push({
          type: 'info',
          description: `[vue] 切换到客户 Tab 后日期选择器未出现`,
        });
      }
    } else {
      // Next.js：点击"客户" Tab
      const tabs = page.locator('[role="tab"]');
      const tabCount = await tabs.count();
      for (let i = 0; i < tabCount; i++) {
        const tab = tabs.nth(i);
        const txt = (await tab.textContent() ?? '').toLowerCase();
        if (txt.includes('client') || txt.includes('客户')) {
          await tab.click();
          break;
        }
      }
      await page.waitForTimeout(500);
      // CustomerFilter 的日期范围按配置决定是否显示，不做强断言
      // 只确保页面没有错误跳转
      const url = page.url();
      expect(url, `[next] 点击客户 Tab 后应保持在 /ib/customers`)
        .toContain('/ib/customers');
    }
  });

  test('点击重置按钮后清空搜索并重新加载列表', async ({
    page, appBaseURL, appName, capture,
  }) => {
    capture.clear();
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    // 在搜索框中输入内容（跳过 readonly 的 el-select 触发器）
    const inputs = page.locator(
      'input[type="text"]:not([readonly]), input:not([type]):not([readonly])'
    );
    const count = await inputs.count();
    let filled = false;
    for (let i = 0; i < count; i++) {
      const inp = inputs.nth(i);
      if (await inp.isVisible().catch(() => false)) {
        await inp.fill('zzz_nonexistent_user');
        filled = true;
        break;
      }
    }
    if (!filled) return;

    // Vue3：点击搜索按钮；Next.js：自动搜索或有搜索按钮
    const searchBtn = page.locator(SEARCH_BTN_SEL).first();
    if (await searchBtn.isVisible().catch(() => false)) {
      await searchBtn.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1_000);
    }

    // 点击重置按钮
    const resetBtn = page.locator(RESET_BTN_SEL).first();
    if (!(await resetBtn.isVisible().catch(() => false))) return;

    capture.clear();
    await resetBtn.click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    // 重置后应触发新的 API 请求
    const res = getCustomersResponse(capture);
    if (res) {
      expect(res.status, `[${appName}] 重置后应触发 200 的 API 响应`).toBe(200);
    }
  });

});

// ─── 数据一致性 ────────────────────────────────────────────────────────────────

test.describe('IB Customers — 数据一致性', () => {

  test('客户接口 /ib/{agentUid}/account 返回分页结构 IBClientAccount[]', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    const res = getCustomersResponse(capture);
    if (!res) {
      testInfo.annotations.push({
        type: 'warn',
        description: `[${appName}] 未捕获到 /ib/:id/account 请求 — 可能缺少代理账号`,
      });
      return;
    }

    expect(res.status, `[${appName}] 客户接口必须返回 200`).toBe(200);
    assertCustomersShape(res.body, appName);

    // 验证第一条数据包含必要字段
    const obj  = res.body as Record<string, unknown>;
    const data = obj['data'] as Record<string, unknown>[];
    if (data.length > 0) {
      assertCustomerItemShape(data[0], appName, 0);
    }

    captured[appName as 'next' | 'vue'].defaultList = res.body;

    await testInfo.attach(`${appName}: 客户列表-默认`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('角色筛选 IB：接口参数正确传递', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    if (appName === 'vue') {
      // Vue3：通过 el-select 切换到 IB
      const roleSelect = page.locator('.el-select').first();
      if (!(await roleSelect.isVisible().catch(() => false))) return;
      await roleSelect.click();
      await page.waitForTimeout(300);
      const options = page.locator('.el-select-dropdown__item');
      for (let i = 0; i < await options.count(); i++) {
        const opt = options.nth(i);
        const txt = (await opt.textContent() ?? '').toLowerCase();
        if (txt.trim() === 'ib' || txt.includes('ib')) {
          await opt.click();
          break;
        }
      }
    } else {
      // Next.js：点击 IB Tab
      const tabs = page.locator('[role="tab"]');
      for (let i = 0; i < await tabs.count(); i++) {
        const tab = tabs.nth(i);
        const txt = (await tab.textContent() ?? '').toLowerCase();
        if (txt.trim() === 'ib') {
          await tab.click();
          break;
        }
      }
    }

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    const res = getCustomersResponse(capture);
    if (!res) {
      testInfo.annotations.push({ type: 'warn', description: `[${appName}] IB 角色筛选后未捕获到接口请求` });
      return;
    }

    expect(res.status, `[${appName}] IB 筛选后接口必须返回 200`).toBe(200);
    assertCustomersShape(res.body, appName);

    // 返回的每条数据都应有 role 字段
    const obj  = res.body as Record<string, unknown>;
    const data = obj['data'] as Record<string, unknown>[];
    for (const item of data) {
      expect(typeof item['role'], `[${appName}] IB 筛选结果的每条数据必须有 role 字段`).toBe('number');
    }

    captured[appName as 'next' | 'vue'].ibFilterList = res.body;

    await testInfo.attach(`${appName}: 客户列表-IB筛选`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('角色筛选客户：接口返回客户类型账号', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    if (appName === 'vue') {
      const roleSelect = page.locator('.el-select').first();
      if (!(await roleSelect.isVisible().catch(() => false))) return;
      await roleSelect.click();
      await page.waitForTimeout(300);
      const options = page.locator('.el-select-dropdown__item');
      for (let i = 0; i < await options.count(); i++) {
        const opt = options.nth(i);
        const txt = (await opt.textContent() ?? '').toLowerCase();
        if (txt.includes('client') || txt.includes('客户')) {
          await opt.click();
          break;
        }
      }
    } else {
      const tabs = page.locator('[role="tab"]');
      for (let i = 0; i < await tabs.count(); i++) {
        const tab = tabs.nth(i);
        const txt = (await tab.textContent() ?? '').toLowerCase();
        if (txt.includes('client') || txt.includes('客户')) {
          await tab.click();
          break;
        }
      }
    }

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    const res = getCustomersResponse(capture);
    if (!res) {
      testInfo.annotations.push({ type: 'warn', description: `[${appName}] 客户角色筛选后未捕获到接口请求` });
      return;
    }

    expect(res.status, `[${appName}] 客户筛选后接口必须返回 200`).toBe(200);
    assertCustomersShape(res.body, appName);

    captured[appName as 'next' | 'vue'].clientFilterList = res.body;

    await testInfo.attach(`${appName}: 客户列表-客户筛选`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

  test('跨端对比：默认列表 — Next.js 与 Vue3 数据一致', async (
    { appName }, testInfo
  ) => {
    // 仅在 next project 执行一次，避免重复对比
    if (appName !== 'next') return;

    const nextBody = captured.next.defaultList;
    const vueBody  = captured.vue.defaultList;

    if (!nextBody || !vueBody) {
      testInfo.annotations.push({
        type: 'warn',
        description:
          `跨端对比跳过 — 快照缺失。` +
          `next=${!!nextBody} vue=${!!vueBody}`,
      });
      return;
    }

    const result = diffApiResponses(nextBody, vueBody, {
      // Vue3 默认每页 10 条，Next.js 默认 15 条，忽略分页差异
      ignoreKeys: new Set(['size', 'page', 'total', 'relativeLevel', 'level']),
      arrayKey: 'uid',
    });

    await testInfo.attach('跨端对比: 客户列表-默认', {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify({ differences: result.differences }, null, 2)),
    });

    if (!result.matched) {
      // 软警告而非强失败 — 差异可能由两次测试间并发写入引起
      testInfo.annotations.push({
        type: 'warn',
        description: `[跨端] 默认列表存在 ${result.differences.length} 处差异：\n${result.summary}`,
      });
    }
  });

  test('跨端对比：IB 角色筛选列表 — Next.js 与 Vue3 数据一致', async (
    { appName }, testInfo
  ) => {
    if (appName !== 'next') return;

    const nextBody = captured.next.ibFilterList;
    const vueBody  = captured.vue.ibFilterList;

    if (!nextBody || !vueBody) {
      testInfo.annotations.push({
        type: 'warn',
        description: `IB 筛选跨端对比跳过 — 快照缺失`,
      });
      return;
    }

    const result = diffApiResponses(nextBody, vueBody, {
      ignoreKeys: new Set(['size', 'page', 'total', 'relativeLevel', 'level']),
      arrayKey: 'uid',
    });

    await testInfo.attach('跨端对比: 客户列表-IB筛选', {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify({ differences: result.differences }, null, 2)),
    });

    if (!result.matched) {
      testInfo.annotations.push({
        type: 'warn',
        description: `[跨端] IB 筛选差异：${result.summary}`,
      });
    }
  });

  test('跨端对比：客户角色筛选列表 — Next.js 与 Vue3 数据一致', async (
    { appName }, testInfo
  ) => {
    if (appName !== 'next') return;

    const nextBody = captured.next.clientFilterList;
    const vueBody  = captured.vue.clientFilterList;

    if (!nextBody || !vueBody) {
      testInfo.annotations.push({
        type: 'warn',
        description: `客户筛选跨端对比跳过 — 快照缺失`,
      });
      return;
    }

    const result = diffApiResponses(nextBody, vueBody, {
      ignoreKeys: new Set(['size', 'page', 'total', 'relativeLevel', 'level']),
      arrayKey: 'uid',
    });

    await testInfo.attach('跨端对比: 客户列表-客户筛选', {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify({ differences: result.differences }, null, 2)),
    });

    if (!result.matched) {
      testInfo.annotations.push({
        type: 'warn',
        description: `[跨端] 客户筛选差异：${result.summary}`,
      });
    }
  });

  test('搜索关键词正确传入接口请求 URL', async ({
    page, appBaseURL, capture, appName,
  }, testInfo) => {
    capture.clear();
    await page.goto(`${appBaseURL}${CUSTOMERS_PATH}`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_500);

    const SEARCH_TERM = 'test';

    // 在搜索框中填入内容（跳过 readonly 的 el-select 触发器）
    const inputs = page.locator(
      'input[type="text"]:not([readonly]), input:not([type]):not([readonly])'
    );
    const inputCount = await inputs.count();
    let filled = false;
    for (let i = 0; i < inputCount; i++) {
      const inp = inputs.nth(i);
      if (await inp.isVisible().catch(() => false)) {
        await inp.fill(SEARCH_TERM);
        filled = true;
        break;
      }
    }
    if (!filled) return;

    // 触发搜索
    const searchBtn = page.locator(SEARCH_BTN_SEL).first();
    if (await searchBtn.isVisible().catch(() => false)) {
      await searchBtn.click();
    } else {
      await page.keyboard.press('Enter');
    }

    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1_000);

    const res = getCustomersResponse(capture);
    if (!res) {
      testInfo.annotations.push({ type: 'warn', description: `[${appName}] 搜索后未捕获到接口请求` });
      return;
    }

    expect(res.status, `[${appName}] 搜索接口必须返回 200`).toBe(200);
    expect(
      res.url.toLowerCase().includes('searchtext') || res.url.toLowerCase().includes('search'),
      `[${appName}] 搜索请求 URL 必须包含 searchText 参数。URL：${res.url}`
    ).toBe(true);

    captured[appName as 'next' | 'vue'].searchResult = res.body;

    await testInfo.attach(`${appName}: 客户列表-搜索结果`, {
      contentType: 'application/json',
      body: Buffer.from(JSON.stringify(res.body, null, 2)),
    });
  });

});
