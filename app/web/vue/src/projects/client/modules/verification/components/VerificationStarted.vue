<template>
  <div class="w-100 card verify-card" style="max-width: 880px; margin: auto">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("fields.accountInformation") }}
        </h2>
      </div>
      <hr />
      <div class="mb-10">
        <label class="form-label fs-5 fw-bold text-dark required">{{
          $t("fields.currency")
        }}</label>
        <Field
          v-model="verificationStartData.currency"
          tabindex="1"
          class="form-control form-control-lg form-control-solid"
          name="currency"
        >
          <el-select
            v-model="verificationStartData.currency"
            placeholder="Select"
            name="currency"
            size="large"
          >
            <el-option
              v-for="(item, index) in ConfigCurrencySelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </Field>
        <ErrorMessage
          name="currency"
          class="fv-plugins-message-container invalid-feedback"
        ></ErrorMessage>
      </div>

      <!-- Account Type Start -->
      <div class="mb-10" v-if="siteIdCheck">
        <label class="form-label fw-bold text-dark fs-6 required">{{
          $t("fields.accountType")
        }}</label>
        <div class="row">
          <div
            class="col-lg-3 mb-2"
            v-for="typeItem in props.accountTypeSelections"
            :key="'type_' + typeItem.value"
          >
            <Field
              v-model="verificationStartData.accountType"
              type="radio"
              class="btn-check"
              name="accountType"
              :value="typeItem.value"
              :id="'kt_create_account_form_account_type_' + typeItem.label"
            />

            <label
              class="btn btn-outline btn-outline-default d-flex align-items-center text-dark"
              :for="'kt_create_account_form_account_type_' + typeItem.label"
            >
              <span class="d-block fw-semobold text-start">
                <span class="fw-bold d-block fs-4">
                  {{ typeItem.label }}
                </span>
              </span>
            </label>
            <ErrorMessage
              name="accountType"
              class="fv-plugins-message-container invalid-feedback"
            ></ErrorMessage>
          </div>
        </div>
      </div>

      <div class="mb-10" v-else>
        <label class="form-label fs-5 fw-bold text-dark required">{{
          $t("fields.accountType")
        }}</label>
        <Field
          v-model="verificationStartData.accountType"
          tabindex="1"
          class="form-control form-control-lg form-control-solid"
          name="accountType"
        >
          <el-select
            v-model="verificationStartData.accountType"
            placeholder="Select"
            name="accountType"
            size="large"
          >
            <el-option
              v-for="(item, index) in accountTypeSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </Field>
        <ErrorMessage
          name="accountType"
          class="fv-plugins-message-container invalid-feedback"
        ></ErrorMessage>
      </div>
      <!-- Account Type End -->

      <div class="mb-10">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.platform")
        }}</label>
        <Field
          v-model="verificationStartData.serviceId"
          tabindex="3"
          class="form-control form-control-lg form-control-solid"
          name="serviceId"
        >
          <el-select
            v-model="verificationStartData.serviceId"
            name="serviceId"
            size="large"
          >
            <el-option
              v-for="(item, index) in ConfigRealServiceSelections"
              :key="index"
              :value="item.id"
              :label="item.label"
            />
          </el-select>
        </Field>

        <ErrorMessage
          name="serviceId"
          class="fv-plugins-message-container invalid-feedback"
        ></ErrorMessage>
      </div>

      <div class="mb-10" v-show="ConfigLeverageSelections.length > 1">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.leverage")
        }}</label>
        <Field
          v-model="verificationStartData.leverage"
          tabindex="3"
          class="form-control form-control-lg form-control-solid"
          name="leverage"
        >
          <el-select
            v-model="verificationStartData.leverage"
            name="leverage"
            size="large"
          >
            <el-option
              v-for="(item, index) in ConfigLeverageSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </Field>

        <ErrorMessage
          name="leverage"
          class="fv-plugins-message-container invalid-feedback"
        ></ErrorMessage>
      </div>

      <!-- <div class="mb-10" v-if="config.ibEnabled" v-show="siteIdCheck">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.referralCode")
        }}</label>
        <Field
          :value="verificationStartData.referral"
          tabindex="4"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="referral"
          autocomplete="off"
          :disabled="hasReferralCode"
        >
          <el-input
            v-model="verificationStartData.referral"
            name="referral"
            size="large"
            :disabled="hasReferralCode"
          />
        </Field>

        <ErrorMessage
          name="referral"
          class="fv-plugins-message-container invalid-feedback"
        ></ErrorMessage>
      </div> -->
    </div>
  </div>

  <div
    class="w-100 card verify-card mt-5"
    style="max-width: 880px; margin: auto"
  >
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("tip.ansBelowQuestions") }}
        </h2>
      </div>
      <hr />
      <div
        class="mb-5 mt-9"
        v-for="(_, key, index) in verificationStartData.questions"
        :key="index"
      >
        <label class="form-label text-dark fs-6 mb-3">{{
          {
            q1: $t("tip.verificationQ1"),
            q2: $t("tip.verificationQ2"),
            q3: $t("tip.verificationQ3"),
            q4: $t("tip.verificationQ4"),
          }[key]
        }}</label>
        <div class="row">
          <div class="col-6 col-lg-2">
            <Field
              v-model="verificationStartData.questions[key]"
              type="radio"
              class="btn-check"
              :name="key"
              :value="true"
              :id="'verification_start_true' + index"
            />
            <label
              class="btn btn-outline btn-outline-default d-flex align-items-center"
              :for="'verification_start_true' + index"
            >
              <span class="d-block fw-semobold text-start">
                <span class="fw-bold d-block fs-4">{{ $t("action.yes") }}</span>
              </span>
            </label>
          </div>

          <!-- ------------------------------------------------------------------------------------------ -->
          <!-- ------------------------------------------------------------------------------------------ -->

          <div class="col-6 col-lg-2">
            <Field
              v-model="verificationStartData.questions[key]"
              type="radio"
              class="btn-check"
              :name="key"
              :value="false"
              :id="'verification_start_false' + index"
            />
            <label
              class="btn btn-outline btn-outline-default d-flex align-items-center"
              :for="'verification_start_false' + index"
            >
              <span class="d-block fw-semobold text-start">
                <span class="fw-bold d-block fs-4">{{ $t("action.no") }}</span>
              </span>
            </label>
          </div>

          <ErrorMessage
            :name="key"
            class="fv-plugins-message-container invalid-feedback"
          ></ErrorMessage>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, watch, computed } from "vue";
import { Field, ErrorMessage } from "vee-validate";
import { useI18n } from "vue-i18n";
import * as Yup from "yup";
import { useForm } from "vee-validate";
import { VerificationStarted } from "../models/VerificationResult";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import VerificationService from "@/projects/client/modules/verification/services/VerificationService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { useRouter } from "vue-router";
import { Actions } from "@/store/enums/StoreEnums";
import { SiteTypes } from "@/core/types/SiteTypes";
import { AccountTypes } from "@/core/types/AccountInfos";
import {
  ServiceTypes,
  ConfigRealServiceSelections,
} from "@/core/types/ServiceTypes";
import { getUserTenancy, tenancies } from "@/core/types/TenantTypes";
const { t } = useI18n();

const hasMt5 = ref(
  ConfigRealServiceSelections.value.some(
    (item) => item.id === ServiceTypes.MetaTrader5
  )
);
const store = useStore();
const config = computed<PublicSetting>(() => store.state.AuthModule.config);
//如果不是AU 就默认美金，standard账户，最大杠杆，不显示referral code
// const siteIdCheck = computed(() => config.value.siteId == SiteTypes.Australia);
const siteIdCheck = computed(() => getUserTenancy() == tenancies.au);
const router = useRouter();
const hasReferralCode = ref(false);
const emits = defineEmits(["saved", "hasError"]);

const props = defineProps<{
  data?: VerificationStarted;
  referCode: string;
  accountTypeSelections: any;
  step: number;
}>();

const isSubmit = ref(false);

const maxLeverage = () => {
  var res = 20;
  for (let i = 0; i < ConfigLeverageSelections.value.length; i++) {
    if (ConfigLeverageSelections.value[i].value > res) {
      res = ConfigLeverageSelections.value[i].value;
    }
  }
  return res;
};

const setAccountType = () => {
  switch (config.value.siteId) {
    case SiteTypes.Vietnam:
      if (
        props.accountTypeSelections.some(
          (item) => item.value === AccountTypes.Vn
        )
      )
        return AccountTypes.Vn;
      else return props.accountTypeSelections[0].value;
    default:
      if (
        props.accountTypeSelections.some(
          (item) => item.value === AccountTypes.Standard
        )
      )
        return AccountTypes.Standard;
      else return props.accountTypeSelections[0].value;
  }
};

const setCurrency = () => {
  if (siteIdCheck.value) return "";
  if (ConfigCurrencySelections.value.some((currency) => currency.value === 840))
    return 840;
  return ConfigCurrencySelections.value[0].value;
};

const verificationStartData = ref<any>(
  props.data ?? {
    currency: setCurrency(),
    accountType: setAccountType(),
    serviceId: hasMt5.value
      ? ServiceTypes.MetaTrader5
      : ConfigRealServiceSelections.value[0].id,
    leverage: siteIdCheck.value ? 20 : maxLeverage(),
    referral: props.referCode,
    questions: {
      q1: null,
      q2: null,
      q3: null,
      q4: null,
    },
  }
);
const startedSchema = Yup.object().shape({
  currency: Yup.string().required().label("Currency"),
  accountType: Yup.string().required().label("Account Type"),
  serviceId: Yup.string().required().label("Platform"),
  leverage: Yup.string().required().label("Leverage"),
  q1: Yup.string().nullable().required().label("Question 1"),
  q2: Yup.string().nullable().required().label("Question 2"),
  q3: Yup.string().nullable().required().label("Question 3"),
  q4: Yup.string().nullable().required().label("Question 4"),
});

const { resetForm, handleSubmit } = useForm({
  validationSchema: startedSchema,
});

function onInvalidSubmit() {
  emits("hasError");
}

const handleStepSubmit = handleSubmit(async () => {
  if (
    Object.values(verificationStartData.value.questions).some(
      (item) => item === false
    ) &&
    config.value.verificationQuizEnabled
  ) {
    await VerificationService.checkClientAnswer({
      quiz1: verificationStartData.value.questions.q1,
      quiz2: verificationStartData.value.questions.q2,
      quiz3: verificationStartData.value.questions.q3,
      quiz4: verificationStartData.value.questions.q4,
      answerw: 4,
    });

    MsgPrompt.warning(t("tip.cantProcess")).then(async () => {
      await router.push({ name: "sign-in" });
      await store.dispatch(Actions.LOGOUT);
    });
  } else {
    submitForm();
  }
}, onInvalidSubmit);

const submitForm = async () => {
  const selectedService = ConfigRealServiceSelections.value.find(
    (item) => item.id === verificationStartData.value.serviceId
  );
  verificationStartData.value.platform = selectedService?.platform;

  try {
    isSubmit.value = true;
    const res = await VerificationService.postVerificationStarted(
      verificationStartData.value
    );
    verificationStartData.value = res;
    emits("saved", props.step, res); // (function, step, data)
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmit.value = false;
  }
};

onMounted(async () => {
  resetForm();
  hasReferralCode.value = verificationStartData.value.referral !== "";
});

watch(
  () => props.data,
  () => (verificationStartData.value = props.data)
);

defineExpose({
  handleStepSubmit,
});
</script>
