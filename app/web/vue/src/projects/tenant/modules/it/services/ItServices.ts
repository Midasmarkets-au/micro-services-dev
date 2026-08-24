import { axiosInstance2 as axiosV2 } from "@/core/services/api.client";

const prefixV2 = "v2/tenant/it";
const prefix = "api/v1/tenant/";

export default {
  updateClientTranslation: async (formData: any) =>
    (await axiosV2.put(prefixV2 + `/translations/client`, formData)).data,

  updateWebTranslation: async (formData: any) =>
    (await axiosV2.put(prefixV2 + `/translations/web`, formData)).data,

  uploadPublicFiles: async (
    formData: any,
    type = "public"
  ): Promise<string[]> =>
    (
      await axiosV2.post(prefix + "upload/public", formData, {
        params: { type },
        // axiosV2's instance default forces Content-Type: application/json,
        // which stops axios/the browser from setting the multipart boundary
        // for this FormData body. Clearing it lets that happen automatically.
        headers: { "Content-Type": undefined },
      })
    ).data,
};
