import MenuItem from "@/core/models/MenuItem";

const DocMenu: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.dashboard",
        route: "/",
        svgIcon: "/images/icons/abstract/abs030.svg",
        fontIcon: "bi-app-indicator",
        permissions: ["any"],
      },
    ],
    permissions: ["any"],
  },
];
const SystemMenu: Array<MenuItem> = [];

export { DocMenu, SystemMenu };
