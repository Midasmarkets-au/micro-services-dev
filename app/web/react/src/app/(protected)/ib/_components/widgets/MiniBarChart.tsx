'use client';

import { useMemo } from 'react';

interface MiniBarChartProps {
  data?: number[];
  grouped?: boolean;
}

function generateRandomData(count: number): number[] {
  return Array.from({ length: count }, () => 2 + Math.random() * 8);
}

export function MiniBarChart({ data, grouped = false }: MiniBarChartProps) {
  const chartData = useMemo(() => data || generateRandomData(grouped ? 18 : 12), [data, grouped]);
  const max = Math.max(...chartData, 1);

  if (grouped) {
    const pairs: [number, number][] = [];
    for (let i = 0; i < chartData.length - 1; i += 2) {
      pairs.push([chartData[i], chartData[i + 1]]);
    }

    return (
      <div className="flex size-full items-end gap-1">
        {pairs.map(([a, b], i) => {
          const ha = Math.max((a / max) * 100, 6);
          const hb = Math.max((b / max) * 100, 6);
          return (
            <div key={i} className="flex flex-1 items-end gap-0.5">
              <div
                className="flex-1 rounded-t bg-border"
                style={{ height: `${ha}%`, minHeight: '3px' }}
              />
              <div
                className="flex-1 rounded-t bg-(--color-primary)/80"
                style={{ height: `${hb}%`, minHeight: '3px' }}
              />
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <div className="flex size-full items-end gap-px">
      {chartData.map((val, i) => {
        const h = Math.max((val / max) * 100, 4);
        return (
          <div
            key={i}
            className="flex-1 rounded-t bg-(--color-primary)/70"
            style={{ height: `${h}%`, minHeight: '2px' }}
          />
        );
      })}
    </div>
  );
}
