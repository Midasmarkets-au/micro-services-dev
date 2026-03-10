import { axiosInstance2 as axiosV2 } from "@/core/services/api.client";

const prefixV2 = "api/v2/tenant/documents";

export default {
  //Documents API
  queryDocumentsList: async (criteria?: any) =>
    (await axiosV2.get(prefixV2, { params: criteria })).data,

  queryDocumentHistoryById: async (id: number, criteria: any) =>
    (await axiosV2.get(prefixV2 + `/${id}`, { params: criteria })).data,

  uploadDocuments: async (id: number, formData: any, criteria?: any) =>
    (
      await axiosV2.post(prefixV2 + `/${id}/upload`, formData, {
        params: criteria,
      })
    ).data,

  addNewDocument: async (formData: any) =>
    (await axiosV2.post(prefixV2, formData)).data,

  //Contract Specs API
  queryContractSpecsList: async (criteria?: any) =>
    (await axiosV2.get(prefixV2 + "/contractspecs", { params: criteria })).data,

  updateContractSpecs: async (id: number, formData: any) =>
    (await axiosV2.put(prefixV2 + `/contractspecs/${id}`, formData)).data,

  updateContractSpecsStatus: async (id: number) =>
    (await axiosV2.put(prefixV2 + `/contractspecs/${id}/status`)).data,
};
