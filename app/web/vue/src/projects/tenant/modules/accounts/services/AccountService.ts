import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";
import { AccountStatusTypes } from "@/core/types/AccountInfos";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";
import { normalizeAmountList } from "@/lib/utils";
const prefix = "api/v1/tenant/";

const prefixv2 = "api/v2/tenant/";

export type AccountWizardType = {
  kycFormCompleted: boolean;
  paymentAccessGranted: boolean;
  welcomeEmailSent: boolean;
  noticeEmailSent: boolean;
};

const AccountService = {
  enableAccountTransferPermissionById: async (id: number) =>
    (await axios.put(prefixv2 + "account/" + id + "/enable-transfer")).data,

  disableAccountTransferPermissionById: async (id: number) =>
    (await axios.put(prefixv2 + "account/" + id + "/disable-transfer")).data,

  validateAccountNumber: async (accountNumber: number) =>
    (await axios.get(prefix + "account/check-account-number/" + accountNumber))
      .data,

  queryTradeStatByAccountNumber: async (
    accountNumber: number,
    criteria?: any
  ) =>
    (
      await axios.get(prefixv2 + "account/" + accountNumber + "/trade/stat", {
        params: criteria,
      })
    ).data,

  queryTradeStatByAccountNumbers: async (criteria?: any) =>
    (
      await axios.get(prefixv2 + "account/trade/stat", {
        params: criteria,
      })
    ).data,

  setDefaultClient: async (accountId: number, code: string) =>
    (
      await axios.put(
        prefix +
          "referral/code/" +
          code +
          "/account/" +
          accountId +
          "/default-client"
      )
    ).data,

  exportAccounts: async (formData: any) =>
    (await axios.post(prefix + "report/request", formData)).data,

  confirmAutoCreateAccount: async (id: number) =>
    (await axios.post(prefixv2 + "account/" + id + "/confirm-auto-create"))
      .data,

  deleteDemoAccount: async (id: number) =>
    (await axios.delete(prefix + "trade-demo-account/" + id)).data,
  queryDemoAccounts: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "trade-demo-account", { params: criteria })
    ).data;
    const re = {
      ...res,
      data: res.data ? normalizeAmountList(res.data, ["balance"]) : res.data,
    };
    return re;
  },
  updateFundType: async (id: number, fundType: number) =>
    (
      await axios.put(
        prefix + "account/" + id + "/fund-type?fundType=" + fundType
      )
    ).data,

  getReferralCodeDetailByCode: async (code: string) =>
    (await axios.get(prefix + "referral/referred-by-account/" + code)).data,

  getReferralHistory: async (criteria?: any) =>
    (await axios.get(prefix + "referral/history", { params: criteria })).data,

  getReferralCode: async (criteria?: any) =>
    (await axios.get(prefix + "referral", { params: criteria })).data,

  getDepositPaymentMethods: async (criteria?: any) =>
    (await axios.get(prefixv2 + "payment-method/deposit", { params: criteria }))
      .data,
  getWithDrawPaymentMethods: async (criteria?: any) =>
    (
      await axios.get(prefixv2 + "payment-method/withdrawal", {
        params: criteria,
      })
    ).data,
  updateReferralDefaultPaymentMethods: async (
    id: number | string,
    paymentMethodIds: number[],
    type: string
  ) =>
    (
      await axios.put(prefix + "referral/" + id + "/default-payment-method", {
        PaymentMethodIds: paymentMethodIds,
        type: type,
      })
    ).data,

  queryAccountLog: async (criteria?: any) =>
    (
      await axios.get(prefix + "account/log", {
        params: criteria,
      })
    ).data,

  updateGroupName: async (id: number, formData: any) =>
    (
      await axios.post(
        prefix + "account/" + id + "/change/agent-group-name",
        formData
      )
    ).data,

  postRebateAgentRule: async (data: any) =>
    (await axios.post(prefix + "rebate-agent-rule", data)).data,

  clearLevelSetting: async (id: number) =>
    (
      await axios.put(
        prefix + "rebate-agent-rule/" + id + "/clear-level-setting"
      )
    ).data,

  updateLevelSettingItems: async (id: number, data: any) =>
    (
      await axios.put(
        prefix + "rebate-agent-rule/" + id + "/level-setting",
        data
      )
    ).data,

  getChildAccountStat: async (criteria?: any) => {
    const data = (
      await axios.get(prefix + "account/child/stat", { params: criteria })
    ).data;
    data.forEach((element) => {
      if (element.rebateAmounts) {
        Object.keys(element.rebateAmounts).forEach((key) => {
          element.rebateAmounts[key] = normalizeAmountList(
            element.rebateAmounts[key]
          );
        });
      }
      if (element.depositAmounts) {
        Object.keys(element.depositAmounts).forEach((key) => {
          element.depositAmounts[key] = normalizeAmountList(
            element.depositAmounts[key]
          );
        });
      }
      if (element.netAmounts) {
        Object.keys(element.netAmounts).forEach((key) => {
          element.netAmounts[key] = normalizeAmountList(
            element.netAmounts[key]
          );
        });
      }
      if (element.withdrawalAmounts) {
        Object.keys(element.withdrawalAmounts).forEach((key) => {
          element.withdrawalAmounts[key] = normalizeAmountList(
            element.withdrawalAmounts[key]
          );
        });
      }
      if (element.profitAmounts) {
        Object.keys(element.profitAmounts).forEach((key) => {
          element.profitAmounts[key] = normalizeAmountList(
            element.profitAmounts[key]
          );
        });
      }
    });

    return data;
  },

  getChildRebateStat: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "account/child/stat/rebate/symbol-grouped", {
        params: criteria,
      })
    ).data;
    Object.keys(res).forEach((key) => {
      Object.keys(res[key].amounts).forEach((currecyId) => {
        res[key].amounts[currecyId] = normalizeAmountList(
          res[key].amounts[currecyId]
        );
      });
    });
  },

  resetWholesaleApplication: async (partyId: number) =>
    (
      await axios.delete(
        prefix +
          "user/" +
          partyId +
          "/permission/application-wholesale-disabled"
      )
    ).data,

  queryAccounts: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "account", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data;
    // res.data.forEach((item) => {
    //   if (item.tradeAccount) {
    //     item.tradeAccount = normalizeAmountList(item.tradeAccount, [
    //       "balance",
    //       "balanceInCents",
    //     ]);
    //   }
    // });
    return res;
  },

  fuzzySearchAccounts: async (
    fuzzyKey: string,
    currentPage = 1,
    pageSize = 10
  ) =>
    (
      await axios.get(prefix + "search/account", {
        params: { q: fuzzyKey, page: currentPage, pageSize },
      })
    ).data,

  fuzzySearchTradeAccounts: async (
    fuzzyKey: string,
    currentPage = 1,
    pageSize = 10
  ) =>
    (
      await axios.get(prefix + "search/trade-account", {
        params: { q: fuzzyKey, page: currentPage, pageSize },
      })
    ).data,

  fuzzySearchAccountByCriteria: async (criteria: any) =>
    (
      await axios.get(prefix + "search/account", {
        params: criteria,
      })
    ).data,

  getServices: async () => (await axios.get(prefix + "trade/service")).data,

  getTradeAccount: async (criteria?: any) =>
    (await axios.get(prefix + "trade-account", { params: criteria })).data,

  getAccountDetailById: async (id: number) =>
    (await axios.get(prefix + "account/" + id)).data,

  refreshMTDataById: async (id: number) =>
    (await axios.get(prefix + "account/" + id + "/refresh")).data,

  queryTrades: async (criteria?: any) =>
    (await axios.get(prefix + "trade", { params: criteria })).data,

  queryTransactions: async (criteria?: any) => {
    const res = (await axios.get(prefix + "transaction", { params: criteria }))
      .data;
    const re = {
      ...res,
      data: res.data
        ? normalizeAmountList(res.data, [
            "targetAccountBalanceInCents",
            "sourceAccountBalanceInCents",
            "amount",
          ])
        : res.data,
    };
    return re;
  },

  queryApplications: async (criteria?: any) =>
    (
      await axios.get(prefix + "application", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  approveApplicationsById: async (id: number, formData?: any) =>
    (await axios.put(prefix + "application/" + id + "/approve", formData)).data,

  rejectApplicationsById: async (id: number, formData?: any) =>
    (await axios.put(prefix + "application/" + id + "/reject", formData)).data,

  reverseApplicationsById: async (id: number, formData?: any) =>
    (
      await axios.put(
        prefix + "application/" + id + "/reverse-reject",
        formData
      )
    ).data,
  // tbd

  queryVerificationByCriteria: async (criteria?: any) =>
    (await axios.get(prefix + "verification/", { params: criteria })).data,

  getVerificationById: async (id: number) =>
    (await axios.get(prefix + "verification/" + id)).data,

  getTradeAccountReadOnlyCode: async (id: number) =>
    (await axios.get(prefix + "trade-account/" + id + "/read-only-code")).data,

  postSourceToTargetRebateRule: async (formData: any) =>
    (await axios.post(prefix + "rebate-rule", formData)).data,

  updateSourceToTargetRebateRule: async (id: number, formData: any) =>
    (await axios.put(prefix + "rebate-rule/" + id + "/items", formData)).data,

  getReadOnlyCodeMailReview: async (id: number, formData?: any) =>
    (
      await axios.post(
        prefix + "trade-account/" + id + "/read-only-code/notice/preview",
        {},
        { params: formData }
      )
    ).data,

  postReadOnlyCodeMail: async (id: number, formData?: any) =>
    (
      await axios.post(
        prefix + "trade-account/" + id + "/read-only-code/notice",
        {},
        { params: formData }
      )
    ).data,

  queryCommunications: async (criteria?: any) =>
    (await axios.get(prefix + "communicate", { params: criteria })).data,

  getAccountWizardStatus: async (id: number): Promise<AccountWizardType> =>
    (await axios.get(prefix + "account/" + id + "/wizard")).data,

  updateAccountStatus: async (
    accountId: number,
    status: AccountStatusTypes,
    formData: any
  ) =>
    (
      await axios.put(
        prefix + "account/" + accountId + "/status/" + status,
        formData
      )
    ).data,

  updateTradeAccountLeverage: async (accountId: number, leverage: any) =>
    (
      await axios.put(
        prefix + "trade-account/" + accountId + "/leverage/" + leverage
      )
    ).data,

  getUserKycHistory: async (partyId: number) =>
    (await axios.get(prefix + "kyc/" + partyId + "/history")).data,

  getUserLatestKyc: async (partyId: number) => {
    let res: any;
    try {
      res = (await axios.get(prefix + "kyc/" + partyId)).data;
    } catch {
      return {};
    }
    return res?.verificationItems?.length ? res.verificationItems[0].data : {};
  },

  createKycForm: async (partyId: number, formData: any) =>
    (await axios.post(prefix + "kyc/" + partyId, formData)).data,

  updateKycFormToAwaitingReview: async (partyId: number) =>
    (await axios.put(prefix + "kyc/" + partyId + "/awaiting-review")).data,

  createDeposit: async (formData: any) =>
    (await axios.post(prefix + "deposit", formData)).data,

  createWithdrawalForAccount: async (accountUid: number, formData: any) =>
    (
      await axios.post(
        prefix + "withdrawal/from-account/" + accountUid,
        formData
      )
    ).data,

  getFullAccountGroupNames: async (type: AccountGroupTypes, keywords = "") =>
    (
      await axios.get(prefix + "account/group/name-list", {
        params: { type, keywords },
      })
    ).data,

  assignAccountToSales: async (accountId: number, salesId: number) =>
    (
      await axios.post(
        prefix + "account/" + accountId + "/assign/sales/" + salesId
      )
    ).data,

  assignAccountToAgent: async (accountId: number, agentId: number) =>
    (
      await axios.post(
        prefix + "account/" + accountId + "/assign/agent/" + agentId
      )
    ).data,

  removeAccountFromAgent: async (accountId: number, agentId: number) =>
    (
      await axios.post(
        prefix + "account/" + accountId + "/remove/agent/" + agentId
      )
    ).data,

  renameSelfGroupName: async (groupId: number, formData) =>
    (await axios.put(prefix + "group/" + groupId, formData)).data,

  getActivityReport: async (id: number, formData) =>
    (await axios.post(prefix + "account/" + id + "/activity-report", formData))
      .data,

  getAccountById: async (id: number) =>
    (await axios.get(prefix + "account/" + id)).data,

  updateAccountTagsById: async (id: number, formData: any) =>
    (await axios.put(prefix + "account/" + id + "/tags", formData)).data,

  changeAgentGroup: async (id: number, agentId: number) =>
    (await axios.post(prefix + "account/" + id + "/change/agent/" + agentId))
      .data,

  removeAgentGroup: async (id: number, agentId: number) =>
    (await axios.post(prefix + "account/" + id + "/remove/agent/" + agentId))
      .data,

  changeSalesGroup: async (id: number, salesId: number) =>
    (await axios.post(prefix + "account/" + id + "/change/sales/" + salesId))
      .data,
  updateAccountLevelRule: async (id: number, hasLevelRule: number) =>
    (
      await axios.put(
        prefix +
          "account/" +
          id +
          "/has-level-rule?hasLevelRule=" +
          hasLevelRule
      )
    ).data,

  getCreditList: async (criteria?: any) =>
    (
      await axios.get(prefix + "audit/account-change-balance", {
        params: criteria,
      })
    ).data,

  getReferCode: async (id: number) =>
    (await axios.get(prefix + "account/" + id + "/referral-codes")).data,

  addReferralCode: async (formData: any) =>
    (await axios.post(prefix + "referral", formData)).data,

  queryParentAccounts: async (id: number) =>
    (await axios.get(prefix + "account/" + id + "/parent-accounts")).data,
  changeTrade: async (id: number, type: string, params: any) =>
    (
      await axios.put(
        prefix + "tradepassword/account/" + id + `/trade-password/${type}`,
        params
      )
    ).data,
  changeCrm: async (id: number, params: any) =>
    (
      await axios.put(
        prefix + "tradepassword/account/" + id + "/crm-password/reset",
        params
      )
    ).data,

  getInitialTradePassword: async (accountId: number) =>
    (
      await axios.get(
        prefix +
          "tradepassword/account/" +
          accountId +
          "/trade-password/initial"
      )
    ).data,

  resetToInitial: async (accountId: number, params: any) =>
    (
      await axios.post(
        prefix +
          "tradepassword/account/" +
          accountId +
          "/trade-password/reset-to-initial",
        params
      )
    ).data,

  getPasswordHistory: async (accountId: number, params?: any) =>
    (
      await axios.get(
        prefix +
          "tradepassword/account/" +
          accountId +
          "/trade-password/history",
        {
          params,
        }
      )
    ).data,
  getAccountPrefixes: async (serviceId: number) =>
    (
      await axios.get(
        prefix + `tradeserviceconfiguration/${serviceId}/account-prefixes`
      )
    ).data,

  updateAccountPrefixes: async (serviceId: number, data: any) =>
    (
      await axios.put(
        prefix + `tradeserviceconfiguration/${serviceId}/account-prefixes`,
        data
      )
    ).data,
};

export const generateAutoCompleteHandler = (
  fetchListHandler: (
    keywords: string
  ) => Promise<Array<{ value: any; label: string }>>
): ((queryString: string, cb: (arg: any) => void) => Promise<void>) => {
  let prevStr = "";
  let prevList = Array<{ value: any; label: string }>();

  return async (queryString: string, cb: (arg: any) => void) => {
    if (
      queryString === "null" ||
      queryString === "" ||
      prevStr === queryString
    ) {
      cb(prevList);
      return;
    }
    prevStr = queryString == "null" ? "" : queryString;
    prevList = await fetchListHandler(prevStr);
    cb(prevList);
  };
};

export default AccountService;
