import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.platforms",
        route: "/plateforms",
        svgIcon: "/images/icons/art/art002.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["any"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
