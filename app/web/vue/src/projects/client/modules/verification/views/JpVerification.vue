<template>
  <div v-if="isLoading">
    <div
      class="d-flex align-items-center justify-content-center"
      style="min-height: 600px"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
  </div>
  <div v-else>
    <StepDiplay />

    <el-form
      ref="ruleFormRef"
      class="pb-10"
      :class="{
        'mt-5 ': currentStepIndex !== 5,
      }"
      :model="formData"
      :rules="rules"
      :hide-required-asterisk="true"
      label-width="auto"
      label-position="top"
    >
      <div>
        <div
          class="w-100 card verify-card"
          style="max-width: 880px; margin: auto"
          v-if="
            items.data?.status == VerificationStatusTypes.Approved ||
            items.data?.status == VerificationStatusTypes.CodeVerified ||
            items.data?.status == VerificationStatusTypes.Rejected
          "
        >
          <!-- <div
            class="mx-auto w-250px h-300px shadow-sm d-flex flex-column justify-content-center align-items-center gap-5 p-5"
          >
            <img src="/images/success.png" alt="" class="w-75" />
            <h2 style="color: #bbe285; font-size: 32px">
              {{ $t("status.success") + "!" }}
            </h2>
            <p class="text-center">
              {{ $t("tip.verificationFinishedWaitApproval") }}
            </p>
          </div> -->
          <div class="w-100 card mt-6">
            <div
              class="mx-auto h-500px d-flex flex-column justify-content-center align-items-center gap-5 p-5"
            >
              <img src="/images/success.svg" alt="" />
              <h2 style="color: #0a46aa; font-size: 32px">
                {{ $t("status.success") + "!" }}
              </h2>
              <p class="text-center">
                {{ $t("tip.verificationFinishedWaitApproval") }}
              </p>
            </div>
          </div>
        </div>
        <VPending
          v-else-if="items.data?.status != VerificationStatusTypes.Incomplete"
        />
        <!-- <VStarted v-if="currentStepIndex === 0" /> -->
        <VInfo v-else-if="currentStepIndex === 1" />
        <VFinancial v-else-if="currentStepIndex === 2" />
        <VAgreement v-else-if="currentStepIndex === 3" />
        <VReview v-else-if="currentStepIndex === 4" />

        <div
          v-if="
            currentStepIndex < totalSteps &&
            items.data?.status == VerificationStatusTypes.Incomplete
          "
          class="d-flex mt-5"
          :class="{
            'justify-content-center gap-5': isMobile,
            'justify-content-end': !isMobile,
          }"
        >
          <button
            type="button"
            class="btn btn-sm btn-light btn-radius me-3 d-flex"
            v-if="currentStepIndex > 1"
            @click="previousStep"
          >
            <span class="svg-icon svg-icon-4 me-1">
              <inline-svg src="/images/icons/arrows/arr063.svg" />
            </span>
            {{ $t("action.back") }}
          </button>

          <button
            class="btn btn-lg btn-primary me-3"
            ref="submitButton"
            @click="submit($event)"
          >
            <span
              class="indicator-label"
              v-if="currentStepIndex === totalSteps - 1"
            >
              {{ $t("action.submit") }}
              <span class="svg-icon svg-icon-3 ms-2 me-0">
                <inline-svg src="/images/icons/arrows/arr064.svg" />
              </span>
            </span>

            <span class="indicator-label" v-else>
              {{ $t("action.next") }}
              <span class="svg-icon svg-icon-3 ms-2 me-0">
                <inline-svg src="/images/icons/arrows/arr064.svg" />
              </span>
            </span>

            <span class="indicator-progress">
              {{ $t("action.pleaseWait") }}
              <span
                class="spinner-border spinner-border-sm align-middle ms-2"
              ></span>
            </span>
          </button>
        </div>
      </div>
    </el-form>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, provide, onMounted, reactive } from "vue";
import StepDiplay from "../components/jp/StepDiplay.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import VStarted from "../components/jp/VerificationStartedJp.vue";
import VInfo from "../components/jp/VerificationInfoJp.vue";
import VFinancial from "../components/jp/VerificationFinancialJp.vue";
import VAgreement from "../components/jp/VerificationAgreementJp.vue";
import VReview from "../components/jp/VerificationReviewJp.vue";
import VPending from "../components/jp/VerificationPending.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import type { FormInstance } from "element-plus";
import { isMobile } from "@/core/config/WindowConfig";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
import GlobalService from "@/projects/client/services/ClientGlobalService";

const ruleFormRef = ref<FormInstance>();
const formData = ref<any>({});

provide("formData", formData);

const { t } = useI18n();
const router = useRouter();
const isLoading = inject<any>("isLoading");
const isSubmitting = ref(false);
const currentStepIndex = ref<any>(1);
const totalSteps = ref(5);
const items = ref<any>([]);
const steps = ref<any>([]);
const submitButton = ref<HTMLButtonElement | null>(null);

provide("currentStepIndex", currentStepIndex);
provide("totalSteps", totalSteps);
provide("items", items);
provide("steps", steps);
provide("isSubmitting", isSubmitting);

const submit = async (e) => {
  e.preventDefault();
  if (!ruleFormRef.value) return;
  let isValid = true;

  await ruleFormRef.value.validate((valid, fields) => {
    if (!valid) {
      isValid = false;
      scrollToFirstErrorFields();
    }
  });

  if (!isValid) return;

  if (submitButton.value) {
    submitButton.value!.disabled = true;
    submitButton.value.setAttribute("data-kt-indicator", "on");
  }
  isSubmitting.value = true;
  try {
    switch (currentStepIndex.value) {
      case 0:
        await handleStarted();
        break;

      case 1:
        await handleInfo();
        break;

      case 2:
        await handleFinancial();
        break;

      case 3:
        await handleAgreement();
        break;
    }
    currentStepIndex.value++;
    await submitVerification();
    window.scrollTo(0, 0);
  } catch (error: any) {
    console.error(error);
    MsgPrompt.error(error);
  }

  if (submitButton.value) {
    submitButton.value.disabled = false;
    submitButton.value.removeAttribute("data-kt-indicator");
  }
  isSubmitting.value = false;
};

const handleStarted = async () => {
  formData.value.platform = formData.value.serviceId == 30 ? 30 : 20;
  await VerificationService.postVerificationStarted(formData.value);
  items.value.data.started = formData.value;
};

const handleInfo = async () => {
  await VerificationService.postVerificationInfo(formData.value);
  items.value.data.info = formData.value;
};

const handleFinancial = async () => {
  await VerificationService.postVerificationFinancial(formData.value);
  items.value.data.financial = formData.value;
};

const handleAgreement = async () => {
  await VerificationService.postVerificationAgreement(formData.value);
  items.value.data.agreement = formData.value;
};

const submitVerification = async () => {
  if (currentStepIndex.value === totalSteps.value) {
    await VerificationService.submitVerification();
    await createBankInfo()
      .then(() => {
        MsgPrompt.success(t("tip.verificationFormSuccess"));
        items.value.data.status = 1;
      })
      .catch(() => MsgPrompt.error(t("tip.fail")));
  }
};

const createBankInfo = async () => {
  var bankInfo = {
    holder: items.value.data.financial.accountHolderFullWidth,
    bankName: items.value.data.financial.financialInstiutionName,
    branchName: items.value.data.financial.branchName,
    swiftCode: items.value.data.financial.branchCode,
    accountNo: items.value.data.financial.accountNumber,
    confirmAccountNo: items.value.data.financial.accountNumber,
    bankCountry: items.value.data.info.citizen,
    state: items.value.data.info.region,
    city: items.value.data.info.village,
  };

  await GlobalService.createUserPaymentInfo({
    paymentPlatform: 100, // Bank
    name: items.value.data.financial.financialInstiutionName,
    info: bankInfo,
  });
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const verificationRes = await VerificationService.getVerification();
    items.value = verificationRes;
    steps.value = verificationRes.settings;
    await calculateCurrentStep(verificationRes);
    totalSteps.value = verificationRes.settings.length;
  } catch (error: any) {
    console.error(error);
    if (error.response.data.message == "__VERIFICATION_DISABLED__") {
      MsgPrompt.error(t("error.__VERIFICATION_DISABLED__")).then(async () => {
        await router.push({ name: "dashboard" });
      });
    } else {
      MsgPrompt.error(error).then(async () => {
        await router.push({ name: "dashboard" });
      });
    }
  }
  isLoading.value = false;
};

const previousStep = () => {
  if (currentStepIndex.value > 0) {
    currentStepIndex.value--;
    window.scrollTo(0, 0);
  }
};

const calculateCurrentStep = async (verificationRes) => {
  currentStepIndex.value = steps.value.reduce(
    (totalSteps: number, stepName: string) =>
      verificationRes.data[stepName] === null ? totalSteps : totalSteps + 1,
    0
  );

  if (verificationRes.data["info"] === null) {
    currentStepIndex.value = 1;
  } else {
    // 加一 因为start 是空的
    currentStepIndex.value++;
  }

  if (
    currentStepIndex.value === totalSteps.value &&
    items.value.data.status == 0
  ) {
    currentStepIndex.value = totalSteps.value - 1;
  }
  if (items.value.data.status == 1) {
    currentStepIndex.value = totalSteps.value;
  }
  console.log("currentStepIndex", currentStepIndex.value);
};

const scrollToFirstErrorFields = () => {
  // only works in EL-FORM ~!

  const errorFields = document.querySelectorAll(".el-form-item.is-error");
  if (errorFields && errorFields.length > 0) {
    // Get the first error field
    const firstErrorField = errorFields[0];
    // Scroll to the error field
    firstErrorField.scrollIntoView({
      behavior: "smooth",
      block: "center",
    });

    const input = firstErrorField.querySelector("input, select, textarea");
    if (input) {
      setTimeout(() => {
        input.focus();
      }, 500);
    }
  }
};

const rules = reactive<any>({
  // Started
  leverage: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountType: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  // Info
  firstName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
  ],
  lastName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
  ],
  nativeNameLast: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
    {
      validator: (rule: any, value: string, callback: any) => {
        const japaneseRegex =
          /^[\u3000-\u303F\u3040-\u309F\u30A0-\u30FF\u31F0-\u31FF\u4E00-\u9FAF\uFF65-\uFF9F\s]*$/;
        if (value && !japaneseRegex.test(value)) {
          callback(new Error(t("error.japaneseOnly")));
        } else {
          callback();
        }
      },
      trigger: "blur",
    },
  ],

  nativeNameFirst: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
    {
      validator: (rule: any, value: string, callback: any) => {
        const japaneseRegex =
          /^[\u3000-\u303F\u3040-\u309F\u30A0-\u30FF\u31F0-\u31FF\u4E00-\u9FAF\uFF65-\uFF9F\s]*$/;
        if (value && !japaneseRegex.test(value)) {
          callback(new Error(t("error.japaneseOnly")));
        } else {
          callback();
        }
      },
      trigger: "blur",
    },
  ],
  gender: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  birthday: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  postalCode: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  region: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  village: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  town: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  street: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  buildingNumber: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],

  email: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
    { type: "email", message: t("tip.invalidEmail"), trigger: "blur" },
  ],
  phone: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
  ],
  occupation: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  companyName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  companyPhone: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  citizen: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  otherCitizen: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  countryOfResidence: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  usTax: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  declarationRegardingForeignPeps: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  otherPeps: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  // Financial
  accountRole: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountTypes: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  investorDistinction: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  annualIncome: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  investment: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  investmentFunds: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  mainIncomeSource: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  transactionMotive: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  assetManagementPeriod: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  mainFund: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  fx: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  stockTradingSpot: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  stockTradingCredit: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  indexOption: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  commodityFuturesTrading: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  otherDerivativesTransaction: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  investmentPurpose: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  financialInstiutionName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  financialInstiutionType: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  financialInstitutionCode: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountNumber: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  branchName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  branchCode: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  depositType: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountHolderFullWidth: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountHolderFullWidthKatakana: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  // Agreement
  check_1: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_2: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_3: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_4: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_5: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_6: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_7: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_8: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_9: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_10: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  check_11: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
});

onMounted(() => {
  fetchData();
});
</script>
<style scoped></style>
