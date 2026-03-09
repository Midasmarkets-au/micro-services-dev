<template>
  <span v-if="currencyId != -1">{{ currencyShow }}</span>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import filters from "@/core/helpers/filters";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { useStore } from "@/store";
const language = useStore().state.AuthModule.user.language;
const props = withDefaults(
  defineProps<{ balance?: any; currencyId?: number }>(),
  {
    balance: 0,
    currencyId: CurrencyTypes.USD,
  }
);
const currencyId = ref(props.currencyId);

const currencyShow = computed(() =>
  filters.toCurrency(props.balance, currencyId.value, language)
);

onMounted(() => {
  if (currencyId.value == -1) {
    currencyId.value = CurrencyTypes.USD;
  }
});
</script>

<style scoped></style>
