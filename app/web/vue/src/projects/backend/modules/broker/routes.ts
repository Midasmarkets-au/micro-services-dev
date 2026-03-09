import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import Index from "./views/index.vue";

export default (router) => {
  router.addRoute({
    path: "/broker",
    redirect: "/broker",
    component: BackendLayout,
    children: [
      {
        path: "/broker",
        name: "broker-index",
        component: Index,
        meta: {
          pageTitle: "title.broker",
          breadcrumbs: ["title.broker"],
          permissions: ["broker.view"],
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
