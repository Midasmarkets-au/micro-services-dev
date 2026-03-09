<template>
  <!--begin::Wrapper-->
  <div class="w-lg-500px p-10">
    <!--begin::form-->
    <div class="form w-100" @submit="onSubmitLogin">
      <!--begin::Heading-->
      <div class="text-center mb-10">
        <!--begin::Title-->
        <h1 class="text-dark mb-3">{{ $t("tip.twoFA") }} Backend</h1>
        <!--end::Title-->
      </div>
      <!--begin::Input group-->
      <div class="fv-row mb-10">
        <!--begin::Label-->
        <label class="form-label fs-6 fw-bold text-dark">
          {{ $t("fields.code") }}
        </label>
        <!--end::Label-->

        <!--begin::Input-->
        <Field
          v-model="twoFaCode"
          tabindex="1"
          class="form-control form-control-lg form-control-solid"
          type="text"
          name="code"
          autocomplete="off"
        />
        <!--end::Input-->
        <div class="fv-plugins-message-container">
          <div class="fv-help-block">
            <ErrorMessage name="code" />
          </div>
        </div>
      </div>
      <!--end::Input group-->

      <!--begin::Actions-->

      <LoadingButton
        @click="submit2FaCode(twoFaCode)"
        class="btn btn-lg btn-primary w-100 mb-5"
        :is-loading="isLoading"
        :save-title="$t('action.continue')"
      >
      </LoadingButton>

      <!--end::Actions-->
    </div>
    <!--end::form-->
  </div>
  <!--end::Wrapper-->
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { ErrorMessage, Field } from "vee-validate";
import { Actions } from "@/store/enums/StoreEnums";
import { useStore } from "@/store";
import { useRouter } from "vue-router";
import Swal from "sweetalert2/dist/sweetalert2.min.js";
import { useI18n } from "vue-i18n";
import LoadingButton from "@/components/buttons/LoadingButton.vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const store = useStore();
console.log(store.state.AuthModule.hasUserVerified2Fa);
const router = useRouter();
const { t } = useI18n();
const submitButton = ref<HTMLButtonElement | null>(null);

onMounted(() => {
  store.dispatch(Actions.REMOVE_BODY_CLASSNAME, "page-loading");
});

const twoFaCode = ref("");

const isLoading = ref(false);
const submit2FaCode = async (verificationCode: string) => {
  isLoading.value = true;
  try {
    await TenantGlobalService.verify2FaVerificationCode(verificationCode);
    MsgPrompt.success("Two Factor Authentication Verification Succeed")
      .then(async () => await store.dispatch(Actions.TWOFA_VERIFIED))
      .then(() => router.push({ name: "dashboard" }));
  } catch (error) {
    MsgPrompt.error(error || "Error");
  } finally {
    isLoading.value = false;
  }
};
//form submit function
const onSubmitLogin = async (values) => {
  if (submitButton.value) {
    // eslint-disable-next-line
    submitButton.value!.disabled = true;
    // Activate indicator
    submitButton.value.setAttribute("data-kt-indicator", "on");
  }

  // Send 2fa request
  await store.dispatch(Actions.TWOFA, values);
  const [errorName] = Object.keys(store.getters.getErrors);
  const error = store.getters.getErrors[errorName];
  if (!error) {
    Swal.fire({
      text: t("common.logInSucceed"),
      icon: "success",
      buttonsStyling: false,
      confirmButtonText: t("tip.gotIt"),
      customClass: {
        confirmButton: "btn fw-semobold btn-light-primary",
      },
    }).then(function () {
      // Go to page after successfully login
      router.push({ name: "dashboard" });
    });
  } else {
    Swal.fire({
      text: error[0],
      icon: "error",
      buttonsStyling: false,
      confirmButtonText: t("tip.tryAgain"),
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
</script>
