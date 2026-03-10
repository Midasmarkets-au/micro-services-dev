import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.trade",
    route: "/trade",
    permissions: ["TenantAdmin"],
    pages: [
      {
        heading: "title.equityCheck",
        route: "/trade/equity-check",
        svgIcon: "/images/icons/general/gen040.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.briefDetail",
        route: "/trade/brief-detail",
        svgIcon: "/images/icons/general/gen041.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.offsetCheck",
        route: "/trade/offset-check",
        svgIcon: "/images/icons/general/gen042.svg",
        permissions: ["TenantAdmin", "WebOffsetCheck"],
      },
      {
        heading: "title.profitLoss",
        route: "/trade/profit-loss",
        svgIcon: "/images/icons/general/gen043.svg",
        permissions: ["TenantAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}

export default registerMenu;
