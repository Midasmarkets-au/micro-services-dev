'use client';

import { create } from 'zustand';
import type { AgentAccount } from '@/types/ib';

interface IBState {
  agentAccount: AgentAccount | null;
  agentAccountList: AgentAccount[];

  setAgentAccount: (account: AgentAccount | null) => void;
  setAgentAccountList: (accounts: AgentAccount[]) => void;
  clearStore: () => void;
}

export const useIBStore = create<IBState>()((set) => ({
  agentAccount: null,
  agentAccountList: [],

  setAgentAccount: (account) => set({ agentAccount: account }),
  setAgentAccountList: (accounts) => set({ agentAccountList: accounts }),
  clearStore: () => set({ agentAccount: null, agentAccountList: [] }),
}));
