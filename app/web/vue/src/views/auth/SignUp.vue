<template>
  <div v-if="isLoading">Loading</div>
  <div v-else class="w-lg-500px p-10">
    <Form
      class="form w-100"
      id="kt_register_signup_form"
      @submit="onSubmitRegister"
      :validation-schema="register"
    >
      <div class="text-center mb-10">
        <h1 class="text-dark mb-3">{{ $t("action.signup") }}</h1>
      </div>

      <!-- First Name -->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("signup.first_name")
        }}</label>
        <Field
          tabindex="1"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="first_name"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="first_name" />
          </div>
        </div>
      </div>

      <!-- Last Name -->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("signup.last_name")
        }}</label>
        <Field
          tabindex="2"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="last_name"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="last_name" />
          </div>
        </div>
      </div>

      <!--countryCode-->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("signup.country_code")
        }}</label>
        <Field
          tabindex="3"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="country_code"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="country_code" />
          </div>
        </div>
      </div>

      <!--phone-->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.phone")
        }}</label>
        <div class="row">
          <div class="col-lg-3 fv-row">
            <Field
              class="form-control form-control-lg form-control-solid"
              name="ccc"
              as="select"
            >
              <option value="" disabled>code</option>
              <option
                v-for="(item, index) in phoneDataList"
                :key="index"
                :value="computeIntToString(item.dialCode)"
              >
                + {{ item.dialCode }} {{ item.name }}
              </option>
            </Field>
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="ccc" />
              </div>
            </div>
          </div>
          <div class="col-lg-9 fv-row">
            <Field
              tabindex="4"
              class="form-control form-control-lg form-control-solid"
              type="text"
              name="phone"
              autocomplete="off"
            />
            <div class="fv-plugins-message-container">
              <div class="fv-help-block">
                <ErrorMessage name="phone" />
              </div>
            </div>
          </div>
        </div>
      </div>

      <!--currency-->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.currency")
        }}</label>
        <Field
          tabindex="5"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="currency"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="currency" />
          </div>
        </div>
      </div>

      <!--email-->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.email")
        }}</label>
        <Field
          tabindex="6"
          class="form-control form-control-lg form-control-solid"
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

      <!--referral-->
      <div class="fv-row mb-10">
        <label class="form-label fs-6 fw-bold text-dark">{{
          $t("fields.referralCode")
        }}</label>
        <Field
          tabindex="7"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="refer_code"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="refer_code" />
          </div>
        </div>
      </div>

      <!--password-->
      <div class="fv-row mb-10">
        <label class="form-label fw-bold text-dark fs-6 mb-0">{{
          $t("fields.password")
        }}</label>
        <Field
          tabindex="8"
          class="form-control form-control-lg form-control-solid"
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

      <!--confirm password-->
      <div class="fv-row mb-10">
        <label class="form-label fw-bold text-dark fs-6 mb-0">{{
          $t("fields.confirmedPassword")
        }}</label>
        <Field
          tabindex="9"
          class="form-control form-control-lg form-control-solid"
          type="password"
          name="password_confirmation"
          autocomplete="off"
        />
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="password_confirmation" />
          </div>
        </div>
      </div>
      <!--end::Input group-->

      <!--begin::Actions-->
      <div class="text-center">
        <!--begin::Submit button-->
        <button
          tabindex="10"
          type="submit"
          ref="submitButton"
          id="kt_sign_in_submit"
          class="btn btn-lg btn-primary w-100 mb-5"
        >
          <span class="indicator-label"> Continue </span>

          <span class="indicator-progress">
            Please wait...
            <span
              class="spinner-border spinner-border-sm align-middle ms-2"
            ></span>
          </span>
        </button>
        <!--end::Submit button-->

        <!--begin::Separator-->
        <div class="text-center text-muted text-uppercase fw-bold mb-5">or</div>
        <!--end::Separator-->

        <!--begin::Google link-->
        <a
          href="#"
          class="btn btn-flex flex-center btn-light btn-lg w-100 mb-5"
        >
          Sign In
        </a>
        <!--end::Google link-->
      </div>
      <!--end::Actions-->
    </Form>
    <!--end::form-->
  </div>
  <!--end::Wrapper-->
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import { ErrorMessage, Field, Form } from "vee-validate";
import JwtService from "@/core/services/JwtService";
import { Actions } from "@/store/enums/StoreEnums";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import { WSSignalR } from "@/core/plugins/signalr";
import Swal from "sweetalert2/dist/sweetalert2.min.js";
import * as Yup from "yup";
import phoneData from "@/core/data/phonesData";

const isLoading = ref<boolean>(false);
const store = useStore();
const router = useRouter();

const submitButton = ref<HTMLButtonElement | null>(null);
const wsSignalR = inject("wsPusher") as WSSignalR;
const phoneDataList = ref(phoneData);

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

//Create form validation object
const register = Yup.object().shape({
  first_name: Yup.string().required().label("First Name"),
  last_name: Yup.string().required().label("Last Name"),
  country_code: Yup.string().required().label("Nationality"),
  ccc: Yup.string().required().label("Code"),
  phone: Yup.string().required().label("Phone"),
  currency: Yup.string().required().label("Currency"),
  refer_code: Yup.string().label("Referral Code"),
  email: Yup.string().email().required().label("Email"),
  password: Yup.string().min(4).required().label("Password"),
  password_confirmation: Yup.string()
    .min(4)
    .required()
    .label("Confirm Password"),
});

const computeIntToString = (value: number) => {
  return value.toString();
};

//form submit function
const onSubmitRegister = async (values) => {
  console.log(values);
  // Clear existing errors
  await store.dispatch(Actions.LOGOUT);

  if (submitButton.value) {
    // eslint-disable-next-line
    submitButton.value!.disabled = true;
    // Activate indicator
    submitButton.value.setAttribute("data-kt-indicator", "on");
  }

  // Send register request
  await store.dispatch(Actions.REGISTER, values);
  const [errorName] = Object.keys(store.getters.getErrors);
  const error = store.getters.getErrors[errorName];
  if (!error) {
    wsSignalR.setup(JwtService.getToken());
    Swal.fire({
      text: "You have successfully registered!",
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok, got it!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    }).then(function () {
      // Go to page after successfully register
      router.push({ name: "sign-in" });
    });
  } else {
    Swal.fire({
      text: error,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Try again!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });
  }

  //Deactivate indicator
  submitButton.value?.removeAttribute("data-kt-indicator");
  // eslint-disable-next-line
  submitButton.value!.disabled = false;
};
/**
 {
  "language": "en-us",
  "confirmUrl": "http://client.localhost:8084/confirm-email",
  "otp": 771578,
  "email": "yixuan.yu+test090100@bacera.com",
  "password": "Pass!1234",
  "confirmPassword": "Pass!1234",
  "FirstName": "IT_test",
  "LastName": "IT",
  "countryCode": "us",
  "ccc": 1,
  "phone": "6263902933"
}
 */
</script>
