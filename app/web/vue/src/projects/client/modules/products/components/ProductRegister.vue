<template>
  <div v-if="isLoading" style="max-width: 880px; margin: auto">
    <div
      class="d-flex align-items-center justify-content-center"
      style="min-height: 600px"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
  </div>
  <div v-else>
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
      <div
        class="w-100 card verify-card"
        style="max-width: 880px; margin: auto"
        v-if="
          items.status == VerificationStatusTypes.Approved ||
          items.status == VerificationStatusTypes.CodeVerified ||
          items.status == VerificationStatusTypes.Rejected
        "
      >
        <div
          class="mx-auto w-250px h-300px shadow-sm d-flex flex-column justify-content-center align-items-center gap-5 p-5"
        >
          <img src="/images/success.png" alt="" class="w-75" />
          <h2 style="color: #bbe285; font-size: 32px">
            {{ $t("status.success") + "!" }}
          </h2>
          <p class="text-center">
            {{ $t("tip.verificationFinishedWaitApproval") }}
          </p>
        </div>
      </div>

      <ProductPending
        v-else-if="items.status != VerificationStatusTypes.Incomplete"
      />
      <ProductInfo v-else-if="currentStepIndex == 1" />
      <ProductFinancial v-else-if="currentStepIndex == 2" />
      <ProductsAgreement v-else-if="currentStepIndex == 3" />
      <ProductReview v-else-if="currentStepIndex == 4" />
      <div
        v-if="
          currentStepIndex < totalSteps &&
          items.status == VerificationStatusTypes.Incomplete
        "
        class="d-flex mt-5"
        :class="{
          'justify-content-center gap-5': isMobile,
          'justify-content-end': !isMobile,
        }"
      >
        <button
          type="button"
          class="btn btn-lg btn-light-primary me-3"
          v-if="currentStepIndex == 4"
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
    </el-form>
  </div>
</template>
<script setup lang="ts">
import { ref, provide, onMounted, reactive, inject } from "vue";
import ProductsAgreement from "./ProductsAgreement.vue";
import ProductInfo from "../../verification/components/jp/VerificationInfoJp.vue";
import ProductFinancial from "./ProductFinancial.vue";
import ProductReview from "./ProductReview.vue";
import ProductPending from "./ProductPending.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import type { FormInstance } from "element-plus";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { isMobile } from "@/core/config/WindowConfig";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { getItemByValue } from "@/core/helpers/helpers";
import { accountTypesSelection } from "@/core/types/jp/verificationFinancial";
import { VerificationStatusTypes } from "@/core/types/VerificationInfos";
const { t } = useI18n();
const isSubmitting = ref(false);
const currentStepIndex = ref<any>(0);
const totalSteps = ref(2);
const items = ref<any>([]);
const ruleFormRef = ref<FormInstance>();
const formData = ref<any>({});
const isLoading = ref(false);
const submitButton = ref<HTMLButtonElement | null>(null);
const selectedAccountTypes = inject<any>("selectedAccountTypes");
const selectedVerification = inject<any>("selectedVerification");
const verificationData = inject<any>("verificationData");

provide("formData", formData);
provide("currentStepIndex", currentStepIndex);
provide("totalSteps", totalSteps);
provide("items", items);
provide("isSubmitting", isSubmitting);

const submit = async (e) => {
  e.preventDefault();
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  if (submitButton.value) {
    submitButton.value!.disabled = true;
    submitButton.value.setAttribute("data-kt-indicator", "on");
  }
  isSubmitting.value = true;
  try {
    switch (currentStepIndex.value) {
      case 1:
        await handleInfo();
        break;
      case 2:
        await handleFinancial();
        break;
      case 3:
        await handleAgreement();
        break;
      case 4:
        await submitVerification();
        break;
    }
    if (currentStepIndex.value != 4) {
      currentStepIndex.value = 4;
    } else {
      currentStepIndex.value = 5;
    }

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

const handleInfo = async () => {
  items.value.data.info = formData.value;
};

const handleFinancial = async () => {
  items.value.data.financial = formData.value;
};

const handleAgreement = async () => {
  items.value.data.agreement = formData.value;
  items.value.data.financial.accountRole = formData.value.accountRole;
};

const submitVerification = async () => {
  await VerificationService.submitNewVerification();
  await saveNewVerificationData();
  items.value.status = 1;
};

const saveNewVerificationData = async () => {
  await Promise.all([
    // saveStarted(),
    saveInfo(),
    saveFinancial(),
    saveAgreement(),
  ]);

  await saveVerification().then(() => {
    MsgPrompt.success(t("status.success"));
  });
};

const saveStarted = async () => {
  const startedFormData = new FormData();
  var started = items.value.data.started;
  var platform = formData.value.serviceId == 30 ? 30 : 20;

  for (const [key, value] of Object.entries(started)) {
    startedFormData.append(key, value);
  }
  startedFormData.append("platform", platform);
  // await VerificationService.postVerificationStarted(formData.value);
};

const saveInfo = async () => {
  var info = items.value.data.info;
  await VerificationService.postVerificationInfo(info);
};

const saveFinancial = async () => {
  var financial = items.value.data.financial;
  await VerificationService.postVerificationFinancial(financial);
};

const saveAgreement = async () => {
  var agreement = items.value.data.agreement;
  await VerificationService.postVerificationAgreement(agreement);
};

const saveVerification = async () => {
  await VerificationService.submitVerification();
};

// const fetchData = async () => {
//   isLoading.value = true;

//   try {
//     const verificationRes =
//       await VerificationService.getExistingVerifications();

//     const { info, financial, agreement, ...restVerification } =
//       verificationRes.at(-1);

//     const data = {
//       data: {
//         info,
//         financial,
//         agreement,
//       },
//       ...restVerification,
//     };

//     items.value = data;
//     items.value.status = 0;
//     steps.value = 3;
//     totalSteps.value = 5;
//     await processAccTypes();
//     currentStepIndex.value = 3;
//   } catch (error: any) {
//     console.error(error);
//   }

//   isLoading.value = false;
// };

const fetchData = async () => {
  isLoading.value = true;

  try {
    let info, financial, agreement, restVerification;
    if (selectedVerification.value == null) {
      const lastVerification = verificationData.value.at(-1);
      lastVerification.status = 0;
      lastVerification.financial.accountTypes = selectedAccountTypes.value;
      ({ info, financial, agreement, ...restVerification } = lastVerification);
      currentStepIndex.value = 3;
    } else {
      verificationData.value.forEach((item) => {
        console.log("item", item.financial.accountTypes);
        console.log(
          "selectedVerification",
          selectedVerification.value.accountTypes
        );
        if (
          item.financial.accountTypes.includes(
            selectedVerification.value.verificationAccType
          )
        ) {
          ({ info, financial, agreement, ...restVerification } = item);
        }
      });
    }

    const data = {
      data: {
        info,
        financial,
        agreement,
      },
      ...restVerification,
    };

    items.value = data;
    console.log("items", items.value);
    totalSteps.value = 5;
  } catch (error: any) {
    console.error(error);
  }

  isLoading.value = false;
};

const processAccTypes = async () => {
  items.value.data.financial.accountTypes = selectedAccountTypes.value;
};

const previousStep = () => {
  if (currentStepIndex.value > 0) {
    currentStepIndex.value--;
    window.scrollTo(0, 0);
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
  ],
  nativeNameFirst: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "blur" },
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
  branchName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountNumber: [
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

onMounted(async () => {
  await fetchData();
});
</script>
<style>
h1 {
  color: #8a7633;
}
</style>
