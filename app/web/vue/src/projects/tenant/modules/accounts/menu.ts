import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.account",
    route: "/account",
    permissions: ["TenantAdmin", "Compliance", "WebAccounts", "AccountAdmin"],
    pages: [
      // {
      //   heading: "title.accounts",
      //   route: "/account/index",
      //   svgIcon: "/images/icons/graphs/gra010.svg",
      //   permissions: ["account.view", "account.all"],
      // },
      {
        heading: "title.client",
        route: "/account/clients/all",
        svgIcon: "/images/icons/communication/com014.svg",
        permissions: ["TenantAdmin", "Compliance", "WebClient"],
      },
      {
        heading: "title.accountPrefixes",
        route: "/account/prefixes",
        svgIcon: "/images/icons/general/gen019.svg",
        permissions: ["TenantAdmin"],
      },
      // {
      //   heading: "title.mt4",
      //   route: "/account/clients/mt4",
      //   svgIcon: "/images/icons/brand/mt4.svg",
      //   permissions: ["TenantAdmin", "Compliance", "WebClient"],
      // },
      // {
      //   heading: "title.mt5",
      //   route: "/account/clients/mt5",
      //   svgIcon: "/images/icons/brand/mt5.svg",
      //   permissions: ["TenantAdmin", "Compliance", "WebClient"],
      // },
      {
        heading: "title.application",
        route: "/account/applications",
        svgIcon: "/images/icons/coding/cod002.svg",
        permissions: [
          "TenantAdmin",
          "Compliance",
          "WebApplication",
          "AccountAdmin",
        ],
        stat: "AwaitingAccountApplicationCount",
      },
      {
        heading: "title.passwordActivity",
        route: "/account/activity/password",
        svgIcon: "/images/icons/general/gen047.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        stat: "AwaitingChangePasswordApplicationCount",
      },
      {
        heading: "title.leverageActivity",
        route: "/account/activity/leverage",
        svgIcon: "/images/icons/coding/cod009.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        stat: "AwaitingChangeLeverageApplicationCount",
      },
      {
        heading: "title.wholesaleActivity",
        route: "/account/activity/wholesale",
        svgIcon: "/images/icons/abstract/abs025.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        stat: "AwaitingWholesaleApplicationCount",
      },
      {
        heading: "title.wholesaleReferral",
        route: "/account/activity/referral",
        svgIcon: "/images/icons/abstract/abs025.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
      },
      {
        heading: "title.log",
        route: "/account/log",
        svgIcon: "/images/icons/general/gen013.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "fields.referralCode",
        route: "/account/referral-code",
        svgIcon: "/images/icons/general/gen014.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
      },
      {
        heading: "title.demoAccounts",
        route: "/account/demo-accounts",
        svgIcon: "/images/icons/general/gen013.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
      },
      {
        heading: "title.autoCreateAccounts",
        route: "/account/auto-create-accounts",
        svgIcon: "/images/icons/graphs/gra010.svg",
        permissions: ["TenantAdmin", "Compliance", "WebApplication"],
        stat: "AwaitingAutoCreatedAccountCount",
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
