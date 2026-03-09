import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import RebateReciver from "./views/RebateReciver.vue";
import RebateRecord from "./views/RebateRecord.vue";
import RebateScheme from "./views/RebateScheme.vue";
import RebateIB from "./views/RebateIB.vue";
import ExchangeRate from "./views/ExchangeRate.vue";
import ProductsList from "./views/ProductsList.vue";
import ProductsListNew from "./views/ProductsListNew.vue";
import PendingConfirmation from "./views/PendingConfirmation.vue";
import TradeRebate from "@/projects/tenant/modules/rebate/views/TradeRebate.vue";
import SalesRebateSchema from "@/projects/tenant/modules/rebate/views/SalesRebateSchema.vue";
import SalesRebateRecord from "@/projects/tenant/modules/rebate/views/SalesRebateRecord.vue";

export default (router) => {
  router.addRoute({
    path: "/rebate",
    redirect: "/rebate",
    component: BackendLayout,
    name: "rebate",
    children: [
      {
        path: "/rebate/reciver",
        name: "RebateReciver",
        component: RebateReciver,
        meta: {
          pageTitle: "title.rebateReciver",
          breadcrumbs: ["title.rebateReciver"],
          permissions: ["SuperAdmin", "WebRebateReciver"],
        },
      },
      {
        path: "/rebate/record",
        name: "RebateRecord",
        component: RebateRecord,
        meta: {
          pageTitle: "title.rebateRecord",
          breadcrumbs: ["title.rebateRecord"],
          permissions: ["TenantAdmin", "WebRebateRecord"],
        },
      },
      {
        path: "/rebate/trade-rebate",
        name: "TradeRebate",
        component: TradeRebate,
        meta: {
          pageTitle: "title.tradeRebate",
          breadcrumbs: ["title.tradeRebate"],
          permissions: ["TenantAdmin", "WebTradeRebate"],
        },
      },
      {
        path: "/rebate/scheme",
        name: "RebateScheme",
        component: RebateScheme,
        meta: {
          pageTitle: "title.rebateScheme",
          breadcrumbs: ["title.rebateScheme"],
          permissions: ["TenantAdmin", "WebRebateScheme"],
        },
      },
      {
        path: "/rebate/ib",
        name: "RebateIB",
        component: RebateIB,
        meta: {
          pageTitle: "title.RebateIB",
          breadcrumbs: ["title.RebateIB"],
          permissions: ["SuperAdmin", "WebRebateIB"],
        },
      },
      {
        path: "/rebate/exchange-rate",
        name: "ExchangeRate",
        component: ExchangeRate,
        meta: {
          pageTitle: "title.exchangeRate",
          breadcrumbs: ["title.exchangeRate"],
          permissions: ["TenantAdmin", "WebExchangeRate"],
        },
      },
      {
        path: "/rebate/products-list",
        name: "ProductsList",
        component: ProductsList,
        meta: {
          pageTitle: "title.productsList",
          breadcrumbs: ["title.productsList"],
          permissions: ["TenantAdmin", "WebProductsList"],
        },
      },
      {
        path: "/rebate/products-list-new",
        name: "ProductsListNew",
        component: ProductsListNew,
        meta: {
          pageTitle: "title.productsList",
          breadcrumbs: ["title.productsList"],
          permissions: ["TenantAdmin", "WebProductsList"],
        },
      },
      {
        path: "/rebate/pending-confirmation",
        name: "PendingConfirmation",
        component: PendingConfirmation,
        meta: {
          pageTitle: "title.pendingConfirmation",
          breadcrumbs: ["title.pendingConfirmation"],
          permissions: ["TenantAdmin", "WebPendingConfirmation"],
        },
      },
      {
        path: "/rebate/sales-rebate-schema",
        name: "SalesRebateSchema",
        component: SalesRebateSchema,
        meta: {
          pageTitle: "title.salesRebateSchema",
          breadcrumbs: ["title.salesRebateSchema"],
          permissions: ["TenantAdmin", "WebSalesRebateSchema"],
        },
      },
      {
        path: "/rebate/sales-rebate-record",
        name: "SalesRebateRecord",
        component: SalesRebateRecord,
        meta: {
          pageTitle: "title.salesRebateRecord",
          breadcrumbs: ["title.salesRebateRecord"],
          permissions: ["TenantAdmin", "WebSalesRebate"],
        },
      },
    ],
  });
};
