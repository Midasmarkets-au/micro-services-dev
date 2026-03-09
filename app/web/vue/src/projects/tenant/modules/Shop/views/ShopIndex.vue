<template>
  <headerMenu :tabs="tabs" @change-Tab="handleTabs" />
  <template v-for="tab in tabs" :key="tab.id">
    <template v-if="currentTab === tab.id">
      <component :is="getComponent(tab.component)"></component>
    </template>
  </template>
</template>
<script setup lang="ts">
import { ref } from "vue";
import headerMenu from "@/projects/tenant/modules/Shop/components/HeaderMenu.vue";
import ShopInventory from "@/projects/tenant/modules/Shop/components/ShopInventory.vue";
import OrderList from "@/projects/tenant/modules/Shop/components/OrderList.vue";

const tabs = ref([
  {
    name: "Inventory",
    component: "ShopInventory",
    id: 1,
  },
  {
    name: "Order List",
    component: "OrderList",
    id: 2,
  },
]);
const componentMap = {
  ShopInventory,
  OrderList,
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};

const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
</script>
