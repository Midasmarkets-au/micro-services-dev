import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import ItIcons from "./views/Icons.vue";
import UsefulScript from "@/projects/tenant/modules/it/views/UsefulScript.vue";
import TranslationIndex from "./views/TranslationIndex.vue";
import UploadPublicFile from "./views/UploadPublicFile.vue";
export default (router) => {
  router.addRoute({
    path: "/it",
    redirect: "/it/icons",
    component: BackendLayout,
    name: "it",
    children: [
      {
        path: "/it/icons",
        name: "ItIndex",
        component: ItIcons,
        meta: {
          pageTitle: "title.itIcons",
          breadcrumbs: ["title.itIcons"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/it/scripts",
        name: "ScriptsIndex",
        component: UsefulScript,
        meta: {
          pageTitle: "title.scripts",
          breadcrumbs: ["title.scripts"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/it/translations",
        name: "TranslationIndex",
        component: TranslationIndex,
        meta: {
          pageTitle: "title.translation",
          breadcrumbs: ["title.translation"],
          permissions: ["superAdmin"],
        },
      },
      {
        path: "/it/upload-public-file",
        name: "UploadPublicFile",
        component: UploadPublicFile,
        meta: {
          pageTitle: "title.uploadPublicFile",
          breadcrumbs: ["title.uploadPublicFile"],
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
