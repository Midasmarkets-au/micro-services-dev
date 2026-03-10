'use client';

import { useMemo } from 'react';

interface MiniAreaChartProps {
  data?: number[];
}

function generateRandomData(count: number): number[] {
  const arr: number[] = [];
  let val = 3 + Math.random() * 4;
  for (let i = 0; i < count; i++) {
    val += (Math.random() - 0.45) * 3;
    val = Math.max(0.5, Math.min(10, val));
    arr.push(val);
  }
  return arr;
}

function buildSmoothPath(points: [number, number][]): string {
  if (points.length < 2) return '';
  let d = `M${points[0][0]},${points[0][1]}`;
  for (let i = 0; i < points.length - 1; i++) {
    const [x0, y0] = points[i];
    const [x1, y1] = points[i + 1];
    const cpx = (x0 + x1) / 2;
    d += ` C${cpx},${y0} ${cpx},${y1} ${x1},${y1}`;
  }
  return d;
}

export function MiniAreaChart({ data }: MiniAreaChartProps) {
  const chartData = useMemo(() => data || generateRandomData(20), [data]);

  const max = Math.max(...chartData, 1);
  const w = 300;
  const h = 112;
  const padY = 6;

  const points: [number, number][] = chartData.map((val, i) => [
    (i / (chartData.length - 1)) * w,
    h - padY - ((val / max) * (h - padY * 2)),
  ]);

  const linePath = buildSmoothPath(points);
  const lastPt = points[points.length - 1];
  const areaPath = `${linePath} L${lastPt[0]},${h} L0,${h} Z`;

  const gradId = useMemo(() => `ag-${Math.random().toString(36).slice(2, 8)}`, []);

  return (
    <svg width="100%" height="100%" viewBox={`0 0 ${w} ${h}`} preserveAspectRatio="none" className="block">
      <defs>
        <linearGradient id={gradId} x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="var(--color-primary)" stopOpacity="0.25" />
          <stop offset="100%" stopColor="var(--color-primary)" stopOpacity="0.01" />
        </linearGradient>
      </defs>
      <path d={areaPath} fill={`url(#${gradId})`} />
      <path d={linePath} fill="none" stroke="var(--color-primary)" strokeWidth="1.5" />
    </svg>
  );
}
