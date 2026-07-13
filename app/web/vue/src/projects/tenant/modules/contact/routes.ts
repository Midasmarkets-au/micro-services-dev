import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ContactIndex from "./views/ContactIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/contact",
    redirect: "/contact/index",
    component: BackendLayout,
    name: "contact",
    children: [
      {
        path: "/contact/index",
        name: "ContactIndex",
        component: ContactIndex,
        meta: {
          pageTitle: "title.contacts",
          breadcrumbs: ["title.contacts"],
          permissions: ["TenantAdmin", "SuperAdmin", "MailAdmin"],
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
