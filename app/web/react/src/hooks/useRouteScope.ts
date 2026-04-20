'use client';

import { useCallback, useEffect, useRef } from 'react';
import { usePathname } from 'next/navigation';

/**
 * 路由作用域请求管理
 *
 * 作用：
 * 1. 为每个 effect 提供一个 AbortSignal，底层 fetch 可接入，路由切换时自动 abort
 * 2. 同时提供 isActive() 判断，用于结果回来后是否仍可安全 setState
 * 3. 组件卸载、pathname 变化、或 begin() 被再次调用时，都会自动 abort 上一次的请求
 *
 * 用法示例：
 *   const { begin } = useRouteScope('/ib');
 *   useEffect(() => {
 *     if (!deps) return;
 *     const { signal, isActive } = begin();
 *     (async () => {
 *       const res = await fetchXxx({ signal });
 *       if (!isActive()) return;
 *       setState(res.data);
 *     })();
 *   }, [deps, begin]);
 */
export function useRouteScope(expectedPath?: string) {
  const pathname = usePathname();
  const pathRef = useRef(pathname);
  const tokenRef = useRef(0);
  const controllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    pathRef.current = pathname;
    // pathname 变化：立刻把当前所有 in-flight 请求中止
    if (expectedPath && pathname !== expectedPath) {
      tokenRef.current += 1;
      controllerRef.current?.abort();
      controllerRef.current = null;
    }
  }, [pathname, expectedPath]);

  useEffect(() => {
    return () => {
      // 组件卸载：中止所有尚未完成的请求
      tokenRef.current += 1;
      controllerRef.current?.abort();
      controllerRef.current = null;
    };
  }, []);

  const begin = useCallback(() => {
    // 调用 begin 意味着开启一组新请求：先中止上一组
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const myToken = ++tokenRef.current;

    const isActive = () =>
      myToken === tokenRef.current &&
      !controller.signal.aborted &&
      (!expectedPath || pathRef.current === expectedPath);

    return { signal: controller.signal, isActive };
  }, [expectedPath]);

  const abort = useCallback(() => {
    tokenRef.current += 1;
    controllerRef.current?.abort();
    controllerRef.current = null;
  }, []);

  return { begin, abort };
}
