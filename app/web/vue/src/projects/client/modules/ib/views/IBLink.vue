<template>
  <IBLayout activeMenuItem="link">
    <div v-if="isLoading" class="h-full">
      <table
        class="table align-middle table-row-bordered gy-5 mb-0"
        id="kt_permissions_table"
      >
        <tbody>
          <LoadingRing />
        </tbody>
      </table>
    </div>
    <div v-else class="h-full">
      <IBLinkLevel
        v-if="
          projectConfig?.rebateEnabled &&
          detail.levelSetting.distributionType == 2
        "
      />
      <IBLinkDirect
        v-else-if="
          projectConfig?.rebateEnabled &&
          detail.levelSetting.distributionType == 3
        "
      />
      <IBLinkOnly v-else />
    </div>
  </IBLayout>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
import IbService from "../services/IbService";
import { ref, onMounted } from "vue";

import IBLinkLevel from "./IBLinkLevel.vue";
import IBLinkDirect from "./IBLinkDirect.vue";
import IBLinkOnly from "./IBLinkOnly.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import IBLayout from "../components/IBLayout.vue";

const store = useStore();
const detail = ref({} as any);
const isLoading = ref(true);
const projectConfig: PublicSetting = store.state.AuthModule.config;

onMounted(async () => {
  isLoading.value = true;
  try {
    detail.value = await IbService.getRebateRuleDetail();
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
});
</script>
