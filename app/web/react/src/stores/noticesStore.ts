import { create } from 'zustand';

interface NoticeItem {
  id: number;
  title: string;
  content: string;
  createdOn: string;
  updatedOn?: string;
}

interface NoticesState {
  latestNotice: NoticeItem | null;
  setLatestNotice: (notice: NoticeItem | null) => void;
}

export const useNoticesStore = create<NoticesState>((set) => ({
  latestNotice: null,
  setLatestNotice: (notice) => set({ latestNotice: notice }),
}));
