import { axiosInstance as axios } from "@/core/services/api.client";
const prefix = "api/v1/client/";
const event = "EventShop";
import { normalizeAmountList } from "@/lib/utils";
export default {
  queryShopCategoryList: async () =>
    (await axios.get(prefix + "event/shop/item/category-name")).data,

  updateOrderAddress: async (orderHashId: string, addressHashId: string) =>
    (
      await axios.put(
        prefix + "event/shop/order/" + orderHashId + "/address/" + addressHashId
      )
    ).data,
  confirmDelivery: async (hashId: string) =>
    (await axios.put(prefix + "event/shop/order/" + hashId + "/succeed")).data,
  queryRewardRebateList: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/reward/rebate", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data, "point") : res.data,
    };
  },

  activateReward: async (hashId: string) =>
    (await axios.put(prefix + "/" + hashId + "/active")).data,

  queryRewardList: async (criteria?: any) => {
    const params = {
      EventKey: "EventShop",
      ...criteria,
    };

    const response = await axios.get(prefix + "event/shop/reward", { params });
    console.log("response", response);
    return response.data;
  },

  queryEventUserDetail: async () => {
    const res = (await axios.get(prefix + "event/" + event + "/user")).data;
    return normalizeAmountList(res, ["point", "totalPoint"]);
  },

  queryEventByKey: async (key: string) =>
    (await axios.get(prefix + "event/" + key)).data,

  registerEventByKey: async (key: string) =>
    (await axios.post(prefix + "event/" + key + "/apply")).data,

  getItemDetail: async (hashId: string) => {
    const res = (await axios.get(prefix + "event/shop/item/" + hashId)).data;
    return {
      ...res,
      point: res?.point ? normalizeAmountList(res?.point) : res?.point,
    };
  },

  getItemList: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/item", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data, "point") : res.data,
    };
    return res;
  },

  purchaseItem: async (formData: any) =>
    (await axios.post(prefix + "event/shop/order", formData)).data,

  purchaseReward: async (formData: any) =>
    (await axios.post(prefix + "event/shop/reward", formData)).data,

  getOrderList: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/order", { params: criteria })
    ).data;
    return {
      ...res,
      data: res?.data
        ? normalizeAmountList(res?.data, "totalPoint")
        : res?.data,
    };
  },

  getOrderDetail: async (hashId: string) => {
    const res = (await axios.get(prefix + "event/shop/order/" + hashId)).data;
    //
    const data = {
      ...res,
      totalPoint: normalizeAmountList(res?.totalPoint),
    };
    return data;
  },

  getShopEvent: async () => (await axios.get(prefix + "event/ShopEvent")).data,

  getPointsHistory: async (criteria?: any) => {
    const res = await axios.get(prefix + "event/shop/point/transaction", {
      params: criteria,
    });
    return {
      ...res.data,
      data: res.data.data
        ? normalizeAmountList(res.data.data, "point")
        : res.data.data,
    };
    return res;
  },

  getImagesWithGuid: async (guid: string) => {
    const response = await axios.get(`${prefix}media/${guid}`, {
      responseType: "blob",
    });
    const imageBlob = new Blob([response.data], { type: "image/jpeg" });
    const imageBlobUrl = URL.createObjectURL(imageBlob);
    return imageBlobUrl;
  },
};
