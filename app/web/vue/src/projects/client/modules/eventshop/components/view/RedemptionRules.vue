<template>
  <div class="modal-body w-100 overflow-auto md:my-6 ps-3 card">
    <div class="w-100 notice-content card-body" v-html="eventDetail.term"></div>
  </div>
</template>

<script setup lang="ts">
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import { inject } from "vue";
const eventDetail = inject(ClientGlobalInjectionKeys.EVENT_SHOP_DETAIL);
</script>
