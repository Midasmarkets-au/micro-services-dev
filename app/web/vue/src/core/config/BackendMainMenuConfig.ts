import MenuItem from "@/core/models/MenuItem";

const DocMenu: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.dashboard",
        route: "/",
        svgIcon: "/images/icons/abstract/abs030.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["any"],
      },
    ],
    permissions: ["any"],
  },
  {
    pages: [
      {
        heading: "title.accountOverview",
        route: "/account-overview",
        svgIcon: "/images/icons/communication/com006.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["Admin"],
      },
    ],
    permissions: ["Admin"],
  },
];
const SystemMenu: Array<MenuItem> = [];
// function initMainMenu() {
//   const menuItems = DocMenuConfig.concat(moduleMenu ?? {}, SystemMenuConfig);
//   return menuItems;
// }
// const menuItems = DocMenuConfig.concat(moduleMenu ?? {}, SystemMenuConfig);
// export default initMainMenu;
export { DocMenu, SystemMenu };
