import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/WalletIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/wallet",
    redirect: "/wallet",
    component: ClientLayout,
    name: "wallet",
    children: [
      {
        path: "/wallet",
        name: "WalletIndex",
        component: () =>
          import(/*webpackChunkName:'wallet'*/ "./views/WalletIndex.vue"),
        meta: {
          pageTitle: "title.wallet",
          breadcrumbs: ["title.wallet"],
          permissions: ["Client"],
        },
      },
      {
        path: "/wallet/:currencyId?/:fundType?",
        name: "WalletIndex",
        component: () =>
          import(/*webpackChunkName:'wallet'*/ "./views/WalletIndex.vue"),
        meta: {
          pageTitle: "title.wallet",
          breadcrumbs: ["title.wallet"],
          permissions: ["Client"],
        },
      },
    ],
  });
};
