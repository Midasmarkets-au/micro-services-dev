<template>
  <div
    class="card"
    :class="{ 'pt-5': !isMobile, 'mt-10': !isMobile }"
    v-if="mainStep == 0"
  >
    <div class="card-body pt-10">
      <ProductsCheck />
    </div>
  </div>
  <ProductRegister v-else-if="mainStep == 1" />
</template>
<script lang="ts" setup>
import { isMobile } from "@/core/config/WindowConfig";
import ProductsCheck from "../components/ProductsCheck.vue";
import ProductRegister from "../components/ProductRegister.vue";
import { ref, provide } from "vue";
const mainStep = ref(0);
const selectedAccountTypes = ref<any>([]);
const selectedVerification = ref<any>([]);
const verificationData = ref<any>([]);

provide("verificationData", verificationData);
provide("selectedVerification", selectedVerification);
provide("mainStep", mainStep);
provide("selectedAccountTypes", selectedAccountTypes);
</script>
