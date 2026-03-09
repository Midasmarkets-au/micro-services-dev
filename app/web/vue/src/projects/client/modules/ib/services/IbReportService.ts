import { axiosInstance as axios } from "@/core/services/api.client";
import { computed } from "vue";
import store from "@/store";
import { TimeZoneService } from "@/core/plugins/TimerService";
import { axiosInstance2 as axiosV2 } from "@/core/services/api.client";
import { normalizeAmountList } from "@/lib/utils";
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const agentPrefix = computed(() => `api/v1/ib/${agentAccount.value?.uid}/`);

export default {
  createIbMonthlyReport: async (criteria?: any) =>
    (await axiosV2.get("v2/client/ib/report/monthly", { params: criteria }))
      .data,

  getRebateTodayValue: async (timezoneOffset?: number) => {
    const res = await axios.get(
      `${agentPrefix.value}report/rebate/today-value`,
      {
        params: { timezoneOffset },
      }
    );
    return normalizeAmountList(res.data);
  },

  getRebateTotalValue: async (criteria?: any) => {
    const res = await axios.get(
      `${agentPrefix.value}report/rebate/total-value`,
      {
        params: criteria,
      }
    );
    console.log("res", res);
    return normalizeAmountList(res.data);
  },

  getRebateDailySeries: async (timezoneOffset?: number) => {
    const res = await axios.get(`${agentPrefix.value}report/rebate/daily`, {
      params: { timezoneOffset },
    });
    console.log("res-dayly", res.data);
    return normalizeAmountList(res.data, "totalValue");
  },

  getRebateHourlySeries: async (timezoneOffset?: number) => {
    const res = await axios.get(`${agentPrefix.value}report/rebate/hourly`, {
      params: { timezoneOffset },
    });
    console.log("res-hourly", res.data);
    return normalizeAmountList(res.data, "totalValue");
  },

  getRebateMonthlySeries: async (timezoneOffset?: number) => {
    const res = await axios.get(`${agentPrefix.value}report/rebate/monthly`, {
      params: { timezoneOffset },
    });
    console.log("res-monty", res.data);
    return normalizeAmountList(res.data, "totalValue");
    return res.data;
  },

  getTradeTodayVolume: async () =>
    (
      await axios.get(`${agentPrefix.value}report/trade/today-volume`, {
        params: { timezoneOffset: TimeZoneService.getTimeZoneOffsetInHours() },
      })
    ).data,

  getTradeSymbolTodayVolume: async () =>
    (
      await axios.get(`${agentPrefix.value}report/trade/today-symbol-volume`, {
        params: { timezoneOffset: TimeZoneService.getTimeZoneOffsetInHours() },
      })
    ).data,

  getLatestDeposits: async (count?: number) => {
    const res = (
      await axios.get(`${agentPrefix.value}report/deposit/latest`, {
        params: { count },
      })
    ).data;
    return normalizeAmountList(res);
  },

  getTodayAccountCreationCount: async () =>
    (await axios.get(`${agentPrefix.value}report/account/today-creation`)).data,

  getDepositTodayValue: async () => {
    const res = (
      await axios.get(`${agentPrefix.value}report/deposit/today-value`)
    ).data;
    return normalizeAmountList(res);
  },
};
