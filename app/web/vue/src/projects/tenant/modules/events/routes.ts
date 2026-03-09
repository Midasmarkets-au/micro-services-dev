import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import EventIndex from "./views/EventIndex.vue";
import ShopIndex from "./views/ShopIndex.vue";
import ShopCustomer from "./views/ShopCustomer.vue";
import ShopInventory from "./views/ShopInventory.vue";
import ShopOrders from "./views/ShopOrders.vue";
import ShopRewards from "./views/ShopRewards.vue";
import ShopRewardRebate from "./views/ShopRewardRebate.vue";
import ShopPointTransaction from "./views/PointTransaction.vue";
import ShopCategory from "./views/ShopCategory.vue";

export default (router) => {
  router.addRoute({
    path: "/events",
    redirect: "/events",
    component: BackendLayout,
    name: "Events",
    children: [
      {
        path: "/events",
        name: "EventIndex",
        component: EventIndex,
        meta: {
          pageTitle: "title.events",
          breadcrumbs: ["title.events"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop",
        name: "EventShopIndex",
        component: ShopIndex,
        meta: {
          pageTitle: "title.shop",
          breadcrumbs: ["title.shop"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/inventory",
        name: "EventShopInventory",
        component: ShopInventory,
        meta: {
          pageTitle: "title.shopInventory",
          breadcrumbs: ["title.shop", "title.shopInventory"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/customer",
        name: "EventShopCustomer",
        component: ShopCustomer,
        meta: {
          pageTitle: "title.shopCustomers",
          breadcrumbs: ["title.shop", "title.shopCustomers"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/orders",
        name: "EventShopOrders",
        component: ShopOrders,
        meta: {
          pageTitle: "title.shopOrders",
          breadcrumbs: ["title.shop", "title.shopOrders"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/rewards",
        name: "EventShopRewards",
        component: ShopRewards,
        meta: {
          pageTitle: "title.shopRewards",
          breadcrumbs: ["title.shop", "title.shopRewards"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/rewardRebate",
        name: "EventShopRewardRebate",
        component: ShopRewardRebate,
        meta: {
          pageTitle: "title.rewardRebate",
          breadcrumbs: ["title.shop", "title.rewardRebate"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/pointTransaction",
        name: "EventShopPointTransaction",
        component: ShopPointTransaction,
        meta: {
          pageTitle: "title.pointTransaction",
          breadcrumbs: ["title.shop", "title.pointTransaction"],
          permissions: ["TenantAdmin", "EventAdmin"],
        },
      },
      {
        path: "/events/shop/category",
        name: "EventShopCategory",
        component: ShopCategory,
        meta: {
          pageTitle: "title.shopCategory",
          breadcrumbs: ["title.shop", "title.shopCategory"],
          permissions: ["TenantAdmin", "EventAdmin"],
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
