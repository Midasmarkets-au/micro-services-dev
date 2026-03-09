import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    // heading: "title.supports",
    // route: "/supports/contact-us",
    // permissions: ["any"],
    pages: [
      {
        heading: "title.supports",
        route: "/supports",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["any"],
      },
      // {
      //   heading: "title.notices",
      //   route: "/supports/notices",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["any"],
      // },
      // {
      //   heading: "title.documents",
      //   route: "/supports/documents",
      //   svgIcon: "/images/icons/art/art002.svg",
      //   fontIcon: "bi-app-indicator",
      //   permissions: ["any"],
      // },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
