<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="$t('title.signature')"
      :is-loading="isLoading"
      disable-footer
    >
      <Vue3Signature
        ref="signatureRef"
        :sigOption="state.option"
        :w="'600px'"
        :h="'200px'"
        :disabled="state.disabled"
        class="example"
        style="border: 1px solid #000"
      ></Vue3Signature>
      <div class="d-flex justify-content-end mt-3">
        <div>
          <button class="btn btn-light btn-warning btn-md me-3" @click="clear">
            {{ $t("action.clear") }}
          </button>
          <button class="btn btn-light btn-success btn-md me-3" @click="undo">
            {{ $t("action.undo") }}
          </button>
        </div>
      </div>
      <div class="d-flex justify-content-center mt-5">
        <div>
          <button
            class="btn btn-light btn-primary btn-lg me-3"
            @click="save('image/jpeg')"
          >
            {{ $t("action.save") }}
          </button>
        </div>
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import Vue3Signature from "vue3-signature";

const isLoading = ref(true);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const signatureRef = ref<InstanceType<typeof Vue3Signature>>();

const emits = defineEmits<{
  (e: "handelSignature", signature: any): void;
}>();

const state = reactive({
  count: 0,
  option: {
    penColor: "rgb(0, 0, 0)",
    backgroundColor: "rgb(255,255,255)",
  },
  disabled: false,
});

const save = (t) => {
  emits("handelSignature", signatureRef.value?.save(t));
};

const clear = () => {
  signatureRef.value?.clear();
};

const undo = () => {
  signatureRef.value?.undo();
};

// const fromDataURL = (url) => {
//   signatureRef.value?.fromDataURL(
//     "https://avatars2.githubusercontent.com/u/17644818?s=460&v=4"
//   );
// };

defineExpose({
  async show() {
    modalForm.value?.show();
  },
  hide() {
    modalForm.value?.hide();
  },
  clear,
});
</script>

<style scoped>
.example {
  margin: 0 auto;
}
</style>
