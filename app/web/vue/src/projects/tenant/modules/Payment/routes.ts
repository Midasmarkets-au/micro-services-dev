import BackendLayout from "@/layouts/backend-layout/BackendLayout.vue";
import TransferIndex from "./views/TransferIndex.vue";
import DepositIndex from "./views/DepositIndex.vue";
import PaymentInfo from "./views/PaymentInformation.vue";
import WithdrawIndex from "./views/WithdrawIndex.vue";
import WalletIndex from "./views/WalletIndex.vue";
import PaymentServiceIndex from "./views/PaymentServiceIndex.vue";
import CreditIndex from "./views/CreditIndex.vue";
import ExchangeRate from "./views/ExchangeRate.vue";
import Refund from "./views/RefundIndex.vue";
import WalletAdjust from "./views/WalletAdjust.vue";
import CryptoIndex from "./views/CryptoIndex.vue";
export default (router) => {
  router.addRoute({
    path: "/funding",
    redirect: "/funding/withdraw",
    component: BackendLayout,
    name: "funding",
    children: [
      {
        path: "/funding/withdraw",
        name: "WithdrawIndex",
        component: WithdrawIndex,
        meta: {
          pageTitle: "title.withdraw",
          breadcrumbs: ["title.withdraw"],
          permissions: [
            "TenantAdmin",
            "WebWithdrawal",
            "DepositAdmin",
            "FundingAdmin",
          ],
        },
      },
      {
        path: "/funding/deposit",
        name: "DepositIndex",
        component: DepositIndex,
        props: true,
        meta: {
          pageTitle: "title.deposit",
          breadcrumbs: ["title.deposit"],
          permissions: [
            "TenantAdmin",
            "WebDeposit",
            "DepositAdmin",
            "FundingAdmin",
          ],
        },
      },
      {
        path: "/funding/transfer",
        name: "TransferIndex",
        component: TransferIndex,
        meta: {
          pageTitle: "title.transfer",
          breadcrumbs: ["title.transfer"],
          permissions: ["TenantAdmin", "WebTransfer"],
        },
      },
      {
        path: "/funding/refund",
        name: "RefundIndex",
        component: Refund,
        meta: {
          pageTitle: "title.refund",
          breadcrumbs: ["title.refund"],
          permissions: ["TenantAdmin", "WebRefund"],
        },
      },
      {
        path: "/funding/exchange-rate",
        name: "FundingExchangeRate",
        component: ExchangeRate,
        meta: {
          pageTitle: "title.exchangeRate",
          breadcrumbs: ["title.exchangeRate"],
          permissions: ["TenantAdmin", "WebExchangeRate"],
        },
      },
      {
        path: "/funding/wallet",
        name: "WalletIndex",
        component: WalletIndex,
        meta: {
          pageTitle: "title.wallet",
          breadcrumbs: ["title.wallet"],
          permissions: ["TenantAdmin", "WebWallet"],
        },
      },

      {
        path: "/funding/payment-service",
        name: "PaymentServiceIndex",
        component: PaymentServiceIndex,
        meta: {
          pageTitle: "title.paymentService",
          breadcrumbs: ["title.paymentService"],
          permissions: ["TenantAdmin", "WebService", "WebPaymentService"],
        },
      },
      {
        path: "/funding/credit",
        name: "CreditIndex",
        component: CreditIndex,
        meta: {
          pageTitle: "title.creditAdjust",
          breadcrumbs: ["title.creditAdjust"],
          permissions: ["TenantAdmin", "WebCreditAdjustment"],
        },
      },
      {
        path: "/funding/wallet-adjust",
        name: "WalletAdjust",
        component: WalletAdjust,
        meta: {
          pageTitle: "title.walletAdjust",
          breadcrumbs: ["title.walletAdjust"],
          permissions: ["TenantAdmin", "WebWalletAdjustment"],
        },
      },
      {
        path: "/funding/crypto",
        name: "CryptoIndex",
        component: CryptoIndex,
        meta: {
          pageTitle: "title.crypto",
          breadcrumbs: ["title.crypto"],
          permissions: ["TenantAdmin"],
        },
      },
      {
        path: "/funding/payment-info",
        name: "PaymentInfo",
        component: PaymentInfo,
        meta: {
          pageTitle: "title.paymentInfos",
          breadcrumbs: ["title.paymentInfos"],
          permissions: ["TenantAdmin"],
        },
      },
    ],
  });
};
