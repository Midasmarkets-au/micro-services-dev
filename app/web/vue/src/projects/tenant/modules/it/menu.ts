import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.it",
    route: "/it",
    permissions: ["SuperAdmin"],
    pages: [
      {
        heading: "title.translation",
        route: "/it/translations",
        svgIcon: "/images/icons/communication/com001.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "title.uploadPublicFile",
        route: "/it/upload-public-file",
        svgIcon: "/images/icons/files/fil003.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "title.icons",
        route: "/it/icons",
        svgIcon: "/images/icons/art/art007.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "title.scripts",
        route: "/it/scripts",
        svgIcon: "/images/icons/coding/cod003.svg",
        permissions: ["superAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
