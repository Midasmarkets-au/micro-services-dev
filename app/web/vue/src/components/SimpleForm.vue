<template>
  <el-dialog
    v-model="formVisible"
    :title="title"
    :width="`${props.width}px`"
    align-center
    class="card"
    style="background-color: #fff"
    :append-to-body="appendBody"
    :before-close="handleBeforeClose"
    :style="{
      'min-width': `${minWidth}px !important`,
    }"
    :overflow="props.overflow"
  >
    <div class="">
      <div
        :class="props.overflow ? 'overflow-hidden' : 'overflow-auto'"
        style="max-height: 800px"
        :style="{
          'min-height': `${props.minHeight}px !important`,
        }"
      >
        <slot></slot>
      </div>

      <div v-if="!disableFooter" class="d-flex justify-content-center mt-5">
        <button
          :class="{
            [`btn btn-sm btn-radius btn-bordered btn-${props.discardColor} me-3`]: true,
          }"
          @click.prevent="discardHandler"
          :disabled="props.disableDiscard || isSubmitting"
        >
          <!--              <span v-if="props.isLoading">-->
          <span v-if="isDiscarding">
            {{ props.savingTitle ? props.savingTitle : $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ props.discardTitle ? props.discardTitle : $t("action.discard") }}
          </span>
        </button>

        <button
          :class="{
            [`btn btn-sm btn-radius btn-${props.submitColor} me-3`]: true,
          }"
          @click="submitHandler"
          :disabled="props.isLoading || props.disableSubmit || isDiscarding"
        >
          <span v-if="isSubmitting">
            {{ props.savingTitle ? props.savingTitle : $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ props.saveTitle ? props.saveTitle : $t("action.submit") }}
          </span>
        </button>

        <button
          v-if="$props.delayReview"
          :class="{
            [`btn btn-sm btn-radius btn-bordered btn-success me-3`]: true,
          }"
          @click="delayReviewHandler"
          :disabled="props.isLoading"
        >
          <span v-if="isSubmitting">
            {{ props.savingTitle ? props.savingTitle : $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else> {{ $t("action.delayReview") }} </span>
        </button>
      </div>
    </div>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";

const props = withDefaults(
  defineProps<{
    title: string;
    isLoading: boolean;
    discard?: any;
    submit?: any;
    submitted?: boolean;
    saveTitle?: string;
    savingTitle?: string;
    discardTitle?: string;
    elId?: string;
    width?: number;
    submitColor?: string;
    discardColor?: string;
    disableSubmit?: boolean;
    disableDiscard?: boolean;
    disableHeader?: boolean;
    disableFooter?: boolean;
    appendBody?: boolean;
    minHeight?: number;
    minWidth?: number;
    beforeClose?: any;
    delayReview?: boolean;
    delayReviewHandler?: any;
    overflow?: boolean;
  }>(),
  {
    minHeight: 0,
    submitColor: "primary",
    submitColorTwo: "warning",
    discardColor: "light",
    width: 650,
    appendBody: false,
    disableDiscard: false,
    disableSubmit: false,
    disableHeader: false,
    disableFooter: false,
  }
);

const formVisible = ref(false);
const isSubmitting = ref(false);
const isDiscarding = ref(false);
const submitHandler = async (event) => {
  event.preventDefault();
  isSubmitting.value = true;
  if (props.submit) {
    await props.submit();
  } else {
    formVisible.value = false;
  }
  isSubmitting.value = false;
};

const delayReviewHandler = async (event) => {
  event.preventDefault();
  isSubmitting.value = true;
  if (props.delayReviewHandler) {
    await props.delayReviewHandler();
  } else {
    formVisible.value = false;
  }
  isSubmitting.value = false;
};

const discardHandler = async () => {
  isDiscarding.value = true;
  if (props.discard) {
    await props.discard();
  } else {
    formVisible.value = false;
  }
  isDiscarding.value = false;
};

const handleBeforeClose = () => {
  if (props.beforeClose) {
    props.beforeClose();
  }
  formVisible.value = false;
};

defineExpose({
  hide: () => (formVisible.value = false),
  show: () => {
    isSubmitting.value = false;
    formVisible.value = true;
  },
});

const randomId = ref("");
onMounted(() => {
  randomId.value = Math.random().toString(36).substring(2, 8);
});
</script>
