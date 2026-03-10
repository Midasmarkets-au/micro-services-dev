<template>
  <div>
    <form
      @submit.prevent="submitFeedback"
      class="form fv-plugins-bootstrap5 fv-plugins-framework mb-5"
    >
      <!-- <h1 class="fw-bold text-dark mb-9 mx-auto">
        {{ $t("title.onlineEnquiry") }}
      </h1> -->
      <div class="row">
        <div class="col-lg-6 mb-5">
          <label class="fs-6 mb-2 required">{{ $t("fields.fullName") }}</label>
          <Field
            type="text"
            class="form-control form-control-solid bg-white"
            placeholder=""
            name="name"
            v-model="formData.name"
          />
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="name"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>

        <div class="col-lg-6 mb-5">
          <label class="fs-6 mb-2 required">{{ $t("title.email") }}</label>

          <Field
            type="text"
            class="form-control form-control-solid bg-white"
            placeholder=""
            name="email"
            v-model="formData.email"
          />
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="email"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>

        <div class="col-lg-12 mb-5">
          <label class="fs-6 mb-2 required">{{ $t("fields.subject") }}</label>

          <Field
            class="form-control form-control-solid bg-white"
            placeholder=""
            name="subjects"
            v-model="formData.subjects"
          />
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="subjects"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
        <div class="col-lg-12 mb-5">
          <label class="fs-6 mb-2 required">{{ $t("fields.enquiry") }}</label>

          <Field
            class="form-control form-control-solid bg-white"
            rows="6"
            as="textarea"
            name="enquire"
            placeholder=""
            v-model="formData.enquire"
          >
          </Field>
          <ErrorMessage
            class="fv-plugins-message-container invalid-feedback"
            name="enquire"
            as="div"
          >
            {{ $t("tip.requiredField") }}
          </ErrorMessage>
        </div>
      </div>
      <div class="mt-8 mb-9 text-center">
        <button class="btn btn-primary btn-lg btn-radius" type="submit">
          <span class="indicator-label">{{ $t("action.submit") }}</span>

          <span class="indicator-progress">
            Please wait...
            <span
              class="spinner-border spinner-border-sm align-middle ms-2"
            ></span>
          </span>
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useForm, Field, ErrorMessage } from "vee-validate";
import supportService from "../../services/SupportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const formData = ref<any>({});

const validationSchema = {
  name: "required",
  email: "required",
  subjects: "required",
  enquire: "required",
};

const { handleSubmit, resetForm } = useForm({
  validationSchema,
});

const initFormData = () => {
  formData.value = {};
  resetForm();
};

const isLoading = ref(true);

const submitFeedback = handleSubmit(async () => {
  try {
    isLoading.value = true;
    await supportService.postUserFeedback({
      name: formData.value.name,
      email: formData.value.email,
      phoneNumber: "",
      content: formData.value.subjects + "\n" + formData.value.enquire,
    });
    MsgPrompt.success(t("tip.enquireSubmitSuccess")).then(() => {
      initFormData();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
});

onMounted(() => {
  initFormData();
  setTimeout(() => {
    isLoading.value = false;
  }, 200);
});
</script>
