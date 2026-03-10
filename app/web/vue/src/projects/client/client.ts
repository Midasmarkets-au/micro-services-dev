import { axiosInstance } from "@/core/services/api.client";

import { createApp } from "vue";
import App from "./ClientApp.vue";
import "./index.css";
/*
TIP: To get started with clean router change path to @/router/clean.ts.
 */
import router from "./config/router";
import store, { storeKey } from "@/store";
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
import { createMenu } from "@/core/plugins/menu";
import { createSignalR } from "@/core/plugins/signalr";
import createApi from "@/core/plugins/api";
import ICan from "@/core/plugins/ICan";
import VueGtag from "vue-gtag";
import "@vueup/vue-quill/dist/vue-quill.snow.css";
import "@/core/plugins/prismjs";

// Create App
const app = createApp(App);

// Dependency Injection
app.provide("appConfig", { sidebar: "light-sidebar" });

app.use(i18n);

const menu = createMenu();
app.use(menu);

const api = createApi();
app.use(api);

import initGlobalValidators from "@/core/services/ValidateService";
initGlobalValidators();

// import initGlobalFilter from "@/core/services/FilterService";
// initGlobalFilter(app);

app.config.globalProperties.$filters = {
  digits(value, digits = 2) {
    return Math.round(value * Math.pow(10, digits)) / Math.pow(10, digits);
  },
};

import { clickOutside } from "@/core/plugins/customDirectives";
app.directive("click-outside", clickOutside);
// import { defineRule } from "vee-validate";
// import AllRules from "@vee-validate/rules";
// Object.keys(AllRules).forEach((rule) => {
//   defineRule(rule, AllRules[rule]);
// });

// import Modules from config
import ModuleConfig from "@/projects/client/config/moduleConfig";
ModuleConfig.openModules.map((module) => {
  module && app.use(module, { router, i18n, menu, api, store });
});

router.addRoute({
  name: "catchAll",
  path: "/:pathMatch(.*)*",
  redirect: "/",
});

const toastOptions: PluginOptions = {
  // You can set your default options here
};
app.use(Toast, toastOptions);
app.use(ICan);
app.use(store, storeKey);
app.use(router);
app.use(ElementPlus);

app.use(
  VueGtag,
  {
    config: { id: process.env.VUE_APP_GA_ID },
  },
  router
);

ApiService.init(app);
initApexCharts(app);
initInlineSvg(app);
initVeeValidate();

const wsSignalR = createSignalR(window["api"] + "/hub/client");
wsSignalR.setup(JwtService.getToken());
app.use(wsSignalR);

import UserAvatar from "@/components/UserAvatar.vue";
app.component("UserAvatar", UserAvatar);

import BalanceShow from "@/components/BalanceShow.vue";
app.component("BalanceShow", BalanceShow);

import LoadingRing from "@/components/LoadingRing.vue";
app.component("LoadingRing", LoadingRing);

import TimeShow from "@/components/TimeShow.vue";
app.component("TimeShow", TimeShow);

import ShopPoints from "@/components/ShopPoints.vue";
app.component("ShopPoints", ShopPoints);

import NoDataBox from "@/components/NoDataBox.vue";
app.component("NoDataBox", NoDataBox);

import TableFooter from "@/components/TableFooter.vue";
app.component("TableFooter", TableFooter);

import InlineSvg from "vue-inline-svg";
app.component("InlineSvg", InlineSvg);

import SvgIcon from "@/projects/client/components/SvgIcon.vue";
app.component("SvgIcon", SvgIcon);

import StatusBadge from "@/components/StatusBadge.vue";
import setup from "@/core/services/api.interceptor";
import filters from "@/core/helpers/filters";

app.config.globalProperties.$axios = { ...axiosInstance };
setup();
app.component("StatusBadge", StatusBadge);

import { getTenantName, setFavicon } from "@/core/types/TenantTypes";

const title = getTenantName.value;
window.document.title = title || "MDM";
setFavicon();

app.mount("#app");

export default app;
