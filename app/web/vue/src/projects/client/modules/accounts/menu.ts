import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.accounts",
        route: "/account",
        svgIcon: "/images/icons/art/art002.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["Client"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
