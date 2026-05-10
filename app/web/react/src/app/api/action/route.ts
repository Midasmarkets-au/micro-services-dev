import { NextRequest, NextResponse } from 'next/server';

// 导入所有只读 action 模块（Route Handler 中直接调用，不经过 React Server Action 机制）
import * as userActions from '@/actions/user';
import * as accountActions from '@/actions/accounts';
import * as contactActions from '@/actions/contact';
import * as verificationActions from '@/actions/verification';
import * as paymentActions from '@/actions/payment';
import * as addressActions from '@/actions/address';
import * as accountsActions from '@/actions/accounts';
import * as walletActions from '@/actions/wallet';
import * as eventshopActions from '@/actions/eventshop';
import * as ibActions from '@/actions/ib';
import * as salesActions from '@/actions/sales';
import * as repActions from '@/actions/rep';
import * as depositActions from '@/actions/deposit';

/**
 * Action Bridge — 通用只读 Server Action 代理
 *
 * 解决的问题：
 *   Client Components 在 useEffect 中直接调用 Server Actions 时，Next.js 的 Server Action
 *   响应机制会附带 RSC 重渲染指令，触发路由导航副作用（RSC 导航 Bug）。
 *
 * 工作原理：
 *   1. 客户端调用 POST /api/action，传入 { name, args }
 *   2. 此 Route Handler 在服务端直接调用对应的 action 函数（不经过 React 的 Server Action 机制）
 *   3. 以普通 JSON 返回结果，不附带任何 RSC 重渲染指令
 *
 * 安全：
 *   - 仅允许 get* 前缀的只读函数（通过 allowlist 校验）
 *   - 认证由各 action 内部通过 client.ts 处理
 */

// 只读 action 函数映射（所有以 get 开头的函数 + 个别安全的只读函数）
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const ACTION_REGISTRY: Record<string, (...args: any[]) => Promise<unknown>> = {};

function registerReadActions(module: Record<string, unknown>) {
  Object.entries(module).forEach(([name, fn]) => {
    if (typeof fn === 'function' && name.startsWith('get')) {
      ACTION_REGISTRY[name] = fn as (...args: unknown[]) => Promise<unknown>;
    }
  });
}

registerReadActions(userActions);
registerReadActions(accountActions);
registerReadActions(contactActions);
registerReadActions(verificationActions);
registerReadActions(paymentActions);
registerReadActions(addressActions);
registerReadActions(accountsActions);
registerReadActions(walletActions);
registerReadActions(eventshopActions);
registerReadActions(ibActions);
registerReadActions(salesActions);
registerReadActions(repActions);
registerReadActions(depositActions);

// 额外注册的非 get* 前缀只读函数
if (typeof walletActions.searchTransferTarget === 'function') {
  ACTION_REGISTRY['searchTransferTarget'] = walletActions.searchTransferTarget;
}

export async function POST(request: NextRequest) {
  let name: string;
  let args: unknown[];

  try {
    const body = await request.json() as { name?: unknown; args?: unknown };
    name = typeof body.name === 'string' ? body.name : '';
    args = Array.isArray(body.args) ? body.args : [];
  } catch {
    return NextResponse.json(
      { success: false, error: 'Invalid request body', errorCode: 'badRequest' },
      { status: 400 }
    );
  }

  const action = ACTION_REGISTRY[name];
  if (!action) {
    return NextResponse.json(
      { success: false, error: `Action "${name}" not found or not allowed`, errorCode: 'notFound' },
      { status: 404 }
    );
  }

  try {
    const result = await action(...args);
    return NextResponse.json(result);
  } catch (error) {
    console.error(`[Action Bridge] Error calling "${name}":`, error);
    return NextResponse.json(
      { success: false, error: 'Internal server error', errorCode: 'serverError' },
      { status: 500 }
    );
  }
}
