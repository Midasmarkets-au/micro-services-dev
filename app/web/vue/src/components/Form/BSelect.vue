<template>
  <div class="d-inline-block">
    <span>{{ props.title }}</span>
    <span>
      <select v-model="value" class="fs-6 filter-dropdown-custom-select">
        <option v-for="item in props.items" :key="item.val" :value="item.val">
          {{ $t(item.title) }}
        </option>
      </select>
    </span>
  </div>
</template>
<script setup lang="ts">
import { ref, watch } from "vue";

const props = defineProps<{
  title: string;
  items: any[];
  modelValue?: string;
}>();

const value = ref(props.modelValue);
const emit = defineEmits(["update:modelValue"]);
console.log("title: ", props.title);
console.log("modelValue: ", props.modelValue);
// watch(
//   () => props.modelValue,
//   () => {
//     console.log("props.modelValue: ", props.modelValue);
//     value.value = props.modelValue;
//   }
// );

watch(
  () => value.value,
  () => {
    console.log("value: ", value.value);
    emit("update:modelValue", value.value);
  }
);
</script>

<style>
.filter-dropdown-custom-select {
  padding: 0.5rem; /* optional */
  border: 1px solid #ccc;
  border-radius: 4px;
  background-color: #fff;
}

.filter-dropdown-custom-select:focus {
  outline: none;
  border-color: #aaa;
  box-shadow: 0 0 5px rgba(0, 0, 0, 0.3);
}

.filter-dropdown-custom-select option:checked {
  background-color: #ccc;
}
</style>
