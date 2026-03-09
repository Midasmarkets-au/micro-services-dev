import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import SalesCustomers from "./views/SalesCustomers.vue";
// import SalesLink from "./views/SalesLink.vue";
// import SalesLead from "./views/SalesLead.vue";
// import SalesTrade from "./views/SalesTrade.vue";
// import SalesTransaction from "./views/SalesTransaction.vue";
// import SalesCustomerDetail from "@/projects/client/modules/sales/views/SalesCustomerDetail.vue";
// import SalesDeposit from "@/projects/client/modules/sales/views/SalesDeposit.vue";
// import SalesWithdrawal from "@/projects/client/modules/sales/views/SalesWithdrawal.vue";
// import SalesIndex from "./views/SalesIndex.vue";
// import SalesNewCustomers from "./views/SalesNewCustomers.vue";
export default (router) => {
  router.addRoute({
    path: "/sales",
    redirect: "/sales/customers",
    component: ClientLayout,
    name: "sales",
    children: [
      {
        path: "/sales/dashboard",
        name: "SalesIndex",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesIndex.vue"),
        meta: {
          pageTitle: "title.sales",
          breadcrumbs: ["title.sales"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/statistics",
        name: "SalesStatistics",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesStatistics.vue"),
        meta: {
          pageTitle: "sales.statisticsTable",
          breadcrumbs: ["title.sales", "sales.statisticsTable"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/customers",
        name: "SalesDashboard",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesCustomers.vue"),
        meta: {
          pageTitle: "title.sales",
          breadcrumbs: ["title.sales"],
          permissions: ["Sales"],
        },
      },

      {
        path: "/sales/customers/:accountId/:part?",
        name: "SalesCustomer",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesCustomers.vue"),
        meta: {
          pageTitle: "title.salesCustomers",
          breadcrumbs: ["title.sales", "title.salesCustomers"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/deposit",
        name: "SalesDeposit",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesDeposit.vue"),
        meta: {
          pageTitle: "title.depositHistory",
          breadcrumbs: ["title.sales", "title.depositHistory"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/withdrawal",
        name: "SalesWithdrawal",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesWithdrawal.vue"),
        meta: {
          pageTitle: "title.withdrawHistory",
          breadcrumbs: ["title.sales", "title.withdrawHistory"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/link",
        name: "SalesLink",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesLink.vue"),
        meta: {
          pageTitle: "title.salesLink",
          breadcrumbs: ["title.sales", "title.salesLink"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/lead",
        name: "SalesLead",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesLead.vue"),
        meta: {
          pageTitle: "title.salesLead",
          breadcrumbs: ["title.sales", "title.salesLead"],
          permissions: ["Sales"],
        },
      },

      {
        path: "/sales/transaction",
        name: "SalesTransaction",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesTransaction.vue"),
        meta: {
          pageTitle: "title.transferHistory",
          breadcrumbs: ["title.sales", "title.depositHistory"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/trade",
        name: "SalesTrade",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesTrade.vue"),
        meta: {
          pageTitle: "title.tradeHistory",
          breadcrumbs: ["title.sales", "title.tradeHistory"],
          permissions: ["Sales"],
        },
      },
      {
        path: "/sales/new-customers",
        name: "SalesIncompleteCustomers",
        component: () =>
          import(/*webpackChunkName:'sales'*/ "./views/SalesNewCustomers.vue"),
        meta: {
          pageTitle: "title.newCustomers",
          breadcrumbs: ["title.sales", "title.newCustomers"],
          permissions: ["Sales"],
        },
      },
    ],
  });
};
