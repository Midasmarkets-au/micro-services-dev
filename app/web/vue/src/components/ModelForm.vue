<template>
  <div
    class="modal fade"
    tabindex="-1"
    aria-hidden="true"
    :id="props.elId"
    ref="modelRef"
  >
    <div
      class="modal-dialog modal-dialog-centered"
      :class="{
        [`mw-${props.width}px`]: width,
      }"
    >
      <div class="modal-content">
        <el-form
          @submit.prevent="submitForm()"
          :rules="props.rules"
          :model="props.formData"
          ref="formRef"
        >
          <div class="modal-header" :id="props.elId + 'header'">
            <h2 class="fw-bold">{{ title }}</h2>
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
            <slot></slot>

            <!-- <div
              class="scroll-y me-n7 pe-7"
              :id="props.elId + 'scroll'"
              data-kt-scroll="true"
              data-kt-scroll-activate="{default: false, lg: true}"
              data-kt-scroll-max-height="auto"
              :data-kt-scroll-dependencies="'#' + props.elId + '_header'"
              :data-kt-scroll-wrappers="'#' + props.elId + '_scroll'"
              data-kt-scroll-offset="300px"
            ></div> -->
          </div>

          <div class="modal-footer flex-center">
            <button
              :class="`btn btn-${props.discardColor} me-3`"
              @click.prevent="props.discard"
            >
              {{
                props.discardTitle ? props.discardTitle : $t("action.discard")
              }}
            </button>
            <button
              :class="`btn btn-${props.submitColor} me-3`"
              type="submit"
              :disabled="props.submited || props.isLoading"
            >
              <span v-if="props.isLoading">
                {{
                  props.savingTitle ? props.savingTitle : $t("action.waiting")
                }}
                <span
                  class="spinner-border h-15px w-15px align-middle text-gray-400"
                ></span>
              </span>

              <span v-else>
                {{ props.saveTitle ? props.saveTitle : $t("action.submit") }}
              </span>
            </button>
          </div>
        </el-form>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { ElForm } from "element-plus";
import { hideModal, showModal } from "@/core/helpers/dom";

const props = withDefaults(
  defineProps<{
    title: string;
    submited: boolean;
    isLoading: boolean;
    saveTitle?: string;
    savingTitle?: string;
    discardTitle?: string;
    discard: () => any;
    submit: () => any;
    elId: string;
    rules: any;
    formData: any;
    width?: number;
    submitColor?: string;
    discardColor?: string;
  }>(),
  {
    submitColor: "primary",
    discardColor: "light",
    width: 650,
  }
);

const formRef = ref<InstanceType<typeof ElForm>>();
const modelRef = ref<HTMLElement | null>(null);

const submitForm = () => {
  (formRef.value as InstanceType<typeof ElForm>).validate((valid) => {
    if (valid) {
      props.submit();
    }
  });
};

onMounted(() => {
  const model = document.getElementById(props.elId) as Node;
  document.body.appendChild(model);
});
const hide = () => {
  hideModal(modelRef.value);
};

const show = () => {
  showModal(modelRef.value);
};
defineExpose({
  hide,
  show,
});
</script>
