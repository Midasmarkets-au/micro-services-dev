import { axiosInstance as axios } from "@/core/services/api.client";
const prefix = "api/v1/client/";

export default {
  getTopicCalendars: async (criteria?: any) =>
    (await axios.get(prefix + "topic/calendar", { params: criteria })).data,

  getTopicCalendarById: async (id: string) =>
    (await axios.get(prefix + "topic/calendar/" + id)).data,

  getTopicNews: async (criteria?: any) =>
    (await axios.get(prefix + "topic/news", { params: criteria })).data,

  getTopicNewsById: async (id: string) =>
    (await axios.get(prefix + "topic/news/" + id)).data,

  getTopicNotices: async (criteria?: any) =>
    (await axios.get(prefix + "topic/notice", { params: criteria })).data,

  getTopicNoticesById: async (id: number) =>
    (await axios.get(prefix + "topic/notice/" + id)).data,

  postUserFeedback: async (formData: any) =>
    (await axios.post("api/v1/contact", formData)).data,

  uploadCaseFile: async (type, file: any) =>
    (
      await axios.post("api/v1/client/upload", file, {
        params: { type },
        headers: {
          "Content-Type": "multipart/form-data",
        },
      })
    ).data,

  createCase: async (formData: any) =>
    (await axios.post(prefix + "case", formData)).data,

  queryCasesList: async (criteria?: any) =>
    (await axios.get(prefix + "case", { params: criteria })).data,

  getCaseCategory: async (parentCategoryId?: number) => {
    const url = parentCategoryId
      ? `${prefix}case/category?parentCategoryId=${parentCategoryId}`
      : `${prefix}case/category`;

    return (await axios.get(url)).data;
  },

  getCaseById: async (id: number) =>
    (await axios.get(prefix + "case/" + id)).data,

  replyCaseById: async (id: number, formData: any) =>
    (await axios.post(prefix + "case/" + id + "/reply", formData)).data,

  getImagesWithGuid: async (guid: string) => {
    const response = await axios.get(`${prefix}media/${guid}`, {
      responseType: "blob",
    });
    const imageBlob = new Blob([response.data], { type: "image/jpeg" }); // Adjust the MIME type as necessary
    const imageBlobUrl = URL.createObjectURL(imageBlob);
    return imageBlobUrl;
  },
};
