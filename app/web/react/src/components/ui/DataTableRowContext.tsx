'use client';

import React, { createContext, useCallback, useContext, useMemo, useState } from 'react';

interface DataTableHighlightContextValue {
  activeRowKey: string | number | null;
  setActiveRowKey: (key: string | number | null) => void;
}

interface DataTableRowContextValue {
  rowKey: string | number;
  activateRow: () => void;
}

const DataTableHighlightContext = createContext<DataTableHighlightContextValue | null>(null);
const DataTableRowContext = createContext<DataTableRowContextValue | null>(null);

export function DataTableHighlightProvider({ children }: { children: React.ReactNode }) {
  const [activeRowKey, setActiveRowKey] = useState<string | number | null>(null);
  const value = useMemo(
    () => ({ activeRowKey, setActiveRowKey }),
    [activeRowKey],
  );

  return (
    <DataTableHighlightContext.Provider value={value}>
      {children}
    </DataTableHighlightContext.Provider>
  );
}

export function DataTableRowProvider({
  rowKey,
  children,
}: {
  rowKey: string | number;
  children: React.ReactNode;
}) {
  const highlight = useContext(DataTableHighlightContext);

  const activateRow = useCallback(() => {
    highlight?.setActiveRowKey(rowKey);
  }, [highlight, rowKey]);

  const value = useMemo(
    () => ({ rowKey, activateRow }),
    [rowKey, activateRow],
  );

  return (
    <DataTableRowContext.Provider value={value}>
      {children}
    </DataTableRowContext.Provider>
  );
}

export function useDataTableHighlight() {
  return useContext(DataTableHighlightContext);
}

export function useDataTableRowContext() {
  return useContext(DataTableRowContext);
}
