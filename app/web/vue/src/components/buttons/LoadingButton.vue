<template>
  <button :disabled="props.isLoading">
    <span v-if="isLoading">
      {{ props.savingTitle ? props.savingTitle : $t("action.waiting") }}
      <span
        class="spinner-border h-15px w-15px align-middle text-gray-400"
      ></span>
    </span>
    <span v-else>
      {{ props.saveTitle ? props.saveTitle : $t("action.submit") }}
    </span>
  </button>
</template>

<script setup lang="ts">
const props = defineProps<{
  isLoading: boolean;
  saveTitle?: string;
  savingTitle?: string;
}>();
</script>

<style scoped></style>
