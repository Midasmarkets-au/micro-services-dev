import {
  axiosInstance as axios,
  axiosInstance2 as axiosV2,
} from "@/core/services/api.client";

const prefix = "api/v1/tenant/";
const prefixV2 = "api/v2/tenant/";
const phpPrefix = "v2/tenant/";

export default {
  checkTradeSync: async (criteria?: any) =>
    (
      await axios.get(prefix + "trade-rebate/check-trade-sync", {
        params: criteria,
      })
    ).data,

  updatePromotionListById: async (id: number, data: any) =>
    (await axiosV2.put(phpPrefix + "promotion_list/" + id, data)).data,

  queryPromotionListById: async (id: number) =>
    (await axiosV2.get(phpPrefix + "promotion_list/" + id)).data,

  createPromotionList: async (promotionId: number, data: any) =>
    (await axiosV2.post(phpPrefix + "promotion_list/" + promotionId, data))
      .data,

  updatePromotionStatus: async (promotionId: number) =>
    (await axiosV2.put(phpPrefix + "promotions/status/" + promotionId)).data,

  uploadPromotionFile: async (data: any) =>
    (await axiosV2.post(phpPrefix + "promotions/upload", data)).data,

  queryPromotions: async (criteria?: any) =>
    (await axiosV2.get(phpPrefix + "promotions", { params: criteria })).data,

  editPromotionKey: async (id: number, data: any) =>
    (await axiosV2.put(phpPrefix + "promotions/" + id, data)).data,

  createPromotionKey: async (data: any) =>
    (await axiosV2.post(phpPrefix + "promotions", data)).data,

  queryPromotionList: async (id?: number) =>
    (await axiosV2.get(phpPrefix + "promotions/" + id)).data,

  queryOnlineAdmin: async (criteria?: any) => {
    const res = (
      await axios.get(prefixV2 + "statistic/online-admins", { params: criteria })
    ).data;
    // Proto returns { count, users: [{ partyId, email, since, tenantId }] }
    // Component expects array of [{ tenantId, users: [...] }] grouped by tenant
    const grouped: Record<number, any[]> = {};
    for (const user of res.users ?? []) {
      const tid = user.tenantId ?? 0;
      if (!grouped[tid]) grouped[tid] = [];
      grouped[tid].push(user);
    }
    return Object.entries(grouped).map(([tenantId, users]) => ({
      tenantId: Number(tenantId),
      users,
    }));
  },

  queryOnlineUsers: async (criteria?: any) =>
    (await axios.get(prefixV2 + "statistic/online-users", { params: criteria }))
      .data,

  getExcludeEquityBelowCredit: async () =>
    (
      await axios.get(
        prefix + "configuration/Public/0/ExcludedFromEquityBelowCredit"
      )
    ).data,

  putExcludeEquityBelowCredit: async (data: any) =>
    (
      await axios.put(
        prefix + "configuration/Public/0/ExcludedFromEquityBelowCredit",
        data
      )
    ).data,

  queryConfigs: async (criteria?: any) =>
    (await axios.get(prefix + "configuration", { params: criteria })).data,

  updateConfiguration: async (siteId: number, key: string, data: any) =>
    (await axios.put(prefix + "configuration/site/" + siteId + "/" + key, data))
      .data,

  reloadConfiguration: async () =>
    (await axios.put(prefix + "configuration/reload")).data,

  getConfigurationById: async (id: number) =>
    (await axios.get(prefix + "configuration/site/" + id + "/all")).data,

  getAllConfigurations: async () =>
    (await axios.get(prefix + "configuration/all")).data
      .sort((x, y) => x.name.localeCompare(y.name))
      .map((x: any) => ({
        ...x,
        key: x.name.replace(/([a-z])([A-Z])/g, "$1-$2").toLowerCase(),
      })),

  // Permission APIs
  getPermissions: async (criteria?: any) =>
    (await axios.get(prefix + "admin/permissions", { params: criteria })).data,

  createPermission: async (formData: object) =>
    (await axios.post(prefix + "admin", formData)).data,

  getPermissionUsers: async () =>
    (await axios.get(prefix + "admin/users")).data,

  getPermissionUsersById: async (id: number) =>
    (await axios.get(prefix + "admin/users/" + id)).data,

  getPermissionUsersByIdV2: async (id: number) =>
    (await axiosV2.get(prefixV2 + "permissions/user/" + id)).data,

  getPermissionRoles: async () =>
    (await axios.get(prefix + "admin/roles")).data,

  getPermissionRolesById: async (id: number) =>
    (await axios.get(prefix + "admin/roles/" + id)).data,

  updateRolePermission: async (id: number, permissionId) =>
    (
      await axios.put(
        prefix + "admin/roles/" + id + "/permission/" + permissionId + "/toggle"
      )
    ).data,
  updateUserPermission: async (id: number, permissionId) =>
    (
      await axios.put(
        prefix + "admin/users/" + id + "/permission/" + permissionId + "/toggle"
      )
    ).data,

  updateUserRole: async (id: number, roleId) =>
    (
      await axios.put(
        prefix + "admin/users/" + id + "/role/" + roleId + "/toggle"
      )
    ).data,

  // Config API
  queryConfig: async (rowId: number, category: string) =>
    (await axios.get(prefix + "configuration/" + category + "/" + rowId)).data,

  queryConfigByKey: async (
    category: string,
    rowId: number,
    key: string,
    criteria?: any
  ) =>
    (
      await axios.get(
        prefix + "configuration/" + category + "/" + rowId + "/" + key,
        {
          params: criteria,
        }
      )
    ).data,
  updateConfigByKey: async (
    category: string,
    rowId: number,
    key: string,
    data: any
  ) =>
    (
      await axios.put(
        prefix + "configuration/" + category + "/" + rowId + "/" + key,
        data
      )
    ).data,

  deleteConfigByKey: async (category: string, rowId: number, key: string) =>
    (
      await axios.delete(
        prefix + "configuration/" + category + "/" + rowId + "/" + key
      )
    ).data,

  // Platform API

  getServices: async () => (await axios.get(prefix + "trade/service")).data,

  // GET: {{baseUrl}}/api/v1/tenant/email/suppression-check?email=<string>
  checkEmailSuppression: async (email: string) =>
    (await axios.get(prefix + "email/suppression-check?email=" + email)).data,
  // PUT: {{baseUrl}}/api/v1/tenant/email/suppression-remove?email=<string>
  removeEmailSuppression: async (email: string) =>
    (await axios.put(prefix + "email/suppression-remove?email=" + email)).data,
};
