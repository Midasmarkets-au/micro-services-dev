<template>
  <div
    class="flex flex-col lg:flex-row w-full gap-4"
    :class="isMobile ? ' px-3' : ''"
  >
    <div class="ib_header">
      <headerMenu :activeMenuItem="activeMenuItem" v-if="isMobile" />
      <HeaderMenuLg :activeMenuItem="activeMenuItem" v-if="!isMobile" />
    </div>
    <div class="content flex-1 d-flex flex-column overflow-auto">
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
import headerMenu from "./menu/headerMenu.vue";
import HeaderMenuLg from "./menu/headerMenuLg.vue";

const props = defineProps<{
  activeMenuItem: string;
}>();
</script>
