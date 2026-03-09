import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.cases",
    route: "/cases",
    permissions: ["SuperAdmin", "TenantAdmin", "WebAdminSupport"],
    pages: [
      {
        heading: "title.cases",
        route: "/cases",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["SuperAdmin", "TenantAdmin", "WebAdminSupport"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
