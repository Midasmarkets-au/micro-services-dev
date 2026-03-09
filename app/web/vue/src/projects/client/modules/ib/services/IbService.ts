import { axiosInstance as axios } from "@/core/services/api.client";
import store from "@/store";
import { computed } from "vue";
const clientPrefix = "api/v1/client/";
const brokerPrefix = "api/v1/broker/";

const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const agentPrefix = computed(() => `api/v1/ib/${agentAccount.value?.uid}/`);
import { normalizeAmountList } from "@/lib/utils";
export default {
  getEmailByCode: async (uid: number, code: number) =>
    (
      await axios.get(
        agentPrefix.value + "account/" + uid + "/view-email/" + code
      )
    ).data,

  getViewEmailCode: async (uid: number) =>
    (await axios.get(agentPrefix.value + "account/" + uid + "/view-email-code"))
      .data,

  queryRequests: async (criteria?: any) =>
    (
      await axios.get(agentPrefix.value + "report/request", {
        params: criteria,
      })
    ).data,

  setDefaultClient: async (code: string) =>
    (
      await axios.put(
        agentPrefix.value + "referral/code/" + code + "/default-client"
      )
    ).data,

  updateIbLink: async (id: number, formData: any) =>
    (await axios.put(agentPrefix.value + "referral/code/" + id, formData)).data,

  getIbStat: async (criteria?: any) => {
    const data = (
      await axios.get(agentPrefix.value + "account/child/stat", {
        params: criteria,
      })
    ).data;
    if (data.rebateAmounts) {
      Object.keys(data.rebateAmounts).forEach((key) => {
        data.rebateAmounts[key] = normalizeAmountList(data.rebateAmounts[key]);
      });
    }
    if (data.depositAmounts) {
      Object.keys(data.depositAmounts).forEach((key) => {
        data.depositAmounts[key] = normalizeAmountList(
          data.depositAmounts[key]
        );
      });
    }
    if (data.netAmounts) {
      Object.keys(data.netAmounts).forEach((key) => {
        data.netAmounts[key] = normalizeAmountList(data.netAmounts[key]);
      });
    }
    if (data.profitAmounts) {
      Object.keys(data.profitAmounts).forEach((key) => {
        data.profitAmounts[key] = normalizeAmountList(data.profitAmounts[key]);
      });
    }
    if (data.withdrawalAmounts) {
      Object.keys(data.withdrawalAmounts).forEach((key) => {
        data.withdrawalAmounts[key] = normalizeAmountList(
          data.withdrawalAmounts[key]
        );
      });
    }
    return data;
  },
  getIbRebateStat: async (criteria?: any) => {
    const res = (
      await axios.get(
        agentPrefix.value + "account/child/stat/rebate/symbol-grouped",
        { params: criteria }
      )
    ).data;
    Object.keys(res).forEach((key) => {
      Object.keys(res[key].amounts).forEach((currecyId) => {
        res[key].amounts[currecyId] = normalizeAmountList(
          res[key].amounts[currecyId]
        );
      });
    });
    return res;
  },

  getDefaultLevelSetting: async () =>
    (await axios.get(agentPrefix.value + "rebate-rule/default-level-setting"))
      .data,

  getAccountDefaultLevelSetting: async (uid: any) =>
    (
      await axios.get(
        agentPrefix.value + "account/" + uid + "/default-level-setting"
      )
    ).data,

  getRebateRuleDetailByUid: async (uid: any) =>
    (await axios.get(agentPrefix.value + "rebate-rule/account/" + uid)).data,

  putIBRebateRule: async (_: any, id: any, formData: any) =>
    (await axios.put(agentPrefix.value + "rebate-rule/" + id, formData)).data,

  getAccountDetail: async (uid: any) =>
    (await axios.get(clientPrefix + "account/" + uid)).data,

  getIBAccountDetail: async (criteria?: any) =>
    (await axios.get(clientPrefix + "account", { params: criteria })).data,

  getCategory: async () =>
    (await axios.get(clientPrefix + "rebate/symbol/category")).data,

  getRebateRuleDetail: async () =>
    (await axios.get(agentPrefix.value + "rebate-rule/detail")).data,

  getRebateRuleRemain: async () =>
    (await axios.get(agentPrefix.value + "rebate-rule/remain")).data,

  getSymbolCategory: async () =>
    (await axios.get(clientPrefix + "symbol/category")).data,

  getIbLinkDetail: async (code: any) =>
    (await axios.get(agentPrefix.value + "referral/" + code)).data,

  getIbLinks: async (criteria?: any) =>
    (await axios.get(agentPrefix.value + "referral", { params: criteria }))
      .data,

  getBrokerRules: async (uid: any) =>
    (await axios.get(brokerPrefix + uid + "rebate-rule/broker")).data,

  getAgentRules: async () =>
    (await axios.get(agentPrefix.value + "rebate-rule")).data,

  // getRemainRate: async () =>
  //   (await axios.get(agentPrefix.value + "referral/rate/remain")).data,

  postIbLink: async (formData: any) =>
    (await axios.post(agentPrefix.value + "referral", formData)).data,

  postIBLinkForIB: async (formData: any) =>
    (await axios.post(agentPrefix.value + "referral/ib", formData)).data,

  postIBLinkForClient: async (formData: any) =>
    (await axios.post(agentPrefix.value + "referral/client", formData)).data,

  queryAgentClientAccountsByAgent: async (criteria?: any) => {
    const res = (
      await axios.get(agentPrefix.value + "account", { params: criteria })
    ).data;
    // res.data.forEach((item) => {
    //   if (item.tradeAccount) {
    //     item.tradeAccount = normalizeAmountList(item.tradeAccount, [
    //       "balance",
    //       "balanceInCents",
    //       "equity",
    //       "equityInCents",
    //     ]);
    //   }
    // });
    return res;
  },

  queryAgentClientReferralHistory: async (criteria?: any) =>
    (
      await axios.get(agentPrefix.value + "referral/user-history", {
        params: criteria,
      })
    ).data,

  queryTradesOfTradeAccountOfAgent: async (
    accountUid: number,
    criteria?: any
  ) =>
    (
      await axios.get(
        agentPrefix.value + "trade-account/" + accountUid + "/trade",
        { params: criteria }
      )
    ).data,

  queryClientTransaction: async (accountUid: number, criteria?: any) => {
    const res = (
      await axios.get(
        agentPrefix.value + "trade-account/" + accountUid + "/transaction",
        { params: criteria }
      )
    ).data;
    res.data = normalizeAmountList(res.data);
    return res;
  },

  queryDeposit: async (criteria?: any) => {
    delete criteria?.totalAmount;
    const res = (
      await axios.get(agentPrefix.value + "deposit", { params: criteria })
    ).data;
    return {
      criteria: normalizeAmountList(res?.criteria, "totalAmount"),
      data: normalizeAmountList(res?.data),
    };
  },

  queryWithdrawal: async (criteria?: any) => {
    delete criteria?.totalAmount;
    const res = (
      await axios.get(agentPrefix.value + "withdrawal", { params: criteria })
    ).data;
    return {
      ...res,
      criteria: normalizeAmountList(res?.criteria, "totalAmount"),
      data: normalizeAmountList(res?.data),
    };
  },

  queryTradeReportsOfAgent: async (criteria?: any) =>
    (
      await axios.get(agentPrefix.value + "tradetransaction", {
        params: criteria,
      })
    ).data,

  queryRebateReportsOfAgent: async (criteria: any) => {
    const res = await axios.get(agentPrefix.value + "rebate", {
      params: criteria,
    });
    const re = {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
      criteria: {
        ...res.data?.criteria,
      },
    };
    return re;
  },

  queryRebateDistributionRulesOfAgent: async (criteria?: any) =>
    (
      await axios.get(agentPrefix.value + "rebate-rule/distribution", {
        params: criteria,
      })
    ).data,

  getReferralCode: async (criteria?: any) =>
    (await axios.get(agentPrefix.value + "referral", { params: criteria }))
      .data,

  postReferralCode: async (data: object) =>
    (await axios.post(agentPrefix.value + "referral", data)).data,

  putReferralCode: async (editCodeId: string, data: object) =>
    (await axios.put(agentPrefix.value + "referral/" + editCodeId, data)).data,
};
