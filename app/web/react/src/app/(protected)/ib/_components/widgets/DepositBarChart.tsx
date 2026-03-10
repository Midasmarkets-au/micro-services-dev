'use client';

import { useMemo } from 'react';

const BAR_COUNT = 9;
const BAR_WIDTH = 11;
const BAR_GAP = 23;
const START_LEFT = 22;
const CONTAINER_H = 108;
const MAX_BAR_H = 86;
const BAR_TOP = 14;

function randomHeights(count: number): number[] {
  return Array.from({ length: count }, () => Math.round(5 + Math.random() * 65));
}

export function DepositBarChart() {
  const heights = useMemo(() => randomHeights(BAR_COUNT), []);

  return (
    <div className="relative" style={{ width: 240, height: CONTAINER_H }}>
      {Array.from({ length: BAR_COUNT }).map((_, i) => {
        const left = START_LEFT + i * BAR_GAP;
        const h = heights[i];
        return (
          <div key={i}>
            {/* Background bar (gray) */}
            <div
              className="absolute rounded-sm bg-border"
              style={{ left, top: BAR_TOP, width: BAR_WIDTH, height: MAX_BAR_H }}
            />
            {/* Foreground bar (primary) */}
            <div
              className="absolute rounded-sm bg-primary"
              style={{ left, top: BAR_TOP + MAX_BAR_H - h, width: BAR_WIDTH, height: h }}
            />
          </div>
        );
      })}
    </div>
  );
}
