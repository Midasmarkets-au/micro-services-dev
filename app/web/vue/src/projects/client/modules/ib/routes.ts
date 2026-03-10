import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/IBIndex.vue";
// import IBCustomers from "./views/IBCustomers.vue";
// import IBFunding from "./views/IBFunding.vue";
// import IBRebate from "./views/IBRebate.vue";
// import IBReferral from "./views/IBReferral.vue";
// import IBTrade from "./views/IBTrade.vue";
// import IBLink from "./views/IBLink.vue";
// import IBNewCustomers from "./views/IBNewCustomers.vue";
// import IBDeposit from "@/projects/client/modules/ib/views/IBDeposit.vue";
// import IbWithdrawal from "@/projects/client/modules/ib/views/IbWithdrawal.vue";

export default (router) => {
  router.addRoute({
    path: "/ib",
    redirect: "/ib/index",
    component: ClientLayout,
    name: "ib",
    children: [
      {
        path: "/ib/index",
        name: "IBDashboard",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBIndex.vue"),
        meta: {
          pageTitle: "title.ibCenter",
          breadcrumbs: ["title.ib"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/new-customers",
        name: "IBNewCustomer",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBNewCustomers.vue"),
        meta: {
          pageTitle: "title.newCustomers",
          breadcrumbs: ["title.ib", "title.ibCustomer"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/customers",
        name: "IBCustomer",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBCustomers.vue"),
        meta: {
          pageTitle: "title.ibCustomer",
          breadcrumbs: ["title.ib", "title.ibCustomer"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/customers/:accountId/:part?",
        name: "IBCustomerDetail",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBCustomers.vue"),
        meta: {
          pageTitle: "title.ibCustomer",
          breadcrumbs: ["title.ib", "title.ibCustomer"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/trade",
        name: "IBTrade",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBTrade.vue"),
        meta: {
          pageTitle: "title.trade",
          breadcrumbs: ["title.ib", "title.trade"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/deposit",
        name: "IBDeposit",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBDeposit.vue"),
        meta: {
          pageTitle: "title.deposit",
          breadcrumbs: ["title.ib", "title.deposit"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/withdrawal",
        name: "IBWithdrawal",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IbWithdrawal.vue"),
        meta: {
          pageTitle: "title.withdrawal",
          breadcrumbs: ["title.ib", "title.withdrawal"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/rebate",
        name: "IBRebate",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBRebate.vue"),
        meta: {
          pageTitle: "title.ibRebate",
          breadcrumbs: ["title.ib", "title.ibRebate"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/iblink",
        name: "IBLink",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBLink.vue"),
        meta: {
          pageTitle: "title.ibLink",
          breadcrumbs: ["title.ib", "title.ibLink"],
          permissions: ["IB", "Sales"],
        },
      },
      {
        path: "/ib/referral",
        name: "IBReferral",
        component: () =>
          import(/*webpackChunkName:'agent'*/ "./views/IBReferral.vue"),
        meta: {
          pageTitle: "title.ibReferral",
          breadcrumbs: ["title.ib", "title.ibReferral"],
          permissions: ["IB", "Sales"],
        },
      },
    ],
  });
};
