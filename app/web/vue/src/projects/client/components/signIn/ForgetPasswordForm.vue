<template>
  <Form
    class="forgot-password-form"
    @submit="onResetPassword"
    :validation-schema="reset"
  >
    <div class="title">
      {{ $t("title.resetPassword") }}
    </div>
    <div class="fv-row mb-9">
      <label class="label required mb-1">{{ $t("fields.email") }}</label>

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

    <div>
      <button
        tabindex="3"
        type="submit"
        ref="submitButton"
        id="kt_sign_in_submit"
        class="btn btn-lg w-100 mb-5 text-light loginBtn"
      >
        <span class="indicator-label">
          {{ $t("action.send") }}
        </span>

        <span class="indicator-progress">
          {{ $t("action.pleaseWait") }}
          <span
            class="spinner-border spinner-border-sm align-middle ms-2"
          ></span>
        </span>
      </button>
      <div class="text-center" style="font-size: 16px">
        <a href="#" @click="showPage = 'LoginPage'">{{
          $t("action.backToLogin")
        }}</a>
      </div>
    </div>
  </Form>
</template>
<script lang="ts" setup>
import { ref, inject } from "vue";
import * as Yup from "yup";
import { Actions } from "@/store/enums/StoreEnums";
import Swal from "sweetalert2/dist/sweetalert2.min.js";
import { useStore } from "@/store";
import axios from "axios";
import i18n from "@/core/plugins/i18n";
import { ErrorMessage, Field, Form } from "vee-validate";

const store = useStore();
const showPage = inject<any>("showPage");
const formData = ref<any>({});
const submitButton = ref<HTMLButtonElement | null>(null);
const { t } = i18n.global;

const reset = Yup.object().shape({
  email: Yup.string().email().required(t("tip.emailRequire")).label("Email"),
});

const onResetPassword = async (values) => {
  await store.dispatch(Actions.LOGOUT);

  if (!submitButton.value) {
    return;
  }

  submitButton.value.disabled = true;
  submitButton.value.setAttribute("data-kt-indicator", "on");

  try {
    var resetUrl =
      window.location.protocol +
      "//" +
      window.location.host +
      "/reset-password";
    await axios.post("/api/v1/auth/password/forgot", {
      email: values.email,
      ResetUrl: resetUrl,
    });
    // "Password reset link has been sent to your email."
    Swal.fire({
      text: t("tip.passwordResetLinkSent"),
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    });

    showPage.value = "LoginPage";
  } catch (error) {
    Swal.fire({
      text: error,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Try again!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });
    // console.error(error);
  }

  submitButton.value.disabled = false;
  submitButton.value.setAttribute("data-kt-indicator", "off");
};
</script>
