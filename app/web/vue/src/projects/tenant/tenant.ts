import { createApp } from "vue";
import App from "./TenantApp.vue";
import CKeditor from "@ckeditor/ckeditor5-vue";
/*
TIP: To get started with clean router change path to @/router/clean.ts test auto deployment
 */
import router from "./config/router";
import store, { storeKey } from "@/store";
import ElementPlus from "element-plus";
import i18n from "@/core/plugins/i18n";
import Toast, { PluginOptions } from "vue-toastification";
import "vue-toastification/dist/index.css";
import ApiService from "@/core/services/ApiService";
import JwtService from "@/core/services/JwtService";
import { initApexCharts } from "@/core/plugins/apexcharts";
import { initInlineSvg } from "@/core/plugins/inline-svg";
import { initVeeValidate } from "@/core/plugins/vee-validate";
import { createSignalR } from "@/core/plugins/signalr";
import { createMenu } from "@/core/plugins/backendMenu";
import TenantStoreModule from "./store/TenantModules";

import createApi from "@/core/plugins/api";

import ICan from "@/core/plugins/ICan";

// import VueNativeSock from "vue-native-websocket-vue3";

import "@/core/plugins/prismjs";
const app = createApp(App);
app.provide("appConfig", { sidebar: "light-sidebar" });

app.use(i18n);

const menu = createMenu();
const api = createApi();

app.use(menu);
app.use(api);

app.use(CKeditor);

import setupAxiosInstance from "@/core/services/api.interceptor";
setupAxiosInstance();

import initGlobalValidators from "@/core/services/ValidateService";
initGlobalValidators();

import initGlobalFilter from "@/core/plugins/Filters";
initGlobalFilter(app);

import BalanceShow from "@/components/BalanceShow.vue";
app.component("BalanceShow", BalanceShow);

import ShopPoints from "@/components/ShopPoints.vue";
app.component("ShopPoints", ShopPoints);

import LoadingRing from "@/components/LoadingRing.vue";
app.component("LoadingRing", LoadingRing);

import FileIcon from "@/components/FileIcon.vue";
app.component("FileIcon", FileIcon);

import TimeShow from "@/components/TimeShow.vue";
app.component("TimeShow", TimeShow);

import NoDataBox from "@/components/NoDataBox.vue";
app.component("NoDataBox", NoDataBox);

import TableFooter from "@/components/TableFooter.vue";
app.component("TableFooter", TableFooter);

// Modules for Tenant only

import AccountsModule from "./modules/accounts";
app.use(AccountsModule, { router, i18n, menu, api });

import UsersModule from "./modules/users";
app.use(UsersModule, { router, i18n, menu, api });

import CaseModule from "./modules/case";
app.use(CaseModule, { router, menu, api });

import FundingModule from "./modules/Payment";
app.use(FundingModule, { router, i18n, menu, api });

import TradingModule from "./modules/trade";
app.use(TradingModule, { router, i18n, menu, api });

import RebateModule from "./modules/rebate";
app.use(RebateModule, { router, i18n, menu, api });

import ToolsModule from "./modules/tools";
app.use(ToolsModule, { router, i18n, menu, api });

import EventsModule from "./modules/events";
app.use(EventsModule, { router, menu, api });

import ReportModule from "./modules/report";
app.use(ReportModule, { router, menu, api });

import TopicModule from "./modules/topic";
app.use(TopicModule, { router, i18n, menu, api });

// import ShopModule from "./modules/Shop";
// app.use(ShopModule, { router, menu, api });

import DocumentsModule from "./modules/documents";
app.use(DocumentsModule, { router, menu, api });

import SystemModule from "./modules/system";
app.use(SystemModule, { router, i18n, menu, api });

import ServerModule from "./modules/server";
app.use(ServerModule, { router, i18n, menu, api });

import ItModule from "./modules/it";
app.use(ItModule, { router, menu, api });
app.use(store, storeKey);

// Modules for Tenant only

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

app.use(router);

app.use(ElementPlus);

ApiService.init(app);

initApexCharts(app);
initInlineSvg(app);
initVeeValidate();
const wsSignalR = createSignalR(process.env.VUE_APP_API_URL + "/hub/client");
wsSignalR.setup(JwtService.getToken());

app.use(wsSignalR);

import UserAvatar from "@/components/UserAvatar.vue";
app.component("UserAvatar", UserAvatar);

import AuthImage from "@/components/AuthImage.vue";
app.component("AuthImage", AuthImage);

import UserInfo from "@/components/UserInfo.vue";
app.component("UserInfo", UserInfo);

import IbSalesInfo from "@/components/IbSalesInfo.vue";
app.component("IbSalesInfo", IbSalesInfo);

import StatusBadge from "@/components/StatusBadge.vue";
app.component("StatusBadge", StatusBadge);

import AccountStatusBadge from "@/components/AccountStatusBadge.vue";
app.component("AccountStatusBadge", AccountStatusBadge);

import UserVerify from "@/components/UserVerify.vue";
app.component("UserVerify", UserVerify);

store.registerModule("TenantModule", TenantStoreModule);

import { setFavicon } from "@/core/types/TenantTypes";

setFavicon();

app.mount("#app");
export default app;
