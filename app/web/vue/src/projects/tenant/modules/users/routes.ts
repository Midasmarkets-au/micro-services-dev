import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import UserIndex from "./views/UsersIndex.vue";
import Verifications from "./views/UsersVerifications.vue";
import KycForm from "./views/KycForm.vue";
import KycFinalize from "./views/KycFinalize.vue";
import AllUsers from "./views/AllUsers.vue";
import UserLead from "./views/UserLead.vue";
export default (router) => {
  router.addRoute({
    path: "/users",
    redirect: "/users/index",
    component: BackendLayout,
    name: "users",
    children: [
      {
        path: "/users/index",
        name: "UsersIndex",
        component: UserIndex,
        meta: {
          pageTitle: "title.users",
          breadcrumbs: ["title.users"],
          permissions: ["TenantAdmin", "Compliance", "WebUser"],
        },
      },
      {
        path: "/users/verifications",
        name: "UsersVerifications",
        component: Verifications,
        meta: {
          pageTitle: "title.verifications",
          breadcrumbs: ["title.users", "title.verifications"],
          permissions: [
            "TenantAdmin",
            "Compliance",
            "AccountAdmin",
            "WebVerification",
          ],
        },
      },
      {
        path: "/users/kyc",
        name: "UsersKyc",
        component: KycForm,
        meta: {
          pageTitle: "title.kyc",
          breadcrumbs: ["title.users", "title.kyc"],
          permissions: ["Compliance", "KycOfficer", "WebKyc"],
        },
      },
      {
        path: "/users/kyc-finalize",
        name: "UsersKycFinalize",
        component: KycFinalize,
        meta: {
          pageTitle: "title.kycFinalize",
          breadcrumbs: ["title.users", "title.kycFinalize"],
          permissions: ["Compliance", "WebKyc"],
        },
      },
      {
        path: "/users/all-users",
        name: "AllUsers",
        component: AllUsers,
        meta: {
          pageTitle: "title.allUsers",
          breadcrumbs: ["title.users", "title.allUsers"],
          permissions: ["SuperAdmin", "WebAllUser"],
        },
      },
      {
        path: "/users/user-lead",
        name: "UserLead",
        component: UserLead,
        meta: {
          pageTitle: "title.lead",
          breadcrumbs: ["title.users", "title.lead"],
          permissions: ["SuperAdmin", "TenantAdmin"],
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
