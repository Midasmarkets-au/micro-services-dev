import { headers } from 'next/headers';

/** 取 XFF 链最左一段，即最初的客户端 IP */
function leftmost(value: string | null): string | undefined {
  const ip = value?.split(',')[0]?.trim();
  return ip || undefined;
}

/**
 * 透传客户端真实 IP 给后端。
 *
 * Server Action / Route Handler 里的 fetch 是从 Next 服务端发出的，后端看到的
 * 源 IP 是 Next 的出口 IP。后端 GetRemoteIpAddress() 取 X-Forwarded-For 的第一段，
 * 所以这里把入站请求的客户端 IP 原样带上，后端取到的第一段即为它。
 *
 * ingress-nginx 会把 X-Forwarded-For 覆盖成 $remote_addr（即 NLB 的 ENI 地址），
 * 同时把上游传来的原始值保留在 X-Original-Forwarded-For。生产环境入口是 Cloudflare，
 * 真实 IP 只在后者里，所以优先读它。
 *
 * 本地开发浏览器直连时两个头都没有，返回空对象，行为与改动前一致。
 */
export async function clientIpHeaders(): Promise<Record<string, string>> {
  try {
    const h = await headers();
    const clientIp =
      leftmost(h.get('x-original-forwarded-for')) ??
      leftmost(h.get('x-forwarded-for')) ??
      h.get('x-real-ip')?.trim();
    if (!clientIp) return {};
    return {
      'X-Forwarded-For': clientIp,
      'X-Real-IP': clientIp,
    };
  } catch {
    // 静态渲染期调用 headers() 会抛错
    return {};
  }
}
