import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "Shop",
    route: "/shop",
    permissions: ["SuperAdmin"],
    pages: [
      {
        heading: "Shop",
        route: "/shop",
        svgIcon: "/images/icons/ecommerce/ecm004.svg",
        permissions: ["SuperAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}

export default registerMenu;
