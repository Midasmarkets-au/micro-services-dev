<template>
  <div class="d-flex flex-column justify-content-center">
    <div class="step-title">
      {{ $t("title.reviewDepositDetail") }}
    </div>

    <div class="overflow-hidden review-wrapper d-flex flex-column">
      <div class="row flex-fill align-items-center">
        <div
          class="d-flex py-4 fields-title"
          :class="{
            'col-6 ps-10': !isMobile,
            'col-6  ps-10': isMobile,
          }"
        >
          {{ $t("action.action") }}
        </div>
        <div
          class="d-flex py-4 fields-title"
          :class="{
            'col-6': !isMobile,
            'col-6 ps-5': isMobile,
          }"
        >
          {{ $t("tip.depositInToTradeAccount") }}
          {{ $t("type.currency." + paymentRequireData.account.currencyId) }}
        </div>
      </div>
      <div class="row flex-fill">
        <div
          class="d-flex py-4 fields-title"
          :class="{
            'col-6 ps-10': !isMobile,
            'col-6  ps-10': isMobile,
          }"
        >
          {{ $t("fields.amount") }} ({{
            $t("type.currency." + paymentRequireData.account.currencyId)
          }})
        </div>
        <div
          class="d-flex py-4 field-amount"
          :class="{
            'col-6': !isMobile,
            'col-6 ps-5': isMobile,
          }"
        >
          {{ paymentRequireData.request.amount }}
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { inject, ref } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import clientGlobalService from "@/projects/client/services/ClientGlobalService";

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();

const { t } = useI18n();
const isLoading = inject<any>("isLoading");
const isSuccess = inject<any>("isSuccess");
const currentStep = inject<any>("currentStep");
const showInstruction = inject<any>("showInstruction");
const paymentRequireData = inject<any>("paymentRequireData");
const handleThirdPartyPay = inject<any>("handleThirdPartyPay");
const selectedThirdPartyService = inject<any>("selectedThirdPartyService");

const submit = async () => {
  isLoading.value = true;

  const currentLocation = window.location.href;
  paymentRequireData.value.request.returnUrl = currentLocation;

  try {
    const response = await clientGlobalService.postAccountDeposit(
      paymentRequireData.value.account.uid,
      {
        hashId: paymentRequireData.value.groupInfo.hashId,
        amount: paymentRequireData.value.request.amount * 100,
        request: paymentRequireData.value.request,
      }
    );

    if (response.isSuccess == false) {
      MsgPrompt.error(t("error.__REQUEST_FAIL__"));
      isLoading.value = false;

      return;
    }

    isSuccess.value = true;
    selectedThirdPartyService.value = response;

    // check if it needs to redirect to third party
    if (response.action == "Post" || response.action == "Redirect") {
      showInstruction.value = false;
      MsgPrompt.success(t("tip.redirectPaymentPage")).then(() => {
        handleThirdPartyPay();
      });
    } else {
      showInstruction.value = true;
      paymentRequireData.value.groupInfo.instruction = response.instruction;
      MsgPrompt.success(t("tip.formSuccessSubmit"));
    }

    currentStep.value += 1;

    emits("onCreated");
  } catch (error: any) {
    if (
      error?.code === "ERR_CANCELED" ||
      error?.message === "Load failed" ||
      error?.message === "Network Error"
    ) {
      isLoading.value = false;
      return;
    }
    await MsgPrompt.error(error);
    isLoading.value = false;
  }
};

defineExpose({
  submit,
});
</script>

<style lang="scss" scoped>
.review-wrapper {
  border-radius: 30px;
  border: 1px solid #e4e6ef;
  overflow: hidden;
  background-image: url("/images/bg/deposit_amount-bg.svg");
  background-size: cover;
  background-repeat: no-repeat;
  height: 176px;
  .fields-title {
    color: #000f32;
    font-weight: 500;
    font-size: 16px;
  }
  .field-amount {
    font-weight: 600;
    font-size: 20px;
    color: #0a46aa;
    line-height: 20px;
  }
  @media (max-width: 768px) {
    width: 100%;
  }
}
.review-wrapper .outline {
  border: 1px solid #e4e6ef;
}
.review-wrapper .title-item {
  color: #0053ad;
  font-size: 14px;
  font-family: Lato;
  background-color: #f5f7fa;
  padding: 16px 24px;

  @media (max-width: 768px) {
    padding: 10px 20px;
  }
}
</style>
