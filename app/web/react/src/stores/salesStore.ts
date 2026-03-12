'use client';

import { create } from 'zustand';
import type { SalesAccount } from '@/types/sales';

interface SalesState {
  salesAccount: SalesAccount | null;
  salesAccountList: SalesAccount[];
  isInitialized: boolean;

  setSalesAccount: (account: SalesAccount | null) => void;
  setSalesAccountList: (accounts: SalesAccount[]) => void;
  setInitialized: (initialized: boolean) => void;
  clearStore: () => void;
}

export const useSalesStore = create<SalesState>()((set) => ({
  salesAccount: null,
  salesAccountList: [],
  isInitialized: false,

  setSalesAccount: (account) => set({ salesAccount: account }),
  setSalesAccountList: (accounts) => set({ salesAccountList: accounts }),
  setInitialized: (isInitialized) => set({ isInitialized }),
  clearStore: () => set({ salesAccount: null, salesAccountList: [], isInitialized: false }),
}));
