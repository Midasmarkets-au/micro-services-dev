import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import Index from "./views/AccountsIndex.vue";
import Clients from "./views/AccountClients.vue";
import Requests from "./views/AccountApplications.vue";
import AccountActivities from "@/projects/tenant/modules/accounts/views/AccountActivities.vue";
import AccountLog from "./views/AccountLog.vue";
import ReferralIndex from "./views/ReferralIndex.vue";
import DemoAccounts from "./views/DemoAccounts.vue";
import AutoCreateAccounts from "./views/AutoCreateAccounts.vue";
import AccountPrefixes from "./views/AccountPrefixes.vue";
export default (router) => {
  router.addRoute({
    path: "/account",
    redirect: "/account/index",
    component: BackendLayout,
    name: "Accounts",
    children: [
      {
        path: "/account/index",
        name: "AccountsIndex",
        component: Index,
        meta: {
          pageTitle: "title.accounts",
          breadcrumbs: ["title.accounts"],
          permissions: ["TenantAdmin", "Compliance", "WebClient"],
        },
      },
      {
        path: "/account/clients/:type",
        name: "accountClients",
        component: Clients,
        meta: {
          pageTitle: "title.clients",
          breadcrumbs: ["title.accounts", "title.clients"],
          permissions: ["TenantAdmin", "Compliance", "WebClient"],
        },
      },
      {
        path: "/account/prefixes",
        name: "AccountPrefixes",
        component: AccountPrefixes,
        meta: {
          pageTitle: "title.accountPrefixes",
          breadcrumbs: ["title.accounts", "title.accountPrefixes"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/account/applications",
        name: "AccountApplications",
        component: Requests,
        meta: {
          pageTitle: "title.accountApplications",
          breadcrumbs: ["title.accounts", "title.accountApplications"],
          permissions: [
            "TenantAdmin",
            "Compliance",
            "AccountAdmin",
            "WebApplication",
          ],
        },
      },
      {
        path: "/account/activity/:type",
        name: "AccountActivities",
        component: AccountActivities,
        meta: {
          pageTitle: "title.accountActivities",
          breadcrumbs: ["title.accounts", "title.accountActivities"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
      {
        path: "/account/log",
        name: "AccountLog",
        component: AccountLog,
        meta: {
          pageTitle: "title.log",
          breadcrumbs: ["title.accounts", "title.log"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
      {
        path: "/account/referral-code",
        name: "Referral",
        component: ReferralIndex,
        meta: {
          pageTitle: "fields.referralCode",
          breadcrumbs: ["title.accounts", "fields.referralCode"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
      {
        path: "/account/referral-code",
        name: "Referral",
        component: ReferralIndex,
        meta: {
          pageTitle: "Referral Code",
          breadcrumbs: ["title.accounts", "Referral Code"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
      {
        path: "/account/demo-accounts",
        name: "DemoAccounts",
        component: DemoAccounts,
        meta: {
          pageTitle: "title.demoAccounts",
          breadcrumbs: ["title.accounts", "title.demoAccounts"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
      {
        path: "/account/auto-create-accounts",
        name: "AutoCreateAccounts",
        component: AutoCreateAccounts,
        meta: {
          pageTitle: "title.autoCreateAccounts",
          breadcrumbs: ["title.accounts", "title.autoCreateAccounts"],
          permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        },
      },
    ],
  });
  router.removeRoute("catchAll");
  router.addRoute({
    name: "catchAll",
    path: "/:pathMatch(.*)*",
    redirect: "/404",
  });
};
