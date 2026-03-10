import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import EquityIndex from "./views/EquityIndex.vue";
import SpecialListIndex from "./views/SpecialListIndex.vue";
import ZoomIndex from "./views/ZoomIndex.vue";
import GroupManageIndex from "./views/GroupManageIndex.vue";
import TwoFaCodeIndex from "./views/TwoFaCodeIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/tools",
    redirect: "/tools",
    component: BackendLayout,
    name: "tools",
    children: [
      {
        path: "/tools/equity-below-credit",
        name: "EquityIndex",
        component: EquityIndex,
        meta: {
          pageTitle: "title.equityBelowCredit",
          breadcrumbs: ["title.equityBelowCredit"],
          permissions: ["TenantAdmin", "WebEquityBelowCredit"],
        },
      },
      {
        path: "/tools/special-list",
        name: "SpecialListIndex",
        component: SpecialListIndex,
        meta: {
          pageTitle: "title.specialList",
          breadcrumbs: ["title.specialList"],
          permissions: ["TenantAdmin", "WebSpecialList"],
        },
      },
      {
        path: "/tools/zoom",
        name: "ZoomIndex",
        component: ZoomIndex,
        meta: {
          pageTitle: "Zoom",
          breadcrumbs: ["Zoom"],
          permissions: ["TenantAdmin", "WebZoom"],
        },
      },
      {
        path: "/tools/group-manage",
        name: "GroupManageIndex",
        component: GroupManageIndex,
        meta: {
          pageTitle: "title.groupManage",
          breadcrumbs: ["title.groupManage"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/tools/twofa-code",
        name: "TwoFaCodeIndex",
        component: TwoFaCodeIndex,
        meta: {
          pageTitle: "title.twoFaCode",
          breadcrumbs: ["title.twoFaCode"],
          permissions: ["TenantAdmin"],
        },
      },
    ],
  });
};
