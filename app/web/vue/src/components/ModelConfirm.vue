<template>
  <div
    class="modal fade"
    :id="props.elId"
    ref="modelRef"
    tabindex="-1"
    aria-hidden="true"
  >
    <div class="modal-dialog modal-dialog-centered mw-320px">
      <div class="modal-content">
        <div class="modal-header" :id="props.elId + 'header'">
          <h2 class="fw-bold">{{ title }} {{ "ref_" + props.elId }}</h2>
          <div
            :id="props.elId + 'close'"
            data-bs-dismiss="modal"
            class="btn btn-icon btn-sm btn-active-icon-primary"
          >
            <span class="svg-icon svg-icon-1">
              <inline-svg src="/images/icons/arrows/arr061.svg" />
            </span>
          </div>
        </div>
        <div class="modal-body py-10 px-lg-17">
          <div
            class="scroll-y me-n7 pe-7"
            :id="props.elId + 'scroll'"
            data-kt-scroll="true"
            data-kt-scroll-activate="{default: false, lg: true}"
            data-kt-scroll-max-height="auto"
            :data-kt-scroll-dependencies="'#' + props.elId + '_header'"
            :data-kt-scroll-wrappers="'#' + props.elId + '_scroll'"
            data-kt-scroll-offset="300px"
          >
            <slot></slot>
          </div>
        </div>

        <div class="modal-footer flex-center">
          <button
            id="model_form_close"
            class="btn btn-light me-3"
            data-bs-dismiss="modal"
            @click.prevent="cancel"
          >
            {{ props.cancelTitle ? props.cancelTitle : $t("action.discard") }}
          </button>
          <button
            id="model_form_submit"
            class="btn btn-primary"
            @click="ok"
            :disabled="props.submited"
          >
            <span v-if="submited">
              {{ props.submitTitle ? props.submitTitle : $t("action.saving") }}
              <span
                class="spinner-border h-15px w-15px align-middle text-gray-400"
              ></span>
            </span>
            <span v-else>
              {{ props.okTitle ? props.okTitle : $t("action.save") }}
            </span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";

const props = defineProps({
  title: { type: String, required: true },
  submited: { type: Boolean, required: true },
  okTitle: String,
  submitTitle: String,
  cancelTitle: String,
  cancel: { type: Function, required: false },
  ok: { type: Function, required: true },
  elId: { type: String, required: true },
});
const modelRef = ref(null);

const hide = () => {
  hideModal(modelRef.value);
};

const show = () => {
  console.log("show model");
  showModal(modelRef.value);
};

const ok = () => {
  props.ok();
};

const cancel = () => {
  if (props.cancel) {
    props.cancel();
  }
};
defineExpose({
  hide,
  show,
});
</script>
