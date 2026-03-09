import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";
import {
  TenantRebateCriteria,
  TenantRebateViewModel,
} from "@/core/models/Rebate";
import { Result } from "@/core/types/Result";
import { normalizeAmountList } from "@/lib/utils";
const prefix = "api/v1/tenant/";
const prefixV2 = "v2/tenant/";

export default {
  querySalesRebate: async (criteria?: any) =>
    (
      await axios.get(prefix + "sales-rebate", {
        params: criteria,
      })
    ).data,

  updateSalesRebateActiveStatus: async (id: number, data: any) =>
    (await axios.put(prefix + "sales-rebate/" + id, data)).data,

  postSalesRebateSchema: async (data: any) =>
    (await axios.post(prefix + "sales-rebate-schema", data)).data,

  putSalesRebateSchema: async (id: number, data: any) =>
    (await axios.put(prefix + "sales-rebate-schema/" + id, data)).data,

  putSalesRebateSchemaStatus: async (id: number) =>
    (await axios.put(prefix + "sales-rebate-schema/" + id + "/approve")).data,

  deleteSalesRebateSchema: async (id: number) =>
    (await axios.put(prefix + "sales-rebate-schema/" + id + "/delete")).data,

  querySalesRebateSchemas: async (criteria?: any) =>
    (
      await axios.get(prefix + "sales-rebate-schema", {
        params: criteria,
      })
    ).data,

  resendRebate: async (id: number) =>
    (await axios.post(prefix + "trade-rebate/" + id + "/reset")).data,

  getRebateCheck: async (rebateId?: number) => {
    let res = (
      await axios.get(prefix + "trade-rebate/" + rebateId + "/check-rebates")
    ).data;
    res = normalizeAmountList(res);
    return res;
  },

  getDefaultLevelSetting: async (uid: number) =>
    (
      await axios.get(
        prefix + "rebate-agent-rule/" + uid + "/default-level-setting"
      )
    ).data,

  putDistributionType: async (id: number, data: any) =>
    (
      await axios.put(
        prefix + "rebate-client-rule/distribution-type/" + id,
        data
      )
    ).data,

  // ========================================================= Utility

  getCategory: async () => (await axios.get(prefix + "symbol/category")).data,

  getRebateCategory: async () =>
    (await axios.get(prefix + "rebate/symbol/category")).data,

  getRemainLevelSettingById: async (id: number) =>
    (await axios.get(prefix + "account/" + id + "/level-setting")).data,

  getAgentRules: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-agent-rule", { params: criteria })).data,

  updateTopAgentRule: async (id: number, data: any) =>
    (
      await axios.put(
        prefix + "rebate-agent-rule/" + id + "/level-setting",
        data
      )
    ).data,

  updateAgentRule: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-agent-rule/" + id + "/schema", data))
      .data,

  getRemainRebate: async (uid: number) =>
    (await axios.get(prefix + "rebate-agent-rule/" + uid + "/remain")).data,

  // ========================================================= Rebate Schema

  getSchemaUsedByDirectRule: async (id: number, criteria?: any) =>
    (
      await axios.get(
        prefix + "rebate-direct-schema/" + id + "/used-direct-rule-schema",
        {
          params: criteria,
        }
      )
    ).data,

  getSchemaUsedByClientRule: async (id: number, criteria?: any) =>
    (
      await axios.get(
        prefix + "rebate-direct-schema/" + id + "/used-client-rule-schema",
        {
          params: criteria,
        }
      )
    ).data,

  getRebateSchemaList: async (criteria?: any) =>
    (
      await axios.get(prefix + "rebate-direct-schema/list", {
        params: criteria,
      })
    ).data,

  queryRebateSchemas: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-direct-schema", { params: criteria }))
      .data,

  getRebateSchema: async (id: number) =>
    (await axios.get(prefix + "rebate-direct-schema/" + id)).data,

  postRebateSchema: async (data: any) =>
    (await axios.post(prefix + "rebate-direct-schema/", data)).data,

  putRebateSchema: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-direct-schema/" + id, data)).data,

  putRebateSchemaItems: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-direct-schema/" + id + "/items", data))
      .data,

  deleteRebateSchema: async (id: number) =>
    (await axios.delete(prefix + "rebate-direct-schema/" + id)).data,

  // ========================================================= Rebate Base Schema
  getBaseSchemaList: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-base-schema/list", { params: criteria }))
      .data,

  deleteBaseSchemaList: async (id: number) =>
    (await axios.delete(prefix + "rebate-base-schema/" + id)).data,

  queryBaseRebateSchemas: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-base-schema", { params: criteria })).data,

  // ========================================================= Rebate Bundle Schema

  getBundleList: async (criteria?: any) =>
    (
      await axios.get(prefix + "rebate-schema-bundle/list", {
        params: criteria,
      })
    ).data,

  queryRebateSchemaBundle: async (criteria?: any) =>
    (
      await axios.get(prefix + "rebate-schema-bundle", {
        params: criteria,
      })
    ).data,

  deleteRebateSchemaBundle: async (id: number) =>
    (await axios.delete(prefix + "rebate-schema-bundle/" + id)).data,
  // ========================================================= Confirmation

  putConfirmRebateRule: async (id: number) =>
    (await axios.put(prefix + "rebate-rule/" + id + "/confirm")).data,

  putConfirmDirectRebateRule: async (id: number) =>
    (await axios.put(prefix + "rebate-direct-rule/" + id + "/confirm")).data,

  // ========================================================= Rebate Rules
  postRebateDirectRule: async (formData: any) =>
    (await axios.post(prefix + "rebate-direct-rule/", formData)).data,

  putRebateDirectRule: async (id: number, formData: any) =>
    (await axios.put(prefix + "rebate-direct-rule/" + id, formData)).data,

  putRebateAllocationRule: async (id: number, formData: any) =>
    (await axios.put(prefix + "rebate-client-rule/" + id, formData)).data,

  deleteSymbolRebate: async (id: number, sid: number) =>
    (await axios.delete(prefix + "rebate-rule/" + id + "/symbol/" + sid)).data,

  queryRebates: async (
    criteria?: TenantRebateCriteria
  ): Promise<Result<Array<TenantRebateViewModel>, TenantRebateCriteria>> => {
    const res = await axios.get(prefix + "rebate", { params: criteria });
    return {
      ...res.data,
      data: res.data?.data
        ? normalizeAmountList(res.data.data)
        : res.data?.data,
    };
  },

  queryTradeRebate: async (criteria?: any) =>
    (await axios.get(prefix + "trade-rebate", { params: criteria })).data,

  getTradeRebateDetails: async (id: number) => {
    const res = await axios.get(prefix + "trade-rebate/" + id);
    console.log("res", res);
    return {
      ...res.data,
      rebates: res.data?.rebates
        ? normalizeAmountList(res.data.rebates)
        : res.data?.rebates,
    };
    return res;
  },

  deleteRebateRule: async (id: number) =>
    (await axios.delete(prefix + "rebate-rule/" + id)).data,
  putClientBaseSchema: async (id: number, schemaID: number) =>
    (await axios.put(prefix + "trade-account/" + id + "/template/" + schemaID))
      .data,
  putSymbolRebate: async (id: number, data: any) =>
    (
      await axios.put(
        prefix + "rebate-rule/" + id + "/symbol/" + data.sid,
        data
      )
    ).data,

  getTradeFromRebate: async (criteria?: any) =>
    (await axios.get(prefix + "trade", { params: criteria })).data,
  // =========================================================

  getExchangeRate: async (criteria?: any) =>
    (await axios.get(prefix + "exchange-rate", { params: criteria })).data,

  putExchangeRate: async (id: number, data: any) =>
    (await axios.put(prefix + "exchange-rate/" + id, data)).data,

  postExchangeRate: async (data: any) =>
    (await axios.post(prefix + "exchange-rate/", data)).data,

  getExchangeRateHistory: async (id: number) =>
    (await axios.get(prefix + "exchange-rate/" + id + "/history")).data,

  // =========================================================
  getSymbolsList: async () =>
    (await axios.get(prefix + "rebate/symbol/all")).data,

  getProductsList: async (criteria?: any) =>
    (await axios.get(prefix + "symbol", { params: criteria })).data,

  getProductsListByType: async (type?: number) => {
    const res = (
      await axios.get(prefix + "symbol/categories", {
        params: { type },
      })
    ).data;
    return res;
  },
  getRebatePipExLink: async (id: number) =>
    (await axios.get(prefix + "rebate-rule/pip/" + id)).data,

  putRebatePipExLink: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-rule/pip/" + id, data)).data,

  postRebatePipExLink: async (data: any) =>
    (await axios.post(prefix + "rebate-rule/pip", data)).data,

  // =========================================================
  queryDirectRebateRules: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-direct-rule", { params: criteria })).data,

  deleteSymbolsRebateRule: async (id: number) =>
    (await axios.delete(prefix + "rebate-rule/" + id)).data,

  postRebateBaseSchema: async (data: any) =>
    (await axios.post(prefix + "rebate-base-schema", data)).data,

  putRebateBaseSchema: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-base-schema/" + id, data)).data,

  putRebateBaseSchemaItems: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-base-schema/" + id + "/items", data))
      .data,

  getRebateBaseSchemaById: async (id: number) =>
    (await axios.get(prefix + "rebate-base-schema/" + id)).data,

  getRebateSchemaBundleById: async (id: number) =>
    (await axios.get(prefix + "rebate-schema-bundle/" + id)).data,

  postRebateSchemaBundle: async (data: any) =>
    (await axios.post(prefix + "rebate-schema-bundle", data)).data,

  putRebateSchemaBundle: async (id: number, data: any) =>
    (await axios.put(prefix + "rebate-schema-bundle/" + id, data)).data,

  // =========================================================
  getRebateDirectRule: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-direct-rule/", { params: criteria }))
      .data,

  getRebateDirectRuleById: async (id: number) =>
    (await axios.get(prefix + "rebate-direct-rule/" + id)).data,

  getRebateClientRule: async (criteria?: any) =>
    (await axios.get(prefix + "rebate-client-rule/", { params: criteria }))
      .data,

  getRebateClientRuleById: async (id: number) =>
    (await axios.get(prefix + "rebate-client-rule/" + id)).data,

  deleteRebateDirectRuleById: async (id: number) =>
    (await axios.delete(prefix + "rebate-direct-rule/" + id)).data,

  queryRebateRecordV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "rebate", { params: criteria })).data,

  queryRebateRecordExportV2: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "rebate/export", { params: criteria })).data,

  // ========================================================= Symbol Management
  createSymbol: async (data: any) =>
    (await axios.post(prefix + "symbol", data)).data,

  updateSymbol: async (id: number, data: any) =>
    (await axios.put(prefix + "symbol/" + id, data)).data,

  deleteSymbol: async (id: number) =>
    (await axios.delete(prefix + "symbol/" + id)).data,

  batchDeleteSymbols: async (ids: number[]) =>
    (await axios.delete(prefix + "symbol/batch", { data: { Ids: ids } })).data,

  // ========================================================= Category Management
  createCategory: async (data: any) =>
    (await axios.post(prefix + "symbol/category", data)).data,

  updateCategory: async (id: number, data: any) =>
    (await axios.put(prefix + "symbol/category/" + id, data)).data,

  deleteCategory: async (id: number) =>
    (await axios.delete(prefix + "symbol/category/" + id)).data,

  // ========================================================= Default Rebate Level
  getDefaultRebateLevel: async () =>
    (await axios.get(prefix + "DefaultRebateLevel")).data,

  updateDefaultRebateLevel: async (data: any) =>
    (await axios.put(prefix + "DefaultRebateLevel", data)).data,

  // ========================================================= Archive
  postSymbolRebate: async (id: number, data: any) =>
    (
      await axios.post(
        prefix + "rebate-rule/" + id + "/symbol/" + data.sid,
        data
      )
    ).data,
};
