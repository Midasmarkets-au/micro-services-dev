import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";

const prefix = "api/v1/tenant/";
const prefixV2 = "v2/tenant/";
const zoomPrefix = "api.zoom.us/v2/";

export default {
  // Equity Below Credit
  getEquityBelowCredit: async (siteId: number) =>
    (await axios.get(prefix + "configuration/site/" + siteId + "/equity-below"))
      .data,
  sendEquityBelowCredit: async (siteId: number, formData: object) =>
    (
      await axios.post(
        prefix + "configuration/site/" + siteId + "/equity-below",
        formData
      )
    ).data,

  // Brief Detail
  getBriefDetail: async () =>
    (await axios.get(prefix + "trade" + "?accoutNumber=32810655")).data,

  // Cheater IP
  getCheaterIpList: async (criteria?: any) =>
    (await axios.get(prefix + "ip-black-list", { params: criteria })).data,

  addCheaterIp: async (formData: object) =>
    (await axios.post(prefix + "ip-black-list", formData)).data,

  deleteCheaterIp: async (id: number) =>
    (await axios.delete(prefix + "ip-black-list/" + id)).data,

  updateCheaterIp: async (id: number, formData: object) =>
    (await axios.put(prefix + "ip-black-list/" + id, formData)).data,
  // Blocked List
  getBlockedList: async (criteria?: any) =>
    (await axios.get(prefix + "user-black-list", { params: criteria })).data,

  addBlockedUser: async (formData: object) =>
    (await axios.post(prefix + "user-black-list", formData)).data,

  editBlockedUser: async (id: number, formData: object) =>
    (await axios.put(prefix + "user-black-list/" + id, formData)).data,

  removeBlockedUser: async (id: number) =>
    (await axios.delete(prefix + "user-black-list/" + id)).data,

  // Violation List
  getViolationList: async (siteId: number) =>
    (await axios.get(prefix + "configuration/site/" + siteId + "/violation"))
      .data,
  removeViolationUser: async (siteId: number, ip: string) =>
    (
      await axios.delete(
        prefix + "configuration/site/" + siteId + "/violation/" + ip
      )
    ).data,

  // Zoom Meeting
  getZoomMeetings: async (userId: number) =>
    (await axios.get(zoomPrefix + "users/" + userId + "meetings")).data,
};
