<template>
  <el-dialog
    v-model="CreateWithdrawalModalRef"
    :width="isMobile ? '100%' : '776'"
    class="rounded-3"
    align-center
    style="max-height: 90vh; overflow: auto"
  >
    <template #header>
      <div class="my-header">
        <h2
          class="fw-bold"
          :class="{
            'fs-1': isMobile,
            'fs-3 mt-2': !isMobile,
          }"
        >
          {{ $t("title.withdraw") }} {{ currentStep }}
        </h2>
      </div>
    </template>
    <div class="d-flex border-top">
      <StepDisplay />
      <div class="content">
        <div
          v-if="isWithdrawLocked"
          class="alert alert-warning mb-4"
          role="alert"
        >
          {{ $t("tip.withdrawalBlockedAfterPasswordChange24h") }}
        </div>
        <div :class="{ 'withdraw-lock': isWithdrawLocked }">
          <StepOne v-if="currentStep == 1" ref="stepOneRef" />
          <StepTwo v-if="currentStep == 2" />
          <StepThree v-if="currentStep == 3" ref="stepThreeRef" />
          <StepFour
            v-if="currentStep == 4"
            ref="stepFourRef"
            @on-created="emits('onCreated')"
          />
          <StepFive v-if="currentStep == 5" />
        </div>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <div v-if="currentStep == 1">
          <el-button
            class="btn btn-sm btn-radius btn-light btn-bordered"
            size="large"
            color="#ffce00"
            plain
            @click="CreateWithdrawalModalRef = false"
            >{{ $t("action.cancel") }}</el-button
          >
          <el-button
            color="#ffce00"
            class="btn btn-sm btn-radius btn-primary"
            @click="handleStep"
            @keyon.enter="handleStep"
            size="large"
            :loading="isLoading"
            :disabled="isWithdrawLocked"
          >
            {{ $t("action.next") }}
          </el-button>
        </div>
        <div v-else-if="currentStep > 1 && currentStep < 5">
          <el-button
            class="btn btn-sm btn-radius btn-light btn-bordered"
            size="large"
            color="#ffce00"
            plain
            @click="currentStep--"
            :disabled="isLoading || isWithdrawLocked"
            >{{ $t("action.back") }}</el-button
          >
          <el-button
            class="btn btn-sm btn-radius btn-primary"
            color="#ffce00"
            @click="handleStep"
            @keyon.enter="handleStep"
            size="large"
            :loading="isLoading"
            :disabled="isWithdrawLocked"
          >
            {{ currentStep == 4 ? $t("action.submit") : $t("action.next") }}
          </el-button>
        </div>
        <div v-else class="d-flex justify-content-end">
          <el-button
            class="btn btn-radius btn-light btn-bordered d-flex align-items-center"
            v-if="currentStep == 5"
            color="#ffce00"
            :loading="isLoading"
            @click="CreateWithdrawalModalRef = false"
          >
            {{ $t("action.close") }}
          </el-button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import { ref, provide, nextTick, computed } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useStore } from "@/store";
import StepOne from "./withdrawModal/StepOne.vue";
import StepTwo from "./withdrawModal/StepTwo.vue";
import StepThree from "./withdrawModal/StepThree.vue";
import StepFour from "./withdrawModal/StepFour.vue";
import StepFive from "./withdrawModal/StepFive.vue";
import { isMobile } from "@/core/config/WindowConfig";
import StepDisplay from "./withdrawModal/StepDisplay.vue";

const emits = defineEmits<{
  (e: "onCreated"): void;
}>();

const { t } = useI18n();
const store = useStore();
const isUSDT = ref(false);
const currentStep = ref(0);
const isLoading = ref(true);
const selectedForm = ref("");
const services = ref<any>([]);
const stepFourRef = ref<any>(null);
const stepThreeRef = ref<any>(null);
const paymentRequireData = ref<any>({});
const CreateWithdrawalModalRef = ref(false);
const stepOneRef = ref<InstanceType<typeof StepOne>>();
const isWithdrawLocked = computed(
  () => store.state.AuthModule.config?.passwordChangedWithinLast24h === true
);

provide("paymentRequireData", paymentRequireData);
provide("selectedForm", selectedForm);
provide("currentStep", currentStep);
provide("isLoading", isLoading);
provide("services", services);

const handleStep = async () => {
  if (isWithdrawLocked.value) {
    MsgPrompt.warning(t("tip.withdrawalBlockedAfterPasswordChange24h"));
    return;
  }

  switch (currentStep.value) {
    case 1:
      if (!paymentRequireData.value.selectedServiceHashId) {
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
      if (stepThreeRef.value.formErrorCheck()) {
        isLoading.value = true;
        currentStep.value += 1;
      }
      break;
    case 4:
      await stepFourRef.value?.submit();
      break;
  }
};

const show = async (_isAccount: any, _data?: any) => {
  currentStep.value = 0;
  isUSDT.value = false;
  selectedForm.value = "";
  paymentRequireData.value = {};
  paymentRequireData.value.request = {};

  paymentRequireData.value.isAccount = _isAccount;
  paymentRequireData.value.detail = _data;

  if (_isAccount) {
    const availableAmount =
      _data.tradeAccount.equityInCents -
      _data.tradeAccount.creditInCents -
      _data.tradeAccount.margin * 100;
    paymentRequireData.value.availableAmount =
      availableAmount <= 0 ? 0 : availableAmount;
  } else {
    paymentRequireData.value.availableAmount = _data.balance;
  }

  await nextTick();

  currentStep.value = 1;
  CreateWithdrawalModalRef.value = true;
};

defineExpose({
  show,
});
</script>
<style lang="scss" scoped>
.step-title {
  font-size: 18px;
  font-family: Lato;
  margin-bottom: 24px;
}

.border-top {
  border-top: 1px solid #e4e6ef;
  border-bottom: 1px solid #e4e6ef;
  color: #000;
}
.content {
  width: 100%;
  padding: 20px 35px;
  overflow-y: auto;
}
.secondary-btn:hover {
  color: #000;
}

.withdraw-lock {
  pointer-events: none;
  opacity: 0.6;
}
</style>
