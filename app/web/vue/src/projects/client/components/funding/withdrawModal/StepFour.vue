<template>
  <div
    v-if="isLoading"
    class="d-flex align-items-center justify-content-center h-400px"
  >
    <LoadingRing />
  </div>

  <div v-else>
    <div class="amount-tip mb-11">
      <inline-svg src="/images/icons/general/gen066.svg" />
      <span class="ms-2 me-1">{{ $t("tip.amountAvailable") + " " }}</span>
      <BalanceShow
        :currency-id="paymentRequireData.detail.currencyId"
        :balance="paymentRequireData.availableAmount ?? 0"
      />
    </div>
    <div class="step-title">
      {{ $t("title.reviewWithdrawDetail") }}
    </div>
    <div class="review-wrapper d-flex">
      <div class="col-6">
        <div class="title-item">{{ $t("fields.action") }}</div>
        <div class="title-item">
          {{ $t("fields.amount") }} ({{
            $t("type.currency." + paymentRequireData.detail.currencyId)
          }})
        </div>
        <div class="title-item" v-if="paymentRequireData.request.isUSDT">
          {{ $t("title.usdtWalletAddress") }}
        </div>
        <div v-else>
          <div class="title-item">
            {{ $t("title.accountName") }}
          </div>
          <div class="title-item">
            {{ $t("title.accountNumber") }}
          </div>
        </div>
      </div>

      <div class="col-6">
        <div class="content-item">
          {{
            paymentRequireData.isAccount
              ? $t("tip.withdrawFromAccount")
              : $t("tip.withdrawFromWallet")
          }}
          -
          {{ $t("type.currency." + paymentRequireData.detail.currencyId) }}
        </div>
        <div class="content-item field-amount">
          {{ paymentRequireData.request.amount }}
        </div>
        <div class="content-item" v-if="paymentRequireData.request.isUSDT">
          {{ selectedPaymentMethod.info.walletAddress }}
        </div>

        <div v-else>
          <div class="content-item">
            {{ selectedPaymentMethod.info.name }}
            -
            {{ selectedPaymentMethod.info.bankName
            }}<span v-if="selectedPaymentMethod.info.branchName">
              (
              {{ selectedPaymentMethod.info.branchName }}
              )</span
            >
          </div>
          <div class="content-item">
            {{ selectedPaymentMethod.info.accountNo }}
          </div>
        </div>
      </div>
    </div>
    <div v-if="showVerificationCode" class="d-flex mt-9">
      <div class="d-flex flex-column mb-5 fv-row w-100">
        <label class="fs-5 fw-semobold mb-2 required">
          {{ $t("fields.oneTimeCode") }}
        </label>

        <div class="d-flex align-items-center gap-3">
          <input
            type="text"
            class="form-control form-control-solid flex-grow-1"
            :placeholder="$t('fields.enterYourCode')"
            v-model="verificationCode"
          />

          <button
            type="button"
            class="btn btn-primary"
            :disabled="isSendingCode || countdown > 0"
            @click="sendVerificationCode"
            style="min-width: 120px"
          >
            <span v-if="countdown > 0">
              {{ Math.floor(countdown / 60) }}:{{
                String(countdown % 60).padStart(2, "0")
              }}
            </span>
            <span v-else>
              {{ $t("action.sendCode") }}
            </span>
          </button>
        </div>

        <div class="fv-plugins-message-container">
          <p v-if="codeSent" class="text-success mt-2 mb-0">
            {{ $t("tip.codeSentToEmail") }}
          </p>
        </div>
      </div>
    </div>
    <div class="d-flex mt-9" style="color: #e02b1d">
      <div class="me-3">*</div>
      <div>
        <strong>{{ $t("tip.pleaseNote") }}: </strong>
        {{ $t("tip.walletWidthdrawalNote") }}
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, onMounted, onUnmounted, computed } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "@/projects/client/modules/accounts/services/AccountService";
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import i18n from "@/core/plugins/i18n";
import store from "@/store";

const { t } = i18n.global;
const selectedPaymentMethod = ref({} as any);

const paymentRequireData = inject<any>("paymentRequireData");
const currentStep = inject<any>("currentStep");
const isLoading = inject<any>("isLoading");

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();

// 验证码相关
const isSendingCode = ref(false);
const codeSent = ref(false);
const countdown = ref(0);
const verificationCode = ref("");
let countdownTimer: number | null = null;

// 是否显示验证码输入框 - 根据配置决定
const showVerificationCode = computed(() => {
  return true;
  // return (
  //   store.state.AuthModule.config.twoFactorAuthForTransactions?.withdrawal ===
  //   true
  // );
});

// 重置验证码状态
const resetVerificationCode = () => {
  codeSent.value = false;
  countdown.value = 0;
  verificationCode.value = "";
  if (countdownTimer) {
    clearInterval(countdownTimer);
    countdownTimer = null;
  }
};

// 开始倒计时
const startCountdown = (expires: number) => {
  countdown.value = expires;
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
  countdownTimer = setInterval(() => {
    countdown.value--;
    if (countdown.value <= 0) {
      if (countdownTimer) {
        clearInterval(countdownTimer);
        countdownTimer = null;
      }
    }
  }, 1000) as unknown as number;
};

// 发送验证码
const sendVerificationCode = async () => {
  if (countdownTimer) {
    return;
  }
  isSendingCode.value = true;
  try {
    const params = {
      authType: "Withdrawal",
    };
    const res = await WalletService.sendTransferVerificationCode(params);
    codeSent.value = true;
    startCountdown(res.expiresIn);
    MsgPrompt.success(t("tip.codeSentToEmail"));
  } catch (error) {
    console.error("Send verification code error:", error);
    MsgPrompt.error(error);
  } finally {
    isSendingCode.value = false;
  }
};

const submit = async () => {
  // 只有显示验证码时才进行验证码验证
  if (showVerificationCode.value) {
    if (!verificationCode.value || verificationCode.value.trim() === "") {
      MsgPrompt.error(t("tip.verificationCodeRequired"));
      return;
    }
  }
  isLoading.value = true;
  const currentLocation = window.location.href;

  try {
    const requestData = {
      hashId: paymentRequireData.value.selectedServiceHashId,
      amount: paymentRequireData.value.request.amount * 100,
      verificationCode: verificationCode.value,
      request:
        paymentRequireData.value.isUSDT == true
          ? {
              returnUrl: currentLocation,
              walletAddress: selectedPaymentMethod.value.info.walletAddress,
            }
          : {
              ...selectedPaymentMethod.value.info,
              returnUrl: currentLocation,
            },
    };

    if (paymentRequireData.value.isAccount == true) {
      await AccountService.createWithdrawalRequestV2(
        paymentRequireData.value.detail.uid,
        requestData
      );
    } else {
      await AccountService.createWalletWithdrawalRequestV2(
        paymentRequireData.value.detail.hashId,
        requestData
      );
    }

    currentStep.value += 1;
    emits("onCreated");
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  isLoading.value = true;

  selectedPaymentMethod.value = paymentRequireData.value.request.targetAccount;

  isLoading.value = false;
});

// 清理定时器
onUnmounted(() => {
  if (countdownTimer) {
    clearInterval(countdownTimer);
  }
});

defineExpose({
  submit,
});
</script>
<style lang="scss" scoped>
.content {
  width: 100%;
  padding: 20px 35px;
  height: 500px;
  overflow-y: auto;
}

.review-wrapper {
  overflow: hidden;
  background-image: url("/images/bg/deposit_amount-bg.svg");
  background-size: cover;
  background-repeat: no-repeat;
  height: 176px;
  border-radius: 24px;
}
// .review-wrapper .outline-color {
//   border: 1px solid #e4e6ef;
//   background-color: #f5f7fa;
// }
.review-wrapper .title-item {
  font-size: 16px;
  padding: 16px 24px;
  font-weight: 500;
  color: #000f32;
}

.review-wrapper .content-item {
  font-size: 16px;
  padding: 16px 24px;
  font-weight: 400;
}

.field-amount {
  font-weight: 600;
  font-size: 20px;
  color: #0a46aa;
  line-height: 20px;
}
</style>
