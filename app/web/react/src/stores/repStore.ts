'use client';

import { create } from 'zustand';
import type { RepAccount } from '@/types/rep';

interface RepState {
  repAccount: RepAccount | null;
  repAccountList: RepAccount[];
  isInitialized: boolean;

  setRepAccount: (account: RepAccount | null) => void;
  setRepAccountList: (accounts: RepAccount[]) => void;
  setInitialized: (initialized: boolean) => void;
  clearStore: () => void;
}

export const useRepStore = create<RepState>()((set) => ({
  repAccount: null,
  repAccountList: [],
  isInitialized: false,

  setRepAccount: (account) => set({ repAccount: account }),
  setRepAccountList: (accounts) => set({ repAccountList: accounts }),
  setInitialized: (isInitialized) => set({ isInitialized }),
  clearStore: () => set({ repAccount: null, repAccountList: [], isInitialized: false }),
}));
