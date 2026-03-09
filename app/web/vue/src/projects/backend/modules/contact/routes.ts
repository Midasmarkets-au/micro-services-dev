import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import Department from "./views/department/DepartmentIndex.vue";
import Office from "./views/office/OfficeIndex.vue";
import Contact from "./views/contact/ContactIndex.vue";

export default (router) => {
  router.addRoute({
    path: "/contacts",
    redirect: "/contacts",
    component: BackendLayout,
    children: [
      {
        path: "/contacts/departments",
        name: "ContactsDepartments",
        component: Department,
        meta: {
          pageTitle: "title.department",
          breadcrumbs: ["title.contacts", "title.department"],
          permissions: ["department.view"],
        },
      },
      {
        path: "/contacts/offices",
        name: "ContactsCffices",
        component: Office,
        meta: {
          pageTitle: "title.Office",
          breadcrumbs: ["title.contacts", "title.Office"],
          permissions: ["office.view"],
        },
      },
      {
        path: "/contacts/contacts",
        name: "ContactsContacts",
        component: Contact,
        meta: {
          pageTitle: "title.Contact",
          breadcrumbs: ["title.contacts", "title.Contact"],
          permissions: ["contact.view"],
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
