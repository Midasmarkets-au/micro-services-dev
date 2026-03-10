import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";
const prefix = "api/v1/client/";
const prefix2 = "api/v2/client/";
const userPrefix = "api/v1/user/";
import { normalizeAmountList } from "@/lib/utils";
export default {
  getConfig: async (uid: number) =>
    (await axios.get(prefix2 + "account/" + uid + "/config")).data,

  updateDefaultSalesAccount: async (uid: number) =>
    (await axios.put(prefix2 + "account/" + uid + "/default-parent")).data,

  getAccountTypesByReferCode: async (formData: any) =>
    (
      await axios.get(prefix2 + "account/type/referral", {
        params: formData,
      })
    ).data,

  getDemoAccountConfig: async () =>
    (await axios.get(prefix2 + "account/demo/application-config")).data,

  checkAccountExist: async (formData: any) =>
    (
      await axios.get(prefix + "trade-account/wholesale-referral-check", {
        params: formData,
      })
    ).data,

  getCreateAccountConfig: async () =>
    (await axios.get(prefix2 + "account/application-config")).data,

  updateAlias: async (formData: any) =>
    (await axios.put(userPrefix + "account/alias", formData)).data,

  queryAccounts: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "account", {
        params: criteria,
      })
    ).data;
    // res.data.forEach((element) => {
    //   if (element.tradeAccount) {
    //     element.tradeAccount = normalizeAmountList(element.tradeAccount, [
    //       "balance",
    //       "balanceInCents",
    //       "equityInCents",
    //       "equity",
    //     ]);
    //   }
    // });
    return {
      ...res,
      data: normalizeAmountList(res.data),
    };
    return res;
  },

  createLiveAccount: async (formData: any) =>
    (await axios.post(prefix + "application/trade-account", formData)).data,

  queryApplications: async (criteria?: any) =>
    (
      await axios.get(prefix + "application", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  getDemoAccounts: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "trade-demo-account", {
        params: criteria,
      })
    ).data;
    return {
      ...res,
      data: res?.data
        ? normalizeAmountList(res?.data, ["balanceInCents", "balance"])
        : res?.data,
    };
    console.log("res", res);
    return res;
  },

  createDemoAccount: async (formData: any) =>
    (await axios.post(prefix + "trade-demo-account", formData)).data,

  getServices: async (criteria?: any) =>
    (await axios.get(prefix + "trade/service", { params: criteria })).data,

  getTransactions: async (criteria?: any) =>
    (await axios.get(prefix + "tradetransaction", { params: criteria })).data,

  getTradesByTradeAccountUid: async (tradeAccountUid: string, criteria?: any) =>
    (
      await axios.get(prefix + "trade-account/" + tradeAccountUid + "/trade", {
        params: criteria,
      })
    ).data,

  getTransactionsByTradeAccountUid: async (
    tradeAccountUid: string,
    criteria?: any
  ) => {
    const res = (
      await axios.get(
        prefix + "trade-account/" + tradeAccountUid + "/transaction",
        {
          params: criteria,
        }
      )
    ).data;
    return {
      ...res,
      data: res?.data ? normalizeAmountList(res?.data) : res?.data,
    };
    return res;
  },

  submitLeverageChangeRequest: async (
    accountUid: number,
    accountNumber: number,
    leverage: number
  ) =>
    (
      await axios.post(prefix + "application/change-leverage", {
        accountUid,
        accountNumber,
        leverage,
      })
    ).data,

  submitWholesaleReferralRequest: async (formData: any) =>
    (await axios.post(prefix + "application/wholesale-referral", formData))
      .data,

  submitPasswordResetRequest: async (formData: any) =>
    (await axios.post(prefix + "application/change-password", formData)).data,

  getWalletInfos: async (criteria: any) =>
    (await axios.get(prefix + "wallet", { params: criteria })).data,

  validateTransferInRequest: async (
    tradeAccountUid: number,
    walletId: number,
    amount: number
  ) =>
    (
      await axios.post(prefix + "transaction/check/to-trade-account", {
        tradeAccountUid,
        walletId,
        amount,
      })
    ).data,

  submitTransferInRequest: async (formData: any) =>
    (await axios.post(prefix + "transaction/to/trade-account", formData)).data,

  submitTransferOutRequest: async (formData: any) =>
    (await axios.post(prefix + "transaction/to/wallet", formData)).data,

  submitTransferOutToAccountRequest: async (formData: any) =>
    (await axios.post(prefix + "transaction/between-trade-account", formData))
      .data,

  createDepositRequest: async (formData: any) =>
    (await axios.post(prefix + "deposit", formData)).data,

  createWithdrawalRequest: async (accountUid: number, formData: any) =>
    (
      await axios.post(
        prefix + "withdrawal/from-account/" + accountUid,
        formData
      )
    ).data,

  createWithdrawalRequestV2: async (accountUid: number, formData: any) =>
    (
      await axios.post(
        prefix2 + "account/" + accountUid + "/withdrawal",
        formData
      )
    ).data,

  createWalletWithdrawalRequestV2: async (hashId: any, formData: any) =>
    (await axios.post(prefix2 + "wallet/" + hashId + "/withdrawal", formData))
      .data,

  postWholesaleApplication: async (formData: any) =>
    (await axios.post(prefix + "application/whole-sale-account", formData))
      .data,
  refreshMTDataById: async (id: number) =>
    (await axios.get(prefix + "account/" + id + "/refresh")).data,
};

// import { Paged } from "@/core/models/Paged";
// import {
//   Account,
//   AccountCriteria,
//   TradeAccount,
//   TradeAccountCriteria,
//   TradeDemoAccount,
//   TradeDemoAccountCriteria,
// } from "@/projects/client/modules/account/model/TradeAccount";
// import { axiosInstance as api } from "@/core/services/api.client";
// import { ApplicationSupplement } from "@/projects/client/modules/account/model/ApplicationSupplement";
// import { Application } from "@/projects/client/modules/account/model/Application";

// export class AccountService2 {
//   public static async queryAccounts(
//     criteria: AccountCriteria | undefined
//   ): Promise<Paged<Account, AccountCriteria>> {
//     const rs = await api.get<Paged<Account, AccountCriteria>>(
//       "/api/v1/client/account",
//       { params: criteria }
//     );
//     return rs.data;
//   }

//   public static async queryTradeAccounts(
//     criteria: TradeAccountCriteria | undefined
//   ): Promise<Paged<TradeAccount, TradeAccountCriteria>> {
//     const rs = await api.get<Paged<TradeAccount, TradeAccountCriteria>>(
//       "/api/v1/client/account/trade",
//       { params: criteria }
//     );
//     return rs.data;
//   }

//   public static async queryDemoAccounts(): Promise<
//     Paged<TradeDemoAccount, TradeDemoAccountCriteria>
//   > {
//     const rs = await api.get<Paged<TradeDemoAccount, TradeDemoAccountCriteria>>(
//       "/api/v1/client/account/demo"
//     );
//     return rs.data;
//   }

//   public static async getTradeAccount(id: number): Promise<TradeAccount> {
//     const rs = await api.get<TradeAccount>(
//       "/api/v1/client/account/" + id + "/trade"
//     );
//     return rs.data;
//   }

//   public static async createDemoAccount(): Promise<TradeDemoAccount> {
//     const rs = await api.post<TradeDemoAccount>("/api/v1/client/account/demo");
//     return rs.data;
//   }

//   public static async createApplication(
//     supplement: ApplicationSupplement
//   ): Promise<Application> {
//     const res = await api.post<Application>(
//       "/api/v1/client/application",
//       supplement
//     );
//     return res.data;
//   }
// }
