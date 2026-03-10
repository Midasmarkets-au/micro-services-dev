import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    pages: [
      {
        heading: "title.wallet",
        route: "/wallet",
        svgIcon: "/images/icons/finance/fin008.svg",
        permissions: ["IB", "Sales", "Client"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
