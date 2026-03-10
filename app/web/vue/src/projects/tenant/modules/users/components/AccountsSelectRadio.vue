import { computed } from 'vue';
<template>
  <div class="d-flex mb-10">
    <!--begin::Label-->
    <label
      class="d-flex justify-content-end fw-semibold pe-5 fs-5 w-150px"
      :class="{
        required: required,
      }"
      >{{ title }}</label
    >
    <!--end::Label-->
    <!--begin::Roles-->
    <div class="d-flex gap-5 w-100">
      <!--begin::Input row-->

      <div
        class="d-flex fv-row"
        v-for="({ label, value }, index) in selections"
        :key="index"
      >
        <!--begin::Radio-->
        <div class="form-check form-check-custom form-check-solid">
          <!--begin::Input-->
          <input
            class="form-check-input"
            type="radio"
            :name="title"
            :value="value"
            :id="`${title}_${index}`"
            checked
            @input="selectChangeHandler"
          />
          <!--end::Input-->
          <!--begin::Label-->
          <label class="form-check-label" :for="`${title}_${index}`">
            <div class="fw-bold text-gray-800">{{ label }}</div>
          </label>
          <!--end::Label-->
        </div>
        <!--end::Radio-->
      </div>

      <!--end::Input row-->
      <!--end::Roles-->
    </div>
  </div>
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{
    required?: boolean;
    title: string;
    selections: any[];
  }>(),
  {
    required: false,
  }
);

const emits = defineEmits<{
  (event: "update:modelValue", value: number): void;
}>();

const selectChangeHandler = (event: Event) => {
  const target = event.target as HTMLInputElement;
  const value = parseInt(target.value);
  emits("update:modelValue", value);
};
</script>

<style scoped></style>
