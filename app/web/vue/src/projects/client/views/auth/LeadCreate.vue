<template>
  <div class="authentication-login-group auth-page-wrapper">
    <!-- <div class="side">
      <div class="side-text-wrap">
        <div class="welcome-text text-uppercase">Welcome To</div>
        <a href="/"
          ><img class="logo" src="/images/auth/login-logo.png" alt=""
        /></a>
      </div>

      <div class="switch">
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
      </div>
    </div> -->
    <div class="absolute top-3 left-12">
      <img class="h-12 w-12" alt="Logo" :src="getTenantLogo['src']" />
    </div>
    <div class="absolute top-12 right-20 flex">
      <LanguageDropdown />
    </div>
    <div class="form-container">
      <!-- <div>
        <img class="mobile-logo" src="/images/auth/login-logo.png" alt="" />
      </div>

      <div class="auth-language-dropdown">
        <LanguageDropdown />
      </div> -->
      <!-- @submit="onSubmitCreate" -->
      <div class="auth-box">
        <!-- <div class="flex text-xl text-center leading-[4.375rem]">
          <div class="flex-1 text-black">
            {{ $t("action.login") }}
          </div>
          <div
            class="flex-1 bg-[#F2F4F7] rounded-tr-3xl cursor-pointer"
            @click="toggleAuthType"
          >
            {{ $t("action.register") }}
          </div>
        </div> -->
        <div class="auth-box-header">
          <span class="text-3xl font-bold"> {{ $t("title.contactUs") }}</span>
        </div>
        <div class="auth-form">
          <el-form
            class="login-form"
            id="kt_register_signup_form"
            :model="formData"
            :rules="formRule"
            ref="leadFormRef"
            style="min-width: 200px"
          >
            <!-- <div class="mb-10 mt-lg-0 mt-10">
              <h1 class="mb-3" style="color: #000000e0">
                {{ $t("title.contactUs") }}
              </h1>
            </div> -->

            <!-- First Name -->
            <div class="row mb-6">
              <div class="col-lg-6">
                <el-form-item prop="name">
                  <el-input
                    size="large"
                    v-model="formData.name"
                    :placeholder="$t('fields.fullName')"
                  ></el-input>
                </el-form-item>
              </div>
              <div class="col-lg-6">
                <el-form-item prop="email">
                  <el-input
                    size="large"
                    v-model="formData.email"
                    :placeholder="$t('fields.email')"
                  ></el-input>
                </el-form-item>
              </div>
            </div>

            <div class="row mb-6">
              <div class="col-lg-6">
                <el-form-item prop="countryCode">
                  <el-select
                    v-model="contryCode"
                    :placeholder="$t('tip.selectCountry')"
                    size="large"
                    filterable
                  >
                    <el-option
                      v-for="(item, index) in phoneCountryData"
                      :key="index"
                      :value="item.dialCode"
                      :label="item.name"
                    >
                    </el-option>
                  </el-select>
                </el-form-item>
              </div>
              <div class="col-lg-6">
                <div class="row">
                  <div class="col-4">
                    <el-input
                      v-model="contryCode"
                      size="large"
                      :readonly="true"
                    >
                    </el-input>
                  </div>
                  <div class="col-8">
                    <el-form-item prop="phone">
                      <el-input
                        type="tel"
                        v-model="formData.phoneNumber"
                        size="large"
                        :placeholder="$t('fields.phone')"
                      ></el-input>
                    </el-form-item>
                  </div>
                </div>
              </div>
            </div>

            <div class="row mb-10">
              <div class="col-lg-12">
                <el-form-item prop="note">
                  <el-input
                    v-model="formData.note"
                    :autosize="{ minRows: 2, maxRows: 12 }"
                    type="textarea"
                    size="large"
                    :placeholder="$t('fields.notes')"
                  ></el-input>
                </el-form-item>
              </div>
            </div>

            <div class="mb-10" style="margin: 0 auto">
              <button class="btn loginBtn" @click.prevent="submit(leadFormRef)">
                <span v-if="!isLoading" class="indicator-label">
                  {{ $t("action.sendToUs") }}
                </span>

                <span v-else class="indicator-progress">
                  Please wait...
                  <span
                    class="spinner-border spinner-border-sm align-middle ms-2"
                  ></span>
                </span>
              </button>

              <div class="text-center mt-5" style="font-size: 16px">
                <router-link to="/sign-in">{{
                  $t("action.backToLogin")
                }}</router-link>
              </div>
            </div>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, reactive } from "vue";
import { Actions } from "@/store/enums/StoreEnums";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import Swal from "sweetalert2/dist/sweetalert2.min.js";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import PhoneCountryData from "@/core/data/phonesData";
import LanguageDropdown from "../../components/LanguageDropdown.vue";
import AuthSwitch from "@/projects/client/components/AuthSwitch.vue";
import { type FormRules, FormInstance } from "element-plus";
import { getTenantLogo, getTenancy, tenancies } from "@/core/types/TenantTypes";

const store = useStore();
const router = useRouter();
const isLoading = ref(false);

const contryCode = ref("");
const leadFormRef = ref<FormInstance>();
const submitButton = ref<HTMLButtonElement | null>(null);
const phoneCountryData = ref(PhoneCountryData);

const formData = ref({
  name: "",
  email: "",

  phoneNumber: "",
  note: "",
});

const formRule = reactive({
  name: [
    {
      required: true,
      message: "Please enter your name",
      trigger: "blur",
    },
  ],
  email: [
    {
      required: true,
      message: "Please enter your email",
      trigger: "blur",
    },
    {
      type: "email",
      message: "Please enter a correct email",
      trigger: "blur",
    },
  ],
  countryCode: [
    {
      required: false,
      message: "Please select your country",
      trigger: "blur",
    },
  ],
  phone: [
    {
      required: false,
      type: "number",
      message: "Please enter your phone number",
      trigger: "blur",
    },
  ],
  note: [
    {
      required: true,
      message: "Please enter your notes",
      trigger: "blur",
    },
  ],
});

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

const authType = ref(false);
const toggleAuthType = (_authType) => {
  authType.value = _authType;
  router.push({ name: "sign-up" });
};

const submit = async (formEl: FormInstance | undefined) => {
  isLoading.value = true;

  try {
    await formEl?.validate((valid) => {
      if (valid) {
        if (contryCode.value !== "" && formData.value.phoneNumber !== "") {
          formData.value.phoneNumber =
            contryCode.value + formData.value.phoneNumber;
        }

        GlobalService.createLead(formData.value);
        Swal.fire({
          text: "We have received your information and will contact you soon.",
          icon: "success",
          buttonsStyling: false,
          confirmButtonText: "Ok!",
          customClass: {
            confirmButton: "btn fw-semobold btn-light-primary",
          },
        }).then(function () {
          // Go to page after successfully login
          router.push("/");
        });
      }
    });
  } catch (error: any) {
    Swal.fire({
      text: error.message,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Try again!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });
  }

  isLoading.value = false;
};

const onSubmitCreate = async (values) => {
  if (!submitButton.value) {
    return;
  }

  submitButton.value.disabled = true;
  submitButton.value.setAttribute("data-kt-indicator", "on");

  values.confirmUrl = "temporaryConfirmUrl";

  // Send register request
  try {
    await GlobalService.createLead(formData.value);
    Swal.fire({
      text: "We have received your information and will contact you soon.",
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: "Ok!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    }).then(function () {
      // Go to page after successfully login
      router.push("/");
    });
  } catch (error: any) {
    Swal.fire({
      text: error.message,
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: "Try again!",
      customClass: {
        confirmButton: "btn fw-semobold btn-light-danger",
      },
    });
  }

  submitButton.value.disabled = false;
  submitButton.value.setAttribute("data-kt-indicator", "off");
};
</script>
