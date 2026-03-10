<template>
  <div v-if="getUserTenancy() === tenancies.jp">
    <jpVerification />
  </div>
  <div v-else>
    <bviVerification />
  </div>
</template>
<script setup lang="ts">
import { getUserTenancy, tenancies } from "@/core/types/TenantTypes";
import bviVerification from "../components/verification/bviVerification.vue";
import jpVerification from "../components/verification/jpVerification.vue";
</script>
