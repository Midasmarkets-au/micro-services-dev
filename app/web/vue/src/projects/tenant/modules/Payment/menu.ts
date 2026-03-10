import MenuItem from "@/core/types/MenuItem";

const menuItems: Array<MenuItem> = [
  {
    heading: "title.funding",
    route: "/funding",
    permissions: ["TenantAdmin", "WebFunding", "FundingAdmin", "DepositAdmin"],
    pages: [
      {
        heading: "title.deposit",
        route: "/funding/deposit",
        svgIcon: "/images/icons/finance2/040-savings.svg",
        permissions: [
          "TenantAdmin",
          "WebDeposit",
          "FundingAdmin",
          "DepositAdmin",
        ],
        stat: "AwaitingDepositCount",
      },
      {
        heading: "title.withdraw",
        route: "/funding/withdraw",
        svgIcon: "/images/icons/finance/fin002.svg",
        permissions: [
          "TenantAdmin",
          "WebWithdrawal",
          "FundingAdmin",
          "DepositAdmin",
        ],
        stat: "AwaitingWithdrawalCount",
      },
      {
        heading: "title.transfer",
        route: "/funding/transfer",
        svgIcon: "/images/icons/files/fil023.svg",
        permissions: ["TenantAdmin", "WebTransfer"],
        stat: "AwaitingTransferCount",
      },
      {
        heading: "title.refund",
        route: "/funding/refund",
        svgIcon: "/images/icons/finance/fin004.svg",
        permissions: ["TenantAdmin", "WebRefund"],
        stat: "AwaitingRefundCount",
      },
      {
        heading: "title.exchangeRate",
        route: "/funding/exchange-rate",
        svgIcon: "/images/icons/finance/fin001.svg",
        permissions: ["TenantAdmin", "WebExchangeRate"],
      },
      {
        heading: "title.wallet",
        route: "/funding/wallet",
        svgIcon: "/images/icons/finance/fin008.svg",
        permissions: ["TenantAdmin", "WebWallet"],
      },
      {
        heading: "title.creditAdjust",
        route: "/funding/credit",
        svgIcon: "/images/icons/finance/fin010.svg",
        permissions: ["TenantAdmin", "WebCreditAdjustment"],
      },
      {
        heading: "title.walletAdjust",
        route: "/funding/wallet-adjust",
        svgIcon: "/images/icons/finance/fin007.svg",
        permissions: ["TenantAdmin", "WebServiceFee"],
      },
      // {
      //   heading: "title.service",
      //   route: "/funding/service",
      //   svgIcon: "/images/icons/finance/fin006.svg",
      //   permissions: ["TenantAdmin", "WebPaymentService"],
      // },
      {
        heading: "title.paymentService",
        route: "/funding/payment-service",
        svgIcon: "/images/icons/finance/fin001.svg",
        permissions: ["TenantAdmin", "WebPaymentService"],
      },
      {
        heading: "title.paymentInfos",
        route: "/funding/payment-info",
        svgIcon: "/images/icons/finance/fin001.svg",
        permissions: ["TenantAdmin"],
      },
      {
        heading: "title.crypto",
        route: "/funding/crypto",
        svgIcon: "/images/icons/finance/fin009.svg",
        permissions: ["TenantAdmin"],
      },
    ],
  },
];

function registerMenu(menu) {
  menu.addMenu(menuItems);
}
export default registerMenu;
