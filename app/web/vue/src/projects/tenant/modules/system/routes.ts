import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import SystemUsers from "./views/user/UserIndex.vue";
import SystemRoles from "./views/role/RoleIndex.vue";
import SystemPermissions from "./views/PermissionIndex.vue";
import SystemSetting from "./views/SystemSetting.vue";
import ConfigSettingVue from "./views/ConfigSetting.vue";
import PlatformSettingVue from "./views/PlatformSetting.vue";
import ApiLog from "@/projects/tenant/modules/system/views/ApiLog.vue";
export default (router) => {
  router.addRoute({
    path: "/system",
    redirect: "/system/users",
    component: BackendLayout,
    name: "System",
    children: [
      {
        path: "/system/users",
        name: "system.users",
        component: SystemUsers,
        meta: {
          pageTitle: "title.user",
          breadcrumbs: ["title.setting", "title.user"],
          permissions: ["superAdmin", "WebAdminUser"],
        },
      },
      {
        path: "/system/permissions",
        name: "system-permissions",
        component: SystemPermissions,
        meta: {
          pageTitle: "title.permission",
          breadcrumbs: ["title.setting", "title.permission"],
          permissions: ["superAdmin", "WebAdminPermission"],
        },
      },
      {
        path: "/system/apilog",
        name: "system-apilog",
        component: ApiLog,
        meta: {
          pageTitle: "title.apiLog",
          breadcrumbs: ["title.setting", "title.apiLog"],
          permissions: ["superAdmin", "TenantAdmin"],
        },
      },
      {
        path: "/system/config",
        name: "system-config",
        component: ConfigSettingVue,
        meta: {
          pageTitle: "title.config",
          breadcrumbs: ["title.setting", "title.config"],
          permissions: ["superAdmin", "WebAdminConfig"],
        },
      },
      {
        path: "/system/platform",
        name: "system-platform",
        component: PlatformSettingVue,
        meta: {
          pageTitle: "fields.platform",
          breadcrumbs: ["title.setting", "fields.platform"],
          permissions: ["superAdmin", "WebAdminPlatform"],
        },
      },
      // {
      //   path: "/system/settings",
      //   name: "system-settings",
      //   component: SystemSetting,
      //   meta: {
      //     pageTitle: "title.setting",
      //     breadcrumbs: ["title.setting", "title.setting"],
      //     permissions: ["superAdmin", "WebAdminSetting"],
      //   },
      // },
    ],
  });
  router.removeRoute("catchAll");
  router.addRoute({
    name: "catchAll",
    path: "/:pathMatch(.*)*",
    redirect: "/404",
  });
};
