<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="createEmailTemplateModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content">
        <el-form
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          id="kt_modal_add_event_form"
          @submit.prevent="createEmailTemplate"
          :model="detailForm"
          :rules="emailFormRules"
          ref="ruleFormRef"
        >
          <div class="modal-header">
            <h2 class="fw-bold">{{ $t("title.addNewEmailTemplate") }}</h2>
            <div
              class="btn btn-icon btn-sm btn-active-icon-primary"
              id="kt_modal_add_event_close"
              data-bs-dismiss="modal"
            >
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>
          <div class="modal-body flex-center">
            <el-form-item :label="$t('fields.title')" prop="title">
              <el-input v-model="detailForm.title" />
            </el-form-item>
          </div>
          <div class="modal-footer flex-center">
            <button
              data-bs-dismiss="modal"
              type="reset"
              id="kt_modal_add_event_cancel"
              class="btn btn-light me-3"
            >
              {{ $t("status.cancel") }}
            </button>
            <button
              :data-kt-indicator="isLoading ? 'on' : null"
              class="btn btn-lg btn-primary"
              type="submit"
              :disabled="isLoading"
            >
              <span v-if="!isLoading" class="indicator-label">
                {{ $t("action.submit") }}
                <span class="svg-icon svg-icon-3 ms-2 me-0">
                  <inline-svg src="/images/icons/arrows/arr064.svg" />
                </span>
              </span>
              <span v-if="isLoading" class="indicator-progress">
                {{ $t("title.inProgress") }}
                <span
                  class="spinner-border spinner-border-sm align-middle ms-2"
                ></span>
              </span>
            </button>
          </div>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from "vue";
import { TopicTypes } from "@/core/types/TopicTypes";
import GlobalService from "../../../services/TenantGlobalService";
import type { FormRules } from "element-plus";
import { hideModal, showModal } from "@/core/helpers/dom";

const isLoading = ref(true);
const submited = ref(false);

const createEmailTemplateModalRef = ref<null | HTMLElement>(null);

const emits = defineEmits<{
  (e: "eventSubmit", type: TopicTypes): void;
}>();

const detailForm = ref({
  title: "",
  type: TopicTypes.Email,
  language: "en-us",
  content: "content",
  author: "system",
  effectiveFrom: "",
  effectiveTo: "",
});

const emailFormRules = reactive<FormRules>({
  title: [{ required: true, message: "Please input the email template key" }],
});

const createEmailTemplate = async () => {
  submited.value = true;
  await GlobalService.createEmailTemplate(detailForm.value);
  emits("eventSubmit");
  submited.value = false;
  hide();
};

const show = () => {
  // initialize topic selectins list by TopicType enum
  detailForm.value = {
    title: "",
    type: TopicTypes.Email,
    language: "en-us",
    content: "content",
    author: "system",
    effectiveFrom: "",
    effectiveTo: "",
  };
  showModal(createEmailTemplateModalRef.value);
  isLoading.value = false;
};

const hide = () => {
  hideModal(createEmailTemplateModalRef.value);
};

defineExpose({
  show,
});
</script>
