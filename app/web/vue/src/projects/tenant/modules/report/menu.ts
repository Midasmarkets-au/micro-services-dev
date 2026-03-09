import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.report",
    route: "/report",
    permissions: ["SuperAdmin", "TenantAdmin", "WebReport"],
    pages: [
      // {
      //   heading: "title.report",
      //   route: "/report",
      //   svgIcon: "/images/icons/files/fil017.svg",
      //   permissions: ["TenantAdmin", "WebReportRecord"],
      // },
      {
        heading: "title.equityReport",
        route: "/report/equity",
        svgIcon: "/images/icons/files/fil022.svg",
        permissions: ["SuperAdmin"],
      },
      {
        heading: "title.reportRecord",
        route: "/report/record",
        svgIcon: "/images/icons/files/fil018.svg",
        permissions: ["TenantAdmin", "WebReportRecord"],
      },
      {
        heading: "Client Confirmation",
        route: "/report/confirmation",
        svgIcon: "/images/icons/files/fil020.svg",
        permissions: ["TenantAdmin", "WebClientConfirmation"],
      },
      {
        heading: "title.messageRecord",
        route: "/report/message",
        svgIcon: "/images/icons/files/fil019.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.commentRecord",
        route: "/report/comment",
        svgIcon: "/images/icons/files/fil021.svg",
        permissions: ["TenantAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
