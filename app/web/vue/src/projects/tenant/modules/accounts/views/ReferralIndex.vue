<template>
  <headerMenu :tabs="tabs" @change-Tab="handleTabs" />
  <template v-for="tab in tabs" :key="tab.id">
    <template v-if="currentTab === tab.id">
      <component :is="getComponent(tab.component)"></component>
    </template>
  </template>
</template>
<script setup lang="ts">
import { ref, provide } from "vue";
import HeaderMenu from "../components/referral/HeaderMenu.vue";
import ReferralCode from "../components/referral/ReferralCode.vue";
import ReferralHistory from "../components/referral/ReferralHistory.vue";
import { useI18n } from "vue-i18n";

const t = useI18n().t;
const isLoading = ref(false);
provide("isLoading", isLoading);
const tabs = ref([
  {
    name: t("fields.referralCode"),
    component: "ReferralCode",
    id: 1,
  },
  {
    name: t("fields.referralHistory"),
    component: "ReferralHistory",
    id: 2,
  },
]);
const componentMap = {
  ReferralCode,
  ReferralHistory,
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};

const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
</script>
