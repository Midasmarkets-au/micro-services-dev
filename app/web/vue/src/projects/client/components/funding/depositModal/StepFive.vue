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
        <div v-if="showQrCode">
          <div class="mt-5 fs-7" v-if="!showPaidButton">
            {{ $t("tip.qrCodeNotice") }}
          </div>
          <div class="qr-code mt-5">
            <img
              v-if="qrCodeImageSrc"
              class="base64-qr-image"
              :src="qrCodeImageSrc"
              alt="QR code"
            />
            <div v-else class="qrcode" ref="qrCodeUrl"></div>
          </div>
          <div
            v-if="!qrCodeImageSrc"
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
          <div class="mt-2 fs-4">
            <span>{{ $t("tip.paymentExpireTime") }} : </span>
            <span class="text-danger">{{ countDownText }}</span>
          </div>
          <div v-if="showPaidButton" class="mt-5">
            <button
              class="btn btn-primary"
              :disabled="isPaidSubmitting || isPaidConfirmed || isExpired"
              @click="notifyPaid"
            >
              {{
                isPaidSubmitting
                  ? $t("action.loading")
                  : $t("action.completePayment")
              }}
            </button>
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
import moment, { Moment } from "moment";
import { DepositActions } from "@/core/types/deposit/DepositActions";
import MethodCard from "@/projects/client/modules/wallet/components/MethodCard.vue";
import DOMPurify from "dompurify";
import PaypalView from "./PaypalView.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";
import { processErrorMessage } from "@/core/types/ErrorMessage";

const qrCodeUrl = ref();
const countDown = ref(0);
const isExpired = ref(false);
const qrCodeLink = ref("");
const qrCodeImageSrc = ref("");
const qrPayType = ref("");
const qrTransactionId = ref("");
const isPaidSubmitting = ref(false);
const isPaidConfirmed = ref(false);
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

const getBase64ImageDataUrl = (text: string) => {
  const value = text?.trim();
  if (!value) return "";

  // Already a complete data URL.
  if (/^data:image\/[a-zA-Z0-9.+-]+;base64,/i.test(value)) {
    return value;
  }

  // Raw base64 payloads from backend (without data URL prefix).
  const compactValue = value.replace(/\s/g, "");
  const isBase64Payload = /^[A-Za-z0-9+/]+={0,2}$/.test(compactValue);
  const looksLikeImageBase64 =
    /^(iVBORw0KGgo|\/9j\/|R0lGOD|UklGR)/.test(compactValue) &&
    compactValue.length > 100;
  if (!isBase64Payload || !looksLikeImageBase64) return "";

  let mimeType = "image/png";
  if (compactValue.startsWith("/9j/")) mimeType = "image/jpeg";
  else if (compactValue.startsWith("R0lGOD")) mimeType = "image/gif";
  else if (compactValue.startsWith("UklGR")) mimeType = "image/webp";

  return `data:${mimeType};base64,${compactValue}`;
};

const getStringField = (obj: any, keys: string[]) => {
  if (!obj || typeof obj !== "object") return "";
  for (const key of keys) {
    const value = obj[key];
    if (typeof value === "string" && value.trim()) return value.trim();
  }
  return "";
};

const parseQrMetaFromText = (text: string) => {
  const value = text?.trim();
  if (!value) return;

  // JSON string payload from backend.
  try {
    const parsed = JSON.parse(value);
    const parsedPayType =
      getStringField(parsed, ["paytype", "payType"]).toLowerCase() ||
      getStringField(parsed?.data, ["paytype", "payType"]).toLowerCase();
    const parsedTransactionId =
      getStringField(parsed, ["transactionId", "transactionID"]) ||
      getStringField(parsed?.data, ["transactionId", "transactionID"]);
    if (parsedPayType) qrPayType.value = parsedPayType;
    if (parsedTransactionId) qrTransactionId.value = parsedTransactionId;
    return;
  } catch {
    // Not JSON, continue with pattern detection.
  }

  // Plain text payloads that contain key-value fragments.
  const payTypeMatch = value.match(/paytype\s*[:=]\s*["']?([a-zA-Z0-9_-]+)/i);
  const transactionIdMatch = value.match(
    /transactionid\s*[:=]\s*["']?([a-zA-Z0-9_-]+)/i
  );
  if (payTypeMatch?.[1]) qrPayType.value = payTypeMatch[1].toLowerCase();
  if (transactionIdMatch?.[1]) qrTransactionId.value = transactionIdMatch[1];
};

const extractQrMeta = () => {
  qrPayType.value = "";
  qrTransactionId.value = "";

  const service = selectedThirdPartyService.value;
  if (!service) return;

  const directPayType = getStringField(service, ["paytype", "payType"]);
  const directTransactionId = getStringField(service, [
    "transactionId",
    "transactionID",
  ]);
  if (directPayType) qrPayType.value = directPayType.toLowerCase();
  if (directTransactionId) qrTransactionId.value = directTransactionId;

  parseQrMetaFromText(getStringField(service, ["textForQrCode"]));

  if (!qrPayType.value || !qrTransactionId.value) {
    const form = service.form;
    if (typeof form === "string") {
      parseQrMetaFromText(form);
    } else if (form && typeof form === "object") {
      const formPayType = getStringField(form, ["paytype", "payType"]);
      const formTransactionId = getStringField(form, [
        "transactionId",
        "transactionID",
      ]);
      if (formPayType) qrPayType.value = formPayType.toLowerCase();
      if (formTransactionId) qrTransactionId.value = formTransactionId;
    }
  }
};

const showPaidButton = computed(() => !!qrTransactionId.value);

const notifyPaid = async () => {
  if (isPaidConfirmed.value) return;

  if (!qrTransactionId.value) {
    MsgPrompt.error("transactionId is missing");
    return;
  }

  try {
    isPaidSubmitting.value = true;
    await clientGlobalService.postQrCodePaid(qrTransactionId.value);
    isPaidConfirmed.value = true;
    // MsgPrompt.success("Payment confirmation submitted");
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isPaidSubmitting.value = false;
  }
};

// Countdown driven by `selectedThirdPartyService.message`, which can be either:
//   1. a number (or numeric string) of minutes from now, e.g. 25 / "25";
//   2. an absolute UTC+0 timestamp string, converted to local tz via moment.
let timer: number | null = null;
let expiresAtLocal: Moment | null = null;

const countDownText = computed(() => {
  const total = Math.max(0, countDown.value);
  const hours = Math.floor(total / 3600);
  const minutes = Math.floor((total % 3600) / 60);
  const seconds = total % 60;
  const pad = (n: number) => String(n).padStart(2, "0");
  return hours > 0
    ? `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`
    : `${pad(minutes)}:${pad(seconds)}`;
});

function computeRemainingSeconds() {
  if (!expiresAtLocal) return 0;
  const diff = expiresAtLocal.diff(moment(), "seconds");
  return Math.max(0, diff);
}

function resolveExpiresAt(raw: unknown): Moment | null {
  if (raw == null || raw === "") return null;

  // Case 1: minutes from now (number or numeric string, e.g. 25 / "25").
  if (typeof raw === "number" && Number.isFinite(raw)) {
    return moment().add(raw, "minutes");
  }
  if (typeof raw === "string" && /^\d+(\.\d+)?$/.test(raw.trim())) {
    return moment().add(Number(raw.trim()), "minutes");
  }

  // Case 2: absolute UTC timestamp string; convert to local tz.
  if (typeof raw === "string") {
    const m = moment.utc(raw).local();
    return m.isValid() ? m : null;
  }

  return null;
}

function startCountdown() {
  if (timer !== null) {
    clearInterval(timer);
    timer = null;
  }
  isExpired.value = false;

  expiresAtLocal = resolveExpiresAt(selectedThirdPartyService.value?.message);
  if (!expiresAtLocal) {
    countDown.value = 0;
    return;
  }

  countDown.value = computeRemainingSeconds();
  if (countDown.value <= 0) {
    isExpired.value = true;
    return;
  }

  timer = window.setInterval(() => {
    countDown.value = computeRemainingSeconds();
    if (countDown.value <= 0 && timer !== null) {
      clearInterval(timer);
      timer = null;
      isExpired.value = true;
    }
  }, 1000);
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
    qrCodeImageSrc.value = getBase64ImageDataUrl(qrCodeLink.value);
    extractQrMeta();
    showQrCode.value = true;
  }

  // used in < MethodCard >
  selectedService.value = {
    description: paymentRequireData.value.group,
    categoryName: paymentRequireData.value.group,
    name: paymentRequireData.value.group,
    error: selectedThirdPartyService.value.error
      ? processErrorMessage(selectedThirdPartyService.value.error)
      : "",
    logo: paymentRequireData.value.logo,
  };

  isLoading.value = false;

  await nextTick();

  if (showQrCode.value) {
    if (!qrCodeImageSrc.value) {
      generateLinkQrCode();
    }
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

.base64-qr-image {
  width: 132px;
  height: 132px;
  background-color: #fff;
  padding: 6px;
  box-sizing: border-box;
  object-fit: contain;
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
