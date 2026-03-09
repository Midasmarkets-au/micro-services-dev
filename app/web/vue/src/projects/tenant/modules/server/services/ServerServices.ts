import { axiosInstance as axios } from "@/core/services/api.client";

const prefix = "api/v1/tenant/";
const prefixV2 = "api/v2/tenant/";
const adminPrefix = "api/v1/admin/tenant/";

export default {
  getServersData: async (criteria?: any) =>
    (await axios.get(prefixV2 + "statistic/server", { params: criteria })).data,

  getServerMetrics: async (criteria?: any) =>
    (
      await axios.get(prefixV2 + "statistic/server/metrics", {
        params: criteria,
      })
    ).data,

  // Redis 缓存管理
  getRedisKeys: async () => (await axios.get(adminPrefix + "redis/keys")).data,

  deleteRedisKeys: async (keys: string[]) =>
    await axios.delete(adminPrefix + "redis/keys", { data: keys }),
};
