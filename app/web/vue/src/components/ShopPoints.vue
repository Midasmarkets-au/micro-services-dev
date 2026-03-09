<template>
  <span>{{ points }}</span>
</template>
<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import Decimal from "decimal.js";
const props = withDefaults(defineProps<{ points?: any; format?: any }>(), {
  points: 0,
  format: 2,
});

const points = computed(() => {
  return new Decimal(props.points).div(10000).toFixed(props.format);
});
</script>
