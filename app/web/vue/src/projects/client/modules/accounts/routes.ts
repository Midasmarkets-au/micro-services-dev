import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/AccountsIndex.vue";
// import AccountDetail from "./views/AccountDetail.vue";
// import CreateDemoAccount from "@/projects/client/modules/accounts/views/CreateDemoAccount.vue";
// import ApplyWholesale from "./views/ApplyWholesale.vue";

export default (router) => {
  router.addRoute({
    path: "/account",
    redirect: "/account",
    component: ClientLayout,
    name: "accounts",
    children: [
      {
        path: "/account",
        name: "AccountsIndex",
        component: () =>
          import(/*webpackChunkName:'client'*/ "./views/AccountsIndex.vue"),
        meta: {
          pageTitle: "title.accounts",
          breadcrumbs: ["title.accounts"],
          permissions: ["Client"],
        },
      },
      {
        path: "/account/:accountNumber/:part?",
        name: "AccountDetail",
        component: () =>
          import(/*webpackChunkName:'client'*/ "./views/AccountDetail.vue"),
        meta: {
          pageTitle: "title.accountDetails",
          breadcrumbs: ["title.accounts", "title.Details"],
          permissions: ["Client"],
        },
      },
      {
        path: "/account/:accountNumber/apply-wholesale",
        name: "ApplyWholesale",
        component: () =>
          import(/*webpackChunkName:'client'*/ "./views/ApplyWholesale.vue"),
        meta: {
          pageTitle: "title.applyWholesale",
          breadcrumbs: ["title.accounts", "title.applyWholesale"],
          permissions: ["Client"],
        },
      },
    ],
  });
};
