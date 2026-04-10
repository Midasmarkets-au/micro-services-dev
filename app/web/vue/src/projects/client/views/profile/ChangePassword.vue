<template>
  <div class="card shadow-sm">
    <!--begin::Content-->
    <div class="card-header">
      <div class="card-title">{{ $t("action.changePassword") }}</div>
    </div>
    <div id="kt_profile_change_password" class="card-body">
      <form
        id="kt_profile_change_password_form"
        class="form"
        :validation-schema="validationSchema"
        @submit.prevent="onResetPassword"
      >
        <!--begin::Input group-->
        <div class="row">
          <div class="col-lg-12">
            <div class="mb-5">
              <div class="d-lg-flex align-items-center">
                <label class="required mb-3 w-125px text-md-end mr-2">{{
                  $t("fields.oldPwd")
                }}</label>
                <div class="position-relative flex-1">
                  <Field
                    :type="eyeClick ? 'text' : 'password'"
                    name="oldPassword"
                    class="form-control form-control-solid"
                    v-model="currentPassword"
                  />
                  <i
                    class="fa-solid fa-lg cursor-pointer position-absolute"
                    :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                    @click="eyeClick = !eyeClick"
                    style="top: 50%; right: 15px; transform: translateY(-50%)"
                  ></i>
                </div>
              </div>
              <div class="fv-plugins-message-container md:pl-[130px]">
                <div class="fv-help-block">
                  <ErrorMessage name="oldPassword" />
                </div>
              </div>
            </div>

            <div class="mb-5">
              <div class="d-lg-flex align-items-center">
                <label class="required mb-3 w-125px text-md-end mr-2">{{
                  $t("fields.newPwd")
                }}</label>
                <div class="position-relative flex-1">
                  <Field
                    v-model="newPassword"
                    name="newPassword"
                    :type="eyeClick ? 'text' : 'password'"
                    class="form-control form-control-solid"
                  />
                  <i
                    class="fa-solid fa-lg cursor-pointer position-absolute"
                    :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                    @click="eyeClick = !eyeClick"
                    style="top: 50%; right: 15px; transform: translateY(-50%)"
                  ></i>
                </div>
              </div>
              <div class="fv-plugins-message-container md:pl-[130px]">
                <div class="fv-help-block">
                  <ErrorMessage name="newPassword" />
                </div>
              </div>
            </div>

            <div class="mb-5">
              <div class="d-lg-flex align-items-center">
                <label class="required mb-3 w-125px text-md-end mr-2">{{
                  $t("fields.confirmPassword")
                }}</label>
                <div class="position-relative flex-1">
                  <Field
                    :type="eyeClick ? 'text' : 'password'"
                    name="confirmPassword"
                    class="form-control form-control-solid"
                  />
                  <i
                    class="fa-solid fa-lg cursor-pointer position-absolute"
                    :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                    @click="eyeClick = !eyeClick"
                    style="top: 50%; right: 15px; transform: translateY(-50%)"
                  ></i>
                </div>
              </div>
              <div class="fv-plugins-message-container md:pl-[130px]">
                <div class="fv-help-block">
                  <ErrorMessage name="confirmPassword" />
                </div>
                <div
                  class="fs-8 mt-2 text-warning bg-warning bg-opacity-10 px-3 py-2 rounded"
                >
                  {{ $t("tip.passwordUpdateWithdrawLock24h") }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!--end::Input group-->
        <div class="d-flex gap-5 flex-end mt-5">
          <button
            class="btn btn-sm btn-light btn-bordered btn-radius"
            type="reset"
            @click="resetForm()"
          >
            {{ $t("action.reset") }}
          </button>
          <button
            ref="submitButtonRef"
            class="btn btn-sm btn-primary btn-bordered btn-radius"
            type="submit"
          >
            <span class="indicator-label">
              {{ $t("action.saveChanges") }}
            </span>

            <span class="indicator-progress">
              {{ $t("action.pleaseWait") }}
              <span
                class="spinner-border spinner-border-sm align-middle ms-2"
              ></span>
            </span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import * as Yup from "yup";
import Swal from "sweetalert2/dist/sweetalert2.js";
import { Field, ErrorMessage, useForm } from "vee-validate";
import { axiosInstance as axios } from "@/core/services/api.client";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";

const eyeClick = ref(false);
const { t } = i18n.global;

const newPassword = ref("");
const currentPassword = ref("");
const submitButtonRef = ref<null | HTMLButtonElement>(null);

const validationSchema = Yup.object().shape({
  oldPassword: Yup.string()
    .required(t("error.signup_ps_require"))
    .label("Old Password"),
  newPassword: Yup.string()
    .min(8, t("error.signup_ps_8"))
    .matches(/[a-z]/, t("error.signup_ps_lower"))
    .matches(/[A-Z]/, t("error.signup_ps_upper"))
    .matches(/\d/, t("error.signup_ps_number"))
    .matches(/[!@#$%^&*(),.?":{}|<>]/, t("error.signup_ps_symbol"))
    .required(t("error.signup_ps_require")),
  confirmPassword: Yup.string().oneOf(
    [Yup.ref("newPassword"), null],
    t("error.signup_ps_match")
  ),
});

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const onResetPassword = handleSubmit(async () => {
  if (!submitButtonRef.value) {
    return;
  }

  submitButtonRef.value.disabled = true;
  submitButtonRef.value.setAttribute("data-kt-indicator", "on");

  try {
    await axios.post("/api/v1/auth/password/change", {
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
    });
    MsgPrompt.success(t("tip.passwordResetSuccess")).then(() => {
      resetForm();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  submitButtonRef.value.disabled = false;
  submitButtonRef.value.setAttribute("data-kt-indicator", "off");
});
</script>
