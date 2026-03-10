import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import EmailTemplate from "./views/EmailTemplate.vue";
import NoticeTopic from "./views/NoticeTopic.vue";
import SendEmailBatch from "./views/SendEmailBatch.vue";
import PromotionIndex from "./views/PromotionIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/topic",
    redirect: "/topic/notices",
    component: BackendLayout,
    name: "topic",
    children: [
      {
        path: "/topic/notices",
        name: "Notice",
        component: NoticeTopic,
        meta: {
          pageTitle: "title.notices",
          breadcrumbs: ["title.notices"],
          permissions: ["TenantAdmin", "WebNotice"],
        },
      },
      {
        path: "/topic/email-template",
        name: "Email",
        component: EmailTemplate,
        meta: {
          pageTitle: "title.emailTemplate",
          breadcrumbs: ["title.emailTemplate"],
          permissions: ["SuperAdmin"],
        },
      },
      {
        path: "/topic/promotion",
        name: "Promotion",
        component: PromotionIndex,
        meta: {
          pageTitle: "title.promotion",
          breadcrumbs: ["title.promotion"],
          permissions: ["SuperAdmin"],
        },
      },
      {
        path: "/topic/send-email-batch",
        name: "SendEmailBatch",
        component: SendEmailBatch,
        meta: {
          pageTitle: "title.sendEmailBatch",
          breadcrumbs: ["title.sendEmailBatch"],
          permissions: ["SuperAdmin"],
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
