<template>
  <div class="auth-box">
    <div class="auth-box-header">
      <span class="text-3xl font-bold">{{ $t("title.resetPassword") }}</span>
    </div>
    <div class="auth-form">
      <Form
        class="login-form"
        @submit="onResetPassword"
        :validation-schema="validationSchema"
      >
        <!-- <div class="title">{{ $t("title.resetPassword") }}</div> -->

        <div class="mb-6">
          <label class="label required mb-3">{{ $t("fields.email") }}</label>

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
            class="loginBtn mt-4"
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
          <div class="text-center mt-4 mb-5" style="font-size: 16px">
            <a href="/sign-in">{{ $t("action.backToLogin") }}</a>
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
import Swal from "sweetalert2/dist/sweetalert2.min.js";

import { ErrorMessage, Field, Form } from "vee-validate";
import i18n from "@/core/plugins/i18n";
import { useRouter } from "vue-router";

const { t } = i18n.global;
const store = useStore();
const submitButton = ref<HTMLButtonElement | null>(null);
const router = useRouter();
const formData = inject("data");
const step = inject("step");
onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

const validationSchema = Yup.object().shape({
  email: Yup.string().email().required(t("tip.emailRequire")),
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
    await axios.post("/api/v2/auth/password-reset/code", {
      email: values.email,
    });

    Swal.fire({
      text: t("tip.verificationCodeSend"),
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: t("action.confirm"),
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    })
      .then(() => store.dispatch(Actions.LOGOUT))
      .then(() => (step.value = 1));
  } catch (error) {
    Swal.fire({
      text: error,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: t("error.unknown"),
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });
  }

  submitButton.value.disabled = false;
  submitButton.value.setAttribute("data-kt-indicator", "off");
};
</script>
