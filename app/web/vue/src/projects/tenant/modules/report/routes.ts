import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ReportRecord from "@/projects/tenant/modules/report/views/ReportRecord.vue";
import ClientConfirmation from "@/projects/tenant/modules/report/views/ClientConfirmation.vue";
import ReportIndex from "@/projects/tenant/modules/report/views/ReportIndex.vue";
import EquityReportIndex from "./views/EquityReportIndex.vue";
import MessageRecord from "@/projects/tenant/modules/report/views/MessageRecord.vue";
import CommentRecord from "./views/CommentRecord.vue";

export default (router) => {
  router.addRoute({
    path: "/report",
    redirect: "/report/record",
    component: BackendLayout,
    name: "report",
    children: [
      // {
      //   path: "/report",
      //   name: "Reports",
      //   component: ReportIndex,
      //   meta: {
      //     pageTitle: "title.report",
      //     breadcrumbs: ["title.report"],
      //     permissions: ["TenantAdmin", "WebReport"],
      //   },
      // },
      {
        path: "/report/equity",
        name: "Equity",
        component: EquityReportIndex,
        meta: {
          pageTitle: "title.equityReport",
          breadcrumbs: ["title.equity"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/report/record",
        name: "ReportRecord",
        component: ReportRecord,
        meta: {
          pageTitle: "title.reportRecord",
          breadcrumbs: ["title.reportRecord"],
          permissions: ["TenantAdmin", "WebReportRecord"],
        },
      },
      {
        path: "/report/confirmation",
        name: "ClientConfirmation",
        component: ClientConfirmation,
        meta: {
          pageTitle: "Client Confirmation",
          breadcrumbs: ["Client Confirmation"],
          permissions: ["TenantAdmin", "WebClientConfirmation"],
        },
      },
      {
        path: "/report/message",
        name: "MessageRecord",
        component: MessageRecord,
        meta: {
          pageTitle: "title.messageRecord",
          breadcrumbs: ["title.messageRecord"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/report/comment",
        name: "CommentRecord",
        component: CommentRecord,
        meta: {
          pageTitle: "title.commentRecord",
          breadcrumbs: ["title.commentRecord"],
          permissions: ["TenantAdmin"],
        },
      },
    ],
  });
};
