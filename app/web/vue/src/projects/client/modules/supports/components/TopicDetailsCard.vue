<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    aria-modal="true"
    role="dialog"
    ref="newTargetModalRef"
  >
    <div class="modal-dialog modal-dialog-centered mw-850px">
      <div
        class="modal-content"
        v-if="details != null"
        style="min-height: 700px"
      >
        <div
          class="form fv-plugins-bootstrap5 fv-plugins-framework w-100"
          id="kt_modal_add_event_form"
          :rules="rules"
        >
          <div class="modal-header border-0">
            <h2 class="fs-2">
              {{ details.title }}
              <div class="d-flex fw-normal fs-7 py-2" style="color: #b1b1b1">
                <span>{{ $t("fields.postedOn") }}:&nbsp;</span
                ><TimeShow :date-iso-string="details.updatedOn" />
                <span>&nbsp; • By MM Event Notice</span>
              </div>
            </h2>
            <div id="kt_modal_add_event_close" data-bs-dismiss="modal">
              <span class="svg-icon svg-icon-1">
                <inline-svg src="/images/icons/arrows/arr061.svg" />
              </span>
            </div>
          </div>

          <div class="modal-body px-10 w-100 overflow-auto">
            <div class="w-100 notice-content" v-html="details.content"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";

const newTargetModalRef = ref<null | HTMLElement>(null);
const rules = ref();
const details = ref<any>(null);

const show = async (_detail: any) => {
  details.value = _detail;
  showModal(newTargetModalRef.value);
};

const hide = () => {
  hideModal(newTargetModalRef.value);
};

defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}

.notice-content table {
  border: 1px double #b3b3b3;
  border-collapse: collapse;
  border-spacing: 0;
  height: 100%;
  width: 100%;
}

.notice-content table td {
  border: 1px solid #bfbfbf;
  min-width: 2em;
  padding: 0.4em;
  overflow-wrap: break-word;
  position: relative;
}

.notice-content table td:first-child {
  padding: 0.4em;
}
</style>
