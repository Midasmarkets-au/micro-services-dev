import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.user",
    route: "/users",
    permissions: [
      "TenantAdmin",
      "Compliance",
      "KycOfficer",
      "WebUsers",
      "AccountAdmin",
    ],
    pages: [
      {
        heading: "title.user",
        route: "/users/index",
        svgIcon: "/images/icons/communication/com006.svg",
        permissions: ["TenantAdmin", "Compliance", "WebUser"],
      },
      {
        heading: "title.verification",
        route: "/users/verifications",
        svgIcon: "/images/icons/communication/com005.svg",
        permissions: [
          "TenantAdmin",
          "Compliance",
          "WebVerification",
          "AccountAdmin",
        ],
        stat: "AwaitingVerificationCount",
      },
      {
        heading: "title.kyc",
        route: "/users/kyc",
        svgIcon: "/images/icons/ecommerce/ecm009.svg",
        permissions: ["KycOfficer", "WebKyc"],
        stat: "ProcessingKycCount",
      },
      {
        heading: "title.kycFinalize",
        route: "/users/kyc-finalize",
        svgIcon: "/images/icons/ecommerce/ecm008.svg",
        permissions: ["WebKyc"],
        stat: "AwaitingApproveKycCount",
      },
      {
        heading: "title.allUsers",
        route: "/users/all-users",
        svgIcon: "/images/icons/communication/com007.svg",
        permissions: ["SuperAdmin", "WebAllUser"],
      },
      {
        heading: "title.lead",
        route: "/users/user-lead",
        svgIcon: "/images/icons/communication/com008.svg",
        permissions: ["SuperAdmin", "WebAllUser", "TenantAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
