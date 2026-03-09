import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.rebate",
    route: "/rebate",
    permissions: ["SuperAdmin", "TenantAdmin", "WebRebate"],
    pages: [
      {
        heading: "title.pendingConfirmation",
        route: "/rebate/pending-confirmation",
        svgIcon: "/images/icons/general/gen046.svg",
        permissions: ["TenantAdmin", "WebPendingConfirmation"],
      },
      {
        heading: "title.rebateScheme",
        route: "/rebate/scheme",
        svgIcon: "/images/icons/finance/fin007.svg",
        permissions: ["TenantAdmin", "WebRebateScheme"],
      },
      {
        heading: "title.tradeRebate",
        route: "/rebate/trade-rebate",
        svgIcon: "/images/icons/finance/fin006.svg",
        permissions: ["TenantAdmin", "WebTradeRebate"],
      },
      {
        heading: "title.rebateRecord",
        route: "/rebate/record",
        svgIcon: "/images/icons/general/gen005.svg",
        permissions: ["TenantAdmin", "WebRebateRecord"],
      },
      {
        heading: "title.salesRebateSchema",
        route: "/rebate/sales-rebate-schema",
        svgIcon: "/images/icons/general/gen005.svg",
        permissions: ["TenantAdmin", "WebSalesRebateSchema"],
      },
      {
        heading: "title.salesRebateRecord",
        route: "/rebate/sales-rebate-record",
        svgIcon: "/images/icons/general/gen005.svg",
        permissions: ["TenantAdmin", "WebSalesRebate"],
      },
      // {
      //   heading: "title.tradeRecord",
      //   route: "/rebate/trade",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["SuperAdmin"],
      // },
      // {
      //   heading: "title.exchangeRateBackup",
      //   route: "/rebate/exchange-rate",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["TenantAdmin"],
      // },
      // {
      //   heading: "title.productsList",
      //   route: "/rebate/products-list",
      //   svgIcon: "/images/icons/abstract/abs042.svg",
      //   permissions: ["TenantAdmin", "WebProductList"],
      // },
      {
        heading: "title.productsList",
        route: "/rebate/products-list-new",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["TenantAdmin", "WebProductList"],
      },
      // {
      //   heading: "title.rebateReciver",
      //   route: "/rebate/reciver",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["rebate.reciver"],
      // },
      // {
      //   heading: "title.RebateIB",
      //   route: "/rebate/ib",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["rebate.ib"],
      // },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
