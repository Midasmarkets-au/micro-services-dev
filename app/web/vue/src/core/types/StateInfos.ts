import i18n from "@/core/plugins/i18n";
import { id } from "element-plus/es/locale";

const { t } = i18n.global;

export enum TransactionAccountType {
  Wallet = 1,
  TradeAccount = 2,
}

export enum LedgerSideType {
  Unknown = 0,
  Debit = 1,
  Credit = 2,
}

export enum TransactionStateType {
  Initialed = 0,
  TransferCreated = 200,
  TransferCanceled = 205,
  TransferFailed = 206,
  TransferAwaitingApproval = 210,
  TransferRejected = 215,
  TransferApproved = 220,
  TransferCompleted = 250,
  DepositCreated = 300, //
  DepositCanceled = 305,
  DepositFailed = 306,
  DepositPaymentCompleted = 310, // our system received the payment but not in user's wallet yet
  DepositCallbackTimeOut = 315, // our system received the payment but not in user's wallet yet
  DepositCentralApproved = 320,
  DepositCentralRejected = 325,
  DepositTenantApproved = 330, // tenant approved the deposit request
  DepositTenantRejected = 335,
  DepositCallbackComplete = 345,
  DepositCompleted = 350, // Already in user's wallet
  WithdrawalCreated = 400,
  WithdrawalCanceled = 405,
  WithdrawalFailed = 406,
  WithdrawalTenantApproved = 420,
  WithdrawalTenantRejected = 425,
  WithdrawalPaymentCompleted = 430,
  WithdrawalCompleted = 450,
  RebateCreated = 500,
  RebateCanceled = 505,
  RebateOnHold = 510,
  RebateReleased = 520,
  RebateCompleted = 550,
  RefundCreated = 600,
  RefundCompleted = 650,
  WalletAdjustCreated = 700,
  WalletAdjustCompleted = 750,
}

/**
 * logic type from backend
 */
export const DepositStateTypes = [
  TransactionStateType.DepositCreated,
  TransactionStateType.DepositCanceled,
  TransactionStateType.DepositFailed,
  TransactionStateType.DepositPaymentCompleted,
  TransactionStateType.DepositTenantApproved,
  TransactionStateType.DepositTenantRejected,
  TransactionStateType.DepositCompleted,
];

export const DepositStateSelections = DepositStateTypes.map((state) => ({
  value: state,
  label: t(`type.transactionState.${state}`),
}));

export const simpleDepositSelections = [
  {
    value: TransactionStateType.DepositCreated,
    label: t(`type.transactionState.${TransactionStateType.DepositCreated}`),
  },
  {
    value: TransactionStateType.DepositCompleted,
    label: t(`type.transactionState.${TransactionStateType.DepositCompleted}`),
  },
];

export const simpleDepositToArray = [
  {
    value: [
      TransactionStateType.DepositCreated,
      TransactionStateType.DepositCanceled,
      TransactionStateType.DepositFailed,
      TransactionStateType.DepositPaymentCompleted,
      TransactionStateType.DepositTenantApproved,
      TransactionStateType.DepositTenantRejected,
    ],
    id: TransactionStateType.DepositCreated,
  },
  {
    value: [
      TransactionStateType.DepositCompleted,
      TransactionStateType.DepositCallbackComplete,
    ],
    id: TransactionStateType.DepositCompleted,
  },
];
export const WithdrawalStateTypes = [
  TransactionStateType.WithdrawalCreated,
  TransactionStateType.WithdrawalCanceled,
  TransactionStateType.WithdrawalFailed,
  TransactionStateType.WithdrawalTenantApproved,
  TransactionStateType.WithdrawalTenantRejected,
  TransactionStateType.WithdrawalPaymentCompleted,
  TransactionStateType.WithdrawalCompleted,
];

export const WithdrawalStateSelections = WithdrawalStateTypes.map((state) => ({
  value: state,
  label: t(`type.transactionState.${state}`),
}));

export const simpleWithdrawalSelections = [
  {
    value: TransactionStateType.WithdrawalCreated,
    label: t(`type.transactionState.${TransactionStateType.WithdrawalCreated}`),
  },
  {
    value: TransactionStateType.WithdrawalCompleted,
    label: t(
      `type.transactionState.${TransactionStateType.WithdrawalCompleted}`
    ),
  },
];

export const simpleWithdrawalToArray = [
  {
    value: [
      TransactionStateType.WithdrawalCreated,
      TransactionStateType.WithdrawalCanceled,
      TransactionStateType.WithdrawalFailed,
      TransactionStateType.WithdrawalTenantApproved,
      TransactionStateType.WithdrawalTenantRejected,
    ],
    id: TransactionStateType.WithdrawalCreated,
  },
  {
    value: [TransactionStateType.WithdrawalCompleted],
    id: TransactionStateType.WithdrawalCompleted,
  },
];

export const TransferStateTypes = [
  TransactionStateType.TransferCreated,
  TransactionStateType.TransferCanceled,
  TransactionStateType.TransferFailed,
  TransactionStateType.TransferAwaitingApproval,
  TransactionStateType.TransferRejected,
  TransactionStateType.TransferApproved,
  TransactionStateType.TransferCompleted,
];

export const TransferStateSelections = TransferStateTypes.map((state) => ({
  value: state,
  label: t(`type.transactionState.${state}`),
}));

export const simpleTransferSelections = [
  {
    value: TransactionStateType.TransferCreated,
    label: t(`type.transactionState.${TransactionStateType.TransferCreated}`),
  },
  {
    value: TransactionStateType.TransferCompleted,
    label: t(`type.transactionState.${TransactionStateType.TransferCompleted}`),
  },
];

export const simpleTransferToArray = [
  {
    value: [
      TransactionStateType.TransferCreated,
      TransactionStateType.TransferCanceled,
      TransactionStateType.TransferFailed,
      TransactionStateType.TransferAwaitingApproval,
      TransactionStateType.TransferRejected,
      TransactionStateType.TransferApproved,
    ],
    id: TransactionStateType.TransferCreated,
  },
  {
    value: [TransactionStateType.TransferCompleted],
    id: TransactionStateType.TransferCompleted,
  },
];

export const CreatedStateTypes = [
  TransactionStateType.DepositCreated,
  TransactionStateType.RebateCreated,
  TransactionStateType.TransferCreated,
  TransactionStateType.WithdrawalCreated,
];

export const ApprovedStateTypes = [
  TransactionStateType.DepositCentralApproved,
  TransactionStateType.DepositTenantApproved,
  TransactionStateType.TransferApproved,
  TransactionStateType.WithdrawalTenantApproved,
];

export const CompletedStateTypes = [
  TransactionStateType.DepositCompleted,
  TransactionStateType.RebateCompleted,
  TransactionStateType.TransferCompleted,
  TransactionStateType.WithdrawalCompleted,
];

export const CanceledStateTypes = [
  TransactionStateType.DepositCanceled,
  TransactionStateType.RebateCanceled,
  TransactionStateType.TransferCanceled,
  TransactionStateType.WithdrawalCanceled,
];

export const AwaitingApprovalStateTypes = [
  TransactionStateType.TransferAwaitingApproval,
];

/**
 * for client only, merge ApprovedStateTypes and AwaitingApprovalStateTypes into one PendingStateTypes
 */
export const ClientPendingStateTypes = [
  ...ApprovedStateTypes,
  ...AwaitingApprovalStateTypes,
];

export enum TransactionStatusType {
  Created = 0,
  AwaitingApproval = 1,
  Approved = 2,
  Completed = 3,
  Canceled = 4,
}

export enum DepositStatusType {
  Pending = TransactionStateType.DepositCreated,
  Completed = TransactionStateType.DepositCompleted,
  Rejected = TransactionStateType.DepositTenantRejected,
  Cancelled = TransactionStateType.DepositCanceled,
  All = 0,
}

export enum WithdrawalStatusType {
  Pending = TransactionStateType.WithdrawalCreated,
  Completed = TransactionStateType.WithdrawalCompleted,
  Rejected = TransactionStateType.WithdrawalTenantRejected,
  Cancelled = TransactionStateType.WithdrawalCanceled,
  All = 0,
}
// export const TransactionStatusType = {
//   Created: CreatedStateTypes,
//   AwaitingApproval: AwaitingApprovalStateTypes,
//   Approved: ApprovedStateTypes,
//   Completed: CompletedStateTypes,
//   Canceled: CanceledStateTypes,
// };
