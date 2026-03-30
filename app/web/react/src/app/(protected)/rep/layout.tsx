'use client';

import { usePathname } from 'next/navigation';
import { RepSidebar } from './_components/RepSidebar';
import { useRepAccountInit } from '@/hooks/useRepAccountInit';
import { useRepStore } from '@/stores/repStore';
import { useUserStore } from '@/stores/userStore';
import { AccountTabSwitcher } from '@/components/layout/AccountTabSwitcher';

export default function RepLayout({ children }: { children: React.ReactNode }) {
  useRepAccountInit();
  const pathname = usePathname();
  const { repAccount, repAccountList, setRepAccount } = useRepStore();
  const user = useUserStore((s) => s.user);

  const isHomePage = pathname === '/rep/customers';
  const showTabSwitcher = !isHomePage && repAccountList.length > 1;

  const handleChangeAccount = (uid: number) => {
    const found = repAccountList.find((a) => a.uid === uid);
    if (found) {
      setRepAccount(found);
      localStorage.setItem('rep-storage', JSON.stringify(found));
      window.location.reload();
    }
  };

  const handleUpdate = () => {
    window.location.reload();
  };

  return (
    <div className="flex w-full flex-col gap-3 md:flex-row md:items-stretch md:gap-6">
      <RepSidebar />
      <div className="flex min-h-full min-w-0 flex-1 flex-col">
        {showTabSwitcher && (
          <div className="mb-3 flex justify-end bg-surface p-2">
            <AccountTabSwitcher
              accounts={repAccountList}
              selectedUid={repAccount?.uid}
              defaultAccountUid={user?.defaultSalesAccount}
              onChangeAccount={handleChangeAccount}
              onUpdate={handleUpdate}
            />
          </div>
        )}
        {children}
      </div>
    </div>
  );
}
