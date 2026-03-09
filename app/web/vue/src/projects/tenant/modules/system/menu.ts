import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.setting",
    route: "/system",
    permissions: ["SuperAdmin", "WebAdmin", "TenantAdmin"],
    pages: [
      {
        heading: "title.permission",
        route: "/system/permissions",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "title.apiLog",
        route: "/system/apilog",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["superAdmin", "TenantAdmin"],
      },
      {
        heading: "title.config",
        route: "/system/config",
        svgIcon: "/images/icons/coding/cod001.svg",
        permissions: ["superAdmin", "WebAdminConfig"],
      },
      {
        heading: "fields.platform",
        route: "/system/platform",
        svgIcon: "/images/icons/coding/cod001.svg",
        permissions: ["superAdmin", "WebAdminPlatform"],
      },
      // {
      //   heading: "title.permission",
      //   route: "/system/permissions",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["superAdmin", "WebAdminPermission"],
      // },
      // {
      //   heading: "title.role",
      //   route: "/system/roles",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["superAdmin", "WebAdminRole"],
      // },
      // {
      //   heading: "title.user",
      //   route: "/system/users",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["superAdmin", "WebAdminUser"],
      // },
      // {
      //   heading: "title.setting",
      //   route: "/system/settings",
      //   svgIcon: "/images/icons/coding/cod001.svg",
      //   permissions: ["superAdmin", "WebAdminSetting"],
      // },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
