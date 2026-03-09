import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "Server",
    route: "/system",
    permissions: ["SuperAdmin"],
    pages: [
      {
        heading: "Servers",
        route: "/server/servers",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "WebSocket",
        route: "/server/websocket",
        svgIcon: "/images/icons/graphs/gra011.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "Dockers",
        route: "/server/dockers",
        svgIcon: "/images/icons/graphs/gra012.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "Services",
        route: "/server/services",
        svgIcon: "/images/icons/coding/cod001.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "Logs",
        route: "/server/logs",
        svgIcon: "/images/icons/coding/cod002.svg",
        permissions: ["superAdmin"],
      },
      {
        heading: "Cache",
        route: "/server/cache",
        svgIcon: "/images/icons/general/gen025.svg",
        permissions: ["superAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
