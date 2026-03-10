<template>
  <div
    class="authentication-login-group auth-page-wrapper relative w-full h-screen overflow-hidden bg-[radial-gradient(circle,rgba(22,132,252,0.4),rgba(186,184,253,0))]"
  >
    <UiRipple
      circle-class="border-white bg-[#B5CFFB]/25 shadow-[inset_0_0_20px_10px_rgba(255,255,255,0.6)] rounded-full"
    />

    <div class="form-container relative">
      <!-- <div class="text-dark auth-language-dropdown">
        <LanguageDropdown />
      </div> -->
      <div class="auth-box">
        <div class="auth-box-header">
          <span class="text-3xl font-bold">
            {{ $t("title.changeAccountPassword") }}
            <br />
            {{ $t("title.account") + ": " + accountNumber }}
          </span>
        </div>
        <div class="auth-form">
          <form
            class="login-form"
            id="kt_login_signin_form"
            :validation-schema="validationSchema"
          >
            <!-- <div class="title">
              {{ $t("title.changeAccountPassword") }}
              <br />
              {{ $t("title.account") + ": " + accountNumber }}
            </div> -->

            <div class="mb-6">
              <div class="d-flex justify-content-between mb-1">
                <label class="label required">{{
                  $t("fields.newPassword")
                }}</label>
                <div>
                  <!-- <i
                    class="fa-solid fa-lg eye-icon"
                    :class="passwordShow_1 ? 'fa-eye-slash' : 'fa-eye'"
                    @click="passwordShow_1 = !passwordShow_1"
                  >
                  </i>
                  <span v-if="passwordShow_1">Hide</span
                  ><span v-else>{{ $t("action.show") }}</span> -->
                  <SvgIcon
                    :name="passwordShow_1 ? 'eye_open' : 'eye'"
                    @click="passwordShow_1 = !passwordShow_1"
                  />
                </div>
              </div>

              <Field
                v-model="formData.password"
                tabindex="2"
                class="form-control form-control-lg form-control-solid"
                :type="passwordShow_1 ? 'text' : 'password'"
                name="password"
                autocomplete="off"
              >
                <el-input
                  v-model="formData.password"
                  size="large"
                  :type="passwordShow_1 ? 'text' : 'password'"
                  name="password"
                />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="password" />
                </div>
              </div>
            </div>

            <div class="mb-6">
              <div class="d-flex justify-content-between mb-1">
                <label class="label required">{{
                  $t("fields.confirmPassword")
                }}</label>
                <div>
                  <!-- <i
                    class="fa-solid fa-lg eye-icon"
                    :class="passwordShow_2 ? 'fa-eye-slash' : 'fa-eye'"
                    @click="passwordShow_2 = !passwordShow_2"
                  >
                  </i>
                  <span v-if="passwordShow_2">Hide</span
                  ><span v-else>{{ $t("action.show") }}</span> -->
                  <SvgIcon
                    :name="passwordShow_2 ? 'eye_open' : 'eye'"
                    @click="passwordShow_2 = !passwordShow_2"
                  />
                </div>
              </div>

              <Field
                v-model="confirmPassword"
                class="form-control form-control-lg form-control-solid"
                :type="passwordShow_2 ? 'text' : 'password'"
                name="confirmPassword"
                autocomplete="off"
              >
                <el-input
                  v-model="confirmPassword"
                  size="large"
                  :type="passwordShow_2 ? 'text' : 'password'"
                  name="confirmPassword"
                />
              </Field>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="confirmPassword" />
                </div>
              </div>
            </div>

            <div>
              <button
                @click.prevent="resetPassword"
                id="kt_sign_in_submit"
                class="btn btn-lg w-100 loginBtn mb-6 mt-4"
              >
                <span class="indicator-label" v-if="!isLoading">
                  {{ $t("action.submit") }}
                </span>

                <span class="indicator-progress" v-if="isLoading">
                  {{ $t("action.pleaseWait") + "..." }}
                  <span
                    class="spinner-border spinner-border-sm align-middle ms-2"
                  ></span>
                </span>
              </button>
            </div>
          </form>
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
import * as Yup from "yup";
import { useStore } from "@/store";
import { ref, onMounted } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import { ErrorMessage, Field, useForm } from "vee-validate";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { useRoute, useRouter } from "vue-router";
import i18n from "@/core/plugins/i18n";
import { isMobile } from "@/core/config/WindowConfig";
import { getTenantLogo, getTenancy, tenancies } from "@/core/types/TenantTypes";
import UiRipple from "../../components/ripple/UiRipple.vue";

const { t } = i18n.global;
const store = useStore();
const route = useRoute();
const router = useRouter();
const isLoading = ref(true);
const accountNumber = ref<any>("");
const tenantId = ref<any>("");
const passwordShow_1 = ref(false);
const passwordShow_2 = ref(false);
const formData = ref<any>({
  password: "",
});

const confirmPassword = ref("");

const validationSchema = Yup.object().shape({
  password: Yup.string()
    .min(8, t("error.signup_ps_8"))
    .max(16, "Password must be less than 16 characters")
    .matches(/[a-z]/, t("error.signup_ps_lower"))
    .matches(/[A-Z]/, t("error.signup_ps_upper"))
    .matches(/\d/, t("error.signup_ps_number"))
    .matches(/[!@#$%^&*(),.?":{}|<>]/, t("error.signup_ps_symbol"))
    .required(t("error.signup_ps_require")),
  confirmPassword: Yup.string()
    .required(t("error.signup_cps_require"))
    .oneOf([Yup.ref("password"), null], t("error.signup_ps_match")),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const initForm = () => {
  resetForm();
};

//form submit function
const resetPassword = handleSubmit(async () => {
  try {
    isLoading.value = true;
    await GlobalService.submitResetTradeAccountPassword(
      tenantId.value,
      formData.value
    );

    MsgPrompt.success(t("tip.passwordResetSuccess"))
      .then(() => store.dispatch(Actions.LOGOUT))
      .then(() => router.push({ name: "sign-in" }));
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
});

onMounted(async () => {
  isLoading.value = true;
  await store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
  accountNumber.value = route.query.an;
  tenantId.value = route.params.tenantId;
  formData.value = {
    referenceId: route.query.uid,
    partyId: route.query.pid,
    token: route.query.token,
    password: "",
  };

  initForm();
  isLoading.value = false;
});
</script>

<style>
#reset-form {
  width: 100vw;
  height: 100vh;

  display: flex;
}

#reset-form .side {
  display: flex;
  flex-direction: column;
  justify-content: space-between;

  height: 100vh;
  width: 30%;
  max-width: 483px;

  background-image: url("/images/auth/login-side-bg.png");
  background-size: cover;
  background-repeat: no-repeat;

  padding-top: 100px;
  padding-bottom: 100px;
}

#reset-form .side-text-wrap {
  margin: 0 auto;
  width: 280px;
}

#reset-form .welcome-text {
  color: white;
  font-weight: 400;
  font-size: 16px;
  line-height: 22px;
  font-family: "Lato", sans-serif;
}

#reset-form .logo {
  width: 280px;
}

#reset-form .switch {
  display: flex;
  justify-content: center;
  align-items: center;
  color: white;
  font-size: 14px;
}

/* ================================= */
#reset-form .form-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
}

#reset-form .reset-form {
  width: 472px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  font-family: "Lato", sans-serif;
}

#reset-form .reset-form .title {
  font-family: "Lato", sans-serif;
  font-weight: 600;
  font-size: 32px;
  color: #393d48;
  text-align: center;
  margin-bottom: 32px;
}

#reset-form .reset-form .label {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 14px;
  color: #666666;
}

#reset-form .reset-form .eye-icon {
  cursor: pointer;
  margin-top: 10px;
  margin-right: 8px;
}

#reset-form .reset-form .forgot {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 12px;
  text-align: right;
  text-decoration-line: underline;
  color: #666666;
  cursor: pointer;
}

#reset-form .reset-form .loginBtn {
  box-sizing: border-box;
  color: white;

  width: 472px;
  height: 64px;
  margin-top: 25px;
  background: #393d48;
  border: 1px solid #393d48;
  border-radius: 8px;
}

#reset-form .reset-form .msg_1 {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 12px;
  text-align: center;

  color: #111111;
  margin-top: 24px;
}

#reset-form .reset-form .msg_2 {
  font-family: "Lato", sans-serif;
  font-weight: 400;
  font-size: 12px;
  text-align: center;

  color: #111111;
  margin-top: 140px;
}

#reset-form .company-title {
  position: absolute;
  left: 110px;
  top: 39px;
}

#reset-form .line-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
}

#reset-form .line {
  flex-grow: 1;
  height: 1px;
  background-color: black;
}

#reset-form .or-text {
  padding: 0 20px;
  font-size: 1.2em;
  color: #666666;
}
</style>
