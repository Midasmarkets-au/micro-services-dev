import {
  axiosInstance as axios,
  paramsSerializerForDotnet,
} from "@/core/services/api.client";

const prefix = "api/v1/tenant/";

export type AccountWizardType = {
  kycFormCompleted: boolean;
  paymentAccessGranted: boolean;
  welcomeEmailSent: boolean;
  noticeEmailSent: boolean;
};

const AccountOverviewService = {
  queryAccounts: async (criteria?: any) =>
    (
      await axios.get(prefix + "account", {
        params: criteria,
        paramsSerializer: paramsSerializerForDotnet,
      })
    ).data,

  queryTrades: async (criteria?: any) =>
    (await axios.get(prefix + "trade", { params: criteria })).data,

  queryTransactions: async (criteria?: any) =>
    (await axios.get(prefix + "transaction", { params: criteria })).data,

  queryDeposits: async (criteria?: any) =>
    (await axios.get(prefix + "deposit", { params: criteria })).data,

  queryWithdrawals: async (criteria?: any) =>
    (await axios.get(prefix + "withdrawal", { params: criteria })).data,

  getAccountDetailById: async (id: number) =>
    (await axios.get(prefix + "account/" + id)).data,
};

export default AccountOverviewService;
