<template>
  <div
    class="authentication-register-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <!-- ==================================================================== Side -->
    <!-- ==================================================================== Side -->

    <!-- ==================================================================== Forms -->
    <!-- ==================================================================== Forms -->

    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />
    <div class="form-container relative">
      <div class="auth-box">
        <div class="flex text-xl text-center leading-[4.375rem]">
          <div
            class="flex-1 text-black bg-[#F2F4F7] rounded-tl-[20px] cursor-pointer"
            @click="toggleAuthType"
          >
            {{ $t("action.login") }}
          </div>
          <div class="flex-1">
            {{ $t("action.register") }}
          </div>
        </div>
        <div class="auth-box-header">
          <div>
            <div class="title" v-if="tenantNo == TenantTypes.jp">
              {{ $t("tip.createAnIsecAccount") }}
            </div>
            <div class="title" v-else>{{ $t("tip.createBcrAccount") }}</div>
            <div class="subtitle text-center">
              {{ $t("tip.enterAccountDetailsBelow") }}
            </div>
          </div>
        </div>
        <div class="auth-form">
          <!-- ==================================================================== Step 1 -->
          <!-- ==================================================================== Step 1 -->
          <Form
            v-if="processes == 1"
            @submit="next"
            class="login-form"
            :validation-schema="step1_validationSchema"
          >
            <div>
              <div class="mb-6">
                <label class="label mb-1 required">{{
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
                    class="log-input"
                  />
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="email" />
                  </div>
                </div>
              </div>
              <div class="mb-10 mt-10">
                <div class="d-flex justify-content-between">
                  <label class="label mb-1 required">{{
                    $t("fields.password")
                  }}</label>
                  <div class="mobile-font">
                    <SvgIcon
                      :name="passwordShow_1 ? 'eye_open' : 'eye'"
                      @click="passwordShow_1 = !passwordShow_1"
                    />
                  </div>
                </div>
                <Field
                  v-model="formData.password"
                  tabindex="2"
                  class="mt-1 form-control form-control-lg form-control-solid"
                  :type="passwordShow_1 ? 'text' : 'password'"
                  name="password"
                  autocomplete="off"
                >
                  <el-input
                    v-model="formData.password"
                    :type="passwordShow_1 ? 'text' : 'password'"
                    size="large"
                    :placeholder="
                      $t('tip.use8CharWithMixLetterAndNumberAndSymbol')
                    "
                    clearable
                    class="log-input"
                  >
                  </el-input>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="password" />
                  </div>
                </div>
              </div>
              <div class="mb-6 mt-10">
                <div class="d-flex justify-content-between">
                  <label class="label mb-1 required">{{
                    $t("fields.confirmedPassword")
                  }}</label>
                  <div class="mobile-font">
                    <SvgIcon
                      :name="passwordShow_2 ? 'eye_open' : 'eye'"
                      @click="passwordShow_2 = !passwordShow_2"
                    />
                  </div>
                </div>
                <Field
                  v-model="formData.confirmPassword"
                  tabindex="3"
                  class="mt-1 form-control form-control-lg form-control-solid"
                  :type="passwordShow_2 ? 'text' : 'password'"
                  name="confirmPassword"
                  autocomplete="off"
                >
                  <el-input
                    v-model="formData.confirmPassword"
                    :type="passwordShow_2 ? 'text' : 'password'"
                    size="large"
                    :placeholder="$t('tip.inputPassword')"
                    clearable
                    class="log-input"
                  >
                  </el-input>
                </Field>
                <div class="fv-plugins-message-container">
                  <div class="fv-help-block">
                    <ErrorMessage name="confirmPassword" />
                  </div>
                </div>
              </div>
            </div>
            <button class="nextBtn mb-9" type="submit">
              {{ $t("action.next") }}
            </button>
          </Form>

          <!-- ==================================================================== Step 2 -->
          <!-- ==================================================================== Step 2 -->
          <Form
            class="login-form"
            v-if="processes == 2"
            @submit="onSubmitRegister"
            :validation-schema="step2_validationSchema"
          >
            <div class="prev" @click="prev">
              <i
                class="fa-solid fa-arrow-left fa-xl"
                style="line-height: 0"
              ></i>
            </div>
            <div class="mb-9">
              <div class="mobile-title">
                <div>{{ $t("title.enterYourInfo") }}</div>
                <div class="mobile-prev" @click="prev">
                  <i class="fa-solid fa-arrow-left fa-sm"></i>
                </div>
              </div>
              <div class="row">
                <div class="col-6 mb-4">
                  <label class="label mb-1 required">
                    {{ $t("fields.firstName") }}
                  </label>

                  <Field
                    v-model="formData.FirstName"
                    class="mt-1 form-control form-control-lg form-control-solid"
                    type="text"
                    name="FirstName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="formData.FirstName"
                      :placeholder="$t('tip.pleaseInput')"
                      size="large"
                      class="log-input"
                    />
                  </Field>
                  <div class="fv-plugins-message-container">
                    <div class="fv-help-block">
                      <ErrorMessage name="FirstName" />
                    </div>
                  </div>
                </div>
                <div class="col-6 mb-4">
                  <label class="label mb-1 required">{{
                    $t("fields.lastName")
                  }}</label>

                  <Field
                    v-model="formData.LastName"
                    class="mt-1 form-control form-control-lg form-control-solid"
                    type="text"
                    name="LastName"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="formData.LastName"
                      :placeholder="$t('tip.pleaseInput')"
                      size="large"
                      class="log-input"
                    />
                  </Field>
                  <div class="fv-plugins-message-container">
                    <div class="fv-help-block">
                      <ErrorMessage name="LastName" />
                    </div>
                  </div>
                </div>
              </div>
              <div class="row">
                <div class="col-lg-6 mb-4">
                  <label class="label mb-1 required" style="min-width: 120px">
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
                      class="log-input"
                      filterable
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

                  <div class="fv-plugins-message-container">
                    <div class="fv-help-block">
                      <ErrorMessage name="countryCode" />
                    </div>
                  </div>
                </div>
                <div
                  v-if="
                    c[0] != SiteTypes.Australia && tenantNo != TenantTypes.jp
                  "
                  class="col-lg-6 mb-4"
                >
                  <label class="label mb-1">
                    {{ $t("fields.referralCode") }}
                  </label>

                  <Field
                    v-model="formData.referCode"
                    class="mt-1 form-control form-control-lg form-control-solid"
                    type="text"
                    name="referCode"
                    autocomplete="off"
                    :disabled="props.code != undefined ? true : false"
                  >
                    <el-input
                      v-model="formData.referCode"
                      :placeholder="$t('tip.pleaseInput')"
                      size="large"
                      class="log-input"
                      :disabled="props.code != undefined ? true : false"
                    />
                  </Field>
                  <div class="fv-plugins-message-container">
                    <div class="fv-help-block">
                      <ErrorMessage name="referCode" />
                    </div>
                  </div>
                </div>
                <div class="col-lg-12 mb-4">
                  <label class="label mb-1 required">{{
                    $t("fields.phoneNum")
                  }}</label>
                  <Field
                    v-model="formData.phone"
                    class="mt-1 form-control form-control-lg form-control-solid"
                    type="text"
                    name="phone"
                    autocomplete="off"
                  >
                    <el-input
                      v-model="formData.phone"
                      :placeholder="$t('tip.pleaseInput')"
                      size="large"
                      class="log-input"
                    >
                      <template #prepend>
                        <el-select
                          size="large"
                          :placeholder="$t('action.select')"
                          v-model="phoneRegionCode"
                          style="width: 175px"
                          filterable
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
                  <div class="fv-plugins-message-container">
                    <div class="fv-help-block">
                      <ErrorMessage name="phone" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div>
              <label
                v-if="c[0] == SiteTypes.Australia || tenantNo == TenantTypes.au"
                class="checkboxContainer"
              >
                {{ $t("tip.pleaseEnsure") }}
                <a
                  :href="
                    getBaDocs(baDocs.productDisclosureDocument.title) || ''
                  "
                  target="_blank"
                  >{{
                    $t(`title.${baDocs.productDisclosureDocument.title}`)
                  }}</a
                >
                ,
                <a
                  :href="getBaDocs(baDocs.financialServicesGuide.title) || ''"
                  target="_blank"
                  >{{ $t(`title.${baDocs.financialServicesGuide.title}`) }}</a
                >,
                <a
                  :href="
                    getBaDocs(baDocs.targetMarketDetermination.title) || ''
                  "
                  target="_blank"
                  >{{
                    $t(`title.${baDocs.targetMarketDetermination.title}`)
                  }}</a
                >
                {{ $t("tip.andHaveSought") }}

                <input type="checkbox" v-model="checked" />
                <span class="checkmark"></span>
              </label>
              <div v-if="tenantNo == TenantTypes.jp"></div>
              <label
                v-else-if="
                  c[0] == SiteTypes.Australia || tenantNo == TenantTypes.au
                "
                class="checkboxContainer mt-2"
              >
                {{ $t("tip.byCreateAccountYouAgree") }}
                <a
                  :href="getBaDocs(baDocs.termAndConditions.title) || ''"
                  target="_blank"
                  >{{ $t(`title.${baDocs.termAndConditions.title}`) }}</a
                >
                {{ $t("fields.and") }}
                <a
                  :href="getBaDocs(baDocs.privacyPolicy.title) || ''"
                  target="_blank"
                  >{{ $t(`title.${baDocs.privacyPolicy.title}`) }}
                </a>
                .
                <input type="checkbox" v-model="checkedTwo" />
                <span class="checkmark"></span>
              </label>
              <label v-else class="checkboxContainer mt-2">
                {{ $t("tip.byCreateAccountYouAgree") }}
                <a
                  :href="getBviDocs(bviDocs.TermsAndConditions.title) || ''"
                  target="_blank"
                  >{{ $t(`title.${bviDocs.TermsAndConditions.title}`) }}</a
                >
                {{ $t("fields.and") }}
                <a
                  :href="getBviDocs(bviDocs.PrivacyPolicy.title) || ''"
                  target="_blank"
                  >{{ $t(`title.${bviDocs.PrivacyPolicy.title}`) }}</a
                >
                .
                <input type="checkbox" v-model="checkedTwo" />
                <span class="checkmark"></span>
              </label>
            </div>
            <button
              type="submit"
              ref="submitButton"
              class="submitBtn mb-8"
              :disabled="!checked || !checkedTwo"
            >
              <span class="indicator-label"> {{ $t("action.submit") }} </span>

              <span class="indicator-progress">
                {{ $t("tip.pleaseWait") }}
                <span
                  class="spinner-border spinner-border-sm align-middle ms-2"
                ></span>
              </span>
            </button>
          </Form>

          <!-- ==================================================================== Step 3 -->
          <!-- ==================================================================== Step 3 -->
          <Form v-if="processes == 3">
            <div class="page">
              <!-- <div class="prev" @click="prev">
                <i class="fa-solid fa-arrow-left fa-xl"></i>
              </div> -->
              <div>
                <div class="mobile-title">
                  <div>{{ $t("tip.verifyYourEmail") }}</div>
                  <!-- <div class="mobile-prev" @click="prev">
                    <i class="fa-solid fa-arrow-left fa-sm"></i>
                  </div> -->
                </div>

                <div class="final-wrap mt-9">
                  <div>
                    <div class="finalMessage">
                      {{ $t("tip.thankSignUpAndConfirm") }}
                      <a href="#">{{ formData.email }}</a>
                      {{ $t("tip.toActivateYourAccount") }}
                    </div>
                    <br />
                    <div class="finalMessage">
                      {{ $t("tip.linkExpireContact") }}
                      <router-link to="/lead-create">{{
                        $t("title.contactUs")
                      }}</router-link>
                    </div>
                    <br />
                    <div class="finalMessage">
                      {{ $t("tip.notReceiveEmail") }}
                      <a class="ms-3" href="#" @click="resendConfirmation">
                        {{ $t("action.resendEmail") }}</a
                      >
                    </div>
                  </div>
                  <img
                    class="web-final-icon"
                    src="/images/auth/signup-final-icon.png"
                    alt=""
                  />
                </div>
                <div class="mobile-final-icon">
                  <img src="/images/auth/mobile-signup-final-icon.png" alt="" />
                </div>
              </div>

              <button
                class="changeEmailBtn text-uppercase"
                type="button"
                @click="goLogin"
              >
                {{ $t("tip.backToLogin") }}
              </button>
            </div>
          </Form>
        </div>
        <!-- <div class="mt-12 flex item-center justify-center mb-12">
          <span class='text-gra'> {{ $t("tip.wantContact") }}</span>
          <router-link to="/lead-create" class="text-block">{{
            getFlagEmail
          }}</router-link>
        </div> -->
        <!-- <div class="mobile-switch">
          <span class="text-uppercase" style="margin-right: 18px">{{
            $t("action.login")
          }}</span>
          <AuthSwitch
            :isEnabled="authType"
            color="#F5BF21"
            @toggleAuthType="toggleAuthType"
          ></AuthSwitch>
          <span class="text-uppercase" style="margin-left: 5px">{{
            $t("action.register")
          }}</span>
        </div> -->
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

<script lang="ts" setup>
import * as Yup from "yup";
import axios from "axios";
import { useStore } from "@/store";
import { ref, onMounted, watch, computed, inject } from "vue";
import { useRoute, useRouter } from "vue-router";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Actions } from "@/store/enums/StoreEnums";
import { getRegionCodes } from "@/core/data/phonesData";
import AuthSwitch from "../../components/AuthSwitch.vue";
import { Field, ErrorMessage, Form } from "vee-validate";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import { useI18n } from "vue-i18n";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import { event } from "vue-gtag";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import { SiteTypes } from "@/core/types/SiteTypes";
import { getBaDocs, getBviDocs, baDocs, bviDocs } from "@/core/data/bcrDocs";
import SvgIcon from "../../components/SvgIcon.vue";
import UiRipple from "../../components/ripple/UiRipple.vue";
import { isMobile } from "@/core/config/WindowConfig";
import {
  getTenancy,
  getTenantLogo,
  TenantTypes,
  tenancies,
} from "@/core/types/TenantTypes";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { performLogin } from "../../services/AuthService";

const props = defineProps({
  code: { type: String, required: false },
});

const processes = ref(1);
const store = useStore();
const i18n = useI18n();
const { t } = i18n;
const router = useRouter();
const authType = ref(true);
const checked = ref(true);
const checkedTwo = ref(false);
const formData = ref<any>({});
const passwordShow_1 = ref(false);
const passwordShow_2 = ref(false);
const smsInterval = ref(0);
const tenantNo = ref<any>(TenantTypes.bvi);
const phoneRegionCode = ref("");
const regionCodes = ref<any>(getRegionCodes());
const submitButton = ref<HTMLButtonElement | null>(null);
// const formOneKey = ref(0);
// const formTwoKey = ref(0);
const route = useRoute();
const language = ref<any>();
const c = ref([] as any);

// Inject wsSignalR for auto login
const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);

const emailList = ref({
  [SiteTypes.Australia]: "info@midasmkts.com",
  [SiteTypes.Default]: "info@midasmkts.com",
});
const getFlagEmail = computed(() => {
  if (getTenancy.value == tenancies.jp) {
    return "info@isec.jp";
  }
  if (getTenancy.value == tenancies.au) {
    return emailList.value[SiteTypes.Australia];
  }
  return emailList.value[c.value[0]] || "info@midasmkts.com";
});
const initFormData = () => {
  var passInLang = route.query.lang as string;

  language.value = passInLang || i18n.locale.value || LanguageTypes.enUS;

  if (language.value == "zh-hk") {
    language.value = "zh-tw";
  } else if (language.value == "id-id") {
    language.value = "en-us";
  }

  i18n.locale.value = language.value;
  localStorage.setItem("language", language.value);

  var confirmUrl =
    window.location.protocol + "//" + window.location.host + "/confirm-email";
  if (window.location.href.includes("portal")) {
    confirmUrl =
      window.location.protocol +
      "//" +
      window.location.host +
      "/portal/confirm-email";
  }
  formData.value = {
    language: language.value,
    confirmUrl: confirmUrl,
  };

  if (props.code) {
    formData.value.referCode = props.code;
  }

  phoneRegionCode.value = "";
};

const configTenantId = () => {
  // var tenantId = route.query.tenantId as string;
  var tenantId = parseInt(route.query.tenantId as string, 10);

  if (tenantId) {
    setCookie("tenantId", tenantId.toString(), 30);
  } else {
    const cookieTenantId = getCookie("tenantId");
    tenantId = cookieTenantId ? parseInt(cookieTenantId, 10) : tenantId;
  }

  if (getTenancy.value == tenancies.jp) {
    tenantId = TenantTypes.jp;
    checkedTwo.value = true;
  }

  tenantNo.value = tenantId;
  if (tenantId == TenantTypes.au) {
    checked.value = false;
  }
  if (tenantId) {
    formData.value.tenantId = tenantId;
  }
  if (tenantId == TenantTypes.au) {
    regionCodes.value = getRegionCodes(["au", "cn", "hk", "tw", "vn", "jp"]);
  }
};

const configUtm = () => {
  var utm = route.query.utm as string;
  if (utm) {
    setCookie("utm", utm, 7);
    formData.value.utm = utm;
  }
};

const configCode = () => {
  var code = route.query.code as string;

  if (code) {
    setCookie("code", code, 30);
  }

  code = getCookie("code") ?? "";

  if (code) {
    formData.value.code = code;
  }
};

const setCookie = (name: string, value: string, days: number) => {
  let expires = "";
  if (days) {
    const date = new Date();
    date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
    expires = "; expires=" + date.toUTCString();
  }
  document.cookie = name + "=" + (value || "") + expires + "; path=/";
};

const getCookie = (name: string): string | null => {
  const nameEQ = name + "=";
  const ca = document.cookie.split(";");
  for (let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) == " ") c = c.substring(1, c.length);
    if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
  }
  return null;
};

const getC = async () => {
  try {
    let openAt = (route.query.open_at as string) ?? "bvi";
    const id = route.query.tenantId;
    if (typeof id === "string") {
      const numId = Number(id);
      if (!isNaN(numId)) {
        openAt = TenantTypes[numId];
      }
    }
    let res;
    if (openAt) {
      res = await axios.get("/api/v1/auth/c?openAt=" + openAt);
    } else {
      res = await axios.get("/api/v1/auth/c");
    }
    c.value = res.data;
    if (getTenancy.value == tenancies.au) {
      c.value = [SiteTypes.Australia];
      formData.value.tenantId = TenantTypes.au;
    }
    window.localStorage.setItem("c", c.value);
    if (c.value[0] == SiteTypes.Australia) {
      checked.value = false;
    }
  } catch (error) {
    console.error(error);
  }
};

const toggleAuthType = (_authType) => {
  authType.value = _authType;
  router.push({ name: "sign-in", query: router.currentRoute.value.query });
};

const step1_validationSchema = ref(createValidationSchema());
function createValidationSchema() {
  return Yup.object().shape({
    email: Yup.string()
      .email(t("error.__EMAIL_IS_REQUIRED__"))
      .required(t("error.__EMAIL_IS_REQUIRED__")),
    password: Yup.string()
      .min(8, t("error.signup_ps_8"))
      .matches(/[a-z]/, t("error.signup_ps_lower"))
      .matches(/[A-Z]/, t("error.signup_ps_upper"))
      .matches(/\d/, t("error.signup_ps_number"))
      .matches(/[!@#$%^&*(),.?":{}|<>]/, t("error.signup_ps_symbol"))
      .required(t("error.signup_ps_require")),
    confirmPassword: Yup.string()
      .required(t("error.signup_cps_require"))
      .oneOf([Yup.ref("password"), null], t("error.signup_ps_match")),
  });
}

const step2_validationSchema = ref(createValidation2Schema());
function createValidation2Schema() {
  return Yup.object().shape({
    FirstName: Yup.string().required(t("error.__FIRST_NAME_IS_REQUIRED__")),
    LastName: Yup.string().required(t("error.__LAST_NAME_IS_REQUIRED__")),
    countryCode: Yup.string().required(t("error.__NATIONALITY_IS_REQUIRED")),
    phone: Yup.string().required(t("error.__PHONE_NUMBER_IS_REQUIRED__")),
    // otpCode: Yup.string().required().label("OPT Code"),
    // otpCode: Yup.string().when({
    //   is: projectConfig.value.smsValidationEnabled,
    //   then: Yup.string().required().label("OPT Code"),
    // }),
  });
}

const sendOtpCode = async () => {
  try {
    await ClientGlobalService.sendOneTimeCodeForPhone(
      formData.value.ccc,
      formData.value.phone
    );

    MsgPrompt.success(t("tip.verificationCodeSend")).then(() => {
      smsInterval.value = 60;
      const interval = setInterval(() => {
        smsInterval.value--;
        if (smsInterval.value <= 0) {
          clearInterval(interval);
        }
      }, 1000);
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const onSubmitRegister = async () => {
  await store.dispatch(Actions.LOGOUT);
  formData.value.otp = 771578;
  if (!submitButton.value) {
    return;
  }
  submitButton.value.disabled = true;
  submitButton.value.setAttribute("data-kt-indicator", "on");
  try {
    await ClientGlobalService.registerNewUser({
      ...formData.value,
      siteId: c.value[0],
    });
    event("conversion", { send_to: process.env.VUE_APP_GA_SIGNUP_EVENT });

    // Auto login after successful registration
    await MsgPrompt.success(t("tip.registerSuccess"));

    // Perform auto login
    const loginResult = await performLogin(
      {
        email: formData.value.email,
        password: formData.value.password,
        tenantId: formData.value.tenantId?.toString(),
      },
      {
        router,
        store,
        wsSignalR,
        t,
        redirectTo: "/verification", // 注册后跳转到验证页面
        onTwoFaRequired: () => {
          // If 2FA is required, redirect to login page
          MsgPrompt.info(t("tip.pleaseLoginWith2FA")).then(() => {
            router.push({ name: "sign-in" });
          });
        },
        onMultipleTenants: () => {
          // If multiple tenants, redirect to login page
          MsgPrompt.info(t("tip.pleaseSelectTenant")).then(() => {
            router.push({ name: "sign-in" });
          });
        },
        onError: () => {
          // If login fails, redirect to login page
          router.push({ name: "sign-in" });
        },
      }
    );

    // If auto login succeeds, user will be redirected to /verification by performLogin
    // No need to reset button state as the component will be unmounted
    return;
  } catch (error) {
    const err = error as any;
    if (
      err.response.data.message === "__EMAIL_EXISTS__" ||
      err.response.status == 409
    ) {
      MsgPrompt.error(t("tip.emailExists"));
    } else {
      MsgPrompt.error(error);
    }
    goFirst();
  }

  // Only reset button state if there was an error
  if (submitButton.value) {
    submitButton.value.disabled = false;
    submitButton.value.setAttribute("data-kt-indicator", "off");
  }
};

const resendConfirmation = async () => {
  try {
    await ClientGlobalService.resendConfirmationEmail(
      formData.value.email,
      formData.value.confirmUrl
    );

    MsgPrompt.success(t("tip.confirmationEmailResend"));
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const prev = () => {
  processes.value -= 1;
};

const next = () => {
  processes.value += 1;
};

const goFirst = () => {
  processes.value = 1;
};

const goLogin = () => {
  router.push({ name: "sign-in" });
};

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");

  // 优化逻辑：
  // 1. 有 code（推荐码）：保持参数不变
  // 2. 没有 code：强制设置 tenantId = 10000
  // if (!route.query.code) {
  //   // 没有推荐码，强制设置 tenantId 为 10000 (BVI)
  //   const currentTenantId = route.query.tenantId;

  //   // 只有当 tenantId 不是 10000 时才需要更新路由
  //   if (currentTenantId !== "10000") {
  //     router.replace({
  //       path: route.path,
  //       query: {
  //         ...route.query,
  //         tenantId: "10000", // 强制设置为 BVI (10000)
  //       },
  //     });
  //   }
  // }
  // 如果有 code，保持所有参数不变
  if (!route.query.code) {
    let site = getTenancy.value;
    const type = window.location.hostname.split(".")[0];
    if (!route.query.tenantId) {
      router.replace({
        path: route.path,
        query: {
          ...route.query,
          tenantId: TenantTypes[site],
        },
      });
    } else {
      const id = route.query.tenantId;
      if (typeof id === "string") {
        if (site === type) {
          router.replace({
            path: route.path,
            query: {
              ...route.query,
              tenantId: TenantTypes[site],
            },
          });
        } else {
          router.replace({
            path: route.path,
            query: {
              ...route.query,
              tenantId: route.query.tenantId,
            },
          });
        }
      }
    }
  }
  initFormData();
  setTimeout(() => {
    configTenantId();
    configCode();
    configUtm();
    getC();
  }, 0);
});

watch(phoneRegionCode, () => {
  formData.value.ccc = regionCodes.value[phoneRegionCode.value]?.dialCode;
});

watch(i18n.locale, (newValue) => {
  // console.log(step1_validationSchema.value);
  if (newValue === "zh-hk") {
    language.value = "zh-tw";
    formData.value.language = "zh-tw";
  } else if (newValue === "id-id") {
    language.value = "en-us";
    formData.value.language = "en-us";
  } else {
    language.value = newValue;
    formData.value.language = newValue;
  }

  step1_validationSchema.value = createValidationSchema();
  step2_validationSchema.value = createValidation2Schema();
});
</script>
<style scoped>
.cursor {
  cursor: pointer;
}
.img-rounded {
  border-radius: 50%;
}
</style>
<style scoped>
:deep(.el-input-group--prepend
    .el-input-group__prepend
    .el-select
    .el-input
    .el-input__wrapper) {
  border-radius: 4px;
  box-shadow: none;
}
</style>
