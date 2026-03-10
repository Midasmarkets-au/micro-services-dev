import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "Events",
    route: "/events",
    permissions: ["TenantAdmin", "EventAdmin"],
    pages: [
      {
        heading: "title.events",
        route: "/events",
        svgIcon: "/images/icons/ecommerce/ecm004.svg",
        permissions: ["TenantAdmin", "EventAdmin"],
      },
      {
        sectionTitle: "title.shop",
        route: "/events/shop",
        svgIcon: "/images/icons/ecommerce/ecm004.svg",
        permissions: ["TenantAdmin", "EventAdmin"],
        sub: [
          {
            heading: "title.shopInventory",
            route: "/events/shop/inventory",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.shopCustomers",
            route: "/events/shop/customer",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.shopOrders",
            route: "/events/shop/orders",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.shopRewards",
            route: "/events/shop/rewards",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.rewardRebate",
            route: "/events/shop/rewardRebate",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.pointTransaction",
            route: "/events/shop/pointTransaction",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
          {
            heading: "title.shopCategory",
            route: "/events/shop/category",
            svgIcon: "/images/icons/ecommerce/ecm004.svg",
            permissions: ["TenantAdmin", "EventAdmin"],
          },
        ],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}

export default registerMenu;
