import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.sales",
        route: "/sales",
        svgIcon: "/images/icons/abstract/abs039.svg",
        permissions: ["Sales"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
