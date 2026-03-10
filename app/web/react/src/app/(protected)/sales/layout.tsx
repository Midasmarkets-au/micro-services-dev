'use client';

import { usePathname } from 'next/navigation';
import { SalesSidebar } from './_components/SalesSidebar';
import { useSalesAccountInit } from '@/hooks/useSalesAccountInit';
import { useSalesStore } from '@/stores/salesStore';
import { useUserStore } from '@/stores/userStore';
import { AccountTabSwitcher } from '@/components/layout/AccountTabSwitcher';

export default function SalesLayout({ children }: { children: React.ReactNode }) {
  useSalesAccountInit();
  const pathname = usePathname();
  const { salesAccount, salesAccountList, setSalesAccount } = useSalesStore();
  const user = useUserStore((s) => s.user);

  const isHomePage = pathname === '/sales';
  const showTabSwitcher = !isHomePage && salesAccountList.length > 1;

  const handleChangeAccount = (uid: number) => {
    const found = salesAccountList.find((a) => a.uid === uid);
    if (found) {
      setSalesAccount(found);
      localStorage.setItem('sales-storage', JSON.stringify(found));
      window.location.reload();
    }
  };

  const handleUpdate = () => {
    window.location.reload();
  };

  return (
    <div className="flex w-full flex-col gap-3 md:flex-row md:items-stretch md:gap-6">
      <SalesSidebar />
      <div className="flex min-h-full min-w-0 flex-1 flex-col">
        {showTabSwitcher && (
          <div className="mb-3 flex justify-end  bg-surface p-2">
            <AccountTabSwitcher
              accounts={salesAccountList}
              selectedUid={salesAccount?.uid}
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
