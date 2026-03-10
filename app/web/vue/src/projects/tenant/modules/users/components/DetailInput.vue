<template>
  <div :class="`col-md-${size}`">
    <div class="form-group">
      <label for="first_name">{{ label }}</label>
      <input
        class="form-control"
        id="first_name"
        type="text"
        :value="value"
        :disabled="disabled"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string | number;
  value: string | number | null | object;
  size: number | string;
  disabled: boolean;
}>();
</script>

<style lang="scss" scoped></style>
