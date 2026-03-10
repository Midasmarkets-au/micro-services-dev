'use client';

import { create } from 'zustand';
import type { SalesAccount } from '@/types/sales';

interface SalesState {
  salesAccount: SalesAccount | null;
  salesAccountList: SalesAccount[];

  setSalesAccount: (account: SalesAccount | null) => void;
  setSalesAccountList: (accounts: SalesAccount[]) => void;
  clearStore: () => void;
}

export const useSalesStore = create<SalesState>()((set) => ({
  salesAccount: null,
  salesAccountList: [],

  setSalesAccount: (account) => set({ salesAccount: account }),
  setSalesAccountList: (accounts) => set({ salesAccountList: accounts }),
  clearStore: () => set({ salesAccount: null, salesAccountList: [] }),
}));
