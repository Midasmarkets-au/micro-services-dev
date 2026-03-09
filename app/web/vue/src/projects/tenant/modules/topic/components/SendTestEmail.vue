<template>
  <div
    class="modal fade"
    tabindex="9999"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="sendEmailModalRef"
    style="z-index: 9999 !important"
  >
    <div class="modal-dialog modal-dialog-centered mw-650px">
      <div class="modal-content">
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework"
          id="kt_modal_add_event_form"
        >
          <div class="modal-header">Send email template</div>
          <div class="modal-body py-10 px-lg-17">
            <el-form-item :label="$t('fields.email')" prop="email">
              <el-input v-model="emailAddress" />
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
              @click.prevent="submit"
              type="button"
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
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import GlobalService from "../../../services/TenantGlobalService";

const emailAddress = ref("");
const emailTitle = ref("");
const emailLang = ref("");
const isLoading = ref(true);
const sendEmailModalRef = ref("sendEmailModalRef");

const show = async (title, lang) => {
  emailTitle.value = title;
  emailLang.value = lang;
  showModal(sendEmailModalRef.value);
  isLoading.value = false;
};

const hide = () => {
  hideModal(sendEmailModalRef.value);
};

const submit = async () => {
  isLoading.value = true;
  let data = {
    to: emailAddress.value,
    title: emailTitle.value,
    language: emailLang.value,
  };
  await GlobalService.sendEmailTemplate(data);
  isLoading.value = false;
};

defineExpose({
  show,
  hide,
});
</script>
