import { axiosInstance as axios } from "@/core/services/api.client";

const prefix = "api/v1/tenant/contact";

export interface SendContactEmailSpec {
  title: string;
  subtitle: string;
  content: string;
  language?: string;
}

export default {
  queryContacts: async (criteria?: any) =>
    (await axios.get(prefix, { params: criteria })).data,

  getContact: async (contactId: number) =>
    (await axios.get(`${prefix}/${contactId}`)).data,

  archiveContact: async (contactId: number) =>
    (await axios.put(`${prefix}/${contactId}/archive`)).data,

  unarchiveContact: async (contactId: number) =>
    (await axios.put(`${prefix}/${contactId}/unarchive`)).data,

  sendContactEmail: async (contactId: number, formData: SendContactEmailSpec) =>
    (await axios.post(`${prefix}/${contactId}/email`, formData)).data,
};
