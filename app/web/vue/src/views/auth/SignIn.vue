<template>
  <div class="w-lg-500px p-10">
    <Form
      v-if="currentPage == pages.login"
      class="form w-100"
      id="kt_login_signin_form"
      @submit="onSubmitLogin"
      :validation-schema="login"
    >
      <div class="text-center mb-10">
        <h1 class="text-dark mb-3">{{ $t("title.login") }}</h1>
      </div>
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark required">{{
          $t("fields.email")
        }}</label>

        <Field
          v-model="formData.email"
          tabindex="1"
          class="form-control form-control-lg"
          type="text"
          name="email"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="email" />
          </div>
        </div>
      </div>

      <div class="fv-row mb-10">
        <div class="d-flex flex-stack mb-2">
          <label class="form-label fw-bold text-dark fs-6 mb-0 required">{{
            $t("fields.password")
          }}</label>
          <!-- <a
            @click="currentPage = 'forgetPasswordPage'"
            class="link-primary fs-6 fw-bold cursor"
          >
            Forgot Password ?
          </a> -->
        </div>

        <Field
          v-model="formData.password"
          tabindex="2"
          class="form-control form-control-lg"
          type="password"
          name="password"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="password" />
          </div>
        </div>
      </div>

      <div class="mb-6" v-if="twoFaRequired">
        <div class="d-flex justify-content-between">
          <label class="form-label fw-bold text-dark fs-6 mb-0 required">{{
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
            {{ $t("fields.pleaseEnterAuthenticatorTwoFactorCode") }}
          </div>
        </div>
      </div>
      <div class="text-center">
        <el-button
          class="w-100"
          size="large"
          type="warning"
          :loading="isSubmitting"
          @click="onSubmitLogin"
        >
          {{ $t("action.login") }}
        </el-button>
      </div>
    </Form>
  </div>
  <SelectTenantPage v-if="currentPage == pages.selectTenantPage" />
</template>

<script lang="ts" setup>
import { ref, onMounted, inject, provide } from "vue";
import { ErrorMessage, Field, Form } from "vee-validate";
import JwtService from "@/core/services/JwtService";
import { Actions } from "@/store/enums/StoreEnums";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import * as Yup from "yup";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import ApiService from "@/core/services/ApiService";
import i18n from "@/core/plugins/i18n";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import SelectTenantPage from "./signIn/SelectTenantPage.vue";
import ForgetPasswordForm from "./signIn/ForgetPasswordForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const pages = ref<any>({
  login: "login",
  selectTenantPage: "SelectTenantPage",
  forgetPasswordPage: "forgetPasswordPage",
});

const { t } = i18n.global;
const router = useRouter();
const store = useStore();
const wsSignalR = inject(TenantGlobalInjectionKeys.WS_SIGNAL_R);
const selectedTenant = ref<any>(null);
const currentPage = ref(pages.value.login);
const tenantsOptions = ref([]);
const formData = ref<any>({});
const isSubmitting = ref(false);
const twoFaRequired = ref(false);
const twoFaCode = ref<any>(null);

provide("currentPage", currentPage);
provide("formData", formData);
provide("isSubmitting", isSubmitting);
provide("tenantsOptions", tenantsOptions);
provide("selectedTenant", selectedTenant);
provide("twoFaRequired", twoFaRequired);
provide("twoFaCode", twoFaCode);
provide("pages", pages);

//Create form validation object
const login = Yup.object().shape({
  email: Yup.string().email().required().label("Email"),
  password: Yup.string().min(4).required().label("Password"),
});

const onSubmitLogin = async () => {
  await store.dispatch(Actions.LOGOUT);
  isSubmitting.value = true;
  var errors = null;
  const params = setupLoginParams();

  var login = await ApiService.postToken("connect/token", params).catch(
    ({ response }) => {
      if (response.data.error != undefined) {
        errors = [response.data.error_description ?? response.data.error];
      } else {
        errors = response.data.errors;
      }
    }
  );

  if (errors) {
    errorHandle(errors);
    return;
  }

  var breakPoint = false;

  breakPoint = handleMultipleTenants(login);
  if (breakPoint) {
    return;
  }

  breakPoint = handleTwoFA(login);
  if (breakPoint) {
    return;
  }

  await loginStart(login);

  isSubmitting.value = false;
};

function setupLoginParams() {
  const params = new URLSearchParams();
  params.append("client_id", "api");
  params.append("grant_type", "password");
  params.append("scopes", "api");
  params.append("username", formData.value.email);
  params.append("password", formData.value.password);
  if (selectedTenant.value !== null) {
    params.append("tenantId", selectedTenant.value);
  }
  if (twoFaRequired.value == true) {
    params.append("tf_code", twoFaCode.value);
  }
  return params;
}

function errorHandle(errors: any) {
  if (errors == "__EMAIL_UNCONFIRMED__") {
    if (errors == "__EMAIL_UNCONFIRMED__") {
      currentPage.value = "EmailVerifyPage";
    }
  } else if (errors == "__USER_IS_LOCKED_OUT__") {
    MsgPrompt.error(t("error.__USER_IS_LOCKED_OUT__")).then(() => {
      isSubmitting.value = false;
    });
  } else if (errors == "__USER_UNDER_MAINTENANCE__") {
    MsgPrompt.warning(t("error.__USER_UNDER_MAINTENANCE__")).then(() => {
      isSubmitting.value = false;
    });
  } else if (twoFaRequired.value == true) {
    MsgPrompt.error(t("error.verificationFailed")).then(() => {
      isSubmitting.value = false;
    });
  } else {
    MsgPrompt.error(errors).then(() => {
      isSubmitting.value = false;
    });
  }
}

const handleTwoFA = (login: any) => {
  if (login?.data?.twoFactorRequired == true) {
    twoFaRequired.value = true;
    isSubmitting.value = false;
    return true;
  }
  return false;
};

const handleMultipleTenants = (login: any) => {
  if (login?.data?.hasMultipleTenants == true && selectedTenant.value == null) {
    currentPage.value = "SelectTenantPage";
    tenantsOptions.value = login.data.tenantIds;

    isSubmitting.value = false;
    return true;
  }
  return false;
};

const loginStart = async (login: any) => {
  await store.dispatch(Actions.LOGIN, login.data);
  localStorage.setItem("session_login_time", Date.now().toString());
  await storeTenantList();
  wsSignalR?.setup(JwtService.getToken());
  if (router.currentRoute.value.query.redirect) {
    await router.push(router.currentRoute.value.query.redirect as string);
  } else {
    await router.push({ name: "dashboard" });
  }
};

provide("onSubmitLogin", onSubmitLogin);

const storeTenantList = async () => {
  if (selectedTenant.value != null) {
    localStorage.setItem("tenant", selectedTenant.value.toString());
  }
  const res = await TenantGlobalService.getTenantTokens();
  localStorage.setItem("tenantList", JSON.stringify(res));
};

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});
</script>
<style scoped>
.cursor {
  cursor: pointer;
}
.img-rounded {
  border-radius: 50%;
}
.title {
  font-family: "Lato", sans-serif;
  font-weight: 600;
  font-size: 32px;
  color: #393d48;
  text-align: center;
  margin-bottom: 32px;

  @media (max-width: 768px) {
    font-size: 16px;
    text-align: left;
    color: white;
    margin-bottom: 5px;
  }
}

.label {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 14px;
  color: #666666;
}
</style>
