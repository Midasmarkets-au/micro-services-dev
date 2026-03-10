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
import headerMenu from "../components/shop/HeaderMenu.vue";
import OrderList from "../components/shop/OrderList.vue";
import CustomerList from "../components/shop/CustomerList.vue";
import RebateSetting from "../components/shop/RebateSetting.vue";
import RebateList from "../components/shop/RebateList.vue";
const tabs = ref([
  {
    name: "Customer List",
    component: "CustomerList",
    id: 1,
  },

  {
    name: "Order List",
    component: "OrderList",
    id: 3,
  },
  {
    name: "Rebate List",
    component: "RebateList",
    id: 4,
  },
  {
    name: "Rebate Setting",
    component: "RebateSetting",
    id: 5,
  },
]);
const componentMap = {
  OrderList,
  CustomerList,
  RebateSetting,
  RebateList,
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};

const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
</script>
