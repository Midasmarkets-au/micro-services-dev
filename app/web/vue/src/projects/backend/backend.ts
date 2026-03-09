import { createApp } from "vue";
import App from "./BackendApp.vue";

/*
TIP: To get started with clean router change path to @/router/clean.ts.
 */
import router from "./config/router";
import store from "@/store";
import ElementPlus from "element-plus";
import i18n from "@/core/plugins/i18n";

import Toast, { PluginOptions } from "vue-toastification";
import "vue-toastification/dist/index.css";

//imports for app initialization
import ApiService from "@/core/services/ApiService";
import JwtService from "@/core/services/JwtService";
import { initApexCharts } from "@/core/plugins/apexcharts";
import { initInlineSvg } from "@/core/plugins/inline-svg";
import { initVeeValidate } from "@/core/plugins/vee-validate";
import { createSignalR } from "@/core/plugins/signalr";
import { createMenu } from "@/core/plugins/menu";
import createApi from "@/core/plugins/api";

import ICan from "@/core/plugins/ICan";

// import VueNativeSock from "vue-native-websocket-vue3";

import "@/core/plugins/prismjs";

const app = createApp(App);
// app.provide("appConfig", { sidebar: "light-sidebar" });
app.provide("appConfig", { sidebar: "dark-sidebar" });

app.use(i18n);

const menu = createMenu();
const api = createApi();

app.use(menu);
app.use(api);

import setupAxiosInstance from "@/core/services/api.interceptor";
setupAxiosInstance();

import initGlobalValidators from "@/core/services/ValidateService";
initGlobalValidators();

import ContactModule from "./modules/contact";
app.use(ContactModule, { router, i18n, menu, api });
// End Modules for Backend only

router.removeRoute("catchAll");
router.addRoute({
  name: "catchAll",
  path: "/:pathMatch(.*)*",
  redirect: "/404",
});

const toastOptions: PluginOptions = {
  // You can set your default options here
};
app.use(Toast, toastOptions);
app.use(ICan);
app.use(store);
app.use(router);

app.use(ElementPlus);

ApiService.init(app);

initApexCharts(app);
initInlineSvg(app);
initVeeValidate();

const wsSignalR = createSignalR(window["api"] + "/hub/client");
wsSignalR.setup(JwtService.getToken());
app.use(wsSignalR);

import TenantStoreModule from "./store/TenantModules";

import UserAvatar from "@/components/UserAvatar.vue";
app.component("UserAvatar", UserAvatar);

import AuthImage from "@/components/AuthImage.vue";
app.component("AuthImage", AuthImage);

store.registerModule("TenantModule", TenantStoreModule);
window.document.title = process.env.VUE_APP_PAGE_TITLE || "MM";
app.mount("#app");
export default app;
