import BackendLayoutVue from "@/layouts/backend-layout/BackendLayout.vue";
import CaseIndex from "./views/CaseIndex.vue";
import router from "../../config/router";

export default (router) => {
  router.addRoute({
    path: "/cases",
    redirect: "/cases",
    component: BackendLayoutVue,
    name: "Cases",
    children: [
      {
        path: "/cases",
        name: "cases",
        component: CaseIndex,
        meta: {
          pageTitle: "title.cases",
          breadcrumbs: ["title.cases"],
          permissions: ["SuperAdmin", "TenantAdmin", "WebAdminSupport"],
        },
      },
    ],
  });
};
