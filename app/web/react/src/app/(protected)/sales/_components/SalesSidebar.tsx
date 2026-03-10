'use client';

import { CenterSidebar, type CenterMenuItem } from '@/components/layout/CenterSidebar';

const salesMenuItems: CenterMenuItem[] = [
  { id: 'dashboard', path: '/sales', labelKey: 'dashboard', icon: '/images/icons/ib/gailan.svg', group: 0 },
  { id: 'new-customers', path: '/sales/new-customers', labelKey: 'newCustomers', icon: '/images/icons/ib/xinkehu.svg', group: 1 },
  { id: 'customers', path: '/sales/customers', labelKey: 'customers', icon: '/images/icons/ib/kehu.svg', group: 1 },
  { id: 'trade', path: '/sales/trade', labelKey: 'trade', icon: '/images/icons/ib/jiaoyi.svg', group: 2 },
  { id: 'deposit', path: '/sales/deposit', labelKey: 'deposit', icon: '/images/icons/ib/rujin.svg', group: 2 },
  { id: 'withdrawal', path: '/sales/withdrawal', labelKey: 'withdrawal', icon: '/images/icons/ib/qukuan.svg', group: 2 },
  { id: 'link', path: '/sales/link', labelKey: 'link', icon: '/images/icons/ib/lianjie.svg', group: 3 },
];

export function SalesSidebar() {
  return <CenterSidebar items={salesMenuItems} basePath="/sales" translationNamespace="sales" />;
}
