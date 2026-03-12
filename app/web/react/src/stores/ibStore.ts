'use client';

import { create } from 'zustand';
import type { AgentAccount } from '@/types/ib';

interface IBState {
  agentAccount: AgentAccount | null;
  agentAccountList: AgentAccount[];
  isInitialized: boolean;

  setAgentAccount: (account: AgentAccount | null) => void;
  setAgentAccountList: (accounts: AgentAccount[]) => void;
  setInitialized: (initialized: boolean) => void;
  clearStore: () => void;
}

export const useIBStore = create<IBState>()((set) => ({
  agentAccount: null,
  agentAccountList: [],
  isInitialized: false,

  setAgentAccount: (account) => set({ agentAccount: account }),
  setAgentAccountList: (accounts) => set({ agentAccountList: accounts }),
  setInitialized: (isInitialized) => set({ isInitialized }),
  clearStore: () => set({ agentAccount: null, agentAccountList: [], isInitialized: false }),
}));
