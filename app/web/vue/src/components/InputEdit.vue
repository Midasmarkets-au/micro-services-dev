<template>
  <div class="position-relative">
    <input
      type="text"
      class="form-control form-control-solid"
      v-model="inputValue"
      :disabled="props.disabled"
    />

    <span
      class="position-absolute cursor-pointer"
      style="top: 50%; right: 15px; transform: translateY(-50%)"
      ><i v-if="inputValue == ''" class="bi bi-pencil-fill fs-7"></i>
      <i
        v-else
        class="fa-solid fa-xmark fs-7"
        @click="!props.disabled && (inputValue = '')"
      ></i
    ></span>
  </div>
  <!-- <div class="btn btn-primary">{{ inputValue }}</div> -->
</template>

<script setup lang="ts">
import { computed } from "vue";

const props = withDefaults(
  defineProps<{
    modelValue?: string;
    name?: string;
    id?: string;
    disabled?: boolean;
  }>(),
  {
    disabled: false,
  }
);

const emits = defineEmits<{
  (e: "update:modelValue", value: string | undefined): void;
}>();

const inputValue = computed({
  get: () => props.modelValue,
  set: (value) => {
    emits("update:modelValue", value);
  },
});
</script>

<style scoped></style>
