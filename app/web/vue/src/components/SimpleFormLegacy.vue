<template>
  <div class="modal fade" tabindex="-1" aria-hidden="true" ref="modelRef">
    <div
      class="modal-dialog modal-dialog-centered"
      :style="{
        'max-width': `${props.width}px !important`,
      }"
    >
      <div class="modal-content">
        <!--        <form @submit.prevent="submitHandler">-->
        <div
          v-if="!disableHeader"
          class="modal-header"
          :id="randomId + 'header'"
        >
          <h2
            class="fw-bold"
            :class="{
              'fs-1': isMobile,
            }"
          >
            {{ title }}
          </h2>
          <div
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
        </div>

        <div v-if="!disableFooter" class="modal-footer flex-center">
          <button
            :class="{
              [`btn btn-${props.discardColor} me-3`]: true,
              'fs-6': !isMobile,
              'fs-3': isMobile,
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
              {{
                props.discardTitle ? props.discardTitle : $t("action.discard")
              }}
            </span>
          </button>
          <button
            :class="{
              [`btn btn-${props.submitColor} me-3`]: true,
              'fs-6': !isMobile,
              'fs-3': isMobile,
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
        </div>
        <!--        </form>-->
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { hideModal, showModal } from "@/core/helpers/dom";
import { isMobile } from "@/core/config/WindowConfig";

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
  }>(),
  {
    submitColor: "primary",
    discardColor: "light",
    width: 650,
    disableDiscard: false,
    disableSubmit: false,
    disableHeader: false,
    disableFooter: false,
  }
);

const modelRef = ref<HTMLElement | null>(null);
const isSubmitting = ref(false);
const isDiscarding = ref(false);

const submitHandler = async (event) => {
  event.preventDefault();
  isSubmitting.value = true;

  if (props.submit) {
    await props.submit();
  } else {
    hideModal(modelRef.value);
  }
  isSubmitting.value = false;
};

const discardHandler = async () => {
  isDiscarding.value = true;
  if (props.discard) {
    await props.discard();
  } else {
    hideModal(modelRef.value);
  }
  isDiscarding.value = false;
};

defineExpose({
  hide: () => hideModal(modelRef.value),
  show: () => showModal(modelRef.value),
});

const randomId = ref("");
onMounted(() => {
  randomId.value = Math.random().toString(36).substring(2, 8);
  document.body.appendChild(modelRef.value as Node);
});
</script>
