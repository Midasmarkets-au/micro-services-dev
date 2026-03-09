import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";

import { PaymentInfo } from "@/core/types/PaymentTypes";
import { Result } from "@/core/types/Result";
import {
  PaymentInfoCriteria,
  PaymentInfoTenantModal,
} from "@/core/models/PaymentInfos";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { FundTypes } from "@/core/types/FundTypes";
const prefix = "api/v1/tenant/";
const prefixV2 = "v2/tenant/";
const prefixApiV2 = "api/v2/tenant/";
import { normalizeAmountList } from "@/lib/utils";
/*
public class WalletStatisticViewModel
{
    public long Balance { get; set; }
    public CurrencyTypes CurrencyId { get; set; }
    public FundTypes FundType { get; set; }
}
 */
export interface WalletStatisticViewModel {
  balance: number;
  currencyId: CurrencyTypes;
  fundType: FundTypes;
}

export default {
  getStateDetail: async (id: number) =>
    (await axios.get(prefix + "matter/" + id + "/state-detail")).data,

  getAccountPaymentMethodById: async (Id: number) =>
    (await axios.get(prefixApiV2 + "payment-method/account/" + Id + "/access"))
      .data,

  getWalletPaymentMethodById: async (Id: number) =>
    (await axios.get(prefixApiV2 + "payment-method/wallet/" + Id + "/access"))
      .data,

  putEnableAccountPaymentMethodById: async (
    methodId: number,
    accountId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 +
          "payment-method/" +
          methodId +
          "/account-enable/" +
          accountId
      )
    ).data,

  putDisableAccountPaymentMethodById: async (
    methodId: number,
    accountId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 +
          "payment-method/" +
          methodId +
          "/account-disable/" +
          accountId
      )
    ).data,

  putEnableAccountPaymentMethodByGroup: async (
    group: string,
    accountId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 + "payment-method/account-group-enable/" + accountId,
        { group: group }
      )
    ).data,

  putDisableAccountPaymentMethodByGroup: async (
    group: string,
    accountId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 + "payment-method/account-group-disable/" + accountId,
        { group: group }
      )
    ).data,

  putEnableWalletPaymentMethodById: async (
    methodId: number,
    walletId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 +
          "payment-method/" +
          methodId +
          "/wallet-enable/" +
          walletId
      )
    ).data,

  putDisableWalletPaymentMethodById: async (
    methodId: number,
    walletId: number
  ) =>
    (
      await axios.put(
        prefixApiV2 +
          "payment-method/" +
          methodId +
          "/wallet-disable/" +
          walletId
      )
    ).data,

  updateCryptoExpireTime: async (
    category: string,
    rowId: number,
    key: string,
    formData: object
  ) =>
    (
      await axios.put(
        prefix + "configuration/" + category + "/" + rowId + "/" + key,
        formData
      )
    ).data,

  queryCryptoExpireTime: async (category: string, rowId: number, key: string) =>
    (
      await axios.get(
        prefix + "configuration/" + category + "/" + rowId + "/" + key
      )
    ).data,

  queryCryptoTransactions: async (criteria?: any) =>
    (await axios.get(prefix + "crypto/transaction", { params: criteria })).data,

  updateCryptoWalletStatus: async (id: number, status: number) =>
    (await axios.put(prefix + "crypto/" + id + "/status/" + status)).data,

  queryCryptoWallets: async (criteria?: any) =>
    (await axios.get(prefix + "crypto", { params: criteria })).data,

  createCryptoWallet: async (formData: any) =>
    (await axios.post(prefix + "crypto", formData)).data,

  // TODO: 补充删除 crypto 接口
  deleteCryptoWallet: async (id: number) =>
    (await axios.delete(prefix + "crypto/" + id)).data,

  updateTransferAutoComplete: async (formData: object) =>
    (await axios.put(prefix + "transaction/auto-complete-setting", formData))
      .data,

  transferAutoComplete: async () =>
    (await axios.get(prefix + "transaction/auto-complete-setting")).data,

  createWalletAdjust: async (walletId: number, formData: object) =>
    (await axios.post(prefix + "wallet/" + walletId + "/adjust", formData))
      .data,
  getWalletAdjust: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "wallet/adjust", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data) : res.data,
    };
    return res;
  },

  changeWalletFundType: async (id: number, fundType: number) =>
    (
      await axios.put(
        prefix + "wallet/" + id + "/fund-type?fundType=" + fundType
      )
    ).data,
  completeCallBackByPaymentId: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/complete")).data,

  getCallBackInfo: async () =>
    (
      await axios.get(
        prefix + "configuration/Public/0/PaymentServiceCallbackSetting"
      )
    ).data,

  getHighDollarLatestInfo: async () =>
    (await axios.get(prefix + "audit/high-dollar/latest")).data,

  updateCallbackTime: async (formData: object) =>
    (await axios.put(prefix + "payment-service/callback-setting", formData))
      .data,

  updateHighDollarConfig: async (siteId: number, formData: object) =>
    (
      await axios.put(
        prefix + "configuration/site/" + siteId + "/high-dollar-value",
        formData
      )
    ).data,

  getTradeServices: async () =>
    (await axios.get(prefix + "trade/service")).data,

  getDepositPaymentMethodsV2: async (criteria?: any) =>
    (
      await axios.get(prefixApiV2 + "payment-method/deposit", {
        params: criteria,
      })
    ).data,

  getWithdrawPaymentMethodsV2: async (criteria?: any) =>
    (
      await axios.get(prefixApiV2 + "payment-method/withdrawal", {
        params: criteria,
      })
    ).data,

  putPaymentMethodByIdV2: async (id: number, formData: object) =>
    (await axios.put(prefixApiV2 + "payment-method/" + id, formData)).data,

  deletePaymentMethodById: async (id: number) =>
    (await axios.delete(prefixApiV2 + "payment-method/" + id)).data,

  PaymentMethodsResetCache: async () =>
    (await axios.get(prefixApiV2 + "payment-method/cache/reload")).data,

  postPaymentServices: async (formData: object) =>
    (await axios.post(prefixApiV2 + "payment-method", formData)).data,

  batchUpdateSort: async (formData: object) =>
    (
      await axios.put(
        prefixApiV2 + "payment-method/batch-update-sort",
        formData
      )
    ).data,

  getPaymentServicesUpdateBy: async (criteria?: any) =>
    (await axios.get(prefix + "audit", { params: criteria })).data,

  getPaymentMethodById: async (id: number) =>
    (await axios.get(prefixApiV2 + "payment-method/" + id)).data,

  getPaymentMethodInstructionById: async (id: number) =>
    (await axios.get(prefixApiV2 + "payment-method/" + id + "/instruction"))
      .data,

  getPaymentMethodPolicyById: async (id: number) =>
    (await axios.get(prefixApiV2 + "payment-method/" + id + "/policy")).data,

  getPaymentInfos: async (
    criteria?: PaymentInfoCriteria
  ): Promise<Result<Array<PaymentInfoTenantModal>, PaymentInfoCriteria>> =>
    (await axios.get(prefix + "payment-info", { params: criteria })).data,

  getPaymentInformation: async (criteria: any) =>
    (await axios.get(prefix + "payment-info", { params: criteria })).data,

  putPaymentInformation: async (id: number, formData: object) =>
    (await axios.put(prefix + "payment-info/" + id, formData)).data,

  deletePaymentInformation: async (id: number) =>
    (await axios.delete(prefix + "payment-info/" + id)).data,

  updatePaymentMethodInstructionById: async (id: number, formData: object) =>
    (
      await axios.put(
        prefixApiV2 + "payment-method/" + id + "/instruction",
        formData
      )
    ).data,

  updatePaymentMethodPolicyById: async (id: number, formData: object) =>
    (
      await axios.put(
        prefixApiV2 + "payment-method/" + id + "/policy",
        formData
      )
    ).data,

  updatePaymentMethodDetailById: async (id: number, formData: object) =>
    (await axios.put(prefixApiV2 + "payment-method/" + id, formData)).data,

  queryDeposits: async (criteria?: any) => {
    const res = (await axios.get(prefix + "deposit", { params: criteria }))
      .data;
    res.data.forEach((element) => {
      if (element.payment) {
        element.payment = normalizeAmountList(element.payment);
      }
    });
    return {
      ...res,
      data: normalizeAmountList(res.data),
    };
    return res;
  },

  approveDepositById: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/approve")).data,

  rejectDepositById: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/reject")).data,

  restoreRejectedDepositByPaymentId: async (id: number) =>
    (await axios.put(prefix + "deposit/reject/" + id + "/restore")).data,

  cancelDepositById: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/cancel")).data,

  restoreCanceledDepositByPaymentId: async (id: number) =>
    (await axios.put(prefix + "deposit/cancel/" + id + "/restore")).data,

  completeDepositById: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/complete")).data,

  completeDepositByPaymentId: async (id: number) =>
    (await axios.put(prefix + "deposit/" + id + "/complete-payment")).data,

  queryWithdrawals: async (criteria?: any) => {
    const res = (await axios.get(prefix + "withdrawal", { params: criteria }))
      .data;
    // res.data.forEach((element) => {
    //   if (element.source) {
    //     element.source = normalizeAmountList(element.source, [
    //       "balance",
    //       "balanceInCents",
    //       "equity",
    //       "equityInCents",
    //     ]);
    //   }
    // });
    const re = {
      ...res,
      data: res.data ? normalizeAmountList(res.data) : res?.data,
    };
    return re;
  },
  getWithdrawalInfosById: async (id: number) => {
    const res = (await axios.get(prefix + "withdrawal/" + id + "/info")).data;
    const re = {
      ...res,
      Reference: res?.Reference
        ? normalizeAmountList(res.Reference, "Amount")
        : res?.Reference,
    };
    return re;
  },

  approveWithdrawalById: async (id: number, formData: any) =>
    (await axios.put(prefix + "withdrawal/" + id + "/approve", formData)).data,

  rejectWithdrawalById: async (id: number, formData: any) =>
    (await axios.put(prefix + "withdrawal/" + id + "/reject", formData)).data,

  restoreRejectedWithdrawalById: async (id: number) =>
    (await axios.put(prefix + "withdrawal/reject/" + id + "/restore")).data,
  restoreCanceledWithdrawalById: async (id: number) =>
    (await axios.put(prefix + "withdrawal/cancel/" + id + "/restore")).data,

  cancelWithdrawalById: async (id: number) =>
    (await axios.put(prefix + "withdrawal/" + id + "/cancel")).data,

  completeWithdrawalById: async (id: number) =>
    (await axios.put(prefix + "withdrawal/" + id + "/complete")).data,

  completeWithdrawalByPaymentId: async (id: number) =>
    (await axios.put(prefix + "withdrawal/" + id + "/complete-payment")).data,

  getPaymentInfosById: async (id: number): Promise<PaymentInfo> =>
    (await axios.get(prefix + "payment/" + id)).data,

  completePaymentById: async (id: number) =>
    (await axios.put(prefix + "payment/" + id + "/complete")).data,

  executePaymentById: async (id: number) =>
    (await axios.put(prefix + "payment/" + id + "/execute")).data,

  refundWithdrawById: async (id: number) =>
    (await axios.put(prefix + "withdrawal/" + id + "/refund")).data,

  cancelPaymentById: async (id: number) =>
    (await axios.put(prefix + "payment/" + id + "/cancel")).data,

  queryTransactions: async (criteria?: any) => {
    const res = (await axios.get(prefix + "transaction", { params: criteria }))
      .data;
    return {
      ...res,
      data: normalizeAmountList(res.data, [
        "amount",
        "sourceAccountBalance",
        "sourceAccountBalanceInCents",
        // "targetAccountBalance",
        "targetAccountBalanceInCents",
      ]),
    };
    console.log("xxxtrss");
    return res;
  },

  approveTransactionById: async (id: number) =>
    (await axios.put(prefix + "transaction/" + id + "/approve")).data,

  rejectTransactionById: async (id: number) =>
    (await axios.put(prefix + "transaction/" + id + "/reject")).data,

  cancelTransactionById: async (id: number) =>
    (await axios.put(prefix + "transaction/" + id + "/cancel")).data,

  completeTransactionById: async (id: number) =>
    (await axios.put(prefix + "transaction/" + id + "/complete")).data,

  getDepositReceiptById: async (id: number) =>
    (await axios.get(prefix + "deposit/" + id + "/receipt")).data,

  getDepositReferenceById: async (id: number) =>
    (await axios.get(prefix + "deposit/" + id + "/reference")).data,

  postDepositReceiptFile: async (id: number, file: any) =>
    (await axios.post(prefix + "deposit/" + id + "/receipt", file)).data,

  getTradeAccountList: async (criteria?: any) =>
    (await axios.get(prefix + "trade-account", { params: criteria })).data,
  getCreditList: async (criteria?: any) =>
    (
      await axios.get(prefix + "audit/account-change-balance", {
        params: criteria,
      })
    ).data,

  createCredit: async (id: number, formData: any) =>
    (await axios.put(prefix + "trade-account/" + id + "/credit", formData))
      .data,

  createAdjust: async (id: number, formData: any) =>
    (await axios.put(prefix + "trade-account/" + id + "/balance", formData))
      .data,

  // getAccountIdByAccountNumber: async (accountNumber: number) =>
  //   (
  //     await axios.get(
  //       prefix + "trade-account?accountNumber=" + accountNumber.toString()
  //     )
  //   ).data,
  getAccountIdByAccountNumber: async (accountNumber: number) =>
    (
      await axios.get(
        prefix + "trade-account?accountNumber=" + accountNumber.toString()
      )
    ).data,
  getExchangeRate: async (criteria?: any) =>
    (await axios.get(prefix + "exchange-rate", { params: criteria })).data,

  putExchangeRate: async (id: number, data: any) =>
    (await axios.put(prefix + "exchange-rate/" + id, data)).data,

  postExchangeRate: async (data: any) =>
    (await axios.post(prefix + "exchange-rate/", data)).data,

  getExchangeRateHistory: async (id: number, criteria?: any) =>
    (
      await axios.get(prefix + "exchange-rate/" + id + "/history", {
        params: criteria,
      })
    ).data,

  queryWallet: async (criteria?: any) => {
    const res = await axios.get(prefix + "wallet", { params: criteria });
    const re = {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data, "balance")
        : res.data?.data,
    };
    return re;
  },

  queryWalletStatistic: async (
    criteria?: any
  ): Promise<Array<WalletStatisticViewModel>> => {
    const data = (
      await axios.get(prefix + "wallet/statistic", { params: criteria })
    ).data;
    console.log("data", data);
    return normalizeAmountList(data, "balance");
  },

  uploadImage: async (formData: any) =>
    (await axios.post(prefix + "upload/public?type=public", formData)).data,

  // Refund
  queryRefunds: async (criteria?: any) => {
    const res = await axios.get(prefix + "refund", { params: criteria });
    const re = {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
    console.log("re", re);
    return re;
  },

  createRefund: async (formData: any) =>
    (await axios.post(prefix + "refund", formData)).data,

  uploadBatchFile: async (formData: any) =>
    (await axios.post(prefix + "account/batch-adjust/create", formData)).data,

  getBatchList: async (criteria?: any) => {
    const res = await axios.get(prefix + "account/adjust-batch", {
      params: criteria,
    });
    const re = {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
    return re;
  },

  getAdjustRecord: async (criteria?: any) => {
    const res = await axios.get(prefix + "account/adjust-record", {
      params: criteria,
    });
    const re = {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
    return re;
  },

  confirmBatch: async (id: number) =>
    (await axios.post(prefix + "account/batch-adjust/" + id + "/confirm")).data,

  getBatchDetail: async (criteria?: any) =>
    (await axios.get(prefix + "account/adjust-record", { params: criteria }))
      .data,

  createAdjustRecord: async (formData: any) =>
    (await axios.post(prefix + "account/adjust-record", formData)).data,

  createOFA: async (formData: any) =>
    (await axios.post(prefix + "withdrawal/ofa-df", formData)).data,

  //Version 2
  queryDepositsV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "deposit", { params: criteria })).data,

  queryWithdrawsV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "withdraw", { params: criteria })).data,

  queryWalletV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "wallet", { params: criteria })).data,

  queryWalletStatV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "wallet/stat")).data,

  uploadImageV2: async (formData: any) =>
    (await axiosV2.post(prefixV2 + "upload", formData)).data,

  getDepositwithdrawSummary: async (partyId: number) => {
    const res = await axios.get(
      prefixApiV2 + "finance/deposit-withdraw-summary/" + partyId
    );
    return normalizeAmountList(res.data, [
      "depositSumUsd",
      "needWithdrawAmountUsd",
      "withdrawSumUsd",
    ]);
  },
  getDepositwithdrawDetail: async (
    partyId: number,
    fundType: number,
    criteria?: any
  ) => {
    const url =
      prefixApiV2 + `finance/deposit-withdraw-details/${partyId}/${fundType}`;
    const res = (await axios.get(url, { params: criteria })).data;
    res.data = normalizeAmountList(res.data, ["amountUsd", "amount"]);
    return res;
  },
};
