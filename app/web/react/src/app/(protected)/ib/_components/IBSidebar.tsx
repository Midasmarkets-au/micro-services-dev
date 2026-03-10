'use client';

import { CenterSidebar, type CenterMenuItem } from '@/components/layout/CenterSidebar';

const ibMenuItems: CenterMenuItem[] = [
  { id: 'dashboard', path: '/ib', labelKey: 'dashboard', icon: '/images/icons/ib/gailan.svg', group: 0 },
  { id: 'new-customers', path: '/ib/new-customers', labelKey: 'newCustomers', icon: '/images/icons/ib/xinkehu.svg', group: 1 },
  { id: 'customers', path: '/ib/customers', labelKey: 'customers', icon: '/images/icons/ib/kehu.svg', group: 1 },
  { id: 'trade', path: '/ib/trade', labelKey: 'trade', icon: '/images/icons/ib/jiaoyi.svg', group: 2 },
  { id: 'deposit', path: '/ib/deposit', labelKey: 'deposit', icon: '/images/icons/ib/rujin.svg', group: 2 },
  { id: 'withdrawal', path: '/ib/withdrawal', labelKey: 'withdrawal', icon: '/images/icons/ib/qukuan.svg', group: 2 },
  { id: 'rebate', path: '/ib/rebate', labelKey: 'rebate', icon: '/images/icons/ib/fanyong.svg', group: 2 },
  { id: 'iblink', path: '/ib/iblink', labelKey: 'ibLink', icon: '/images/icons/ib/lianjie.svg', group: 3 },
];

export function IBSidebar() {
  return <CenterSidebar items={ibMenuItems} basePath="/ib" translationNamespace="ib" />;
}
