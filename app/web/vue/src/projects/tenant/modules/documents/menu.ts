import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.documents",
    route: "/documents",
    permissions: ["TenantAdmin"],
    pages: [
      {
        heading: "title.documents",
        route: "/documents/index",
        svgIcon: "/images/icons/general/gen009.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.contractSpecifications",
        route: "/documents/contractspecs",
        svgIcon: "/images/icons/general/gen008.svg",
        permissions: ["TenantAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
