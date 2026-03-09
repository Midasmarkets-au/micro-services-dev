<template>
  <div class="auth-box">
    <div class="auth-box-header">
      <span class="text-3xl font-bold">{{
        $t("title.startResetPassword")
      }}</span>
    </div>
    <div class="auth-form">
      <Form
        class="login-form"
        @submit="onResetPassword"
        :validation-schema="validationSchema"
      >
        <div class="mb-6">
          <label class="label">{{ $t("fields.email") }}</label>

          <Field
            tabindex="1"
            class="mt-1 form-control form-control-lg form-control-solid"
            type="text"
            name="email"
            v-model="formData.email"
            disabled
          />
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="email" />
            </div>
          </div>
        </div>

        <div class="mb-6">
          <div class="d-flex justify-content-between">
            <label class="label">{{ $t("fields.newPassword") }}</label>
            <div>
              <!-- <i
                class="fa-solid fa-lg eye-icon"
                :class="passwordShow_1 ? 'fa-eye-slash' : 'fa-eye'"
                @click="passwordShow_1 = !passwordShow_1"
              >
              </i>
              <span v-if="passwordShow_1">{{ $t("action.hide") }}</span
              ><span v-else>{{ $t("action.show") }}</span> -->
              <SvgIcon
                :name="passwordShow_1 ? 'eye_open' : 'eye'"
                @click="passwordShow_1 = !passwordShow_1"
              />
            </div>
          </div>

          <Field
            tabindex="2"
            class="mt-1 form-control form-control-lg form-control-solid"
            :type="passwordShow_1 ? 'text' : 'password'"
            name="password"
            autocomplete="off"
          />
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="password" />
            </div>
          </div>
        </div>

        <div class="mb-6">
          <div class="d-flex justify-content-between">
            <label class="label">{{ $t("fields.confirmPassword") }}</label>
            <div>
              <!-- <i
                class="fa-solid fa-lg eye-icon"
                :class="passwordShow_2 ? 'fa-eye-slash' : 'fa-eye'"
                @click="passwordShow_2 = !passwordShow_2"
              >
              </i>
              <span v-if="passwordShow_2">{{ $t("action.hide") }}</span
              ><span v-else>{{ $t("action.show") }}</span> -->
              <SvgIcon
                :name="passwordShow_2 ? 'eye_open' : 'eye'"
                @click="passwordShow_2 = !passwordShow_2"
              />
            </div>
          </div>

          <Field
            tabindex="2"
            class="mt-1 form-control form-control-lg form-control-solid"
            :type="passwordShow_2 ? 'text' : 'password'"
            name="confirmPassword"
            autocomplete="off"
          />
          <div class="fv-plugins-message-container">
            <div class="fv-help-block">
              <ErrorMessage name="confirmPassword" />
            </div>
          </div>
        </div>

        <div>
          <button
            tabindex="3"
            type="submit"
            ref="submitButton"
            id="kt_sign_in_submit"
            class="btn btn-lg w-100 loginBtn"
          >
            <span class="indicator-label"> {{ $t("action.reset") }} </span>

            <span class="indicator-progress">
              {{ $t("action.pleaseWait") }}
              <span
                class="spinner-border spinner-border-sm align-middle ms-2"
              ></span>
            </span>
          </button>

          <div class="msg_1 mb-4">
            {{ $t("action.backTo") }}
            <router-link to="/sign-in">{{ $t("action.login") }}</router-link>
          </div>
        </div>
      </Form>
    </div>
    <!--end::form-->
  </div>
</template>

<script lang="ts" setup>
import axios from "axios";
import * as Yup from "yup";
import { useStore } from "@/store";
import { ref, onMounted, inject } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import { ErrorMessage, Field, Form } from "vee-validate";
import i18n from "@/core/plugins/i18n";
import { useRouter } from "vue-router";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const { t } = i18n.global;
const store = useStore();
const passwordShow_1 = ref(false);
const passwordShow_2 = ref(false);
const submitButton = ref<HTMLButtonElement | null>(null);
const router = useRouter();

const formData = inject("data");
const step = inject("step");

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

const validationSchema = Yup.object().shape({
  email: Yup.string().email().required(t("tip.emailRequire")).label("Email"),
  password: Yup.string()
    .min(8, t("error.signup_ps_8"))
    .matches(/[a-z]/, t("error.signup_ps_lower"))
    .matches(/[A-Z]/, t("error.signup_ps_upper"))
    .matches(/\d/, t("error.signup_ps_number"))
    .matches(/[!@#$%^&*(),.?":{}|<>]/, t("error.signup_ps_symbol"))
    .required(t("error.signup_ps_require")),
  confirmPassword: Yup.string()
    .oneOf([Yup.ref("password"), null], t("error.signup_ps_match"))
    .required(t("error.signup_ps_require")),
});

//form submit function
const onResetPassword = async (values) => {
  await store.dispatch(Actions.LOGOUT);

  if (!submitButton.value) {
    return;
  }

  submitButton.value.disabled = true;
  submitButton.value.setAttribute("data-kt-indicator", "on");

  try {
    await axios.post("/api/v1/auth/password/reset", {
      email: formData.value.email,
      password: values.password,
      code: formData.value.code,
    });

    MsgPrompt.success(t("tip.passwordResetSuccess")).then(() => {
      store
        .dispatch(Actions.LOGOUT)
        .then(() => router.push({ name: "sign-in" }));
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  submitButton.value.disabled = false;
  submitButton.value.setAttribute("data-kt-indicator", "off");
};
</script>
