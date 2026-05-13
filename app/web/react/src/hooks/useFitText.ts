'use client';

import { useEffect, useRef } from 'react';

/**
 * 让文字在单行内自动缩小字号以适应容器可用宽度。
 * 正常保持 maxFontSize，超出才缩到 minFontSize。
 */
export function useFitText(maxFontSize = 20, minFontSize = 12) {
  const ref = useRef<HTMLSpanElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const fit = () => {
      const parent = el.parentElement;
      if (!parent) return;

      // 重置到最大字号
      el.style.fontSize = `${maxFontSize}px`;
      el.style.whiteSpace = 'nowrap';
      el.style.display = 'inline-block';

      // getBoundingClientRect 在 flex 子元素中更可靠
      const parentRect = parent.getBoundingClientRect();
      const elRect = el.getBoundingClientRect();

      // 可用宽度 = 父容器右边界 - el 左边界（即 el 之后的剩余空间 + el 自身当前宽度）
      const available = parentRect.right - elRect.left;

      if (available <= 0) return;

      // el 自然宽度超出可用宽度才缩小
      let size = maxFontSize;
      while (el.getBoundingClientRect().width > available && size > minFontSize) {
        size -= 0.5;
        el.style.fontSize = `${size}px`;
      }
    };

    // 监听整个卡片容器（最近的有限宽度祖先）的变化
    const card = el.closest('[class*="rounded-xl"]') as HTMLElement ?? el.parentElement!;
    const ro = new ResizeObserver(fit);
    ro.observe(card);
    fit();

    return () => ro.disconnect();
  }, [maxFontSize, minFontSize]);

  return ref;
}
