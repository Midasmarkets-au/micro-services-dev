import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";
import { AddedLeadCommentSpec } from "@/projects/client/modules/rep/services/RepService";
import store from "@/store";
import { computed } from "vue";
const clientPrefix = "api/v1/client/";

const ibPrefix = "api/v1/ib/";

const salesPrefix = computed(
  () => `api/v1/sales/${store.state.SalesModule.salesAccount.uid}/`
);
const getSalesPrefix = (salesUid?: number) =>
  `api/v1/sales/${salesUid ?? store.state.SalesModule.salesAccount.uid}/`;
import { normalizeAmountList } from "@/lib/utils";
const salesService = {
  getEmailByCode: async (uid: number, code: number) =>
    (
      await axios.get(
        salesPrefix.value + "account/" + uid + "/view-email/" + code
      )
    ).data,

  getViewEmailCode: async (uid: number) =>
    (await axios.get(salesPrefix.value + "account/" + uid + "/view-email-code"))
      .data,

  updateIbDefaultCode: async (code: string) =>
    (
      await axios.put(
        salesPrefix.value + "referral/code/" + code + "/default-client"
      )
    ).data,

  queryAgentClientReferralHistory: async (criteria?: any) =>
    (
      await axios.get(salesPrefix.value + "referral/user-history", {
        params: criteria,
      })
    ).data,
  updateIbLink: async (id: number, formData: any) =>
    (await axios.put(salesPrefix.value + "referral/code/" + id, formData)).data,

  checkReferCode: async (referCode: string) =>
    (await axios.get("api/v1/" + "referralCode/" + referCode)).data,

  queryAccountStat: async (childUid, criteria?: any) =>
    (
      await axios.get(salesPrefix.value + "account/" + childUid + "/stat", {
        params: criteria,
      })
    ).data,
  getReferalPath: async (uid: number) =>
    (await axios.get(salesPrefix.value + "account/referralPath/" + uid)).data,

  getAccountDetailByUid: async (accountUid: number) =>
    (await axios.get(salesPrefix.value + "account/" + accountUid)).data,

  openTradeAccount: async (accountUid: number, formData: any) =>
    (
      await axios.post(
        salesPrefix.value +
          "application/for-user/" +
          accountUid +
          "/trade-account",
        formData
      )
    ).data,
  getIBAccountConfiguration: async (agentUid: number) =>
    (await axios.get(salesPrefix.value + "account/configuration/" + agentUid))
      .data,

  getAccountDefaultLevelSetting: async (agentUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "account/" + agentUid + "/default-level-setting"
      )
    ).data,

  getIbStat: async (criteria?: any) => {
    const data = (
      await axios.get(salesPrefix.value + "account/child/stat", {
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
        salesPrefix.value + "account/child/stat/rebate/symbol-grouped",
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

  getSalesRebateStat: async (criteria?: any) =>
    (
      await axios.get(
        salesPrefix.value + "account/child/stat/trade/symbol-grouped",
        { params: criteria }
      )
    ).data,
  getDefaultLevelSetting: async (salesUid?: number) =>
    (
      await axios.get(
        getSalesPrefix(salesUid) + "rebate-rule/default-level-setting"
      )
    ).data,

  getAvailableAccountTypes: async (salesUid?: number) =>
    (
      await axios.get(
        getSalesPrefix(salesUid) +
          "account/configuration/account-type-available"
      )
    ).data,

  getRebateRuleDetailByUid: async (agentUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "rebate-rule/agent/" + agentUid + "/detail"
      )
    ).data,

  getRebateRuleRemain: async (agentUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "rebate-rule/agent/" + agentUid + "/remain"
      )
    ).data,

  getIBRebateRule: async (uid: any) =>
    (await axios.get(salesPrefix.value + "rebate-rule/agent/" + uid)).data,

  putIBRebateRule: async (agentUid: any, id: any, formData: any) =>
    (
      await axios.put(
        salesPrefix.value + "rebate-rule/agent/" + agentUid + "/" + id,
        formData
      )
    ).data,

  putTopIBRebateRule: async (agentUid: any, id: any, formData: any) =>
    (
      await axios.put(
        salesPrefix.value + "rebate-rule/top-agent/" + agentUid + "/" + id,
        formData
      )
    ).data,

  // ==========================================

  postIbReferralAgentLinkBySales: async (agentUid: number, formData: any) =>
    (
      await axios.post(
        salesPrefix.value + "referral/agent/" + agentUid + "/agent",
        formData
      )
    ).data,

  postIbReferralClientLinkBySales: async (agentUid: number, formData: any) =>
    (
      await axios.post(
        salesPrefix.value + "referral/agent/" + agentUid + "/client",
        formData
      )
    ).data,

  getLevelAccounts: async (childUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "account/" + childUid + "/level-account"
      )
    ).data,

  getAgentRules: async (criteria?: any) =>
    (
      await axios.get(salesPrefix.value + "rebate-rule/agent", {
        params: criteria,
      })
    ).data,

  getIbRebateRuleDetailFromSales: async (agentUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "rebate-rule/agent/" + agentUid + "/detail"
      )
    ).data,

  getIbRebateRuleRemainFromSales: async (agentUid: number) =>
    (
      await axios.get(
        salesPrefix.value + "rebate-rule/agent/" + agentUid + "/remain"
      )
    ).data,

  getCategory: async (_salesUid?: number) =>
    (await axios.get(clientPrefix + "rebate/symbol/category")).data,

  getSymbolsList: async () =>
    (await axios.get(clientPrefix + "rebate-direct-schema/symbol/all")).data,

  queryAccounts: async (criteria?: any) =>
    (await axios.get(salesPrefix.value + "account", { params: criteria })).data,

  getIbLinks: async (criteria?: any, salesUid?: number) =>
    (
      await axios.get(getSalesPrefix(salesUid) + "referral", {
        params: criteria,
      })
    ).data,

  postIbLink: async (formData: any) =>
    (await axios.post(salesPrefix.value + "referral", formData)).data,

  getSalesLinkDetail: async (code: any) =>
    (await axios.get(salesPrefix.value + "referral/code/" + code)).data,

  postSalesLinkForIB: async (formData: any, salesUid?: number) =>
    (
      await axios.post(
        getSalesPrefix(salesUid) + "referral/top-agent",
        formData
      )
    ).data,

  postSalesLinkForClient: async (formData: any, salesUid?: number) =>
    (await axios.post(getSalesPrefix(salesUid) + "referral/client", formData))
      .data,

  getIbLinkDetail: async (code: any) =>
    (await axios.get(salesPrefix.value + "referral/" + code)).data,

  queryTradeReportsOfSales: async (criteria?: any) =>
    (
      await axios.get(salesPrefix.value + "tradetransaction", {
        params: criteria,
      })
    ).data,

  queryTransactionReportsOfSales: async (criteria?: any) => {
    delete criteria?.totalAmount;
    const res = (
      await axios.get(salesPrefix.value + "transaction", {
        params: criteria,
      })
    ).data;
    console.log("slae", res);
    res.data = normalizeAmountList(res.data);
    res.criteria = normalizeAmountList(res.criteria, "totalAmount");
    return res;
  },

  fuzzySearchAccount: async (criteria?: any) =>
    (
      await axios.get(salesPrefix.value + "search/account", {
        params: criteria,
      })
    ).data,

  queryClientTransaction: async (accountUid: number, criteria?: any) =>
    (
      await axios.get(
        salesPrefix.value + "trade-account/" + accountUid + "/transaction",
        { params: criteria }
      )
    ).data,

  queryClientTrade: async (accountUid: number, criteria?: any) =>
    (
      await axios.get(
        salesPrefix.value + "trade-account/" + accountUid + "/trade",
        { params: criteria }
      )
    ).data,

  queryClientDeposit: async (criteria?: any) => {
    delete criteria?.totalAmount;
    const res = (
      await axios.get(salesPrefix.value + "deposit", { params: criteria })
    ).data;
    return {
      criteria: normalizeAmountList(res?.criteria, "totalAmount"),
      data: normalizeAmountList(res?.data),
    };
  },

  queryClientWithdrawal: async (criteria?: any) => {
    delete criteria?.totalAmount;
    const res = (
      await axios.get(salesPrefix.value + "withdrawal", { params: criteria })
    ).data;
    return {
      criteria: normalizeAmountList(res?.criteria, "totalAmount"),
      data: normalizeAmountList(res?.data),
    };
  },

  querySalesLeads: async (criteria?: any) =>
    (await axios.get(salesPrefix.value + "lead", { params: criteria })).data,

  addCommentToLead: async (
    leadId: number,
    addedLeadCommentSpec: AddedLeadCommentSpec
  ) =>
    (
      await axios.post(
        salesPrefix.value + "lead/" + leadId + "/comment",
        addedLeadCommentSpec
      )
    ).data,

  getLeadDetails: async (leadId: number) =>
    (await axios.get(salesPrefix.value + "lead/" + leadId)).data,

  getSalesStatistics: async (criteria?: any) => {
    // ==================== 临时假数据 - 开始 ====================
    // TODO: 等后端接口准备好后，删除此部分假数据生成逻辑，恢复真实API调用

    // 生成时间序列数据
    // const generateTimeSeriesData = () => {
    //   const days = criteria?.timeRange === "7" ? 7 : 30;
    //   const data: any[] = [];
    //   const today = new Date();

    //   for (let i = days - 1; i >= 0; i--) {
    //     const date = new Date(today);
    //     date.setDate(date.getDate() - i);
    //     const dateStr = date.toISOString().split("T")[0];

    //     data.push({
    //       date: dateStr,
    //       trades: Math.floor(Math.random() * 100) + 20,
    //       deposit: Math.floor(Math.random() * 10000000) + 1000000, // 10,000 - 100,000 USD
    //       withdrawal: Math.floor(Math.random() * 5000000) + 500000, // 5,000 - 50,000 USD
    //       netDeposit: Math.floor(Math.random() * 5000000) + 500000,
    //       rebate: Math.floor(Math.random() * 200000) + 50000, // 500 - 2,000 USD
    //     });
    //   }
    //   return data;
    // };

    // // 生成产品分布数据
    // const generateProductDistribution = () => {
    //   const products = [
    //     { symbol: "EURUSD", count: 450, percentage: 35.2 },
    //     { symbol: "GBPUSD", count: 320, percentage: 25.1 },
    //     { symbol: "USDJPY", count: 280, percentage: 21.9 },
    //     { symbol: "GOLD", count: 150, percentage: 11.7 },
    //     { symbol: "OIL", count: 78, percentage: 6.1 },
    //   ];
    //   return products;
    // };

    // // 生成层级数据
    // const generateHierarchyData = () => {
    //   return [
    //     {
    //       id: 10001,
    //       name: "Sales Manager A",
    //       type: "Sale",
    //       groupCode: "SA001",
    //       trades: 1500,
    //       netDeposit: 35000000, // 350,000 USD
    //       deposit: 50000000,
    //       withdrawal: 15000000,
    //       rebate: 1200000, // 12,000 USD
    //       lots: 285.5,
    //       products: ["EURUSD", "GBPUSD", "GOLD"],
    //       hasChildren: true,
    //       children: [
    //         {
    //           id: 10101,
    //           name: "IB Agent B1",
    //           type: "IB",
    //           groupCode: "IB101",
    //           trades: 800,
    //           netDeposit: 18000000,
    //           deposit: 25000000,
    //           withdrawal: 7000000,
    //           rebate: 650000,
    //           lots: 145.2,
    //           products: ["EURUSD", "GBPUSD"],
    //           hasChildren: true,
    //           children: [
    //             {
    //               id: 10201,
    //               name: "Client C1",
    //               type: "Client",
    //               groupCode: "CL201",
    //               trades: 250,
    //               netDeposit: 5000000,
    //               deposit: 7000000,
    //               withdrawal: 2000000,
    //               rebate: 180000,
    //               lots: 45.8,
    //               products: ["EURUSD"],
    //               hasChildren: false,
    //               children: [],
    //             },
    //             {
    //               id: 10202,
    //               name: "Client C2",
    //               type: "Client",
    //               groupCode: "CL202",
    //               trades: 350,
    //               netDeposit: 8000000,
    //               deposit: 11000000,
    //               withdrawal: 3000000,
    //               rebate: 280000,
    //               lots: 68.5,
    //               products: ["GBPUSD", "EURUSD"],
    //               hasChildren: false,
    //               children: [],
    //             },
    //           ],
    //         },
    //         {
    //           id: 10102,
    //           name: "IB Agent B2",
    //           type: "IB",
    //           groupCode: "IB102",
    //           trades: 700,
    //           netDeposit: 17000000,
    //           deposit: 25000000,
    //           withdrawal: 8000000,
    //           rebate: 550000,
    //           lots: 140.3,
    //           products: ["GOLD", "USDJPY"],
    //           hasChildren: false,
    //           children: [],
    //         },
    //       ],
    //     },
    //     {
    //       id: 10002,
    //       name: "Sales Manager B",
    //       type: "Sale",
    //       groupCode: "SA002",
    //       trades: 980,
    //       netDeposit: 22000000,
    //       deposit: 32000000,
    //       withdrawal: 10000000,
    //       rebate: 850000,
    //       lots: 178.9,
    //       products: ["USDJPY", "OIL"],
    //       hasChildren: true,
    //       children: [
    //         {
    //           id: 10103,
    //           name: "IB Agent B3",
    //           type: "IB",
    //           groupCode: "IB103",
    //           trades: 560,
    //           netDeposit: 12000000,
    //           deposit: 17000000,
    //           withdrawal: 5000000,
    //           rebate: 450000,
    //           lots: 98.6,
    //           products: ["USDJPY"],
    //           hasChildren: false,
    //           children: [],
    //         },
    //       ],
    //     },
    //   ];
    // };

    // const timeSeriesData = generateTimeSeriesData();
    // const totalTrades = timeSeriesData.reduce((sum, d) => sum + d.trades, 0);
    // const totalDeposit = timeSeriesData.reduce((sum, d) => sum + d.deposit, 0);
    // const totalWithdrawal = timeSeriesData.reduce(
    //   (sum, d) => sum + d.withdrawal,
    //   0
    // );
    // const totalRebate = timeSeriesData.reduce((sum, d) => sum + d.rebate, 0);

    // const mockData = {
    //   summaryStats: {
    //     totalTrades: totalTrades,
    //     totalNetDeposit: totalDeposit - totalWithdrawal,
    //     totalRebate: totalRebate,
    //     totalDeposit: totalDeposit,
    //     totalWithdrawal: totalWithdrawal,
    //     totalLots: 464.4,
    //   },
    //   timeSeriesData: timeSeriesData,
    //   productDistribution: generateProductDistribution(),
    //   hierarchyData: generateHierarchyData(),
    // };

    // // 模拟API延迟
    // await new Promise((resolve) => setTimeout(resolve, 800));

    // const res = mockData;
    // ==================== 临时假数据 - 结束 ====================

    //真实API调用（暂时注释）
    const res = (
      await axios.get("/api/v1/sales/statistics", {
        params: criteria,
      })
    ).data;

    // Normalize amount fields
    if (res.summaryStats) {
      res.summaryStats = normalizeAmountList(res.summaryStats, [
        "totalNetDeposit",
        "totalRebate",
        "totalDeposit",
        "totalWithdrawal",
      ]) as any;
    }

    if (res.timeSeriesData) {
      res.timeSeriesData = normalizeAmountList(res.timeSeriesData, [
        "deposit",
        "withdrawal",
        "netDeposit",
        "rebate",
      ]) as any;
    }

    if (res.hierarchyData) {
      const normalizeHierarchy = (items: any[]) => {
        return items.map((item) => {
          const normalized = normalizeAmountList(item, [
            "netDeposit",
            "deposit",
            "withdrawal",
            "rebate",
          ]);
          if (normalized.children && normalized.children.length > 0) {
            normalized.children = normalizeHierarchy(normalized.children);
          }
          return normalized;
        });
      };
      res.hierarchyData = normalizeHierarchy(res.hierarchyData);
    }

    return res;
  },
};

export default salesService;
