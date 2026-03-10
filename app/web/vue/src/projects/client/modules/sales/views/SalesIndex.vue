<template>
  <SalesLayout activeMenuItem="dashboard">
    <!-- <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
    <div v-else>
      <SalesCenterMenu activeMenuItem="dashboard" />
    </div>
    <div v-if="salesAccount">
      <SalesDashboard :childUid="salesAccount.uid" />
    </div> -->
    <SalesDashboard :childUid="salesAccount.uid" />
  </SalesLayout>
</template>
<script setup lang="ts">
import { computed } from "vue";
import SalesLayout from "../components/SalesLayout.vue";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import SalesDashboard from "../components/SalesDashboard.vue";
import { useStore } from "@/store";
const store = useStore();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
console.log("salesAccount", salesAccount);
</script>
