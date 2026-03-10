import { createRouter, createWebHistory, RouteRecordRaw } from "vue-router";
import store from "@/store";
import { useStore } from "@/store";
import { Mutations, Actions } from "@/store/enums/StoreEnums";
import Can from "@/core/plugins/ICan";
import SystemLayout from "@/layouts/SystemLayout.vue";
import AuthLayout from "@/layouts/ClientAuthLayout.vue";
import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
import { computed } from "vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { setTimerForLogout } from "@/core/plugins/TimerService";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);
const baseRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    redirect: "/",
    component: ClientLayout,
    children: [
      {
        path: "/",
        name: "dashboard",
        component: () =>
          import(/*webpackChunkName:'client'*/ "../views/ClientDashboard.vue"),
        meta: {
          pageTitle: "title.dashboard",
          breadcrumbs: ["title.dashboard"],
          permissions: ["any"],
        },
      },

      // support routes
      {
        path: "/supports",
        name: "SupportsIndex",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/supports/views/SupportsIndex.vue"
          ),
        meta: {
          pageTitle: "title.supports",
          breadcrumbs: ["title.supports", "title.contactUs"],
          permissions: ["any"],
        },
      },
      {
        path: "/supports/notices",
        name: "SupportsNotices",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/supports/views/SupportsNotices.vue"
          ),
        meta: {
          pageTitle: "title.supportsNotices",
          breadcrumbs: ["title.supports", "title.supportsNotices"],
          permissions: ["any"],
        },
      },
      {
        path: "/supports/documents",
        name: "DocumentIndex",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/supports/views/DocumentsIndex.vue"
          ),
        meta: {
          pageTitle: "title.documents",
          breadcrumbs: ["title.supports", "title.documents"],
          permissions: ["any"],
        },
      },
      {
        path: "/supports/cases",
        name: "CaseIndex",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/supports/views/CaseIndex.vue"
          ),
        meta: {
          pageTitle: "title.cases",
          breadcrumbs: ["title.supports", "title.cases"],
          permissions: ["TenantAdmin"],
        },
      },
      // test upload routes
      {
        path: "/test-upload",
        name: "testUpload",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "../views/profile/components/testUpload/testUpload.vue"
          ),
        meta: {
          pageTitle: "title.fileUpload",
          breadcrumbs: ["title.fileUpload"],
          permissions: ["any"],
        },
      },
      // profile routes
      {
        path: "/profile",
        name: "setting",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "../views/profile/ProfileSetting.vue"
          ),
        meta: {
          pageTitle: "title.setting",
          breadcrumbs: ["title.setting"],
          permissions: ["any"],
        },
      },
      {
        path: "/profile/inbox",
        name: "ProfileInbox",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "../views/profile/ProfileInbox.vue"
          ),
        meta: {
          pageTitle: "title.inbox",
          breadcrumbs: ["title.inbox"],
          permissions: ["any"],
        },
      },
      {
        path: "/profile/bank-infos",
        name: "ProfileBankInfos",
        component: () =>
          import(/*webpackChunkName:'client'*/ "../views/profile/BankInfo.vue"),
        meta: {
          pageTitle: "title.bankInfo",
          breadcrumbs: ["title.bankInfo"],
          permissions: ["any"],
        },
      },
      {
        path: "/profile/file-upload",
        name: "ProfileFileUpload",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "../views/profile/ProfileFileUpload.vue"
          ),
        meta: {
          pageTitle: "title.fileUpload",
          breadcrumbs: ["title.fileUpload"],
          permissions: ["any"],
        },
      },

      {
        path: "/profile/address",
        name: "AddressSetting",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "../views/profile/AddressSetting.vue"
          ),
        meta: {
          pageTitle: "title.address",
          breadcrumbs: ["title.address"],
          permissions: ["any"],
        },
      },

      // tools routes
      {
        path: "/plateforms",
        name: "plateforms",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/tools/views/TradePlateforms.vue"
          ),
        meta: {
          pageTitle: "title.platforms",
          breadcrumbs: ["title.platforms"],
          permissions: ["any"],
        },
      },
      {
        path: "/webTrader/:accountNumber?/:serviceId?",
        name: "webtrader",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/tools/views/ToolsWebtrader.vue"
          ),
        meta: {
          pageTitle: "title.webTrader",
          breadcrumbs: ["title.webTrader"],
          permissions: ["any"],
        },
      },
      {
        path: "/webTrader5/:accountNumber?",
        name: "webtrader5",
        component: () =>
          import(
            /*webpackChunkName:'client'*/ "@/projects/client/modules/tools/views/ToolsWebtrader5.vue"
          ),
        meta: {
          pageTitle: "title.webTrader",
          breadcrumbs: ["title.webTrader"],
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
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/SignIn.vue"),
        meta: {
          requireAuth: false,
        },
      },
      {
        path: "/sign-up",
        name: "sign-up",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/SignUp.vue"),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          code: route.query.code as string,
        }),
      },
      {
        path: "/set-token",
        name: "set-token",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/SetToken.vue"),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          token: route.query.token as string,
        }),
      },
      {
        path: "/reset-password",
        name: "reset-password",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/ResetPassword.vue"),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          code: route.query.code as string,
        }),
      },
      {
        path: "/reset-password-v2",
        name: "reset-password-v2",
        component: () =>
          import(
            /*webpackChunkName:'auth'*/ "../views/auth/ResetPasswordV2.vue"
          ),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          code: route.query.code as string,
        }),
      },
      {
        path: "/change-account-password/:tenantId",
        name: "change-account-password",
        component: () =>
          import(
            /*webpackChunkName:'auth'*/ "@/projects/client/views/auth/ChangeTradeAccountPassword.vue"
          ),
        meta: {
          requireAuth: false,
        },
      },
      {
        path: "/demo-account",
        name: "demo-account",
        component: () =>
          import(
            /*webpackChunkName:'auth'*/ "../views/auth/CreateDemoAccount.vue"
          ),
        meta: {
          requireAuth: false,
        },
      },
      {
        path: "/confirm-email",
        name: "confirm-email",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/ConfirmEmail.vue"),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          code: route.query.code as string,
          email: route.query.email as string,
        }),
      },
      {
        path: "/lead-create",
        name: "lead-create",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/LeadCreate.vue"),
        meta: {
          requireAuth: false,
        },
        props: (route) => ({
          code: route.query.code as string,
        }),
      },
      {
        path: "/2fa",
        name: "2fa",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/TwoFA.vue"),
      },
      {
        path: "/sign-out",
        name: "SignOut",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/SignOut.vue"),
        meta: {
          pageTitle: "title.signOut",
          permissions: ["any"],
        },
      },
      {
        path: "/one-time-password",
        name: "one-time-password",
        component: () =>
          import(/*webpackChunkName:'auth'*/ "../views/auth/OneTimeCode.vue"),
        meta: {
          requireAuth: false,
        },
      },
      // {
      //   path: "/create-demo-account",
      //   name: "CreateDemoAccount",
      //   component: CreateDemoAccount,
      //   meta: {
      //     pageTitle: "",
      //     requireAuth: false,
      //     permissions: ["any"],
      //   },
      // },
    ],
  },
];
const systemRoutes: Array<RouteRecordRaw> = [
  {
    path: "/",
    component: SystemLayout,
    children: [
      {
        path: "/maintain",
        name: "maintain",
        component: () =>
          import(
            /*webpackChunkName:'system'*/ "@/views/errors/MaintainPage.vue"
          ),
        meta: {
          permissions: ["any"],
          requireAuth: false,
        },
      },
    ],
  },
];

const routes: Array<RouteRecordRaw> = baseRoutes.concat(
  authRoutes,
  systemRoutes
);

const router = createRouter({
  history: createWebHistory(process.env.VUE_APP_PATH),
  routes,
});

router.beforeEach(async (to, from, next) => {
  // only for maintain page when system is down
  if (to.name === "maintain") {
    next();
    return;
  }

  const siteStatus = process.env.VUE_APP_SERVICE_STATUS;
  const allowedIps = process.env.VUE_APP_SERVICE_IPS?.split(",") ?? [];
  // siteStatus = "up";
  // siteStatus = "down";

  if (to.name !== "maintain" && siteStatus === "down") {
    const ipInfo = await ClientGlobalService.getCurrentIpInfo();
    if (!allowedIps.includes(ipInfo.ip)) {
      next({ name: "maintain" });
      return;
    }
  }

  store.commit(Mutations.RESET_LAYOUT_CONFIG);

  const targetUrlPermissions = to.meta.permissions as string[];
  // const sitePermissions = to.meta.sitePermissions as string[];

  const requireAuth =
    to.meta.requireAuth !== undefined ? (to.meta.requireAuth as boolean) : true;
  if (!requireAuth) {
    next();
    return;
  }

  const isAuth = await store.dispatch(Actions.VERIFY_AUTH);
  if (isAuth) {
    setTimerForLogout();
  }
  if (!targetUrlPermissions) {
    next();
    return;
  }

  if (!isAuth) {
    next({ name: "sign-in" });
    return;
  }

  if (!Can.cans(targetUrlPermissions)) {
    next({ name: "403" });
    return;
  }
  // const tenancy = store.state?.AuthModule?.user?.tenancy;

  // if (sitePermissions && tenancy) {
  //   if (!sitePermissions.includes(tenancy)) {
  //     next({ name: "dashboard" });
  //     return;
  //   }
  // }
  const ibEnabled = projectConfig.value?.ibEnabled;

  if (targetUrlPermissions.includes("IB") && !ibEnabled) {
    next({ name: "404" });
    return;
  }

  next();
});

router.afterEach(() => {
  // gtag("config", "UA-ID", { page_path: to.fullPath });
});

export default router;
