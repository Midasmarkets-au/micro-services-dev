<template>
  <div>{{ register }}</div>
</template>

<script setup lang="ts">
import { VerificationStarted } from "../types/VerificationDetails";
defineProps<{
  register: VerificationStarted;
}>();
</script>

<style scoped></style>
