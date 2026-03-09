// import ClientLayout from "@/layouts/client-layout/ClientLayout.vue";
// import plateforms from "./views/TradePlateforms.vue";
// import webtrader from "./views/ToolsWebtrader.vue";
// import webtrader5 from "./views/ToolsWebtrader5.vue";

export default (router) => {
  router.addRoute({
    path: "/plateforms",
    redirect: "/plateforms",
    component: () =>
      import(
        /*webpackChunkName:'client'*/ "@/layouts/client-layout/ClientLayout.vue"
      ),
    name: "plateforms",
    children: [],
  });
};
