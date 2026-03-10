import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.topic",
    route: "/topic",
    permissions: ["TenantAdmin", "WebTopic"],
    pages: [
      {
        heading: "title.notices",
        route: "/topic/notices",
        svgIcon: "/images/icons/general/gen007.svg",
        permissions: ["TenantAdmin", "WebNotice"],
      },
      {
        heading: "title.emailTemplate",
        route: "/topic/email-template",
        svgIcon: "/images/icons/communication/com011.svg",
        permissions: ["SuperAdmin"],
      },
      {
        heading: "title.sendEmailBatch",
        route: "/topic/send-email-batch",
        svgIcon: "/images/icons/communication/com012.svg",
        permissions: ["SuperAdmin"],
      },
      {
        heading: "title.promotion",
        route: "/topic/promotion",
        svgIcon: "/images/icons/communication/com013.svg",
        permissions: ["SuperAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
