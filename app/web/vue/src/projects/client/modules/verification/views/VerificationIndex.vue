<template>
  <div>
    <div v-if="getVerificationTenancy() == tenancies.jp">
      <JpVerification />
    </div>
    <div v-else-if="getVerificationTenancy() == tenancies.au">
      <BaVerification />
    </div>

    <div v-else>
      <BviVerification />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { getVerificationTenancy, tenancies } from "@/core/types/TenantTypes";
import BviVerification from "./BviVerification.vue";
import BaVerification from "./BaVerification.vue";
import JpVerification from "./JpVerification.vue";
import { ref, provide, onMounted } from "vue";

const isLoading = ref(false);
provide("isLoading", isLoading);
console.log("tenant", getVerificationTenancy());
onMounted(() => {
  isLoading.value = true;
});
</script>
