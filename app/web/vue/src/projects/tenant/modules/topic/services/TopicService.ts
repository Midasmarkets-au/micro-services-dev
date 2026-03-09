import { axiosInstance as axios } from "@/core/services/api.client";

const prefix = "api/v1/tenant/";

export default {
  updateNoticeTime: async (topicId: number, data: any) =>
    (await axios.put(prefix + "topic/" + topicId, data)).data,

  updateNoticeLanguage: async (topicId: number, id: number, data: any) =>
    (await axios.put(prefix + "topic/" + topicId + "/content/" + id, data))
      .data,

  createNoticeLanguage: async (topicId: number, data: any) =>
    (await axios.post(prefix + "topic/" + topicId + "/content", data)).data,

  createNotice: async (data: any) =>
    (await axios.post(prefix + "topic", data)).data,

  deleteNotice: async (topicId: number) =>
    (await axios.put(prefix + "topic/" + topicId + "/move-to-trash")).data,

  getEmailBatchList: async () =>
    (await axios.get(prefix + "email/batch/info")).data,

  createEmailBatch: async (formData: any) =>
    (await axios.post(prefix + "email/batch", formData)).data,

  sendtestEmailBatch: async (formData: any) =>
    (await axios.post(prefix + "email/batch/test", formData)).data,

  sendEmailBatch: async (formData: any) =>
    (await axios.post(prefix + "email/batch/confirm", formData)).data,

  getEmailBatchDetail: async () =>
    (await axios.get(prefix + "email/batch/detail")).data,
};
