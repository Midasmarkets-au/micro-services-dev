import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ServerIndex from "./views/ServerIndex.vue";
import ServerServices from "./views/ServerServices.vue";
import ServerLogs from "./views/ServerLogs.vue";
import ServerDockers from "./views/ServerDockers.vue";
import WebSocket from "./views/WebSocket.vue";
import ServerCache from "./views/ServerCache.vue";

export default (router) => {
  router.addRoute({
    path: "/server",
    redirect: "/server/servers",
    component: BackendLayout,
    name: "Server",
    children: [
      {
        path: "/server/servers",
        name: "server.servers",
        component: ServerIndex,
        meta: {
          pageTitle: "title.server",
          breadcrumbs: ["title.server"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/server/websocket",
        name: "server.websocket",
        component: WebSocket,
        meta: {
          pageTitle: "WebSocket",
          breadcrumbs: ["title.server", "WebSocket"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/server/dockers",
        name: "server.dockers",
        component: ServerDockers,
        meta: {
          pageTitle: "title.serverDocker",
          breadcrumbs: ["title.server", "title.serverDocker"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/server/services",
        name: "server.services",
        component: ServerServices,
        meta: {
          pageTitle: "title.serverService",
          breadcrumbs: ["title.server", "title.serverService"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/server/logs",
        name: "server.logs",
        component: ServerLogs,
        meta: {
          pageTitle: "title.serverLog",
          breadcrumbs: ["title.server", "title.serverLog"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/server/cache",
        name: "server.cache",
        component: ServerCache,
        meta: {
          pageTitle: "title.serverCache",
          breadcrumbs: ["title.server", "title.serverCache"],
          permissions: ["superAdmin"],
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
