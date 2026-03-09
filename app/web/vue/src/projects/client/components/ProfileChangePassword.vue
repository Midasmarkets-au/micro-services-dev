<template>
  <div class="card">
    <!--begin::Content-->
    <div id="kt_profile_change_password">
      <form
        id="kt_profile_change_password_form"
        class="form"
        :validation-schema="validationSchema"
        @submit.prevent="onResetPassword"
      >
        <div class="card-body p-9">
          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6"
              >Old Password</label
            >
            <div class="d-flex col-lg-8 fv-row">
              <Field
                :type="eyeClick ? 'text' : 'password'"
                id="currentPassword"
                name="oldPassword"
                class="form-control form-control-lg form-control-solid"
                v-model="currentPassword"
              />
              <span
                style="
                  cursor: pointer;
                  margin-left: -40px;
                  margin-top: 15px;
                  height: 25px;
                "
                ><i
                  class="fa-solid fa-lg"
                  :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                  @click="eyeClick = !eyeClick"
                ></i
              ></span>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6"
              >New Password</label
            >
            <div class="col-lg-8 fv-row">
              <div class="d-flex">
                <Field
                  v-model="newPassword"
                  name="newPassword"
                  :type="eyeClick ? 'text' : 'password'"
                  class="form-control form-control-lg form-control-solid"
                />
                <span
                  style="
                    cursor: pointer;
                    margin-left: -40px;
                    margin-top: 15px;
                    height: 25px;
                  "
                  ><i
                    class="fa-solid fa-lg"
                    :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                    @click="eyeClick = !eyeClick"
                  ></i
                ></span>
              </div>

              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="newPassword" />
                </div>
              </div>
            </div>
          </div>
          <!--end::Input group-->

          <!--begin::Input group-->
          <div class="row mb-6">
            <label class="col-lg-4 col-form-label required fw-semibold fs-6">{{
              $t("forgotPassword.confirmPassword")
            }}</label>

            <div class="col-lg-8 fv-row">
              <div class="d-flex">
                <Field
                  :type="eyeClick ? 'text' : 'password'"
                  name="confirmPassword"
                  class="form-control form-control-lg form-control-solid"
                />
                <span
                  style="
                    cursor: pointer;
                    margin-left: -40px;
                    margin-top: 15px;
                    height: 25px;
                  "
                  ><i
                    class="fa-solid fa-lg"
                    :class="eyeClick ? 'fa-eye-slash' : 'fa-eye'"
                    @click="eyeClick = !eyeClick"
                  ></i
                ></span>
              </div>
              <div class="fv-plugins-message-container">
                <div class="fv-help-block">
                  <ErrorMessage name="confirmPassword" />
                </div>
              </div>
            </div>
          </div>
          <!--end::Input group-->
        </div>

        <!--begin::Actions-->
        <div class="card-footer d-flex justify-content-end py-6 px-9">
          <button
            type="reset"
            class="btn btn-light btn-active-light-primary me-6"
          >
            Discard
          </button>

          <button
            ref="submitButtonRef"
            type="submit"
            id="kt_modal_new_password_submit"
            class="btn btn-primary"
          >
            <span class="indicator-label">
              {{ $t("action.update") }}
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
import { useI18n } from "vue-i18n";

const eyeClick = ref(false);
const newPassword = ref("");
const currentPassword = ref("");
const submitButtonRef = ref<null | HTMLButtonElement>(null);
const { t } = useI18n();
const validationSchema = Yup.object().shape({
  newPassword: Yup.string()
    .matches(
      /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$/,
      "Password must include at least 6 characters include one uppercase letter, one lowercase letter, one number, and one special character"
    )
    .min(6)
    .required(t("error.signup_ps_require")),
  confirmPassword: Yup.string().oneOf(
    [Yup.ref("newPassword"), null],
    "Passwords not match"
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
    await axios.post("/api/v1/user/password/change", {
      currentPassword: currentPassword.value,
      newPassword: newPassword.value,
    });

    Swal.fire({
      text: "Your password has been reset successfully!",
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    }).then(() => {
      window.location.href = "/profile";
    });
  } catch (error) {
    Swal.fire({
      text: error,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Something Wrong!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });

    console.error(error);
  }

  submitButtonRef.value.disabled = false;
  submitButtonRef.value.setAttribute("data-kt-indicator", "off");
});
</script>
