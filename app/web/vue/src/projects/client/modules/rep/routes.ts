import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import RepCustomers from "./views/RepCustomers.vue";
// import RepLink from "./views/RepLinkOnly.vue";
// import RepLead from "./views/RepLead.vue";
// import RepTrade from "./views/RepTrade.vue";
// import RepTransaction from "./views/RepTransaction.vue";
// import RepDeposit from "@/projects/client/modules/rep/views/RepDeposit.vue";
// import RepWithdrawal from "@/projects/client/modules/rep/views/RepWithdrawal.vue";
export default (router) => {
  router.addRoute({
    path: "/rep",
    redirect: "/rep/customers",
    component: ClientLayout,
    name: "rep",
    children: [
      {
        path: "/rep/customers",
        name: "RepDashboard",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepCustomers.vue"),
        meta: {
          pageTitle: "title.rep",
          breadcrumbs: ["title.rep"],
          permissions: ["Rep"],
        },
      },

      {
        path: "/rep/customers/:accountId/:part?",
        name: "RepCustomer",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepCustomers.vue"),
        meta: {
          pageTitle: "title.customer",
          breadcrumbs: ["title.rep", "title.customer"],
          permissions: ["Rep"],
        },
      },
      {
        path: "/rep/deposit",
        name: "RepDeposit",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepDeposit.vue"),
        meta: {
          pageTitle: "title.depositHistory",
          breadcrumbs: ["title.rep", "title.depositHistory"],
          permissions: ["Rep"],
        },
      },
      {
        path: "/rep/withdrawal",
        name: "RepWithdrawal",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepWithdrawal.vue"),
        meta: {
          pageTitle: "title.withdrawHistory",
          breadcrumbs: ["title.rep", "title.withdrawHistory"],
          permissions: ["Rep"],
        },
      },
      {
        path: "/rep/lead",
        name: "RepLead",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepLead.vue"),
        meta: {
          pageTitle: "title.repLead",
          breadcrumbs: ["title.rep", "title.repLead"],
          permissions: ["Rep"],
        },
      },

      {
        path: "/rep/transaction",
        name: "RepTransaction",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepTransaction.vue"),
        meta: {
          pageTitle: "title.transferHistory",
          breadcrumbs: ["title.rep", "title.depositHistory"],
          permissions: ["Rep"],
        },
      },
      {
        path: "/rep/trade",
        name: "RepTrade",
        component: () =>
          import(/*webpackChunkName:'rep'*/ "./views/RepTrade.vue"),
        meta: {
          pageTitle: "title.tradeHistory",
          breadcrumbs: ["title.rep", "title.tradeHistory"],
          permissions: ["Rep"],
        },
      },
    ],
  });
};
