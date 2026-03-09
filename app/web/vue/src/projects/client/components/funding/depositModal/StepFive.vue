<template>
  <div class="d-flex flex-column justify-content-center" style="height: 100%">
    <div v-if="isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>
    <div v-else style="overflow: auto">
      <div class="amount-tip fs-7 mb-7">
        <inline-svg src="/images/icons/general/gen066.svg" />
        <span class="ms-2 me-1">{{
          $t("fields.yourRequestAmount") + " "
        }}</span>
        <BalanceShow
          :currency-id="paymentRequireData.account.currencyId"
          :balance="paymentRequireData.request.amount * 100"
        />
      </div>
      <div v-if="showPaypal">
        <PaypalView
          :form="selectedThirdPartyService.form"
          :currency="CurrencyTypes[paymentRequireData.account.currencyId]"
        />
      </div>

      <div v-else-if="isSuccess">
        <div class="d-flex align-items-center">
          <div class="me-7">
            <img src="/images/walletSuccess.png" alt="" width="41" />
          </div>
          <div>
            <h4 class="mb-2 fs-4 font-medium" style="color: #0a46aa">
              {{ $t("tip.orderCreated") }}
            </h4>
            <div class="fs-8">{{ $t("tip.depositSuccessTip") }}</div>
          </div>
        </div>

        <div v-if="showQrCode">
          <div class="mt-5 fs-7">
            {{ $t("tip.qrCodeNotice") }}
          </div>
          <div class="qr-code mt-5">
            <div class="qrcode" ref="qrCodeUrl"></div>
          </div>
          <div
            class="mt-5 d-flex"
            :class="isMobile ? 'flex-column' : 'flex-row'"
          >
            <div class="fs-4">
              {{ $t("fields.walletAddress") }}:
              {{ qrCodeLink }}
            </div>
            <div class="position-relative">
              <button
                class="btn btn-light btn-primary btn-xs"
                :class="isMobile ? 'mt-3' : 'ms-5'"
                style="white-space: nowrap"
                @click="copy"
              >
                {{ $t("action.copy") }}
              </button>
              <span
                class="tip fs-8 badge badge-light"
                :class="{ 'show-tip-animation': showTip }"
                >{{ $t("tip.copiedToClipboard") }}
              </span>
            </div>
          </div>
          <div class="mt-5 fs-4">
            <span>{{ $t("tip.paymentExpireTime") }} : </span>
            <span class="text-danger"
              >{{ countDown }} {{ $t("tip.minutes") }}</span
            >
          </div>
        </div>

        <div v-if="showInstruction" class="mt-7 mb-3">
          <!-- <div v-html="paymentRequireData.groupInfo.instruction"></div> -->
          <div v-html="sanitizedInstruction"></div>
          <h5 style="color: rgb(187, 187, 187)">
            {{ $t("fields.depositAmount") }}
          </h5>
          <h5 style="color: rgb(34, 34, 34)">
            {{ formatAmount(paymentRequireData.targetAmount) }}
          </h5>
        </div>

        <div v-else class="d-flex flex-column">
          <MethodCard class="mt-7" :item="selectedService" :selectedId="0" />
          <div
            class="mt-5"
            v-if="
              selectedThirdPartyService.action == 'Post' ||
              selectedThirdPartyService.action == 'Redirect'
            "
          >
            <span class="text-gray">{{ $t("tip.pleaseClickTheLink") }}</span>

            <a
              @click="handleThirdPartyPay"
              style="cursor: pointer; color: #0a46aa"
              >{{ $t("tip.clickToBeRedirected") }}</a
            >
          </div>
        </div>
      </div>
      <div v-else>
        <div class="d-flex justify-content-center">
          <MethodCard class="mt-7" :item="selectedService" :selectedId="0" />
        </div>
        <div class="text-danger text-center mt-4">
          {{ selectedService.error }}
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import QRCode from "qrcodejs2";
import { isMobile } from "@/core/config/WindowConfig";
import {
  ref,
  inject,
  onMounted,
  onUnmounted,
  nextTick,
  computed,
  provide,
} from "vue";
import { DepositActions } from "@/core/types/deposit/DepositActions";
import MethodCard from "@/projects/client/modules/wallet/components/MethodCard.vue";
import DOMPurify from "dompurify";
import PaypalView from "./PaypalView.vue";

const qrCodeUrl = ref();
const countDown = ref(0);
const qrCodeLink = ref("");
const showTip = ref(false);
const showQrCode = ref(false);
const selectedService = ref({} as any);
const showPaypal = ref(false);
provide("showPaypal", showPaypal);
const isLoading = inject<any>("isLoading");
const isSuccess = inject<any>("isSuccess");
const showInstruction = inject<any>("showInstruction");
const paymentRequireData = inject<any>("paymentRequireData");
const handleThirdPartyPay = inject<any>("handleThirdPartyPay");
const selectedThirdPartyService = inject<any>("selectedThirdPartyService");
import { CurrencyTypes } from "@/core/types/CurrencyTypes";

const sanitizedInstruction = computed(() => {
  return DOMPurify.sanitize(
    paymentRequireData.value.groupInfo.instruction || ""
  );
});
const copy = () => {
  navigator.clipboard.writeText(qrCodeLink.value);
  showTip.value = true;
  setTimeout(() => {
    showTip.value = false;
  }, 1000);
};

const generateLinkQrCode = () => {
  new QRCode(qrCodeUrl.value, {
    text: qrCodeLink.value,
    width: 128,
    height: 128,
    colorDark: "#000000",
    colorLight: "#ffffff",
    correctLevel: QRCode.CorrectLevel.H,
  });
};

// Function to start the countdown
let timer: number | null = null;
function startCountdown() {
  if (timer) clearInterval(timer); // Clear any existing timer

  timer = setInterval(() => {
    if (countDown.value > 0) {
      countDown.value--;
    } else {
      if (timer !== null) {
        clearInterval(timer); // Clear the timer when countdown reaches 0
      }
    }
  }, 60000); // 60000 milliseconds = 1 minute
}

function formatAmount(amount: number) {
  if (amount == null) return ""; // Handle null or undefined amounts gracefully
  return new Intl.NumberFormat("en-US").format(amount);
}

onMounted(async () => {
  console.log(
    "selectedThirdPartyService: ",
    selectedThirdPartyService.value.action
  );
  console.log(
    "selectedThirdPartyService: ",
    selectedThirdPartyService.value.form
  );
  isLoading.value = true;
  if (selectedThirdPartyService.value.action == DepositActions.PayPal) {
    showPaypal.value = true;

    // isSuccess.value = false;
  }
  if (selectedThirdPartyService.value.action == DepositActions.QrCode) {
    qrCodeLink.value = selectedThirdPartyService.value.textForQrCode;
    countDown.value = selectedThirdPartyService.value.message;
    showQrCode.value = true;
  }

  // used in < MethodCard >
  selectedService.value = {
    description: paymentRequireData.value.group,
    categoryName: paymentRequireData.value.group,
    name: paymentRequireData.value.group,
    error: selectedThirdPartyService.value.error,
    logo: paymentRequireData.value.logo,
  };

  isLoading.value = false;

  await nextTick();

  if (showQrCode.value) {
    generateLinkQrCode();
    startCountdown();
  }
});

onUnmounted(() => {
  if (timer !== null) clearInterval(timer); // Clean up the timer when the component is unmounted
});
</script>

<style lang="scss" scoped>
.qrcode {
  display: inline-block;
  img {
    width: 132px;
    height: 132px;
    background-color: #fff; //设置白色背景色
    padding: 6px; // 利用padding的特性，挤出白边
    box-sizing: border-box;
  }
}
.border-top {
  border-top: 1px solid #e4e6ef;
  border-bottom: 1px solid #e4e6ef;
  color: #000;
}
.content {
  width: 100%;
  padding: 20px 35px;
  height: 500px;
  overflow-y: auto;
}
.secondary-btn:hover {
  color: #000;
}

.tip {
  opacity: 0;
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  transition: all 1s ease-in-out;
  pointer-events: none; /* This prevents the tip from being clickable */
}

.show-tip-animation {
  animation: tip-show 1s forwards;
}

@keyframes tip-show {
  0% {
    opacity: 0;
    bottom: 100%;
  }
  40%,
  60% {
    opacity: 1;
    bottom: 160%;
  }
  100% {
    opacity: 0;
    bottom: 160%;
  }
}
</style>
