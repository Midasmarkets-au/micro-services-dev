<template>
  <div
    class="authentication-login-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />
    <div class="form-container relative">
      <div class="auth-box">
        <div class="flex text-xl text-center leading-[4.375rem]">
          <div class="flex-1 text-black">
            {{ $t("action.login") }}
          </div>
          <div
            class="flex-1 bg-[#F2F4F7] rounded-tr-3xl cursor-pointer"
            @click="toggleAuthType"
          >
            {{ $t("action.register") }}
          </div>
        </div>
        <div class="auth-box-header">
          <span class="text-3xl font-bold">{{ $t("action.login") }}</span>
        </div>
        <div class="auth-form">
          <ForgetPasswordForm v-if="showPage == 'ForgetPasswordPage'" />
          <EmailVerifyPage v-if="showPage == 'EmailVerifyPage'" />
          <Form
            v-if="showPage == 'LoginPage'"
            class="login-form"
            :validation-schema="login"
          >
            <div class="mb-6">
              <label class="label required mb-1">{{
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
                  name="email"
                  :placeholder="$t('tip.pleaseInput')"
                  size="large"
                  tabindex="1"
                  class="log-input"
                />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="email" />
                </div>
              </div>
            </div>
            <div class="mb-6 mt-5">
              <div class="d-flex justify-content-between">
                <label class="label required mb-1">{{
                  $t("fields.password")
                }}</label>
                <div>
                  <SvgIcon
                    :name="passwordShow ? 'eye_open' : 'eye'"
                    @click="passwordShow = !passwordShow"
                  />
                </div>
              </div>
              <Field
                tabindex="2"
                v-model="formData.password"
                class="form-control form-control-lg form-control-solid"
                :type="passwordShow ? 'text' : 'password'"
                name="password"
                autocomplete="off"
              >
                <el-input
                  v-model="formData.password"
                  name="password"
                  :placeholder="$t('tip.pleaseInput')"
                  size="large"
                  tabindex="2"
                  class="log-input"
                  :type="passwordShow ? 'text' : 'password'"
                />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="password" />
                </div>
              </div>
              <div class="forgot">
                <a @click="showPage = 'ForgetPasswordPage'">{{
                  $t("action.forgotPassword")
                }}</a>
              </div>
            </div>
            <div class="mb-6" v-if="twoFaRequired">
              <div class="d-flex justify-content-between">
                <label class="label required mb-1">{{
                  $t("fields.twoFactorAuthentication")
                }}</label>
              </div>
              <Field
                tabindex="2"
                v-model="twoFaCode"
                class="form-control form-control-lg form-control-solid"
                type="text"
                name="twoFaCode"
                autocomplete="off"
              >
                <el-input
                  v-model="twoFaCode"
                  name="twoFaCode"
                  :placeholder="$t('tip.pleaseInput')"
                  size="large"
                  tabindex="2"
                  type="text"
                />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  {{ $t("fields.pleaseEnterTwoFactorCode") }}
                </div>
              </div>
            </div>
            <div>
              <div style="display: flex; flex-direction: column" class="mt-8">
                <LoadingButton
                  :is-loading="isSubmitting"
                  :save-title="$t('action.login')"
                  class="w-100 loginBtn"
                  @click.prevent="onSubmitLogin"
                />
                <div class="msg_1">
                  {{ $t("tip.doNotHaveAccount") }}
                  <router-link :to="{ path: '/sign-up', query: $route.query }">
                    {{ $t("action.signup") }}</router-link
                  >
                </div>
              </div>
              <div>
                <!-- <div class="mobil-switch">
                  <span class="text-uppercase" style="margin-right: 18px">
                    {{ $t("action.login") }}
                  </span>
                  <AuthSwitch
                    :isEnabled="authType"
                    color="#F5BF21"
                    @toggleAuthType="toggleAuthType"
                  ></AuthSwitch>
                  <span class="text-uppercase" style="margin-left: 5px">
                    {{ $t("action.register") }}
                  </span>
                </div> -->
                <div class="mt-10 mb-6 flex item-center justify-center">
                  <span class="text-gray"> {{ $t("tip.wantContact") }}</span>
                  <router-link to="/lead-create" class="text-block">{{
                    getFlagEmail
                  }}</router-link>
                </div>
              </div>
            </div>
          </Form>
          <SelectTenantPage v-if="showPage == 'SelectTenantPage'" />
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

<script lang="ts" setup>
import axios from "axios";
import * as Yup from "yup";
import { useStore } from "@/store";
import { useRouter, useRoute } from "vue-router";
import { ref, onMounted, inject, provide, computed, watch } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import JwtService from "@/core/services/JwtService";
import { ErrorMessage, Field, Form } from "vee-validate";
import AuthSwitch from "../../components/AuthSwitch.vue";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import { LanguageTypes } from "@/core/types/LanguageTypes";
import { useI18n } from "vue-i18n";
import ApiService from "@/core/services/ApiService";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import ForgetPasswordForm from "../../components/signIn/ForgetPasswordForm.vue";
import EmailVerifyPage from "../../components/signIn/EmailVerifyPage.vue";
import SelectTenantPage from "../../components/signIn/SelectTenantPage.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { getTenantLogo, getTenancy, tenancies } from "@/core/types/TenantTypes";
import { SiteTypes } from "@/core/types/SiteTypes";
import SvgIcon from "../../components/SvgIcon.vue";
import UiRipple from "../../components/ripple/UiRipple.vue";
import { performLogin } from "../../services/AuthService";
const i18nLang = useI18n();
const { t } = i18nLang;
const store = useStore();
const router = useRouter();
const authType = ref(false);
const passwordShow = ref(false);

const showTwoFaError = ref(false);

// const emailList = ref({
//   2: "info@au.thebcr.com",
//   0: "info@thebcr.com",
//   6: "info@isec.jp",
// });

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

const showPage = ref("LoginPage");
const isSubmitting = ref(false);
const formData = ref<any>({});
const tenantsOptions = ref<any[]>([]);
const selectedTenant = ref<any>(null);
const twoFaRequired = ref(false);
const twoFaCode = ref<any>(null);

provide("showPage", showPage);
provide("formData", formData);
provide("isSubmitting", isSubmitting);
provide("tenantsOptions", tenantsOptions);
provide("selectedTenant", selectedTenant);
provide("twoFaRequired", twoFaRequired);
provide("twoFaCode", twoFaCode);

const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);
const c = ref([] as any);

// const siteEmail = process.env.VUE_APP_CONTACT_EMAIL;
const route = useRoute();
const language = ref<any>();
const getC = async () => {
  try {
    if (getTenancy.value == tenancies.jp) {
      c.value[0] = SiteTypes.Japan;
    } else {
      const res = await axios.get("/api/v1/auth/c");
      c.value = res.data;
    }

    window.localStorage.setItem("c", c.value);
  } catch (error) {
    console.error(error);
  }
};
language.value =
  (route.query.lang as string) ?? i18nLang.locale.value ?? LanguageTypes.enUS;

i18nLang.locale.value = language.value;

onMounted(async () => {
  await configTenantId();
  await getC();
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

watch(i18nLang.locale, (newValue) => {
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
  login.value = createValidationSchema();
});
const configTenantId = async () => {
  var tenantId = route.query.tenantId as string;

  if (tenantId) {
    setCookie("tenantId", tenantId, 30);
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

// const oauth2Endpoint = ref("https://accounts.google.com/o/oauth2/v2/auth?");
// oauth2Endpoint.value +=
//   "scope=https%3A//www.googleapis.com/auth/userinfo.email&";
// oauth2Endpoint.value +=
//   "redirect_uri=" + process.env.VUE_APP_GOOGLE_OAUTH_REDIRECT_URL + "&";
// oauth2Endpoint.value += "response_type=token&";
// oauth2Endpoint.value += "prompt=select_account&";
// oauth2Endpoint.value +=
//   "client_id=" + process.env.VUE_APP_GOOGLE_OAUTH_CLIENT_ID;

// const login = Yup.object().shape({
//   email: Yup.string()
//     .email(t("error.__EMAIL_IS_REQUIRED__"))
//     .required(t("error.__EMAIL_IS_REQUIRED__"))
//     .label("Email"),
//   password: Yup.string()
//     .required(t("error.signup_ps_require"))
//     .label("Password"),
// });
const login = ref(createValidationSchema());
function createValidationSchema() {
  return Yup.object().shape({
    email: Yup.string()
      .email(t("error.__EMAIL_IS_REQUIRED__"))
      .required(t("error.__EMAIL_IS_REQUIRED__"))
      .label("Email"),
    password: Yup.string()
      .required(t("error.signup_ps_require"))
      .label("Password"),
  });
}
//form submit function
const onSubmitLogin = async () => {
  isSubmitting.value = true;

  const result = await performLogin(
    {
      email: formData.value.email,
      password: formData.value.password,
      tenantId: selectedTenant.value,
      twoFaCode: twoFaCode.value,
    },
    {
      router,
      store,
      wsSignalR,
      t,
      onTwoFaRequired: () => {
        twoFaRequired.value = true;
      },
      onMultipleTenants: (tenantIds) => {
        showPage.value = "SelectTenantPage";
        tenantsOptions.value = tenantIds;
      },
      onError: (errors) => {
        if (errors == "__EMAIL_UNCONFIRMED__") {
          showPage.value = "EmailVerifyPage";
        }
      },
    }
  );

  isSubmitting.value = false;
};

provide("onSubmitLogin", onSubmitLogin);

const toggleAuthType = (_authType) => {
  authType.value = _authType;
  router.push({ name: "sign-up", query: router.currentRoute.value.query });
};
</script>
<style scoped>
.cursor {
  cursor: pointer;
}
.img-rounded {
  border-radius: 50%;
}
</style>
