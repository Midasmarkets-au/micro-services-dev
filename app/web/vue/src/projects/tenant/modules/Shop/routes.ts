import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ShopIndex from "./views/ShopIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/shop",
    redirect: "/shop",
    component: BackendLayout,
    name: "Shop",
    children: [
      {
        path: "/shop",
        name: "ShopIndex",
        component: ShopIndex,
        meta: {
          pageTitle: "Shop",
          breadcrumbs: ["Shop"],
          permissions: ["SuperAdmin"],
        },
      },
    ],
  });
};
