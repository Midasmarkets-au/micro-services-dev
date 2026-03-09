<template>
  <span class="badge fs-8 fw-bold" :class="classes">{{ currencyCode }}</span>
</template>

<script lang="ts" setup>
import { computed } from "vue";
const props = defineProps<{
  currency: number;
}>();

const classes = computed(() => {
  return {
    "badge-primary": props.currency === 840,
    "badge-success": props.currency === 978,
    "badge-danger": props.currency === 826,
    "badge-warning": props.currency === 392,
    "badge-info": props.currency === 36,
  };
});

const currencyCode = computed((): string => {
  switch (props.currency) {
    case 840:
      return "USD";
    case 978:
      return "EUR";
    case 826:
      return "GBP";
    case 392:
      return "JPY";
    case 36:
      return "AUD";
    default:
      return "USD";
  }
});
</script>
