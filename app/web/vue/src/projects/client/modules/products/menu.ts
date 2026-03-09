import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.products",
        route: "/products",
        svgIcon: "/images/icons/finance/fin008.svg",
        permissions: ["Client"],
        tenantPermissions: ["jp"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
