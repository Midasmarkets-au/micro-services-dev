import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.tools",
    route: "/tools",
    permissions: ["TenantAdmin", "WebTool"],
    pages: [
      {
        heading: "title.specialList",
        route: "/tools/special-list",
        svgIcon: "/images/icons/abstract/abs017.svg",
        permissions: ["TenantAdmin", "WebSpecialList"],
      },
      {
        heading: "title.groupManage",
        route: "/tools/group-manage",
        svgIcon: "/images/icons/communication/com013.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.twoFaCode",
        route: "/tools/twofa-code",
        svgIcon: "/images/icons/communication/com014.svg",
        permissions: ["TenantAdmin"],
      },

      // {
      //   heading: "Zoom",
      //   route: "/tools/zoom",
      //   svgIcon: "/images/icons/communication/com014.svg",
      //   permissions: ["TenantAdmin", "WebZoom"],
      // },
      // {
      //   heading: "title.equityBelowCredit",
      //   route: "/tools/equity-below-credit",
      //   svgIcon: "/images/icons/general/gen044.svg",
      //   permissions: ["TenantAdmin", "WebEquityBelowCredit"],
      // },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}

export default registerMenu;
