import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import EquityCheck from "./views/EquityCheck.vue";
import BriefDetail from "./views/BriefDetail.vue";
import router from "../../config/router";
import OffsetCheck from "./views/OffsetCheck.vue";
import ProfitLoss from "./views/ProfitLoss.vue";

export default (router) => {
  router.addRoute({
    path: "/trade",
    redirect: "/trade",
    component: BackendLayout,
    name: "trade",
    children: [
      {
        path: "/trade/offset-check",
        name: "OffsetCheck",
        component: OffsetCheck,
        meta: {
          pageTitle: "title.offsetCheck",
          breadcrumbs: ["title.offsetCheck"],
          permissions: ["TenantAdmin", "WebOffsetCheck"],
        },
      },
      {
        path: "/trade/equity-check",
        name: "EquityCheck",
        component: EquityCheck,
        meta: {
          pageTitle: "title.equityCheck",
          breadcrumbs: ["title.equityCheck"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/trade/brief-detail",
        name: "BriefDetail",
        component: BriefDetail,
        meta: {
          pageTitle: "title.briefDetail",
          breadcrumbs: ["title.briefDetail"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/trade/profit-loss",
        name: "ProftLoss",
        component: ProfitLoss,
        meta: {
          pageTitle: "title.profitLoss",
          breadcrumbs: ["title.profitLoss"],
          permissions: ["TenantAdmin"],
        },
      },
    ],
  });
};
