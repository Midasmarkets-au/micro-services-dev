import { LedgerSideType } from "./StateInfos";

export type PaymentService = {
  id: number;
  platform: number | null;
  name: string | null;
  configuration: string | null;
  sequence: number | null;
  description: string | null;
  isActivated: number | null;
  canDeposit: number | null;
  canWithdraw: number | null;
  paymentInfos: any[] | null;
  payments: any[] | null;
};

export enum PaymentStatusTypes {
  Pending = 0,
  Executing = 1,
  Completed = 2,
  Failed = 3,
  Cancelled = 4,
}

export type PaymentInfo = {
  id: number;
  pid: number | null;
  partyId: number | null;
  ledgerSide: LedgerSideType | null;
  paymentServiceId: number | null;
  number: string | null;
  currencyId: number;
  amount: number;
  createdOn: string;
  updatedOn: string | null;
  status: PaymentStatusTypes;
  currency: any;
  deposits: any[] | null;
  inversePidNavigation: any[] | null;
  party: any;
  paymentService: PaymentService | null;
  pidNavigation: null | null;
  withdrawals: any[] | null;
};

export enum PaymentPlatformTypes {
  Undefined = -1,
  System = 1,
  Manual = 2,
  Cash = 10,
  Check = 20,
  Wire = 100, // 以後 100 都用 WireDeposit
  WireDeposit = 100,
  WireWithdraw = 101,
  WholeSaleWireDeposit = 102,
  Help2Pay = 200,
  Poli = 210,
  OCEX = 220,
  ChipPay = 250,
  ChipPayNVD = 251,
  UniotcPay = 260,
  BipiPay = 280,
  // BankWire = 30,
  // UnionPay = 40,
  // Otc365 = 50,
  // FasaPay = 60,
  //
  // // ReSharper disable once InconsistentNaming
  // USDT = 70,
  // Help2Pay = 80,
  // SystemRebate = 500,
}

export enum PaymentServiceTypes {
  Undefined = -1,
  System = 1,
  Manual = 2,
  Cash = 10,
  Check = 20,
  Wire = 100, // 以後 100 都用 WireDeposit
  WireDeposit = 100,
  WireWithdraw = 101,
  WholeSaleWireDeposit = 102,
  Help2Pay = 200,
  Poli = 210,
  OCEX = 220,
  ChipPay = 250,
  ChipPayNVD = 251,
  UniotcPay = 260,
  BipiPay = 280,
  USDTERC = 110,
  USDTTRC = 111,
  // BankWire = 30,
  // UnionPay = 40,
  // Otc365 = 50,
  // FasaPay = 60,
  //
  // // ReSharper disable once InconsistentNaming
  // USDT = 70,
  // Help2Pay = 80,
  // SystemRebate = 500,
}
