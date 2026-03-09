import { axiosInstance as axios } from "@/core/services/api.client";
import { normalizeAmountList } from "@/lib/utils";
const prefix = "api/v1/tenant/";

export default {
  // Event List
  queryEventsList: async (criteria?: any) =>
    (await axios.get(prefix + "event", { params: criteria })).data,

  createEvent: async (formData: any) =>
    (await axios.post(prefix + "event", formData)).data,

  queryEventById: async (id: number) =>
    (await axios.get(prefix + "event/" + id)).data,

  updateEvent: async (id: number, formData: any) =>
    (await axios.put(prefix + "event/" + id, formData)).data,

  publishEvent: async (id: number) =>
    (await axios.put(prefix + "event/" + id + "/publish")).data,

  unpublishEvent: async (id: number) =>
    (await axios.put(prefix + "event/" + id + "/close")).data,

  updateEventLanguage: async (id: number, lang: string, formData: any) =>
    (await axios.put(prefix + "event/" + id + "/lang/" + lang, formData)).data,

  // uploadEventImage: async (formData: any) =>
  //   (await axios.post(prefix + "upload/public?type=public", formData)).data,

  uploadEventImage: async (type, file: any) =>
    (
      await axios.post("api/v1/tenant/upload", file, {
        params: { type },
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
    ).data,

  uploadImage: async (formData: any) =>
    (await axios.post(prefix + "upload/public?type=public", formData)).data,

  getImagesWithGuid: async (guid: string) => {
    const response = await axios.get(`${prefix}media/${guid}`, {
      responseType: "blob",
    });
    const imageBlob = new Blob([response.data], { type: "image/jpeg" });
    const imageBlobUrl = URL.createObjectURL(imageBlob);
    return imageBlobUrl;
  },

  getServices: async () => (await axios.get(prefix + "trade/service")).data,

  // Shop Event
  updateShopCategory: async (formData: any) =>
    (await axios.put(prefix + "event/shop/item/category", formData)).data,

  updateShopCategoryStatus: async (key: string) =>
    (await axios.put(prefix + "event/shop/item/category/" + key + "/status"))
      .data,

  queryShopCategoryList: async (criteria?: any) =>
    (await axios.get(prefix + "event/shop/item/category", { params: criteria }))
      .data,

  adjustPoints: async (formData: any) =>
    (await axios.post(prefix + "event/shop/point/adjust", formData)).data,

  getShopItemList: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/item", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data, "point") : res.data,
    };
  },

  queryShopItemById: async (id: number) => {
    const res = (await axios.get(prefix + "event/shop/item/" + id)).data;
    return {
      ...res,
      point: res?.point ? normalizeAmountList(res?.point, "point") : res?.point,
    };
  },

  getOrderList: async (criteria?: any) =>
    (await axios.get(prefix + "shop/order", { params: criteria })).data,

  approveCustomerById: async (partyId: number) =>
    (await axios.post(prefix + "event/user/" + partyId + "/approve")).data,

  rejectCustomerById: async (partyId: number) =>
    (await axios.post(prefix + "event/user/" + partyId + "/reject")).data,

  cancelCustomerById: async (partyId: number) =>
    (await axios.post(prefix + "event/user/" + partyId + "/cancel")).data,

  queryCustomerList: async (criteria?: any) => {
    const res = (await axios.get(prefix + "event/users", { params: criteria }))
      .data;
    return {
      ...res,
      data: res.data
        ? normalizeAmountList(res.data, ["point", "totalPoint"])
        : res.data,
    };
  },

  createShopItem: async (formData: any) =>
    (await axios.post(prefix + "event/shop/item", formData)).data,

  publishItem: async (id: number) =>
    (await axios.put(prefix + "event/shop/item/" + id + "/publish")).data,

  unpublishItem: async (id: number) =>
    (await axios.put(prefix + "event/shop/item/" + id + "/close")).data,

  updateItem: async (id: number, formData: any) =>
    (await axios.put(prefix + "event/shop/item/" + id, formData)).data,

  updateItemLanguage: async (id: number, lang: string, formData: any) =>
    (await axios.put(prefix + "event/shop/item/" + id + "/" + lang, formData))
      .data,

  // Rewards
  queryShoppRewards: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/reward", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data, "point") : res.data,
    };
  },

  queryShopRewardById: async (id: number) => {
    const res = (await axios.get(prefix + "event/shop/reward/" + id)).data;
    return {
      ...res,
      point: res?.point ? normalizeAmountList(res?.point) : res?.point,
    };
  },

  pendingReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/pending")).data,
  processReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/process")).data,
  approveReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/approve")).data,
  succeedReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/succeed")).data,
  cancelReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/cancel")).data,
  expireReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/expire")).data,
  activeReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/active")).data,
  inactiveReward: async (id: number) =>
    (await axios.put(prefix + "event/shop/reward/" + id + "/inactive")).data,
  // Shop Order
  addTrackingNumber: async (id: number, formData: any) =>
    (
      await axios.put(
        prefix + "event/shop/order/" + id + "/update-shipping",
        formData
      )
    ).data,
  queryShopOrderList: async (criteria?: any) => {
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

  queryOrderDetail: async (id: number) => {
    const res = (await axios.get(prefix + "event/shop/order/" + id)).data;
    const data = {
      ...res,
      totalPoint: normalizeAmountList(res?.totalPoint),
    };
    return data;
  },

  pendingOrder: async (id: number) =>
    (await axios.put(prefix + "event/shop/order/" + id + "/pending")).data,
  processOrder: async (id: number) =>
    (await axios.put(prefix + "event/shop/order/" + id + "/process")).data,
  shipOrder: async (id: number) =>
    (await axios.put(prefix + "event/shop/order/" + id + "/ship")).data,
  succeedOrder: async (id: number) =>
    (await axios.put(prefix + "event/shop/order/" + id + "/succeed")).data,
  cancelOrder: async (id: number) =>
    (await axios.put(prefix + "event/shop/order/" + id + "/cancel")).data,

  queryShopRewardRebate: async (criteria?: any) => {
    const res = (
      await axios.get(prefix + "event/shop/reward/rebate", { params: criteria })
    ).data;
    return {
      ...res,
      data: res.data ? normalizeAmountList(res.data, "point") : res.data,
    };
  },

  queryShopPointTransaction: async (criteria?: any) => {
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
};
