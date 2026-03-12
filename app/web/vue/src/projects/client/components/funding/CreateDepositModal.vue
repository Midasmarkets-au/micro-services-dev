<template>
  <el-dialog v-model="dialogRef" width="776" class="rounded-3" align-center>
    <!-- Header -->
    <template #header>
      <div class="my-header">
        <h2
          class="fw-bold fs-3 mt-2"
          :class="{
            'fs-1': isMobile,
          }"
        >
          {{ $t("title.deposit") }}
        </h2>
      </div>
    </template>

    <!-- Steps -->
    <div class="d-flex border-top">
      <StepDisplay />
      <div :class="isMobile ? 'mobile-content' : 'content'">
        <StepOne v-if="currentStep == 1" ref="stepOneRef" />
        <StepTwo v-if="currentStep == 2" ref="stepTwoRef" />
        <StepThree v-if="currentStep == 3" ref="stepThreeRef" />
        <StepFour
          v-if="currentStep == 4"
          ref="stepFourRef"
          @on-created="emits('onCreated')"
        />
        <StepFive v-if="currentStep == 5" />
      </div>
    </div>

    <!-- Buttons -->
    <template #footer>
      <div class="dialog-footer">
        <div v-if="currentStep == 1">
          <el-button
            class="btn btn-bordered btn-light btn-radius btn-sm"
            size="large"
            plain
            @click="dialogRef = false"
            >{{ $t("action.cancel") }}</el-button
          >
          <el-button
            class="btn btn-bordered btn-primary btn-radius btn-sm"
            @click="handleStep"
            size="large"
            :loading="isLoading"
          >
            {{ $t("action.next") }}
          </el-button>
        </div>
        <div v-else-if="currentStep > 1 && currentStep < 5">
          <el-button
            class="btn btn-bordered btn-light btn-radius btn-sm"
            size="large"
            plain
            @click="currentStep--"
            :disabled="isLoading"
            >{{ $t("action.back") }}</el-button
          >
          <el-button
            @click="handleStep"
            @keyon.enter="handleStep"
            size="large"
            class="btn btn-bordered btn-primary btn-radius btn-sm"
            :loading="isLoading"
          >
            {{ $t("action.next") }}
          </el-button>
        </div>
        <div
          v-else-if="
            currentStep == 5 &&
            selectedThirdPartyService.action != DepositActions.PayPal
          "
        >
          <el-button
            size="large"
            class="btn btn-bordered btn-light btn-radius btn-sm"
            @click="dialogRef = false"
            :disabled="isLoading"
            >{{ $t("action.close") }}</el-button
          >
        </div>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import { ref, provide, nextTick } from "vue";
import StepOne from "./depositModal/StepOne.vue";
import StepTwo from "./depositModal/StepTwo.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import StepFour from "./depositModal/StepFour.vue";
import StepFive from "./depositModal/StepFive.vue";
import StepThree from "./depositModal/StepThree.vue";
import { isMobile } from "@/core/config/WindowConfig";
import StepDisplay from "./depositModal/StepDisplay.vue";
import { DepositActions } from "@/core/types/deposit/DepositActions";

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();

const { t } = useI18n();
const currentStep = ref(0);
const isLoading = ref(true);
const dialogRef = ref(false);
const isSuccess = ref(false);
const showInstruction = ref(false);
const paymentRequireData = ref({} as any);
const selectedThirdPartyService = ref({} as any);

const stepOneRef = ref<InstanceType<typeof StepOne>>();
const stepTwoRef = ref<InstanceType<typeof StepTwo>>();
const stepFourRef = ref<InstanceType<typeof StepFour>>();
const stepThreeRef = ref<InstanceType<typeof StepThree>>();

provide("selectedThirdPartyService", selectedThirdPartyService);
provide("paymentRequireData", paymentRequireData);
provide("showInstruction", showInstruction);
provide("currentStep", currentStep);
provide("isLoading", isLoading);
provide("isSuccess", isSuccess);

//function for step 4 and step 5
const handleThirdPartyPay = () => {
  if (selectedThirdPartyService.value.action == "Redirect") {
    const redirectUrl = selectedThirdPartyService.value.redirectUrl;
    if (!redirectUrl) {
      MsgPrompt.error(t("error.__REQUEST_FAIL__"));
      return;
    }
    window.open(redirectUrl, "_blank");
    return;
  } else if (selectedThirdPartyService.value.action == "Post") {
    const form = document.createElement("form");
    form.method = "POST";
    form.action = selectedThirdPartyService.value.endPoint;
    form.target = "_blank";
    form.enctype = "application/x-www-form-urlencoded"; // Added this line

    // if env environment is local
    if (
      process.env.VUE_APP_ENV === "Development" &&
      form.action.includes("deposit-instruction")
    ) {
      form.action = "http://bvi.bcr:8083/deposit-instruction";
    }

    // Also add the header through setAttribute for additional compatibility
    form.setAttribute("enctype", "application/x-www-form-urlencoded");

    for (const key of Object.keys(selectedThirdPartyService.value.form)) {
      const input = document.createElement("input");
      input.type = "hidden";
      input.name = key;

      if (
        form.action.includes("deposit-instruction") &&
        typeof selectedThirdPartyService.value.form[key] === "object"
      ) {
        input.value = JSON.stringify(selectedThirdPartyService.value.form[key]);
      } else {
        input.value = selectedThirdPartyService.value.form[key];
      }

      form.appendChild(input);
    }
    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
  } else {
    console.log("handleThirdPartyPay: ");
    console.log(selectedThirdPartyService.value);
  }
};
provide("handleThirdPartyPay", handleThirdPartyPay);

const handleStep = async () => {
  switch (currentStep.value) {
    case 1:
      if (!paymentRequireData.value.group) {
        MsgPrompt.warning(t("error.pleaseSelectMethod"));
        return;
      }
      isLoading.value = true;
      currentStep.value += 1;
      break;
    case 2:
      isLoading.value = true;
      currentStep.value += 1;
      break;
    case 3:
      await stepThreeRef.value?.nextStep();
      break;
    case 4:
      await stepFourRef.value?.submit();
      break;
  }
};

const show = async (_account?: any) => {
  currentStep.value = 0; //unmount step 1
  isSuccess.value = false;
  showInstruction.value = false;
  paymentRequireData.value = {} as any;
  paymentRequireData.value.request = {};
  selectedThirdPartyService.value = {} as any;
  paymentRequireData.value.account = _account;

  await nextTick();

  currentStep.value = 1;
  dialogRef.value = true;
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
  border-top: 1px solid #f2f4f7;
  border-bottom: 1px solid #f2f4f7;
  color: #000;
}
.content {
  width: 100%;
  padding: 20px 35px;
  height: 500px;
  overflow-y: auto;
}

.mobile-content {
  width: 100%;
  height: 500px;
  overflow-y: auto;
}
.secondary-btn:hover {
  color: #000;
}
</style>
