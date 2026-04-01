'use client';

import { CenterSidebar, type CenterMenuItem } from '@/components/layout/CenterSidebar';

const repMenuItems: CenterMenuItem[] = [
  { id: 'customers', path: '/rep/customers', labelKey: 'customers', icon: '/images/icons/ib/kehu.svg', group: 1 },
  { id: 'trade', path: '/rep/trade', labelKey: 'trade', icon: '/images/icons/ib/jiaoyi.svg', group: 2 },
  { id: 'transaction', path: '/rep/transaction', labelKey: 'transaction', icon: '/images/icons/ib/jiaoyi.svg', group: 2 },
  { id: 'deposit', path: '/rep/deposit', labelKey: 'deposit', icon: '/images/icons/ib/rujin.svg', group: 2 },
  { id: 'withdrawal', path: '/rep/withdrawal', labelKey: 'withdrawal', icon: '/images/icons/ib/qukuan.svg', group: 2 },
];

export function RepSidebar() {
  return <CenterSidebar items={repMenuItems} basePath="/rep/customers" translationNamespace="rep" />;
}
