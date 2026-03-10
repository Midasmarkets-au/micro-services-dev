import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ContractSpecsIndex from "@/projects/tenant/modules/documents/views/ContractSpecsIndex.vue";
import DocumentsIndex from "@/projects/tenant/modules/documents/views/DocumentsIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/documents",
    redirect: "/documents/index",
    component: BackendLayout,
    name: "documents",
    children: [
      {
        path: "/documents/index",
        name: "Documents",
        component: DocumentsIndex,
        meta: {
          pageTitle: "title.documents",
          breadcrumbs: ["title.documents"],
          permissions: ["TenantAdmin"],
        },
      },

      {
        path: "/documents/contractspecs",
        name: "Report",
        component: ContractSpecsIndex,
        meta: {
          pageTitle: "title.contractSpecifications",
          breadcrumbs: ["title.documents", "title.contractSpecifications"],
          permissions: ["TenantAdmin"],
        },
      },
    ],
  });
};
