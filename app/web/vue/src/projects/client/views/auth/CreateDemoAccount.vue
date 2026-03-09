<template>
  <div
    class="authentication-login-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />
    <div
      class="form-container relative"
      :class="isMobile ? ' h-100' : ''"
      style="color: black !important"
    >
      <div
        v-if="!showSuccess"
        id="create-demo-account"
        class="card w-800px"
        style="border-radius: 20px"
        :class="isMobile ? 'overflow-auto h-100' : ''"
      >
        <div class="card-header">
          <div class="card-title">{{ $t("action.createDemoAccount") }}</div>
        </div>
        <div class="card-body">
          <div>
            <div v-if="isLoading">
              <LoadingCentralBox />
            </div>
            <div class="row" v-if="!isLoading">
              <div class="col-lg-12 mb-2">
                <label class="required fs-6 fw-semibold mb-2">
                  {{ $t("fields.platform") }}
                </label>
                <div class="row">
                  <div
                    class="col-6"
                    v-for="(item, index) in demoPlatformSelections"
                    :key="index"
                  >
                    <Field
                      v-model="formData.platform"
                      tabindex="2"
                      type="radio"
                      class="btn-check"
                      name="platform"
                      :value="item.value"
                      :id="'CreativeDemoAccount' + item.label + item.value"
                    />
                    <label
                      class="btn btn-outline btn-outline-dashed btn-outline-default d-flex align-items-center"
                      :class="{
                        ' w-100px h-100px flex-column  justify-content-center':
                          isMobile,
                      }"
                      :for="'CreativeDemoAccount' + item.label + item.value"
                    >
                      <span
                        class="svg-icon svg-icon-2x"
                        :class="{
                          'mx-0 mb-3 svg-icon-2x': isMobile,
                        }"
                      >
                        <inline-svg :src="item.iconPath" />
                      </span>

                      <span class="d-block fw-semibold text-start">
                        <span class="text-dark fw-bold d-block">{{
                          item.label
                        }}</span>
                      </span>
                    </label>
                  </div>
                </div>
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="platform"
                  as="div"
                >
                  {{ $t("tip.requiredField") }}
                </ErrorMessage>
              </div>

              <div class="col-lg-12 mb-2">
                <label class="required fs-6 fw-semibold mb-2">
                  {{ $t("fields.type") }}
                </label>
                <div class="row">
                  <div
                    v-for="(item, index) in accountTypeSelections"
                    :key="index"
                    class="col-lg-6 mb-3"
                  >
                    <Field
                      v-model="formData.accountType"
                      type="radio"
                      class="btn-check"
                      name="accountType"
                      :value="item.value"
                      :id="'CreativeDemoAccount' + item.label + item.value"
                    >
                    </Field>
                    <label
                      class="btn btn-outline btn-outline-dashed btn-outline-default p-2 d-flex align-items-center"
                      :for="'CreativeDemoAccount' + item.label + item.value"
                    >
                      <span class="svg-icon me-5 svg-icon-1">
                        <!--                        <inline-svg :src="item.iconPath" />-->
                      </span>

                      <span class="d-block fw-semibold text-start">
                        <span class="text-dark fw-bold d-block fs-4">
                          {{ item.label }}
                        </span>
                      </span>
                    </label>
                  </div>
                  <ErrorMessage
                    class="fv-plugins-message-container invalid-feedback"
                    name="accountType"
                    as="div"
                  >
                    {{ $t("tip.requiredField") }}
                  </ErrorMessage>
                </div>
              </div>

              <div class="col-lg-6 mb-2">
                <label class="required fs-6 fw-semibold mb-2">
                  {{ $t("fields.amount") }}
                </label>
                <div>
                  <Field v-model="amount" name="amount">
                    <el-select
                      v-model="amount"
                      name="amount"
                      type="text"
                      size="large"
                      class="w-100"
                      :placeholder="$t('fields.select')"
                    >
                      <el-option
                        value=""
                        disabled
                        :label="$t('fields.select')"
                      />
                      <el-option
                        v-for="(item, index) in amountSelections"
                        :label="item.label"
                        :key="index"
                        :value="item.value"
                      />
                    </el-select>
                  </Field>
                  <ErrorMessage
                    class="fv-plugins-message-container invalid-feedback"
                    name="amount"
                    as="div"
                  >
                    {{ $t("tip.requiredField") }}
                  </ErrorMessage>
                </div>
              </div>
              <div class="col-lg-6 mb-2">
                <label class="required fs-6 fw-semibold mb-2">
                  {{ $t("fields.currency") }}
                </label>
                <div>
                  <Field v-model="formData.currencyId" name="currency">
                    <el-select
                      v-model="formData.currencyId"
                      name="currency"
                      type="text"
                      size="large"
                      class="w-100"
                      :placeholder="$t('tip.selectCurrency')"
                    >
                      <el-option
                        value=""
                        disabled
                        :label="$t('tip.selectCurrency')"
                      />
                      <el-option
                        v-for="({ label, value }, index) in currencySelections"
                        :label="label"
                        :key="index"
                        :value="value"
                      />
                    </el-select>
                  </Field>
                  <ErrorMessage
                    class="fv-plugins-message-container invalid-feedback"
                    name="currency"
                    as="div"
                  >
                    {{ $t("tip.requiredField") }}
                  </ErrorMessage>
                </div>
              </div>

              <div class="col-lg-6 mb-2">
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
                      class="w-100"
                      :placeholder="$t('tip.selectLeverage')"
                    >
                      <el-option value="" disabled>
                        {{ $t("tip.selectLeverage") }}
                      </el-option>
                      <el-option
                        v-for="(item, index) in leverageTypeSelections"
                        :label="item + ':1'"
                        :key="index"
                        :value="item"
                      /> </el-select
                  ></Field>
                  <ErrorMessage
                    as="div"
                    class="fv-plugins-message-container invalid-feedback"
                    name="leverage"
                  />
                </el-form-item>
              </div>

              <div class="col-lg-6 mb-2">
                <label
                  class="required fs-6 fw-semibold mb-2"
                  style="min-width: 160px"
                >
                  {{ $t("fields.countryAndRegion") }}
                </label>

                <Field
                  v-model="formData.countryCode"
                  className="mt-1 form-control form-control-lg form-control-solid"
                  name="countryCode"
                  autoComplete="off"
                >
                  <el-select
                    v-model="formData.countryCode"
                    :placeholder="$t('tip.selectCountry')"
                    @change="phoneRegionCode = formData.countryCode"
                    size="large"
                    class="w-100"
                  >
                    <el-option
                      v-for="(item, index) in regionCodes"
                      :key="index"
                      :label="item.name"
                      :value="item.code"
                    >
                    </el-option>
                  </el-select>
                </Field>

                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="countryCode"
                  as="div"
                >
                  {{ $t("tip.requiredField") }}
                </ErrorMessage>
              </div>

              <div class="col-lg-6 mb-2">
                <label class="required fs-6 fw-semibold mb-2">{{
                  $t("fields.phoneNum")
                }}</label>

                <Field
                  v-model="formData.phoneNumber"
                  class="mt-1 form-control form-control-lg form-control-solid"
                  type="text"
                  name="phoneNumber"
                  autocomplete="off"
                >
                  <el-input
                    v-model="formData.phoneNumber"
                    :placeholder="phoneInputSample"
                    size="large"
                    name="phoneNumber"
                  >
                    <template #prepend>
                      <el-select
                        size="large"
                        :placeholder="$t('action.select')"
                        v-model="phoneRegionCode"
                        style="width: 175px"
                      >
                        <el-option
                          v-for="item in regionCodes"
                          :key="item.code"
                          :label="`${item.name} +${item.dialCode}`"
                          :value="item.code"
                        />
                      </el-select>
                    </template>
                  </el-input>
                </Field>

                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="phoneNumber"
                  as="div"
                >
                  {{ $t("tip.requiredField") }}
                </ErrorMessage>
              </div>

              <div class="col-lg-6 mb-2">
                <label class="required fs-6 fw-semibold mb-2">{{
                  $t("fields.name")
                }}</label>

                <Field
                  v-model="formData.name"
                  tabindex="1"
                  class="form-control form-control-lg form-control-solid"
                  type="text"
                  name="name"
                  autocomplete="off"
                >
                  <el-input
                    v-model="formData.name"
                    autocomplete="off"
                    name="name"
                    @input="validateName"
                    clearable
                    size="large"
                  />
                </Field>
                <div class="text-danger" v-if="nameError">
                  {{ $t("tip.onlyEnglishCharacter") }}
                </div>
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="name"
                  as="div"
                >
                  {{ $t("tip.requiredField") }}
                </ErrorMessage>
              </div>
              <div class="col-lg-6">
                <label class="required fs-6 fw-semibold mb-2">{{
                  $t("fields.email")
                }}</label>

                <Field
                  v-model="formData.email"
                  tabindex="1"
                  class="form-control form-control-lg form-control-solid"
                  type="text"
                  name="email"
                  autocomplete="off"
                >
                  <el-input
                    v-model="formData.email"
                    autocomplete="off"
                    clearable
                    size="large"
                  />
                </Field>
                <ErrorMessage
                  class="fv-plugins-message-container invalid-feedback"
                  name="email"
                  as="div"
                >
                  {{ $t("tip.invalidEmail") }}
                </ErrorMessage>
              </div>
            </div>
          </div>
        </div>
        <div class="d-flex mb-4 mx-4" style="justify-content: center">
          <LoadingButton
            class="btn btn-primary loginBtn"
            @click="submit"
            :is-loading="isSubmitting"
            :save-title="$t('action.register')"
          />
        </div>
      </div>
      <div
        v-if="showSuccess"
        id="create-demo-account"
        class="card w-800px mx-auto"
        style="box-shadow: rgba(99, 99, 99, 0.2) 0px 2px 8px 0px; padding: 20px"
      >
        <div class="card-body text-center">
          <div class="mb-5" style="font-size: 28px">
            {{ $t("tip.congratulations") }} !
          </div>
          <div class="mb-3" style="font-size: 20px">
            {{ $t("tip.createDemoAccountSuccessMsg") }}
          </div>
          <div class="mb-3" style="font-size: 20px">
            {{ $t("tip.createDemoAccountSuccessMsg2") }}
          </div>
          <div class="mb-11" style="font-size: 20px; color: orange">
            {{ formData.email }}
          </div>

          <button class="btn btn-primary" type="button" @click="goRegister">
            {{ $t("action.register") }}
          </button>
        </div>
      </div>
    </div>
    <div class="absolute top-3" :style="{ left: isMobile ? '20px' : '110px' }">
      <img class="h-12 w-12" alt="Logo" :src="getTenantLogo['src']" />
    </div>
    <div
      class="absolute top-10 flex"
      :style="{ right: isMobile ? '20px' : '110px' }"
    >
      <LanguageDropdown />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import { useStore } from "@/store";
import LanguageDropdown from "@/projects/client/components/LanguageDropdown.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { AccountTypes } from "@/core/types/AccountInfos";
import { ServiceTypes } from "@/core/types/ServiceTypes";
import { ErrorMessage, Field, useForm } from "vee-validate";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import { useI18n } from "vue-i18n";
import { getRegionCodes } from "@/core/data/phonesData";
import { isMobile } from "@/core/config/WindowConfig";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import { useRoute, useRouter } from "vue-router";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import * as yup from "yup";
import {
  getTenancy,
  TenantTypes,
  getTenantLogo,
} from "@/core/types/TenantTypes";
import { SiteTypes } from "@/core/types/SiteTypes";
import UiRipple from "../../components/ripple/UiRipple.vue";
import axios from "axios";

const store = useStore();
const isLoading = ref(true);
const isSubmitting = ref(false);
const formData = ref<any>({});
const { t } = useI18n();
const i18nLang = useI18n();
const language = ref<any>();
const route = useRoute();
const nameError = ref(false);
const regionCodes = ref(getRegionCodes());
const phoneRegionCode = ref("");
const phoneInputSample = ref(t("tip.pleaseInput"));
const showSuccess = ref(false);
const amount = ref(null);

const amountSelections = ref([
  { label: "$10,000", value: 10000 },
  { label: "$50,000", value: 50000 },
  { label: "$100,000", value: 100000 },
]);

const tenantId = ref<string | null>(null);
// const validationSchema = {
//   accountType: "required",
//   platform: "required",
//   name: "required",
//   currency: "required",
//   countryCode: "required",
//   phoneNumber: "required",
//   email: "required|email",
// };
const router = useRouter();

const validationSchema = yup.object({
  accountType: yup.string().required(t("tip.requiredField")),
  platform: yup.string().required(t("tip.requiredField")),
  name: yup
    .string()
    .matches(/^[A-Za-z\s]*$/, t("tip.onlyEnglishCharacter"))
    .required(t("tip.requiredField")),

  amount: yup.number().required(t("tip.requiredField")),
  currency: yup.string().required(t("tip.requiredField")),
  countryCode: yup.string().required(t("tip.requiredField")),
  phoneNumber: yup.string().required(t("tip.requiredField")),
  email: yup
    .string()
    .email(t("tip.invalidEmail"))
    .required(t("tip.requiredField")),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const validateName = () => {
  const englishCharactersRegex = /^[A-Za-z\s]*$/;
  console.log(englishCharactersRegex.test(formData.value.name));
  if (!englishCharactersRegex.test(formData.value.name)) {
    nameError.value = true;
  } else {
    nameError.value = false;
  }
};

const initData = () => {
  language.value =
    (route.query.lang as string) ?? i18nLang.locale.value ?? LanguageTypes.enUS;
  i18nLang.locale.value = language.value;
  formData.value = {};
  resetForm({ values: {} });
};

const submit = handleSubmit(async () => {
  isSubmitting.value = true;
  const body = {
    ...formData.value,
    phoneNumber:
      regionCodes.value[phoneRegionCode.value]?.dialCode +
      formData.value.phoneNumber,
    referralCode: route.query["referralCode"],
    tenantId: tenantId.value,
    amount: amount.value,
  };
  // console.log(body);
  try {
    await ClientGlobalService.createDemoAccountFromNonAuth(body);
    MsgPrompt.success(t("tip.createDemoAccountSuccess")).then(() => {
      showSuccess.value = true;
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
});

const demoPlatformSelections = ref(Array<any>());
const accountTypeSelections = ref(Array<any>());
const currencySelections = ref(Array<any>());
const leverageTypeSelections = ref([] as any);

const getC = async () => {
  try {
    const openAt = route.query.open_at as string;
    let res;
    if (openAt) {
      res = await axios.get("/api/v1/auth/c?openAt=" + openAt);
    } else {
      res = await axios.get("/api/v1/auth/c");
    }

    return res.data;
  } catch (error) {
    console.error(error);
  }
};

const getTenantId = async () => {
  tenantId.value =
    (route.query.tenantId as string) ?? TenantTypes[getTenancy.value];
  const res = await getC();
  if (res && res[0] == SiteTypes.Vietnam) {
    console.log("tenantId.value", tenantId.value);
    tenantId.value = TenantTypes.sea;
  }
};

onMounted(async () => {
  isLoading.value = true;

  await getTenantId();
  await store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
  tenantId.value =
    (route.query.tenantId as string) ?? TenantTypes[getTenancy.value];
  accountTypeSelections.value = [
    {
      label: t(`type.account.${AccountTypes.Standard}`),
      value: AccountTypes.Standard,
      iconPath: "/images/icons/communication/com005.svg",
    },
    {
      label: t(`type.account.${AccountTypes.Alpha}`),
      value: AccountTypes.Alpha,
      iconPath: "/images/icons/finance/fin006.svg",
    },
  ];
  currencySelections.value = [
    {
      label: "USD",
      value: 840,
    },
    {
      label: "AUD",
      value: 36,
    },
  ];

  demoPlatformSelections.value = [
    {
      label: "MetaTrader 4",
      value: ServiceTypes.MetaTrader4Demo,
      iconPath: "/images/icons/brand/mt4.svg",
    },
    {
      label: "MetaTrader 5",
      value: ServiceTypes.MetaTrader5Demo,
      iconPath: "/images/icons/brand/mt5.svg",
    },
  ];

  if (tenantId.value == TenantTypes.au) {
    leverageTypeSelections.value = [30];
  } else if (tenantId.value == TenantTypes.sea) {
    leverageTypeSelections.value = [20, 25, 30, 50, 100, 200, 400, 500, 1000];
  } else {
    leverageTypeSelections.value = [20, 25, 30, 50, 100, 200, 400];
  }

  // const publicConfig = await ClientGlobalService.getConfiguration();
  // accountTypeSelections.value = publicConfig?.accountTypeAvailable.map(
  //   (typeEnum: AccountTypes) => {
  //     return {
  //       label: t(`type.account.${typeEnum}`),
  //       value: typeEnum,
  //       iconPath:
  //         {
  //           1: "/images/icons/communication/com005.svg",
  //           2: "/images/icons/finance/fin006.svg",
  //         }[typeEnum] ?? "/images/icons/communication/com005.svg",
  //     };
  //   }
  // );

  // currencySelections.value = publicConfig?.currencyAvailable.map(
  //   (currencyId: number) => {
  //     return {
  //       label: t(`type.currency.${currencyId}`),
  //       value: currencyId,
  //     };
  //   }
  // );

  // demoPlatformSelections.value =
  //   publicConfig?.demoTradingPlatformAvailable?.map(
  //     (availablePlatformId: number) => {
  //       return {
  //         label: t(`type.platform.${availablePlatformId}`),
  //         value: availablePlatformId,
  //         iconPath: `/images/icons/brand/${
  //           {
  //             [ServiceTypes.MetaTrader4Demo]: "mt4",
  //             [ServiceTypes.MetaTrader5Demo]: "mt5",
  //           }[availablePlatformId]
  //         }.svg`,
  //       };
  //     }
  //   );

  isLoading.value = false;
  initData();
});

const goRegister = () => {
  router.push({ name: "sign-up" });
};
</script>

<style scoped>
.form-container {
  label {
    color: #000 !important;
  }
}
</style>
