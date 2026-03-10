import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.eventShop",
        route: "/eventshop",
        svgIcon: "/images/icons/art/art002.svg",
        fontIcon: "bi-app-indicator",
        permissions: [
          "Client",
          "EventShop",
          "IB",
          "Sales",
          "TenantAdmin",
          "EventShopTest",
        ],
        tenantPermissions: ["bvi", "sea"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
