import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/VerificationIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/verification",
    redirect: "/verification",
    component: ClientLayout,
    name: "verify",
    children: [
      {
        path: "/verification",
        name: "VerificationIndex",
        component: () =>
          import(
            /*webpackChunkName:'verification'*/ "./views/VerificationIndex.vue"
          ),
        meta: {
          pageTitle: "title.accountApplication",
          breadcrumbs: ["title.verification"],
          permissions: ["Guest"],
        },
      },
    ],
  });
};
