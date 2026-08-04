import { axiosInstance as axios } from "@/core/services/api.client";

const prefix = "api/v1/tenant/";

export default {
  queryMaterialRequests: async (criteria?: any) =>
    (await axios.get(prefix + "materialrequest", { params: criteria })).data,

  getMaterialRequest: async (id: number) =>
    (await axios.get(prefix + "materialrequest/" + id)).data,

  approveMaterialRequest: async (id: number) =>
    (await axios.put(prefix + "materialrequest/" + id + "/approve")).data,

  rejectMaterialRequest: async (id: number, note: string) =>
    (await axios.put(prefix + "materialrequest/" + id + "/reject", { note }))
      .data,
};
