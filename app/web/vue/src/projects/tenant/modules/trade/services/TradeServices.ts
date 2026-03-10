import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";

const prefix = "api/v1/tenant/";
const prefixV2 = "api/v2/tenant/";

export default {
  queryProfitAndLoss: async (criteria?: any) =>
    (await axios.get(prefixV2 + "account/stat-top", { params: criteria })).data,

  queryBriefDetailList: async (category: string, rowId: number, key: string) =>
    (
      await axios.get(
        prefix + "configuration" + "/" + category + "/" + rowId + "/" + key
      )
    ).data,

  updateBriefDetailList: async (
    category: string,
    rowId: number,
    key: string,
    formData: object
  ) =>
    (
      await axios.put(
        prefix + "configuration" + "/" + category + "/" + rowId + "/" + key,
        formData
      )
    ).data,

  getTwoFaCode: async (criteria?: any) =>
    (await axios.get(prefix + "auto-code", { params: criteria })).data,

  getChecklist: async () => (await axios.get(prefix + "account-check")).data,

  addChecklist: async (formData: object) =>
    (await axios.post(prefix + "account-check", formData)).data,

  updateChecklist: async (id: number, formData: object) =>
    (await axios.put(prefix + "account-check/" + id, formData)).data,

  deleteChecklist: async (id: number) =>
    (await axios.delete(prefix + "account-check/" + id)).data,

  resetCache: async () =>
    (await axios.put(prefix + "account-check/reset-cache")).data,

  getEquityBelowCredit: async (criteria?: any) =>
    (
      await axios.get(prefix + "account-check/equity-below-credit", {
        params: criteria,
      })
    ).data,

  sendEquityBelowCreditEmail: async (data?: any) =>
    (await axios.post(prefix + "account-check/equity-below-credit/email", data))
      .data,
};
