<template>
  <template v-if="getUserTenancy() == tenancies.jp">
    <jpReportIndex />
  </template>
  <template v-else>
    <bviReportIndex />
  </template>
</template>
<script lang="ts" setup>
import { getUserTenancy, tenancies } from "@/core/types/TenantTypes";
import bviReportIndex from "../components/reportIndex/bviReportIndex.vue";
import jpReportIndex from "../components/reportIndex/jpReportIndex.vue";
</script>
