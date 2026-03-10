import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.rep",
        route: "/rep",
        svgIcon: "/images/icons/abstract/abs039.svg",
        permissions: ["Rep"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
