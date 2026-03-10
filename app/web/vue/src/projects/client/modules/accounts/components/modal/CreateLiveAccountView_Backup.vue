<!-- Hank Refactor on 02/01/2024 -->
<template>
  <div class="modal fade" ref="liveAccountCreateRef">
    <div class="modal-dialog modal-dialog-centered mw-750px">
      <div class="modal-content">
        <div class="form fv-plugins-bootstrap5 fv-plugins-framework">
          <!-- ============================================================================= -->
          <!------------------------------------------------------------------- Modal Header -->
          <!-- ============================================================================= -->
          <div class="modal-header">
            <h2
              :class="{
                'fs-1': isMobile,
                'fs-2': !isMobile,
              }"
            >
              {{ $t("action.createTradeAccount") }}
            </h2>
            <div data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <!-- ============================================================================== -->
          <!------------------------------------------------------------------- Modal SideBar -->
          <!-- ============================================================================== -->
          <div class="d-flex align-items-center">
            <div
              class="stepper stepper-pills stepper-column"
              :class="{
                'fs-1': isMobile,
              }"
            >
              <div
                class="stepper-nav p-4 p-lg-7"
                :style="{ 'min-width': isMobile ? '84px' : '230px' }"
              >
                <!--begin::Step 1-->
                <div
                  class="stepper-item"
                  :class="{
                    ['current']: currentStep === 1,
                    ['completed']: currentStep > 1,
                  }"
                >
                  <div class="stepper-wrapper">
                    <div
                      class="stepper-icon stepper-icon-round"
                      :style="{
                        width: isMobile ? '30px' : '40px',
                        height: isMobile ? '30px' : '40px',
                      }"
                    >
                      <i class="stepper-check fas fa-check"></i>
                      <span class="stepper-number">1</span>
                    </div>
                    <div class="stepper-label">
                      <span
                        class="stepper-title"
                        :style="{ 'font-size': isMobile ? '16px' : '14px' }"
                      >
                        {{ $t("title.info") }}
                      </span>
                    </div>
                  </div>
                  <div class="h-40px"></div>
                </div>
                <!--end::Step 1-->

                <!--begin::Step 2-->
                <div
                  class="stepper-item"
                  :class="{
                    ['current']: currentStep === 2,
                    ['completed']: currentStep > 2,
                  }"
                >
                  <div class="stepper-wrapper">
                    <div
                      class="stepper-icon stepper-icon-round"
                      :style="{
                        width: isMobile ? '30px' : '40px',
                        height: isMobile ? '30px' : '40px',
                      }"
                    >
                      <i class="stepper-check fas fa-check"></i>
                      <span class="stepper-number">2</span>
                    </div>
                    <div class="stepper-label">
                      <span
                        class="stepper-title"
                        :style="{ 'font-size': isMobile ? '16px' : '14px' }"
                        >{{ $t("action.review") }}</span
                      >
                    </div>
                  </div>
                </div>
                <!--end::Step 2-->
              </div>
            </div>

            <!-- ===================================================================================== -->
            <!------------------------------------------------------------------ Side Bar - Form 分隔線 -->
            <!-- ===================================================================================== -->

            <form class="form-style">
              <!-- ===================================================================================== -->
              <!--------------------------------------------------------------------------------- STEP 1 -->
              <!-- ===================================================================================== -->
              <div
                v-if="currentStep === 1"
                class="row info-part"
                :class="{
                  ['current']: currentStep === 1,
                }"
              >
                <!-------------------------------------------------------------- Step 1:: SERVER -->
                <div class="col-lg-5">
                  <div class="mb-8">
                    <label class="required mb-5 createAccountTitle">
                      {{ $t("fields.platform") }}
                    </label>

                    <div v-if="availableServices.length != 0" class="row">
                      <div
                        class="col-6 mb-3"
                        v-for="(item, index) in availableServices"
                        :key="index"
                      >
                        <Field
                          v-model="formData.serviceId"
                          tabindex="2"
                          type="radio"
                          class="btn-check"
                          name="serviceId"
                          :value="item.id"
                          :id="'CreativeLiveAccount' + item.label + item.id"
                          @change="serviceIdSelected(index)"
                        />
                        <label
                          class="btn btn-outline btn-outline-dashed btn-outline-default d-flex align-items-center"
                          :class="{
                            ' w-90px h-100px flex-column  justify-content-center':
                              isMobile,
                          }"
                          :for="'CreativeLiveAccount' + item.label + item.id"
                        >
                          <span
                            class="svg-icon svg-icon-3x"
                            :class="{
                              'mx-0 mb-3': isMobile,
                            }"
                          >
                            <inline-svg :src="item.iconPath" />
                          </span>

                          <span class="d-block fw-semobold text-start">
                            <span class="text-dark fw-bold d-block">{{
                              item.label
                            }}</span>
                          </span>
                        </label>
                      </div>
                      <ErrorMessage
                        class="fv-plugins-message-container invalid-feedback"
                        name="serviceId"
                      />
                    </div>
                    <div
                      v-else
                      style="
                        background-color: #feebef;
                        border-radius: 20px;
                        padding: 5px 20px;
                        color: #900000;
                      "
                    >
                      <div>{{ $t("error.NO_PLATFORM") }}</div>
                      <div>{{ $t("error.PLEASE_CONTACT") }}</div>
                    </div>
                  </div>
                </div>

                <div class="col-lg-5 d-flex flex-column">
                  <!-- -------------------------------------------------------------- Step 2: account type -->
                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-2 createAccountTitle">
                      {{ $t("fields.accountType") }}
                    </label>
                    <el-form-item>
                      <Field
                        v-model="formData.accountType"
                        name="accountType"
                        type="text"
                      >
                        <el-select
                          v-model="formData.accountType"
                          name="accountType"
                          type="text"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectAccountType") }}
                          </el-option>
                          <el-option
                            v-for="(
                              { label, value }, index
                            ) in accountTypeSelections"
                            :label="label"
                            :key="index"
                            :value="value"
                          /> </el-select
                      ></Field>
                      <ErrorMessage
                        as="div"
                        class="fv-plugins-message-container invalid-feedback"
                        name="accountType"
                      />
                    </el-form-item>
                  </div>
                  <!-- -------------------------------------------------------------- Step 3: currency -->
                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-3 createAccountTitle">
                      {{ $t("fields.currency") }}
                    </label>
                    <el-form-item>
                      <Field
                        name="currency"
                        type="text"
                        v-model="formData.currencyId"
                      >
                        <el-select
                          as="el-select"
                          v-model="formData.currencyId"
                          name="currency"
                          type="text"
                          :placeholder="$t('tip.selectCurrency')"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectCurrency") }}
                          </el-option>

                          <el-option
                            v-for="(
                              { label, value }, index
                            ) in ConfigCurrencySelections"
                            :label="label"
                            :key="index"
                            :value="value"
                          />
                        </el-select>
                      </Field>
                      <ErrorMessage
                        class="fv-plugins-message-container invalid-feedback"
                        name="currency"
                      />
                    </el-form-item>
                  </div>
                  <!-- -------------------------------------------------------------- Step 4: Leverage -->
                  <div class="fv-row mb-2">
                    <label class="required fw-semobold mb-2 createAccountTitle">
                      {{ $t("fields.leverage") }}
                    </label>
                    <el-form-item>
                      <Field
                        v-model="formData.leverage"
                        name="leverage"
                        type="text"
                      >
                        <el-select
                          v-model="formData.leverage"
                          name="leverage"
                          type="text"
                          :placeholder="$t('tip.selectLeverage')"
                        >
                          <el-option value="" disabled>
                            {{ $t("tip.selectLeverage") }}
                          </el-option>
                          <el-option
                            v-for="(
                              { label, value }, index
                            ) in ConfigLeverageSelections"
                            :label="label"
                            :key="index"
                            :value="value"
                          /> </el-select
                      ></Field>
                      <ErrorMessage
                        as="div"
                        class="fv-plugins-message-container invalid-feedback"
                        name="leverage"
                      />
                    </el-form-item>
                  </div>

                  <!-- -------------------------------------------------------------- Step 5: Referral Code -->
                  <div v-if="user.roles.includes('IB')" class="fv-row mb-2">
                    <label
                      class="required fs-7 fw-semobold mb-2 createAccountTitle"
                    >
                      {{ $t("fields.referralCode") }}
                    </label>
                    <div class="d-flex align-items-center">
                      <Field
                        v-model="formData.referCode"
                        name="referral"
                        style="border: 1px solid #dcdcdc"
                      >
                        <el-input
                          v-model="formData.referCode"
                          name="referral"
                          placeholder="Enter Referral Code"
                        />
                      </Field>
                      <i
                        class="fa-solid fa-rotate-left ms-3"
                        style="cursor: pointer"
                        @click="formData.referCode = defaultReferCode"
                      ></i>
                      <i
                        v-if="!invalidReferCode"
                        class="fa-regular fa-xl fa-circle-check ms-3"
                        style="color: #14a44d"
                      ></i>
                      <i
                        v-else
                        class="fa-regular fa-xl fa-circle-xmark ms-3"
                        style="color: #900000"
                      ></i>
                    </div>

                    <ErrorMessage
                      as="div"
                      class="fv-plugins-message-container invalid-feedback"
                      name="referral"
                    />
                  </div>
                </div>
              </div>
              <!-- ===================================================================================== -->
              <!----------------------------------------------------------------- STEP 1 -> STEP 2 分隔線 -->
              <!-- ===================================================================================== -->
              <div
                v-if="currentStep === totalSteps"
                class="w-100 h-100 d-flex justify-content-center align-items-center"
                :class="{
                  ['current']: currentStep === totalSteps,
                }"
              >
                <div class="overflow-hidden account-review">
                  <div class="row">
                    <div
                      class="d-flex py-4 fields-title"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-8': isMobile,
                      }"
                    >
                      {{ $t("fields.platform") }}
                    </div>
                    <div
                      class="d-flex py-4"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-2': isMobile,
                      }"
                    >
                      {{
                        availableServices.find(
                          (item) => item.id === formData.serviceId
                        )?.label
                      }}
                    </div>
                  </div>

                  <div class="row">
                    <div
                      class="d-flex py-4 fields-title"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-8': isMobile,
                      }"
                    >
                      {{ $t("fields.type") }}
                    </div>
                    <div
                      class="d-flex py-4"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-2': isMobile,
                      }"
                    >
                      <span
                        :class="
                          {
                            [AccountTypes.Alpha]: 'alpha-tag',
                          }[formData.accountType] ?? 'standard-tag'
                        "
                      >
                        {{
                          accountTypeSelections.find(
                            (item) => item.value === formData.accountType
                          )?.label
                        }}</span
                      >
                    </div>
                  </div>

                  <div class="row">
                    <div
                      class="d-flex py-4 fields-title"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-8': isMobile,
                      }"
                    >
                      {{ $t("fields.currency") }}
                    </div>
                    <div
                      class="d-flex py-4"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-2': isMobile,
                      }"
                    >
                      {{
                        ConfigCurrencySelections.find(
                          (item) => item.value === formData.currencyId
                        )?.label
                      }}
                    </div>
                  </div>

                  <div class="row">
                    <div
                      class="d-flex py-4 fields-title"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-8': isMobile,
                      }"
                    >
                      {{ $t("fields.leverage") }}
                    </div>
                    <div
                      class="d-flex py-4"
                      :class="{
                        'col-4 ps-10': !isMobile,
                        'col-6 fs-4 ps-2': isMobile,
                      }"
                    >
                      {{
                        ConfigLeverageSelections.find(
                          (item) => item.value === formData.leverage
                        )?.label
                      }}
                    </div>
                  </div>
                </div>
              </div>
              <!-- ===================================================================================== -->
              <!-------------------------------------------------------------------------- Last Step END -->
              <!-- ===================================================================================== -->
            </form>
          </div>

          <!-- ============================================================================== -->
          <!-------------------------------------------------------------------------- Footer -->
          <!-- ============================================================================== -->
          <div class="modal-footer">
            <div class="d-flex flex-stack">
              <div class="me-2">
                <button
                  v-if="currentStep !== 1"
                  type="button"
                  class="btn btn-lg btn-light me-3"
                  @click="currentStep -= 1"
                >
                  <span class="svg-icon svg-icon-3 me-1">
                    <inline-svg src="/images/icons/arrows/arr063.svg" />
                  </span>
                  {{ $t("action.back") }}
                </button>
              </div>

              <div>
                <button
                  class="btn btn-primary text-uppercase"
                  @click="
                    currentStep < totalSteps
                      ? handleInfoSelectFinished()
                      : submitCreateAccount()
                  "
                  :disabled="isSubmitting || isLoading || !isValidForm"
                >
                  <span v-if="isLoading">
                    {{ $t("action.waiting") }}
                    <span
                      class="spinner-border h-15px w-15px align-middle text-gray-400"
                    ></span>
                  </span>
                  <span v-else
                    >{{
                      currentStep < totalSteps
                        ? $t("action.next")
                        : $t("action.submit")
                    }}
                  </span>
                </button>
              </div>
            </div>
          </div>
          <!-- ================================================================================== -->
          <!-------------------------------------------------------------------------- Footer END -->
          <!-- ================================================================================== -->
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountService from "../../services/AccountService";
import IbService from "@/projects/client/modules/ib/services/IbService";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";

import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { SiteTypes } from "@/core/types/SiteTypes";
import { ref, computed, nextTick, watch, onUnmounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { showModal, hideModal, removeModalBackdrop } from "@/core/helpers/dom";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";

import {
  AccountTypes,
  ConfigAccountTypeSelections,
} from "@/core/types/AccountInfos";

const { t } = useI18n();
const accountTypeSelections = ref({} as any);
const availableServices = ref([] as any);
const currentStep = ref(1);
const defaultReferCode = ref("");
const formData = ref<any>({});
const isLoading = ref(false);
const isValidForm = ref(false);
const isSubmitting = ref(false);
const invalidReferCode = ref(false);
const liveAccountCreateRef = ref<HTMLElement | null>(null);
const store = useStore();
const siteId = computed(() => store.state.AuthModule.config.siteId);
const totalSteps = ref(2);
const user = computed(() => store.state.AuthModule.user);

const emits = defineEmits<{
  (e: "liveAccountCreated"): void;
}>();

const createTradeAccountSchema = Yup.object().shape({
  serviceId: Yup.number().required().label("Platform"),
  currency: Yup.number().required().label("Currency"),
  accountType: Yup.number().required().label("Account type"),
  leverage: Yup.number().required().label("Leverage"),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema: createTradeAccountSchema,
});

// Entry Point ==========================================================================
// ======================================================================================
onUnmounted(() => {
  removeModalBackdrop();
});
const show = async () => {
  showModal(liveAccountCreateRef.value);

  isLoading.value = true;
  isSubmitting.value = false;
  formData.value = {};
  currentStep.value = 1;

  resetForm({ values: {} });
  await nextTick();

  // Get available services
  const systemServices = await AccountService.getServices();
  var systemServiceIds = systemServices.map((item) => item.id);
  availableServices.value = ConfigRealServiceSelections.value.filter((item) =>
    systemServiceIds.includes(item.id)
  );

  // Get Referral Code
  if (user.value.roles.includes("IB")) {
    try {
      const res = await IbService.getIbLinks();
      const links = res.data;
      formData.value.referCode = links[links.length - 1]?.code;
      links.forEach((element) => {
        if (element.isDefault == 1) {
          formData.value.referCode = element.code;
        }
      });
    } catch (e) {
      formData.value.referCode = "";
    }
    defaultReferCode.value = formData.value.referCode;
  } else {
    const res = await ClientGlobalService.getClientReferralCode();
    formData.value.referCode = res.referCode;
  }

  // Get Account Type
  if (formData.value.referCode && formData.value.referCode !== "") {
    await fillAccountTypeSelections();
  } else {
    accountTypeSelections.value = ConfigAccountTypeSelections.value;
  }

  if (siteId.value != SiteTypes.Australia) prefill();
  isLoading.value = false;
};

// Functions ============================================================================
// ======================================================================================
const serviceIdSelected = (_index: any) => {
  formData.value.serviceId = availableServices.value[_index].id;
  formData.value.platform = availableServices.value[_index].platform;
};

const handleInfoSelectFinished = handleSubmit(() => {
  currentStep.value++;
});

const prefill = () => {
  formData.value.serviceId = availableServices.value[0]?.id;
  formData.value.platform = availableServices.value[0]?.platform;
  formData.value.accountType = accountTypeSelections.value[0].value;
  formData.value.currencyId = 840;

  formData.value.leverage =
    ConfigLeverageSelections.value[
      Object.keys(ConfigLeverageSelections.value).length - 1
    ].value;
};

const fillAccountTypeSelections = async () => {
  // If site is AU, return default account type (AU no referral code)
  if (siteId.value == SiteTypes.Australia) {
    accountTypeSelections.value = ConfigAccountTypeSelections.value;
  } else {
    try {
      const res = await ClientGlobalService.getReferralCodeSupplement(
        formData.value.referCode
      );

      invalidReferCode.value = false;

      if (res) {
        const allowAccountTypes =
          res.summary?.schema ??
          res.summary?.allowAccountTypes ??
          res.summary?.Schema;

        accountTypeSelections.value = allowAccountTypes.map((item: any) => ({
          label: t(`type.account.${item.accountType}`),
          value: item.accountType,
        }));

        if (Object.keys(accountTypeSelections.value).length === 0) {
          accountTypeSelections.value = ConfigAccountTypeSelections.value;
        }
      } else {
        accountTypeSelections.value = ConfigAccountTypeSelections.value;
      }
    } catch (error) {
      if (user.value.roles.includes("IB")) invalidReferCode.value = true;
      else accountTypeSelections.value = ConfigAccountTypeSelections.value;
    }
  }

  formData.value.accountType = accountTypeSelections.value[0].value;
};

// Final End Point ======================================================================
// ======================================================================================
// Payload {
//     "accountType": 13,
//     "currencyId": 840,
//     "leverage": 1000,
//     "serviceId": 10,
//     "referCode": "",
//     "platform": 20
// }
// ======================================================================================

const submitCreateAccount = async () => {
  isLoading.value = true;

  try {
    await AccountService.createLiveAccount(formData.value);
    MsgPrompt.success().then(() => {
      emits("liveAccountCreated");
      hideModal(liveAccountCreateRef.value);
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = true;
    isLoading.value = false;
  }
};

// Dynamic Function =======================================================
// ========================================================================
watch(
  formData,
  async () => {
    isValidForm.value = !!(
      formData.value.serviceId &&
      formData.value.currencyId &&
      formData.value.accountType &&
      formData.value.leverage
    );
  },
  {
    deep: true,
  }
);

watch(
  () => formData.value.referCode,
  () => {
    fillAccountTypeSelections();
  }
);

defineExpose({
  show,
});
</script>

<style scoped lang="scss">
.info-part {
  @media (max-width: 768px) {
    label {
      font-size: 16px;
    }
  }
}

.form-style {
  width: 100%;
  height: 400px;
  overflow-y: auto;
  border-left: 1px solid #e4e6ef;
  padding: 30px 30px;
  @media (max-width: 768px) {
    padding: 30px 20px;
  }
}
.fields-title {
  color: #0053ad;
  background-color: #f5f7fa;
  border-right: 1px solid #e4e6ef;
}
.stepper-icon-round {
  border-radius: 100% !important;
}

.createAccountTitle {
  color: #717171;
}

.alpha-tag {
  background: rgba(123, 97, 255, 0.1);
  color: #7b61ff;
  border-radius: 8px;
  padding: 0 8px;
}

.standard-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ff8a00;
  border-radius: 8px;
  padding: 0 8px;
}

.statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-weight: 700;
}

.account-review {
  width: 70%;
  border-radius: 30px;
  border: 1px solid #e4e6ef;
  overflow: hidden;
  margin: auto;

  @media (max-width: 768px) {
    width: 100%;
  }
}
</style>
