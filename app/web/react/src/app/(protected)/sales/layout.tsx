'use client';

import { SalesSidebar } from './_components/SalesSidebar';
import { useSalesAccountInit } from '@/hooks/useSalesAccountInit';
import { useSalesStore } from '@/stores/salesStore';
import { useUserStore } from '@/stores/userStore';
import { AccountTabSwitcher } from '@/components/layout/AccountTabSwitcher';

export default function SalesLayout({ children }: { children: React.ReactNode }) {
  useSalesAccountInit();
  const { salesAccount, salesAccountList, setSalesAccount } = useSalesStore();
  const user = useUserStore((s) => s.user);

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
    <div className="flex min-w-0 w-full flex-1 flex-col gap-3">
      {salesAccountList.length > 1 && (
        <div className="flex w-full shrink-0 justify-end">
          <AccountTabSwitcher
            accounts={salesAccountList}
            selectedUid={salesAccount?.uid}
            defaultAccountUid={user?.defaultSalesAccount}
            onChangeAccount={handleChangeAccount}
            onUpdate={handleUpdate}
          />
        </div>
      )}
      <div className="flex min-h-0 min-w-0 w-full flex-1 flex-col gap-3 md:flex-row md:items-stretch md:gap-6">
        <SalesSidebar />
        <div className="flex min-h-full min-w-0 flex-1 flex-col">
          {children}
        </div>
      </div>
    </div>
  );
}
