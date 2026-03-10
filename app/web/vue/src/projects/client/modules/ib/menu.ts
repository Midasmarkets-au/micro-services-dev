import MenuItem from "@/core/models/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.ibCenter",
    route: "/ib",
    permissions: ["IB"],
    pages: [
      // {
      //   heading: "title.ib",
      //   route: "/ib/index",
      //   svgIcon: "/images/icons/abstract/abs039.svg",
      //   permissions: ["IB", "Sales"],
      // },
      {
        heading: "title.ibCustomer",
        route: "/ib/new-customers",
        svgIcon: "/images/icons/communication/com014.svg",
        permissions: ["IB"],
      },
      {
        heading: "title.ibCustomer",
        route: "/ib/customers",
        svgIcon: "/images/icons/communication/com014.svg",
        permissions: ["IB"],
      },
      {
        heading: "title.ibTrade",
        route: "/ib/trade",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["IB"],
      },
      {
        heading: "title.ibFunding",
        route: "/ib/funding",
        svgIcon: "/images/icons/finance/fin003.svg",
        permissions: ["IB"],
      },
      {
        heading: "title.ibRebate",
        route: "/ib/rebate",
        svgIcon: "/images/icons/graphs/gra003.svg",
        permissions: ["IB"],
      },
      {
        heading: "title.ibReferral",
        route: "/ib/referral",
        svgIcon: "/images/icons/general/gen002.svg",
        permissions: ["IB"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
