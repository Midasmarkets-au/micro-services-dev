// import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import Index from "./views/SupportsIndex.vue";
// // import SupportsCalendar from "./views/SupportsCalendar.vue";
// import SupportsNotices from "./views/SupportsNotices.vue";
// // import SupportsNews from "./views/SupportsNews.vue";
// import DocumentsIndex from "./views/DocumentsIndex.vue";
// import CaseIndexVue from "./views/CaseIndex.vue";
export default (router) => {
  router.addRoute({
    path: "/supports",
    redirect: "/supports",
    component: () =>
      import(
        /*webpackChunkName:'client'*/ "@/layouts/client-layout/ClientLayout.vue"
      ),
    name: "supports",
    children: [],
  });
};
