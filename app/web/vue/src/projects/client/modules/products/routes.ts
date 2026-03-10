import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
import Index from "./views/ProductsIndex.vue";
export default (router) => {
  router.addRoute({
    path: "/products",
    redirect: "/",
    component: ClientLayout,
    name: "title.products",
    children: [
      {
        path: "/products",
        name: "ProductIndex",
        component: Index,
        meta: {
          pageTitle: "title.products",
          breadcrumbs: ["title.products"],
          permissions: ["Client"],
        },
      },
    ],
  });
};
