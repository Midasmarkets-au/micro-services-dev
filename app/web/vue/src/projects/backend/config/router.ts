import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";
import store from "@/store";
import { Mutations, Actions } from "@/store/enums/StoreEnums";
import Can from "@/core/plugins/ICan";

import SystemLayout from "@/layouts/SystemLayout.vue";
import AuthLayout from "@/layouts/AuthLayout.vue";
import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import BackendDashboard from "../views/BackendDashboard.vue";
import ProfileSetting from "../views/profile/ProfileSetting.vue";
import ITIcons from "../views/it/Icons.vue";
import SystemUsers from "../views/system/user/UserIndex.vue";
import SystemRoles from "../views/system/role/RoleIndex.vue";
import SystemPermissions from "../views/system/permission/PermissionIndex.vue";
import SystemSetting from "../views/system/setting.vue";

import AuthSignIn from "@/views/auth/SignIn.vue";
import AuthPasswordReset from "@/views/auth/PasswordReset.vue";
import Auth2FA from "@/views/auth/TwoFA.vue";
import AuthEmailVerification from "@/views/auth/EmailVerification.vue";

import Error403 from "@/views/errors/Error403.vue";
import Error404 from "@/views/errors/Error404.vue";
import Error500 from "@/views/errors/Error500.vue";

const baseRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    redirect: "/dashboard",
    component: BackendLayout,
    children: [
      {
        path: "/dashboard",
        name: "dashboard",
        component: BackendDashboard,
        meta: {
          pageTitle: "title.dashboard",
          breadcrumbs: ["title.dashboard"],
          permissions: ["any"],
        },
      },
      {
        path: "/profile",
        name: "setting",
        component: ProfileSetting,
        meta: {
          pageTitle: "title.setting",
          breadcrumbs: ["title.setting"],
          permissions: ["any"],
        },
      },
      {
        path: "/it/icons",
        name: "icons",
        component: ITIcons,
        meta: {
          pageTitle: "title.icons",
          breadcrumbs: ["title.it", "title.icons"],
          permissions: ["SuperAdmin"],
        },
      },
      {
        path: "/system/users",
        name: "system.users",
        component: SystemUsers,
        meta: {
          pageTitle: "title.user",
          breadcrumbs: ["title.setting", "title.user"],
          permissions: ["user.view"],
        },
      },
      {
        path: "/system/permissions",
        name: "system-permissions",
        component: SystemPermissions,
        meta: {
          pageTitle: "title.permission",
          breadcrumbs: ["title.setting", "title.permission"],
          permissions: ["permission.view"],
        },
      },
      {
        path: "/system/settings",
        name: "system-settings",
        component: SystemSetting,
        meta: {
          pageTitle: "title.setting",
          breadcrumbs: ["title.setting", "title.setting"],
          permissions: ["setting.view"],
        },
      },
      {
        path: "/system/roles",
        name: "system-roles",
        component: SystemRoles,
        meta: {
          pageTitle: "title.Role",
          breadcrumbs: ["title.setting", "title.Role"],
          permissions: ["role.view"],
        },
      },
    ],
  },
];

const authRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    component: AuthLayout,
    children: [
      {
        path: "/sign-in",
        name: "sign-in",
        component: AuthSignIn,
      },
      {
        path: "/reset-password",
        name: "password-reset",
        component: AuthPasswordReset,
      },
      {
        path: "/2fa",
        name: "2fa",
        component: Auth2FA,
      },
    ],
  },
];
const systemRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    component: SystemLayout,
    children: [
      {
        path: "/email-verification",
        name: "email-verification",
        component: AuthEmailVerification,
      },
    ],
  },
];

const errorRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    component: BackendLayout,
    children: [
      {
        // the 404 route, when none of the above matches
        path: "/404",
        name: "404",
        component: Error404,
      },
      {
        // the 403 route, when none of the above matches
        path: "/403",
        name: "403",
        component: Error403,
      },
      {
        path: "/500",
        name: "500",
        component: Error500,
      },
    ],
  },
  {
    name: "catchAll",
    path: "/:pathMatch(.*)*",
    redirect: "/404",
  },
];
const routes: Array<RouteRecordRaw> = baseRoutes.concat(
  authRoutes,
  systemRoutes,
  errorRoutes
);

const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to, from, next) => {
  // reset config to initial state
  store.commit(Mutations.RESET_LAYOUT_CONFIG);
  await store.dispatch(Actions.VERIFY_AUTH);
  if (to.meta == undefined || to.meta.permissions == undefined) {
    next();
  } else {
    if (Can.cans(to.meta.permissions as unknown as string[])) {
      next();
    } else {
      next({ name: "403" });
    }
  }

  // Scroll page to top on every route change
  setTimeout(() => {
    window.scrollTo(0, 0);
  }, 100);
});

export default router;
