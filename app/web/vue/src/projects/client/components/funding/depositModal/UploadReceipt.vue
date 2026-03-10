<template>
  <el-dialog v-model="dialogRef" width="700" class="rounded-3" align-center>
    <template #header>
      <div class="my-header">
        <h2
          class="fw-bold"
          :class="{
            'fs-1': isMobile,
          }"
        >
          {{ $t("title.deposit") + " V2" }}
        </h2>
      </div>
    </template>

    <div v-if="isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>

    <div v-else class="border-top">
      <div v-if="qrCodeExpired">
        <div class="mt-7 d-flex flex-column align-items-center">
          <img class="mb-5" src="/images/walletSuccess.png" alt="" />
          <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
          <div>{{ $t("tip.depositSuccessTip") }}</div>
          <div class="text-danger mt-7 mb-11 fs-4">
            {{ $t("tip.qrCodeExpired") }}
          </div>
        </div>
      </div>
      <!-- <div v-else-if="paymentRequireData.platform == 230">
        <PaypalView />
      </div> -->
      <div v-else-if="showQrCode">
        <div class="mt-7 d-flex flex-column align-items-center">
          <img class="mb-5" src="/images/walletSuccess.png" alt="" />
          <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
          <div>{{ $t("tip.depositSuccessTip") }}</div>
        </div>
        <div class="qr-code mt-5">
          <div class="qrcode" ref="qrCodeUrl"></div>
        </div>
        <div
          class="mt-5 d-flex"
          :class="isMobile ? 'flex-column' : 'flex-row align-items-center'"
        >
          <div class="fs-4">
            {{ $t("fields.walletAddress") }}:
            {{ qrCodeLink }}
          </div>
          <div class="position-relative">
            <button
              class="btn btn-light btn-primary btn-sm"
              :class="isMobile ? 'mt-3' : 'ms-5'"
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

      <div v-else-if="showInstruction" class="mt-7 mb-3">
        <div class="d-flex align-items-center mt-7 mb-7">
          <div class="me-7">
            <img src="/images/walletSuccess.png" alt="" />
          </div>
          <div>
            <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
            <div>{{ $t("tip.depositSuccessTip") }}</div>
          </div>
        </div>
        <div v-html="paymentRequireData.instruction"></div>
      </div>

      <div v-else class="d-flex flex-column align-items-center mb-7">
        <div class="d-flex align-items-center mt-7">
          <div class="me-7">
            <img src="/images/walletSuccess.png" alt="" />
          </div>
          <div>
            <h4 class="mb-2">{{ $t("tip.orderCreated") }}</h4>
            <div>{{ $t("tip.depositSuccessTip") }}</div>
          </div>
        </div>
        <MethodCard class="mt-7" :item="selectedService" :selectedId="0" />
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button
          size="large"
          @click="openUploadModal"
          :disabled="isLoading"
          >{{ $t("action.uploadReceipt") }}</el-button
        >
        <el-button
          size="large"
          color="#ffce00"
          @click="dialogRef = false"
          :disabled="isLoading"
          >{{ $t("action.close") }}</el-button
        >
      </div>
    </template>
  </el-dialog>

  <UploadDepositReceiptModal ref="uploadDepositReceiptRef" />
</template>
<script setup lang="ts">
import QRCode from "qrcodejs2";
import { ref, nextTick } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import UploadDepositReceiptModal from "./UploadDepositReceiptModal.vue";
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import MethodCard from "@/projects/client/modules/wallet/components/MethodCard.vue";
// import PaypalView from "./PaypalView.vue";

const dialogRef = ref(false);
const depositRecordHashId = ref({} as any);
const accountUid = ref(0);
const qrCodeUrl = ref();
const countDown = ref(0);
const qrCodeLink = ref("");
const qrCodeExpired = ref(false);
const showTip = ref(false);
const isLoading = ref(true);
const showQrCode = ref(false);
const showInstruction = ref(false);
const paymentRequireData = ref({} as any);
const selectedService = ref({} as any);
const uploadDepositReceiptRef =
  ref<InstanceType<typeof UploadDepositReceiptModal>>();

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
      clearInterval(timer!); // Clear the timer when countdown reaches 0
    }
  }, 60000); // 60000 milliseconds = 1 minute
}

const openUploadModal = () => {
  uploadDepositReceiptRef.value?.show(
    accountUid.value,
    depositRecordHashId.value
  );
};

const show = async (_item: any, _uid: any) => {
  isLoading.value = true;
  dialogRef.value = true;
  depositRecordHashId.value = _item.hashId;
  accountUid.value = _uid;

  try {
    const res = await WalletService.getDepositGuide(
      _uid,
      depositRecordHashId.value
    );

    if (res.paymentMethodName.includes("Crypto")) {
      if (Object.keys(res.info).length === 0) {
        qrCodeExpired.value = true;
      } else {
        showQrCode.value = true;
        qrCodeExpired.value = false;
        qrCodeLink.value = res.info["address"];
        countDown.value = res.info["remainMinutes"];
      }

      isLoading.value = false;

      await nextTick();

      if (showQrCode.value) {
        generateLinkQrCode();
        startCountdown();
      }
    } else {
      showInstruction.value = res.instruction != "";
      paymentRequireData.value.instruction = res.instruction;
      paymentRequireData.value.platform = res.platform;

      selectedService.value = {
        description: _item.paymentMethodName,
        categoryName: _item.paymentMethodName,
        name: _item.paymentMethodName,
        error: "",
      };
      isLoading.value = false;
    }
  } catch (error) {
    MsgPrompt.error(error);
  }
};

defineExpose({
  show,
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
