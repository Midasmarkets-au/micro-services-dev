import { axiosInstance as axios } from "@/core/services/api.client";
const prefix = "api/v1/tenant/";

export default {
  getItemList: async (criteria?: any) =>
    (await axios.get(prefix + "shop/item", { params: criteria })).data,

  createItem: async (formData: any) =>
    (await axios.post(prefix + "shop/item", formData)).data,

  updateItem: async (id: number, formData: any) =>
    (await axios.put(prefix + "shop/item/" + id, formData)).data,

  getOrderList: async (criteria?: any) =>
    (await axios.get(prefix + "shop/order", { params: criteria })).data,

  uploadImage: async (formData: any) =>
    (await axios.post(prefix + "upload/public?type=public", formData)).data,
};
