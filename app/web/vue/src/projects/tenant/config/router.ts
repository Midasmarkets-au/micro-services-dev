import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";
import store from "@/store";
import { Mutations, Actions } from "@/store/enums/StoreEnums";
import Can from "@/core/plugins/ICan";

import SystemLayout from "@/layouts/SystemLayout.vue";
import AuthLayout from "@/layouts/AuthLayout.vue";
import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import TenantDashboard from "../views/TenantDashboard.vue";
import ProfileSetting from "../views/profile/ProfileSetting.vue";
import AccountOverview from "../views/accountOverview/AccountOverviewIndex.vue";
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
        component: TenantDashboard,
        meta: {
          pageTitle: "title.dashboard",
          breadcrumbs: ["title.dashboard"],
          permissions: ["any"],
        },
      },
      {
        path: "/account-overview",
        name: "account-overview",
        component: AccountOverview,
        meta: {
          pageTitle: "title.accountOverview",
          breadcrumbs: ["title.accountOverview"],
          permissions: ["any"],
        },
      },
      {
        path: "/profile",
        name: "profileSetting",
        component: ProfileSetting,
        meta: {
          pageTitle: "title.setting",
          breadcrumbs: ["title.setting"],
          permissions: ["any"],
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
        path: "/password-reset",
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
  // console.log(to);
  // console.log(router.getRoutes());
  store.commit(Mutations.RESET_LAYOUT_CONFIG);
  if (to.name === "sign-in") {
    await store.dispatch(Actions.LOGOUT);
    next();
    return;
  }

  await store.dispatch(Actions.VERIFY_AUTH);
  // console.log(to);
  if (
    !Can.cans([
      "TenantAdmin",
      "SuperAdmin",
      "DepositAdmin",
      "Compliance",
      "KycOfficer",
      "AccountAdmin",
      "Admin",
    ])
  ) {
    // if route is protected and user doesn't have required permissions
    await router.push({ name: "sign-in" });
    await store.dispatch(Actions.LOGOUT);
    // MsgPrompt.error("You are not authorized to access the backend!!");
    next();
    return;
  }

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
