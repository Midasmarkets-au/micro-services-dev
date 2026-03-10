'use client';

import { usePathname } from 'next/navigation';
import { IBSidebar } from './_components/IBSidebar';
import { useIBAccountInit } from '@/hooks/useIBAccountInit';
import { useIBStore } from '@/stores/ibStore';
import { useUserStore } from '@/stores/userStore';
import { AccountTabSwitcher } from '@/components/layout/AccountTabSwitcher';

export default function IBLayout({ children }: { children: React.ReactNode }) {
  useIBAccountInit();
  const pathname = usePathname();
  const { agentAccount, agentAccountList, setAgentAccount } = useIBStore();
  const user = useUserStore((s) => s.user);

  const isHomePage = pathname === '/ib';
  const showTabSwitcher = !isHomePage && agentAccountList.length > 1;

  const handleChangeAccount = (uid: number) => {
    const found = agentAccountList.find((a) => a.uid === uid);
    if (found) {
      setAgentAccount(found);
      localStorage.setItem('ib-storage', JSON.stringify(found));
      window.location.reload();
    }
  };

  const handleUpdate = () => {
    window.location.reload();
  };

  return (
    <div className="flex w-full flex-col gap-3 md:flex-row md:items-stretch md:gap-6">
      <IBSidebar />
      <div className="flex min-h-full min-w-0 flex-1 flex-col">
        {showTabSwitcher && (
          <div className="mb-3 flex justify-end bg-surface p-2">
            <AccountTabSwitcher
              accounts={agentAccountList}
              selectedUid={agentAccount?.uid}
              defaultAccountUid={user?.defaultAgentAccount}
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
