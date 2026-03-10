<template>
  <div class="w-100 h-100">
    <button class="btn btn-sm fs-8" @click="requestIStatReport">O</button>
    <div
      id="ws_notify"
      class="toast-container position-fixed t-top-60 end-0 p-3 z-index-3"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { onMounted, inject } from "vue";
import { WSSignalR } from "@/core/plugins/signalr";
import { Actions } from "@/store/enums/StoreEnums";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { ElNotification } from "element-plus";
import Can from "@/core/plugins/ICan";
const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

const store = useStore();

const sounds = {
  deposit: "/audios/deposit.wav",
  withdrawal: "/audios/withdrawal.wav",
  application: "/audios/application.wav",
  verification: "/audios/verification.wav",
  paymentCallback: "/audios/paymentCallback.wav",
  transfer: "/audios/transfer.wav",
  leverage: "/audios/leverage.wav",
  accountAutoCreated: "/audios/accountAutoCreated.wav",
  popupSound: "/audios/popup.wav",
};

const handleSignalMsg = (msg: string) => {
  const { event, id, message } = JSON.parse(msg);
  requestIStatReport();
  ((
    {
      ["__VERIFICATION_SUBMITTED__"]: () => void id,
      ["__DEPOSIT_CREATED__"]: () => new Audio(sounds.deposit).play(),
      ["__WITHDRAWAL_CREATED__"]: () => new Audio(sounds.withdrawal).play(),
      ["__APPLICATION_CREATED__"]: () => new Audio(sounds.application).play(),
      ["__TRANSFER_CREATED__"]: () => new Audio(sounds.transfer).play(),
      ["__LEVERAGE_APPLICATION_CREATED__"]: () =>
        new Audio(sounds.leverage).play(),
      ["__DEPOSIT_CALLBACK_COMPLETED__"]: () =>
        new Audio(sounds.paymentCallback).play(),
      ["__ACCOUNT_AUTO_CREATED__"]: () =>
        new Audio(sounds.accountAutoCreated).play(),
      ["__OFFSET_ACCOUNT_TRADE_OPENED__"]: () => {
        MsgPrompt.info(message);
      },
    }[event] ?? (() => void id)
  )());
};

const handleReportMsg = (msg) => {
  const { report, total } = JSON.parse(msg);
  store.dispatch(Actions.PUT_TENANT_ISTAT, { key: report, value: total });
};

const handlePopup = (msg: string) => {
  const outerObj = JSON.parse(msg);
  const { title, text, level } = outerObj;
  new Audio(sounds.popupSound).play();
  ElNotification({
    title: title,
    dangerouslyUseHTMLString: true,
    message: text,
    type: level,
    duration: 15000,
  });
};

onMounted(() => {
  wsSignalR?.connection?.start().then(() => {
    [
      { name: "ReceiveReport", handler: handleReportMsg },
      { name: "ReceiveEvent", handler: handleSignalMsg },
      { name: "ReceivePopup", handler: handlePopup },
      // { name: "ReceiveViewInfoRequest", handler: handleViewInfoRequest },
    ].forEach((method) =>
      wsSignalR?.connection?.on(method.name, (msg) => method.handler(msg))
    );
    requestIStatReport();
  });
});

const handleViewInfoRequest = (msg) => {
  const outerObj = JSON.parse(msg);
  console.log(outerObj);
};

const requestIStatReport = () =>
  [
    "ProcessingKycCount",
    "AwaitingDepositCount",
    "AwaitingTransferCount",
    "AwaitingApproveKycCount",
    "AwaitingWithdrawalCount",
    "AwaitingVerificationCount",
    "AwaitingAccountApplicationCount",
    "AwaitingWholesaleApplicationCount",
    "AwaitingChangeLeverageApplicationCount",
    "AwaitingChangePasswordApplicationCount",
    "AwaitingAutoCreatedAccountCount",
  ].forEach((element) =>
    wsSignalR?.connection?.invoke("RequestReport", element, "", "")
  );
</script>
<style scoped>
.t-top-60 {
  top: 60px;
}
</style>
