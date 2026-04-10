<template>
  <div class="card-title card-title-noicon font-medium fs-2">
    {{ $t("action.addNewLink") }}
  </div>

  <!-- STEP 1 -->
  <div class="d-flex align-items-center mt-5">
    <div class="dot me-3"></div>
    <span class="step me-3">{{ $t("fields.step1") }}</span>
    <span class="stepContent">{{ $t("tip.nameYourLink") }}</span>
    <ErrorMessage class="errorMessage ms-5" name="salesLinkName" />
  </div>
  <Field
    class="form-control form-control-solid w-300px mt-5"
    :class="isMobile ? 'h-35px' : 'w-300px h-55px'"
    :placeholder="$t('tip.pleaseInput')"
    name="salesLinkName"
    v-model="requestData.name"
  />

  <!-- STEP 2 -->
  <div class="d-flex align-items-center" :class="isMobile ? 'mt-5' : 'mt-15'">
    <div class="dot me-3"></div>
    <span class="step me-3">{{ $t("fields.step2") }}</span>
    <span class="stepContent">{{ $t("action.chooseLanguage") }}</span>
    <ErrorMessage class="errorMessage ms-5" name="salesLinkLanguage" />
  </div>
  <Field
    name="salesLinkLanguage"
    class="form-select form-select-solid mt-5 IB-rebate-link-select"
    as="select"
    v-model="requestData.language"
  >
    <option value="en-us">English</option>
    <option value="zh-cn">Simplify Chinese (简体中文)</option>
    <option value="zh-tw">Traditional Chinese (繁体中文)</option>
    <option value="vi-vn">Vietnamese (Tiếng Việt Nam)</option>
    <option value="th-th">Thai (ภาษาไทย)</option>
    <option value="jp-jp">Japanese (日本語)</option>
    <!-- <option value="mn-mn">Mongolian (Монгол хэл)</option> -->
    <option value="id-id">Indonesian (Bahasa Indonesia)</option>
    <option value="ms-my">Malay (Bahasa Melayu)</option>
  </Field>

  <!-- STEP 3 -->
  <div class="d-flex align-items-center" :class="isMobile ? 'mt-5' : 'mt-15'">
    <div class="dot me-3"></div>
    <span class="step me-3" style="white-space: nowrap">{{
      $t("fields.step3")
    }}</span>
    <span class="stepContent">{{ $t("tip.selectAccountTypeUnderLink") }}</span>
    <ErrorMessage class="errorMessage ms-5" name="salesLinkServiceType" />
  </div>
  <div class="mt-5 ms-2" :class="isMobile ? 'mb-5 fs-7' : 'mb-15'">
    <Field
      v-model="requestData.ServiceType"
      class="form-check-input widget-9-check me-3"
      type="radio"
      name="salesLinkServiceType"
      value="300"
      style="border: 1px solid #ccd3e0"
    />
    <label class="me-9" for="accountTypeIB">{{ $t("title.ib") }}</label>
    <Field
      v-model="requestData.ServiceType"
      class="form-check-input widget-9-check me-3"
      type="radio"
      name="salesLinkServiceType"
      value="400"
      style="border: 1px solid #ccd3e0"
    />
    <label class="me-9" for="accountTypeClient">{{
      $t("fields.client")
    }}</label>
  </div>

  <!-- STEP 4 -->
  <div class="d-flex align-items-center mt-7">
    <div class="dot me-3"></div>
    <span class="step me-3">{{ $t("fields.step4") }}</span>
    <span class="stepContent d-flex"
      >{{ $t("tip.selectAccountTypeTopLeft") }}
      <div v-if="selectAccountError" class="errorMessage ms-5 fs-6">
        {{ $t("error.__MUST_SELECT_ACCOUNT__") }}
      </div>
    </span>
  </div>

  <div class="mt-5">
    <div class="row" style="position: relative">
      <!-- Available Accounts -->
      <div
        class="col-11"
        :class="isMobile ? 'ms-4 mb-4' : 'ms-7 mb-7'"
        v-for="(acc, index) in schemaForm"
        :key="index"
        style="
          border-radius: 10px;
          box-shadow: rgba(0, 0, 0, 0.16) 0px 1px 4px;
          padding: 15px;
        "
        :style="isMobile ? 'padding: 10px' : 'padding: 15px'"
      >
        <div
          v-if="requestData.ServiceType == AccountRoleTypes.IB && acc.selected"
          class="d-flex justify-content-end align-items-center"
        >
          <button
            v-for="(option, index) in acc.defaultRebateOptions"
            :key="index"
            class="btn btn-sm btn-light-primary border-0 me-3"
            :class="acc.selectedDefaultRebateOptions == index ? 'active' : ''"
            @click="setAccountRule(acc.accountType, index)"
          >
            {{
              option.optionName == "alpha"
                ? $t("type.shortAccount.alpha")
                : option.optionName
            }}
          </button>
        </div>
        <div class="d-flex align-items-center">
          <input
            :id="acc.accountType"
            class="form-check-input widget-9-check me-3"
            type="checkbox"
            :name="acc.accountType"
            v-model="acc.selected"
            style="border: 1px solid #ccd3e0"
          />
          <div class="text-center" :class="isMobile ? 'fs-7' : 'fs-4'">
            <label for="rebateStdAccount">{{
              $t("type.account." + acc.accountType)
            }}</label>
          </div>
        </div>

        <div
          v-if="
            requestData.ServiceType == AccountRoleTypes.Client && acc.selected
          "
        >
          <BaseRebatePCForm
            class="mt-3"
            ref="BaseRebatePCFormRef"
            :accountRule="acc"
            @setAccountRule="setAccountRule"
          />
        </div>

        <div
          v-if="requestData.ServiceType == AccountRoleTypes.IB && acc.selected"
        >
          <hr />
          <div class="row">
            <div
              class="col-6 col-lg-2 mb-1"
              v-for="(item, index) in schemaForm[acc.accountType].items"
              :key="index"
            >
              <div>
                {{ $t("type.clientSymbolCategory." + item.cid) }}
              </div>
              <el-input
                class="w-100% h-35px mt-1 mb-1"
                v-model="item.r"
                disabled
              />
            </div>
          </div>

          <div v-if="acc.allowPipOptions.length != 0" class="row mt-7 mb-3">
            <label>{{ t("fields.availablePips") }}</label>
            <el-checkbox-group v-model="acc.allowPips" size="large">
              <el-checkbox
                v-for="(p, index) in acc.allowPipOptions"
                class="mt-3"
                :key="'p_' + index"
                :label="p"
                border
                >{{ t("type.pipOptions." + p) }}
              </el-checkbox>
            </el-checkbox-group>
          </div>
          <div v-if="acc.allowCommissionOptions.length != 0" class="row mb-5">
            <label class="mt-5">{{ t("fields.availableCommission") }}</label>
            <el-checkbox-group v-model="acc.allowCommissions" size="large">
              <el-checkbox
                v-for="(c, index) in acc.allowCommissionOptions"
                class="mt-3"
                :key="'c_' + index"
                :label="c"
                border
                >{{ t("type.commissionOptions." + c) }}</el-checkbox
              >
            </el-checkbox-group>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- STEP 5 -->
  <div class="d-flex align-items-center" :class="isMobile ? 'mt-5' : 'mt-15'">
    <div class="dot me-3"></div>
    <span class="step me-3" style="white-space: nowrap">{{
      $t("fields.step5")
    }}</span>
    <span class="stepContent">{{ $t("title.enableAutoCreateAccount") }}</span>
  </div>
  <div
    class="mt-5 ms-2 d-flex align-items-center"
    :class="isMobile ? 'mb-5 fs-7' : 'mb-15 fs-5'"
  >
    <el-switch
      v-model="requestData.isAutoCreatePaymentMethod"
      size="large"
      class="order-switch"
      inline-prompt
      style="--el-switch-on-color: #000f32; --el-switch-off-color: #fafbfd"
      :active-value="1"
      :inactive-value="0"
    />
    <span class="ms-3 fs-6">{{
      requestData.isAutoCreatePaymentMethod === 1
        ? $t("action.yes")
        : $t("action.no")
    }}</span>
  </div>

  <!--------------------------------------------------------------------->
  <!---------------------------------------------------------- Buttons -->
  <!--------------------------------------------------------------------->
  <button
    ref="submitButtonRef"
    id="kt_modal_agree"
    class="btn btn-primary me-3"
    :class="isMobile ? 'mt-5 mb-5 w-150px btn-sm' : 'mt-15 mb-15 w-200px'"
    @click="generateLink"
  >
    <span class="indicator-label">
      {{ $t("action.submit") }}
    </span>
    <span class="indicator-progress">
      {{ $t("action.pleaseWait") }}

      <span class="spinner-border spinner-border-sm align-middle ms-2"></span>
    </span>
  </button>
</template>

<script setup lang="ts">
import * as Yup from "yup";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import SalesService from "../services/SalesService";
import BaseRebatePCForm from "./form/BaseRebatePCForm.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useI18n } from "vue-i18n";
import { useStore } from "@/store";
import { ref, watch } from "vue";
import { getLanguage } from "@/core/types/LanguageTypes";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { processKeysToCamelCase } from "@/core/services/api.client";
const props = defineProps<{
  saleId?: number;
}>();

const emit = defineEmits(["refresh"]);

const { t } = useI18n();
const store = useStore();
const schemaForm = ref({} as any);
const selectAccountError = ref(false);
const defaultLevelSetting = ref({} as any);
const productCategory = ref(Array<any>());
const availableAccounts = ref<any[]>([]);
const submitButtonRef = ref<null | HTMLButtonElement>(null);
const BaseRebatePCFormRef = ref<InstanceType<typeof BaseRebatePCForm>>();
const projectConfig: PublicSetting = store.state.AuthModule.config;

const requestData = ref({
  name: "",
  language: getLanguage.value,
  ServiceType: undefined,
  isAutoCreatePaymentMethod: 0,
});

const validationSchema = Yup.object().shape({
  salesLinkName: Yup.string().required(t("error.NAME_IS_REQUIRED")),
  salesLinkLanguage: Yup.string().required(t("error.LANGUAGE_IS_REQUIRED")),
  salesLinkServiceType: Yup.string().required(
    t("error.SERVICE_TYPE_IS_REQUIRED")
  ),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const reset = () => {
  requestData.value = {
    name: "",
    language: "en-us",
    ServiceType: undefined,
    isAutoCreatePaymentMethod: 0,
  };

  availableAccounts.value.forEach((accountType) => {
    if (defaultLevelSetting.value[accountType] === undefined) {
      return;
    }
    schemaForm.value[accountType]["selected"] = false;
    schemaForm.value[accountType]["allowPips"] = [];
    schemaForm.value[accountType]["allowCommissions"] = [];
    setAccountRule(accountType, 0);
  });

  selectAccountError.value = false;
  resetForm();
};

// ==============================================================================================================================

const setAccountRule = async (
  _accountType: string,
  _index: number | string
) => {
  const currentIndex = Number(_index);
  var _defaultAccount = defaultLevelSetting.value[_accountType][currentIndex];

  schemaForm.value[_accountType] = {
    ...schemaForm.value[_accountType],
    selectedDefaultRebateOptions: currentIndex,
    allowPipOptions: _defaultAccount.allowPipOptions,
    allowCommissionOptions: _defaultAccount.allowCommissionOptions,
  };

  schemaForm.value[_accountType].items = productCategory.value.map(
    (category) => ({
      cid: category.key,
      r: _defaultAccount.category[category.key] ?? 0,
    })
  );
};

const initializeForm = (accountType) => {
  if (defaultLevelSetting.value[accountType] === undefined) {
    return;
  }

  schemaForm.value[accountType] = {
    accountType,
    selected: false,
    defaultRebateOptions: defaultLevelSetting.value[accountType],
    allowPips: [],
    allowCommissions: [],
  };

  setAccountRule(accountType, 0);
};

const setUpForm = async () => {
  // availableAccounts.value = store.state.AuthModule.config.accountTypeAvailable;
  schemaForm.value = {};

  try {
    const [getCategory, getDefaultLevelSetting, getAvailableAccountTypes] =
      await Promise.all([
        SalesService.getCategory(props.saleId),
        SalesService.getDefaultLevelSetting(props.saleId),
        SalesService.getAvailableAccountTypes(props.saleId),
      ]);

    productCategory.value = getCategory;
    availableAccounts.value = getAvailableAccountTypes;
    defaultLevelSetting.value = processKeysToCamelCase(getDefaultLevelSetting);
  } catch (error) {
    MsgPrompt.error(t("error.oopsErrorAccured"), "5263");
  }

  availableAccounts.value.forEach(initializeForm);

  resetForm();
};

// ==============================================================================================================================

const createAllowAccountRequestForClient = () => {
  if (!BaseRebatePCFormRef.value) return [];
  const formRefs = Array.isArray(BaseRebatePCFormRef.value)
    ? BaseRebatePCFormRef.value
    : [BaseRebatePCFormRef.value];
  return formRefs
    .filter((formRef) => formRef.formCheck())
    .map((formRef) => formRef.collectData());
};

const createAllowAccountRequestForIB = () => {
  return Object.values(schemaForm.value)
    .filter((acc: any) => acc.selected)
    .map((acc: any) => ({
      optionName:
        acc.defaultRebateOptions[acc.selectedDefaultRebateOptions].optionName,
      accountType: acc.accountType,
      items: acc.items,
      allowPips: acc.allowPips,
      allowCommissions: acc.allowCommissions,
      pips: null,
      commission: null,
      percentage: 0,
    }));
};

const generateLink = handleSubmit(async () => {
  if (!submitButtonRef.value) return;
  if (!Object.values(schemaForm.value).some((value: any) => value.selected)) {
    selectAccountError.value = true;
    return;
  }

  submitButtonRef.value.disabled = true;
  submitButtonRef.value.setAttribute("data-kt-indicator", "on");

  try {
    if (requestData.value.ServiceType == AccountRoleTypes.IB) {
      await SalesService.postSalesLinkForIB(
        {
          name: requestData.value.name,
          language: requestData.value.language,
          schema: createAllowAccountRequestForIB(),
          isAutoCreatePaymentMethod:
            requestData.value.isAutoCreatePaymentMethod,
        },
        props.saleId
      );
    } else if (requestData.value.ServiceType == AccountRoleTypes.Client) {
      await SalesService.postSalesLinkForClient(
        {
          name: requestData.value.name,
          language: requestData.value.language,
          allowAccountTypes: createAllowAccountRequestForClient(),
          isAutoCreatePaymentMethod:
            requestData.value.isAutoCreatePaymentMethod,
        },
        props.saleId
      );
    }
    MsgPrompt.success(t("tip.formSuccessSubmit")).then(() => {
      reset();
      emit("refresh");
    });
  } catch (error) {
    MsgPrompt.error(t("error.oopsErrorAccured"), "5687");
  } finally {
    submitButtonRef.value.disabled = false;
    submitButtonRef.value.setAttribute("data-kt-indicator", "off");
  }
});

watch(
  () => props.saleId,
  () => {
    if (projectConfig?.rebateEnabled) {
      setUpForm();
    }
  },
  { immediate: true }
);
</script>

<style scoped lang="scss">
.IB-rebate-link-select {
  padding: 16px 20px;
  width: 300px;
  height: 56px;
}
.dot {
  width: 10px;
  height: 10px;

  border-radius: 100px;
  background: #0a46aa;
}
@media (max-width: 768px) {
  .step {
    font-size: 14px !important;
  }
  .stepContent {
    font-size: 12px !important;
  }
  .form-select {
    font-size: 0.85rem !important;
    padding: 0px 16px;
    height: 35px;
  }

  .form-select option {
    font-size: 1.1rem !important;
  }

  .form-control {
    font-size: 0.85rem !important;
  }
}
</style>
