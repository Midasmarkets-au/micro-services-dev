<template>
  <div
    class="flex flex-col lg:flex-row w-full gap-4"
    :class="isMobile ? 'p-3' : ''"
  >
    <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
    <div v-else class="ib_header">
      <headerMenu :activeMenuItem="activeMenuItem" v-if="isMobile" />
      <HeaderMenuLg :activeMenuItem="activeMenuItem" v-if="!isMobile" />
    </div>
    <div class="content flex-1 d-flex flex-column overflow-auto">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps, computed } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import headerMenu from "./menu/headerMenu.vue";
import HeaderMenuLg from "./menu/headerMenuLg.vue";
import { useStore } from "@/store";
const store = useStore();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const props = defineProps<{
  activeMenuItem: string;
}>();
</script>
