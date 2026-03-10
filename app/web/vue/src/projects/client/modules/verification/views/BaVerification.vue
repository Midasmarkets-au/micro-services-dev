<template>
  <div v-if="isLoading">
    <div
      class="d-flex align-items-center justify-content-center"
      style="min-height: 600px"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
  </div>
  <template v-else>
    <StepDiplay />
    <el-form
      ref="ruleFormRef"
      class="pb-10"
      :class="{
        'mt-5 ': currentStepIndex !== 5,
      }"
      :model="formData"
      :rules="rules"
      label-width="auto"
      label-position="top"
      require-asterisk-position="right"
    >
      <div v-if="items.data?.status != 0">
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

      <VStarted v-else-if="currentStepIndex === 0" />
      <VInfo v-else-if="currentStepIndex === 1" />
      <VFinancial v-else-if="currentStepIndex === 2" />
      <VQuiz v-else-if="currentStepIndex === 3" ref="vQuizRef" />
      <VAgreement v-else-if="currentStepIndex === 4" />
      <VDocument v-else-if="currentStepIndex === 5" ref="vDocRef" />

      <div
        v-if="items.data?.status == 0"
        class="d-flex mt-5"
        :class="{
          'justify-content-center gap-5': isMobile,
          'justify-content-end': !isMobile,
        }"
      >
        <button
          type="button"
          class="btn btn-sm btn-light btn-radius me-3 d-flex"
          v-if="currentStepIndex > 0"
          @click="previousStep"
        >
          <span class="svg-icon svg-icon-4 me-1">
            <inline-svg src="/images/icons/arrows/arr063.svg" />
          </span>
          {{ $t("action.back") }}
        </button>

        <button
          class="btn btn-sm btn-radius btn-primary me-3"
          ref="submitButton"
          @click="submit($event)"
        >
          <span
            class="indicator-label d-flex"
            v-if="currentStepIndex + 1 >= totalSteps"
          >
            {{ $t("action.submit") }}
            <span class="svg-icon svg-icon-3 ms-2 me-0">
              <inline-svg src="/images/icons/arrows/arr064.svg" />
            </span>
          </span>

          <span class="indicator-label d-flex" v-else>
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
  </template>
  <FinancialPopup ref="FinancialPopupRef"></FinancialPopup>
</template>

<script lang="ts" setup>
import { ref, onMounted, computed, provide, inject, reactive } from "vue";
import "../assets/css/style.css";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import VStarted from "../components/ba/VerificationStarted.vue";
import VInfo from "../components/ba/VerificationInfo.vue";
import VFinancial from "../components/ba/VerificationFinancial.vue";
import VDocument from "../components/ba/VerificationDocument.vue";
import VQuiz from "../components/ba/VerificationQuiz.vue";
import VAgreement from "../components/ba/VerificationAgreement.vue";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import type { FormInstance } from "element-plus";
import StepDiplay from "../components/ba/StepDiplay.vue";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { Actions } from "@/store/enums/StoreEnums";
import {
  ConfigAccountTypeSelections,
  AccountRoleTypes,
} from "@/core/types/AccountInfos";
import FinancialPopup from "../components/ba/FinancialPopup.vue";

const vQuizRef = ref<any>();
const vDocRef = ref<any>();
const ruleFormRef = ref<FormInstance>();
const formData = ref<any>({});
provide("formData", formData);

const { t } = useI18n();
const router = useRouter();
const referCode = ref("");
const isLoading = inject<any>("isLoading");
const isSubmitting = ref(false);
const currentStepIndex = ref<any>(1);
const totalSteps = ref(5);
const items = ref<any>([]);
const steps = ref<any>([]);
const accountTypeSelections = ref(Array<any>());
const submitButton = ref<HTMLButtonElement | null>(null);
const FinancialPopupRef = ref<HTMLElement | null>(null);
const canMoveForward = ref(true);

const store = useStore();
const config = computed<PublicSetting>(() => store.state.AuthModule.config);

provide("canMoveForward", canMoveForward);
provide("accountTypeSelections", accountTypeSelections);
provide("currentStepIndex", currentStepIndex);
provide("totalSteps", totalSteps);
provide("items", items);
provide("steps", steps);
provide("isSubmitting", isSubmitting);

const submit = async (e) => {
  e.preventDefault();
  if (!ruleFormRef.value) return;
  let isValid = true;
  await ruleFormRef.value.validate(async (valid, fields) => {
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

      // case 2:
      //   await handleFinancial();
      //   break;

      case 2:
        await vQuizRef.value.submit();
        break;

      case 3:
        await handleAgreement();
        break;

      case 4:
        await vDocRef.value.submit();

        break;
    }
    if (canMoveForward.value) {
      currentStepIndex.value++;
      // await submitVerification();
      window.scrollTo(0, 0);
    }
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
  if (
    Object.values(formData.value.questions).some((item) => item === false) &&
    config.value.verificationQuizEnabled
  ) {
    await VerificationService.checkClientAnswer({
      quiz1: formData.value.questions.q1,
      quiz2: formData.value.questions.q2,
      quiz3: formData.value.questions.q3,
      quiz4: formData.value.questions.q4,
      answerw: 4,
    });

    MsgPrompt.warning(t("tip.cantProcess")).then(async () => {
      await router.push({ name: "sign-in" });
      await store.dispatch(Actions.LOGOUT);
    });
    canMoveForward.value = false;
    return;
  }
  formData.value.platform = formData.value.serviceId == 30 ? 30 : 20;
  formData.value.referral = referCode.value;
  await VerificationService.postVerificationStarted(formData.value);
  items.value.data.started = formData.value;
};

const handleInfo = async () => {
  const birthDate = new Date(formData.value.birthday);
  const today = new Date();

  let age = today.getFullYear() - birthDate.getFullYear();
  const monthDiff = today.getMonth() - birthDate.getMonth();

  if (
    monthDiff < 0 ||
    (monthDiff === 0 && today.getDate() < birthDate.getDate())
  ) {
    age--;
  }

  if (age < 18) {
    await VerificationService.checkClientAnswer({
      quiz1: formData.value.birthday,
      quiz2: formData.value.ccc,
      quiz3: formData.value.firstName,
      quiz4: formData.value.lastName,
      answerw: 4,
    });

    MsgPrompt.warning(t("tip.cantProcess")).then(async () => {
      await router.push({ name: "sign-in" });
      await store.dispatch(Actions.LOGOUT);
    });
    canMoveForward.value = false;
    return;
  }

  await VerificationService.postVerificationInfo(formData.value);
  items.value.data.info = formData.value;
};

const handleFinancial = async () => {
  if (
    (formData.value.bg1 == 1 ||
      formData.value.bg2 == 1 ||
      (formData.value.income == 6 && formData.value.investment == 6)) &&
    config.value.verificationQuizEnabled
  ) {
    await VerificationService.checkClientAnswer({
      bg1: formData.value.bg1,
      bg2: formData.value.bg2,
      exp1: formData.value.exp1,
      exp2: formData.value.exp2,
      exp3: formData.value.exp3,
      exp4: formData.value.exp4,
      exp5: formData.value.exp5,
      answerw: 7,
    });

    MsgPrompt.warning(t("tip.cantProcess")).then(async () => {
      await router.push({ name: "sign-in" });
      await store.dispatch(Actions.LOGOUT);
    });
    canMoveForward.value = false;
    return;
  }

  if (
    formData.value.income == 5 ||
    formData.value.income == 6 ||
    formData.value.investment == 5 ||
    formData.value.investment == 6
  ) {
    // await FinancialPopupRef.value.show();
  }

  await VerificationService.postVerificationFinancial(formData.value);
  items.value.data.financial = formData.value;
};

const handleAgreement = async () => {
  await VerificationService.postVerificationAgreement(formData.value);
  items.value.data.agreement = formData.value;
};

const handleDocument = async () => {
  currentStepIndex.value++;
  items.value.data.status = 1;
};

provide("handleDocument", handleDocument);

const calculateCurrentStep = (verificationRes) => {
  currentStepIndex.value = steps.value.reduce(
    (totalSteps: number, stepName: string) =>
      verificationRes.data[stepName] === null ? totalSteps : totalSteps + 1,
    0
  );

  if (verificationRes.data["started"] === null) currentStepIndex.value = 0;

  if (
    currentStepIndex.value === totalSteps.value &&
    items.value.data.status == 0
  ) {
    totalSteps.value = verificationRes.settings.length;
  }
};

const previousStep = () => {
  if (currentStepIndex.value > 0) {
    currentStepIndex.value--;
    window.scrollTo(0, 0);
  }
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    isLoading.value = true;
    const verificationRes = await VerificationService.getVerification();
    verificationRes.settings = verificationRes.settings.filter(
      (item) => item != "financial"
    );
    items.value = verificationRes;
    steps.value = verificationRes.settings;

    await calculateCurrentStep(verificationRes);
  } catch (error) {
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

const fetchReferCodeData = async () => {
  try {
    const referCodeRes = await VerificationService.getMyReferralCode();
    referCode.value = referCodeRes.referCode;

    if (referCode.value !== "") {
      const referInfo = await VerificationService.getReferralInfoByReferralCode(
        referCode.value
      );

      if (referInfo.data.summary.allowAccountTypes?.length > 0) {
        referInfo.data.summary.allowAccountTypes.forEach((acc: any) => {
          accountTypeSelections.value.push({
            label: t(`type.account.${acc.accountType}`),
            value: acc.accountType,
          });
        });
      } else if (referInfo.data.summary.schema?.length > 0) {
        referInfo.data.summary.schema.forEach((acc: any) => {
          accountTypeSelections.value.push({
            label: t(`type.account.${acc.accountType}`),
            value: acc.accountType,
          });
        });
      } else {
        initAccountType();
      }
    } else {
      initAccountType();
    }
  } catch (error) {
    console.error(error);
    initAccountType();
  }
};
const initAccountType = () => {
  accountTypeSelections.value = ConfigAccountTypeSelections.value;
};

onMounted(async () => {
  await fetchReferCodeData();
  await fetchData();
});

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
  currency: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  accountType: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  serviceId: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  leverage: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  questions: {
    q1: [
      {
        required: true,
        message: t("error.fieldIsRequired"),
      },
    ],
    q2: [
      {
        required: true,
        message: t("error.fieldIsRequired"),
      },
    ],
    q3: [
      {
        required: true,
        message: t("error.fieldIsRequired"),
      },
    ],
    q4: [
      {
        required: true,
        message: t("error.fieldIsRequired"),
      },
    ],
  },

  //started
  firstName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  lastName: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  birthday: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  gender: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  citizen: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  address: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  idType: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  idNumber: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  idIssuer: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],

  //financial
  industry: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  position: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  income: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  investment: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  fund: [{ required: true, message: t("error.fieldIsRequired") }],

  otherFunds: [{ required: true, message: t("error.fieldIsRequired") }],

  bg1: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  bg2: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp1: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp1_employer: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp1_position: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp1_remuneration: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp2: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp3: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp4: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp5: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp2_more: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp3_more: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp4_more: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  exp5_more: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
  ],
  consent_1: [
    { required: true, message: t("error.fieldIsRequired"), trigger: "change" },
    {
      validator: (rule, value, callback) => {
        if (value) {
          callback(); // Pass validation if true
        } else {
          callback(new Error(t("error.fieldIsRequired")));
        }
      },
    },
  ],
  consent_2: [{ required: true, message: t("error.fieldIsRequired") }],
});
</script>
