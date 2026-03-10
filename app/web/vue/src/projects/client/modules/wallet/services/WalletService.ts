import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";
const prefix = "api/v1/client/";
const prefixV2 = "api/v2/client/";
import { normalizeAmountList } from "@/lib/utils";
export type PaymentServiceResType = {
  deposit: any[];
  withdrawal: any[];
};

export type WalletsResType = {
  id: number;
  type: number;
  balance: number;
  sequence: number;
  currencyId: number;
  talliedOn: string;
};

export type TradeAccountResType = {
  uid: number;
  serviceId: number;
  accountNumber: number;
  currencyId: number;
  lastSyncedOn: string;
  leverage: number;
  balance: number;
  balanceInCents: number;
  email?: string;
  name?: string;
};

export type DepositReqType = {
  amount: number;
  currencyId: number;
  paymentServiceId: number;
  fundType: number;
  targetTradeAccountUid: number | undefined;
  note: string | undefined;
  request: object | undefined;
  paymentServiceCategoryName: string | undefined;
};

export type WithdrawalReqType = {
  amount: number;
  walletId: number | undefined;
  paymentServiceId: number;
  request: object | undefined;
};

export type TransferReqType = {
  walletId?: number;
  tradeAccountUid?: number;
  amount: number;
  verificationCode?: string;
};

export default {
  queryAccountWithdrawalV2: async (uid: number, criteria?: any) => {
    const res = (
      await axios.get(prefixV2 + "account/" + uid + "/withdrawal", {
        params: criteria,
      })
    ).data;
    return {
      ...res,
      data: res?.data ? normalizeAmountList(res?.data) : res?.data,
    };
    return res;
  },

  getDepositGuide: async (uid: number, depositHashId?: any) =>
    (
      await axios.get(
        prefixV2 + "account/" + uid + "/deposit/" + depositHashId + "/guide"
      )
    ).data,

  queryDepositV2: async (uid: number, criteria?: any) => {
    const res = (
      await axios.get(prefixV2 + "account/" + uid + "/deposit", {
        params: criteria,
      })
    ).data;
    return {
      ...res,
      data: res?.data ? normalizeAmountList(res?.data) : res?.data,
    };
  },

  getUploadReceiptInfo: async (paymentHashId: string) =>
    (await axios.get(prefix + "payment/" + paymentHashId + "/guide")).data,

  queryDeposit: async (criteria?: any) =>
    (
      await axios.get(prefix + "deposit", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  getDeposit: async (id: number) =>
    (await axios.get(prefix + "deposit/" + id)).data,

  postDeposit: async (formData: DepositReqType) =>
    (await axios.post(prefix + "deposit", formData)).data,

  putDeposit: async (id: number, data: any) =>
    (await axios.put(prefix + "deposit/" + id, data)).data,

  cancelDeposit: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/cancel")).data,

  queryWithdrawal: async (criteria?: any) =>
    (await axios.get(prefix + "withdrawal", { params: criteria })).data,

  postWithdrawal: async (data: any) =>
    (await axios.post(prefix + "withdrawal", data)).data,

  putWithdrawal: async (id: number, data: any) =>
    (await axios.put(prefix + "withdrawal/" + id, data)).data,

  cancelWithdrawal: async (id: number) =>
    (await axios.put(prefix + "withdrawal/" + id + "/cancel")).data,

  queryTransfer: async (criteria?: any) =>
    (await axios.get(prefix + "transaction", { params: criteria })).data,

  postTransferToWallet: async (data: any) =>
    (await axios.post(prefix + "transaction/to/wallet", data)).data,

  postTransferToTradeAccount: async (data: any) =>
    (await axios.post(prefix + "transaction/to/trade-account", data)).data,
  postTransferToWalletAccount: async (data: any) =>
    (await axios.post(prefix + "transaction/to/downsidewallet", data)).data,

  getWallets: async () => {
    const res = await axios.get(prefix + "wallet");
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data, "balance")
        : res.data?.data,
    };
    return res.data;
  },

  getWalletsV2: async () => {
    const res = await axios.get(prefixV2 + "wallet");
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data, "balance")
        : res.data?.data,
    };
    return res.data;
  },

  getTradeAccounts: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "trade-account", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data;
    return res;
  },

  searchTradeAccounts: async (criteria?: any) =>
    (
      await axios.get(prefix + "trade-account/search", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  // 搜索转账目标账户（用于转账给他人）
  searchTransferTargetAccount: async (email: string) => {
    const res = (
      await axios.get(prefix + "trade-account/check-target-email", {
        params: { email },
      })
    ).data;
    return res;
  },
  sendTransferVerificationCode: async (params?: any) =>
    (
      await axios.post(
        prefix + "transaction/to/downsidewallet/request-code",
        params
      )
    ).data,
  queryClientTransactionView: async (criteria?: any) =>
    (
      await axios.get(prefix + "transaction/query", {
        params: criteria,
      })
    ).data,

  queryWalletWithdrawView: async (walletId: number, criteria?: any) =>
    (
      await axios.get(prefix + `wallet/${walletId}/withdrawal`, {
        params: criteria,
      })
    ).data,

  queryWalletTransferView: async (walletId: number, criteria?: any) =>
    (
      await axios.get(prefix + `wallet/${walletId}/transfer`, {
        params: criteria,
      })
    ).data,

  queryWalletTransactionView: async (walletId: number, criteria?: any) =>
    (
      await axios.get(prefix + `wallet/${walletId}/transaction`, {
        params: criteria,
      })
    ).data,

  getDepositReceiptFile: async (id: number) =>
    (await axios.get(prefix + "deposit/" + id + "/receipt")).data,

  getDepositReceiptFileV2: async (id: number, hashId: any) =>
    (
      await axios.get(
        prefixV2 + "account/" + id + "/deposit/" + hashId + "/receipt"
      )
    ).data,

  postDepositReceiptFile: async (id: number, file: any) =>
    (await axios.post(prefix + "deposit/" + id + "/receipt", file)).data,

  postDepositReceiptFileV2: async (id: number, hashId: any, file: any) =>
    (
      await axios.post(
        prefixV2 + "account/" + id + "/deposit/" + hashId + "/receipt",
        file
      )
    ).data,

  getMethodInstructionV2: async (id: number) =>
    (await axios.get(prefixV2 + "payment-method/" + id + "/instruction")).data,

  getOrderReference: async (id: number) =>
    (await axios.get(prefix + "deposit/" + id + "/reference")).data,

  getExchangeRate: async (criteria?: any) =>
    (await axios.get(prefix + "exchange-rate", { params: criteria })).data,

  getDepositExchangeRate: async (from: number, to: number) =>
    (await axios.get(prefix + "deposit/" + from + "/to/" + to)).data,

  getUploadReceiptV2: async (uid: number, depositHashId: number) =>
    (
      await axios.get(
        prefixV2 + "account/" + uid + "/deposit/" + depositHashId + "/receipt"
      )
    ).data,

  postUploadReceiptV2: async (uid: number, depositHashId: number, file: any) =>
    (
      await axios.post(
        prefixV2 + "account/" + uid + "/deposit/" + depositHashId + "/receipt",
        file
      )
    ).data,

  queryWalletWithdrawV2: async (hashId: any, criteria?: any) => {
    const res = (
      await axios.get(prefixV2 + `wallet/${hashId}/withdrawal`, {
        params: criteria,
      })
    ).data;
    return {
      ...res,
      data: res?.data ? normalizeAmountList(res.data) : res?.data,
    };
    return res;
  },

  queryWalletTransferV2: async (hashId: any, criteria?: any) => {
    const res = await axios.get(prefixV2 + `wallet/${hashId}/transfer`, {
      params: criteria,
    });
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
  },

  queryWalletAdjustV2: async (hashId: any, criteria?: any) => {
    const res = await axios.get(prefixV2 + `wallet/${hashId}/adjust`, {
      params: criteria,
    });
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
  },

  queryWalletRefundV2: async (hashId: any, criteria?: any) => {
    const res = await axios.get(prefixV2 + `wallet/${hashId}/refund`, {
      params: criteria,
    });
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
  },

  queryWalletRebateV2: async (hashId: any, criteria?: any) => {
    const res = await axios.get(prefixV2 + `wallet/${hashId}/rebate`, {
      params: criteria,
    });
    console.log("wallet", res);
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
    return res.data;
  },
  queryWalletTransferRewardsV2: async (hashId: any, criteria?: any) => {
    const res = await axios.get(
      prefixV2 + `wallet/${hashId}/downline-reward-transfer`,
      {
        params: criteria,
      }
    );
    console.log("wallet", res);
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
    return res.data;
  },
};
