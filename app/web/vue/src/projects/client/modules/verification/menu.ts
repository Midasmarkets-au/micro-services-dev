import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.verification",
        route: "/verification",
        svgIcon: "/images/icons/art/art002.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["Guest"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
