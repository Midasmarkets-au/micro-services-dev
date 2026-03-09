import { axiosInstance as axios } from "@/core/services/api.client";
import { TenantWithReferredAccountInfoResponseModel } from "@/core/models/Referral";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import { getImageUrl } from "@/core/plugins/ProcessImageLink";
import { normalizeAmountList } from "@/lib/utils";

const prefix = "api/v1/tenant/";
export type CreateReportSpec = {
  type: ReportRequestTypes;
  name: string;
  query: object;
};
export default {
  getTenantTokens: async () => (await axios.get(prefix + "admin/tokens")).data,

  uploadFile: async (file: any, type?: string) =>
    (
      await axios.post(prefix + "upload", file, {
        params: { type },
      })
    ).data,

  uploadFileToUser: async (file: any, type?: string, partyId?: number) =>
    (
      await axios.post(prefix + "upload/party/" + partyId, file, {
        params: { type },
      })
    ).data,

  deleteMediaFileByGuid: async (guid: string) =>
    (await axios.delete(prefix + "media/" + guid)).data,

  getMediaFileList: async (criteria?: any) =>
    (await axios.get(prefix + "media/list", { params: criteria })).data,

  queryEventTopics: async (criteria?: any) =>
    (await axios.get(prefix + "topic", { params: criteria })).data,

  getContentFromUrl: async (url: string) => (await axios.get(url)).data,

  getReferralCodeSupplement: async (code: string) =>
    (await axios.get("api/v1/referralcode/" + code)).data,

  getReferralCodeAccountInfo: async (
    code: string
  ): Promise<TenantWithReferredAccountInfoResponseModel> => {
    const res = (
      await axios.get(prefix + "referral/referred-by-account/" + code)
    ).data;
    return res;
  },

  getReferralInfoByReferralCode: async (referralCode: string) =>
    await axios.get(`/api/v1/referralcode/${referralCode}`),

  updateEmailLanguage: async (topicId: number, id: number, data: any) =>
    (await axios.put(prefix + "topic/" + topicId + "/content/" + id, data))
      .data,
  createEmailLanguage: async (topicId: number, data: any) =>
    (await axios.post(prefix + "topic/" + topicId + "/content", data)).data,

  createEmailTemplate: async (data: any) =>
    (await axios.post(prefix + "topic", data)).data,

  sendEmailTemplate: async (data: any) =>
    (await axios.post(prefix + "email/debug", data)).data,

  queryComments: async (criteria?: any) =>
    (await axios.get(prefix + "comment", { params: criteria })).data,

  createComment: async (data: any) =>
    (await axios.post(prefix + "comment", data)).data,

  deleteComment: async (commentId: number, criteria?: any) =>
    (await axios.delete(prefix + "comment/" + commentId, { params: criteria }))
      .data,

  queryWallets: async (criteria?: any) => {
    const res = (await axios.get(prefix + "wallet", { params: criteria })).data;
    res.data = normalizeAmountList(res.data, "balance");
    return res;
  },

  updateUserProfileInfo: async (partyId: number, formData: any) =>
    (await axios.put(prefix + `user/${partyId}/profile`, formData)).data,

  queryReportRequest: async (criteria?: any) =>
    (await axios.get(prefix + "report/request", { params: criteria })).data,

  createReportRequest: async (formData?: CreateReportSpec) =>
    (await axios.post(prefix + "report/request", formData)).data,

  createReportRequestDownload: async (formData?: CreateReportSpec) =>
    (await axios.post(prefix + "report/request/download", formData)).data,

  downloadFileByGuid: async function (guid: string, filename?: string) {
    const url = getImageUrl(guid);
    return await this.downloadFileByLink(url, filename);
  },

  downloadFileByLink: async function (url: string, filename?: string) {
    const response = await axios.get(url, { responseType: "blob" });
    const fileData = response.data;
    const blobUrl = URL.createObjectURL(fileData);

    // Create a temporary download link
    const link = document.createElement("a");
    link.href = blobUrl;
    link.setAttribute("download", filename ?? "default-name");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    return blobUrl;
  },

  getImageUrlWithToken: async (url: string) => {
    const response = await axios.get(url, { responseType: "blob" });
    const imageData = response.data;
    return URL.createObjectURL(imageData);
  },

  getAuthenticationUri: async () =>
    (await axios.get("/api/v1/2fa/authenticator/setup")).data,

  enable2Fa: async () => (await axios.put("/api/v1/2fa/enable")).data,

  disable2Fa: async (verificationCode: string) =>
    (await axios.put("/api/v1/2fa/disable", { verificationCode })).data,

  verify2FaVerificationCode: async (verificationCode: string) =>
    (await axios.post("/api/v1/2fa/authenticator/verify", { verificationCode }))
      .data,

  updateMeiliSearchIndex: async () => {
    await axios.post("/api/v1/tenant/search/account/rebuild", {});
    await axios.post("/api/v1/tenant/search/user/rebuild", {});
  },

  uploadImage: async (type, file: any) =>
    (
      await axios.post("api/v1/tenant/upload", file, {
        params: { type },
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
    ).data,

  queryApiLogs: async (criteria?: any) =>
    (await axios.get(prefix + "api-log", { params: criteria })).data,
  updateTagsById: async (formData: any) =>
    (await axios.put(prefix + "tag", formData)).data,
  getTagsById: async (criteria?: any) =>
    (await axios.get(prefix + "tag", { params: criteria })).data,
};

export const generateAutoCompleteHandler: (
  fetchListHandler: () => Promise<Array<{ value: any; label: string }>>
) => (queryString: string, cb: (arg: any) => void) => void = (
  fetchListHandler
) => {
  let fullSearchableList: Array<{ value: any; label: string }> | null = null;
  let prevShowList: Array<{ value: any; label: string }> | null = [];
  let prevQueryStr: string | null = "";

  const getAgentGroupFullList = async () => {
    if (fullSearchableList) return fullSearchableList;
    fullSearchableList = await fetchListHandler();
    return fullSearchableList;
  };

  const createFilter =
    (queryString: string) => (item: { value: any; label: string }) =>
      item.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0;

  return async (queryString: string, cb: (arg: any) => void) => {
    const fullList = await getAgentGroupFullList();
    if (queryString === "" || queryString === null || queryString === "null") {
      cb(fullList);
      return;
    }

    if (prevQueryStr === queryString) {
      cb(prevShowList);
      return;
    }

    prevShowList = fullList.filter(createFilter(queryString));
    prevQueryStr = queryString;
    cb(prevShowList);
  };
};
