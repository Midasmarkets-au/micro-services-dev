import { axiosInstance2 as axiosV2 } from "@/core/services/api.client";

const prefixV2 = "v2/tenant/it";

export default {
  updateClientTranslation: async (formData: any) =>
    (await axiosV2.put(prefixV2 + `/translations/client`, formData)).data,

  updateWebTranslation: async (formData: any) =>
    (await axiosV2.put(prefixV2 + `/translations/web`, formData)).data,

  uploadPublicFiles: async (formData: any) =>
    (await axiosV2.post(prefixV2 + "/upload-public", formData)).data,
};
