import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.contacts",
    route: "/contact",
    permissions: ["TenantAdmin", "SuperAdmin", "MailAdmin"],
    pages: [
      {
        heading: "title.contacts",
        route: "/contact/index",
        svgIcon: "/images/icons/communication/com011.svg",
        permissions: ["TenantAdmin", "SuperAdmin", "MailAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}

export default registerMenu;
