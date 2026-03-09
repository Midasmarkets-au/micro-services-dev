<template>
  <!--begin::Input group-->
  <div class="d-flex mb-10">
    <!--begin::Label-->
    <label
      class="d-flex justify-content-end pe-5 pt-4 fs-5 fw-semibold w-150px"
    >
      <span
        :class="{
          required: required,
        }"
        >{{ title }}</span
      >
    </label>
    <!--end::Label-->
    <!--begin::Input-->
    <div class="w-100">
      <input
        type="text"
        class="form-control form-control-lg form-control-solid w-100"
        :class="{
          'bg-black bg-opacity-25': disabled,
        }"
        :name="title + Math.random().toString(36).substring(2)"
        :disabled="disabled"
        :placeholder="inputPlaceholder"
        v-model="inputValue"
        @blur="onInputBlured"
      />
      <div
        v-if="required"
        class="fv-plugins-message-container invalid-feedback"
      >
        <div data-field="name" data-validator="notEmpty">
          {{ errorMessage }}
        </div>
      </div>
      <div
        v-if="correctMessage"
        class="fv-plugins-message-container valid-feedback"
      >
        <div data-field="name" data-validator="notEmpty">
          {{ correctMessage }}
        </div>
      </div>
    </div>
    <!--end::Input-->
  </div>
  <!--end::Input group-->
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = withDefaults(
  defineProps<{
    title: string;
    inputPlaceholder?: string;
    errorMessage?: string;
    correctMessage?: string;
    modelValue?: string;
    required?: boolean;
    disabled?: boolean;
    shouldBlur?: boolean;
  }>(),
  {
    required: false,
    inputPlaceholder: "",
    errorMessage: "This field is required",
    disabled: false,
    shouldBlur: false,
  }
);

const emits = defineEmits<{
  (e: "inputBlured", value: string | undefined): void;
  (e: "update:modelValue", value: string | undefined): void;
}>();

const onInputBlured = () => {
  if (!props.shouldBlur) return;
  emits("inputBlured", props.modelValue);
};

const inputValue = computed({
  get: () => props.modelValue,
  set: (value) => {
    emits("update:modelValue", value);
  },
});
</script>

<style scoped></style>
