<template>
  <!--begin::Input group-->
  <div class="d-flex mb-10" data-select2-id="select2-data-123-kglc">
    <!--begin::Label-->
    <label
      class="fw-semibold fs-5 d-flex justify-content-end pe-5 w-25 pt-3 w-150px"
      ><span class="required">{{ title }}</span></label
    >
    <!--end::Label-->
    <!--begin::Input-->
    <el-form-item class="form-control form-control-lg form-control-solid">
      <el-select class="w-100" v-model="selected" :name="title" type="text">
        <el-option
          v-for="({ label, value }, index) in selections"
          :key="index"
          :label="label"
          :value="value"
        />
      </el-select>
    </el-form-item>
    <!--end::Input-->
  </div>
  <!--end::Input group-->
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
const props = defineProps<{
  title: string;
  selections: any;
  modelValue?: number;
}>();

const emits = defineEmits<{
  (event: "update:modelValue", value: number): void;
}>();

const selected = ref<number | string>();

watch(selected, () => {
  emits("update:modelValue", selected.value as number);
});

watch(
  () => props.modelValue,
  () => {
    selected.value = props.modelValue;
  }
);
</script>

<style scoped></style>
