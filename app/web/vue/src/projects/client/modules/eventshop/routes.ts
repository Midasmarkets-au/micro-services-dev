import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/EventshopIndex.vue";
// import ShopRules from "./components/view/ShopRules.vue";
export default (router) => {
  router.addRoute({
    path: "/eventshop",
    redirect: "/eventshop",
    component: ClientLayout,
    name: "eventshop",
    children: [
      {
        path: "/eventshop",
        name: "Eventshop",
        component: () =>
          import(/*webpackChunkName:'shop'*/ "./views/EventshopIndex.vue"),
        meta: {
          // pageTitle: "title.eventShop",
          breadcrumbs: ["title.eventShop"],
          permissions: [
            "Client",
            "EventShop",
            "IB",
            "Sales",
            "TenantAdmin",
            "EventShopTest",
          ],
          // permissions: ["EventShopTest", "EventShop"],
          tenantPermissions: ["bvi"],
        },
      },
      {
        path: "/eventshop/rules",
        name: "ShopRules",
        component: () =>
          import(/*webpackChunkName:'shop'*/ "./components/view/ShopRules.vue"),
        meta: {
          // pageTitle: "title.eventShop",
          breadcrumbs: ["title.eventShop"],
          permissions: [
            "Client",
            "EventShop",
            "IB",
            "Sales",
            "TenantAdmin",
            "EventShopTest",
          ],
          // permissions: ["EventShopTest", "EventShop"],
          tenantPermissions: ["bvi"],
        },
      },
    ],
  });
};
