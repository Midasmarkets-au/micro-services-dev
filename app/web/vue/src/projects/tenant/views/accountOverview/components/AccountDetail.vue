<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        {{ $props.account.user.email }} / {{ props.account.accountNumber }}
      </div>
    </div>
    <div class="card-body">
      <div class="fv-row mb-7">
        <ul
          class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
        >
          <li class="nav-item" v-for="tab in tabs" :key="tab.id">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: isTabActive(tab.id) }"
              data-bs-toggle="tab"
              href="#"
              @click="handleTabs(tab.id)"
              >{{ tab.name }}</a
            >
          </li>
        </ul>
      </div>
      <div class="tab-content">
        <template v-for="tab in tabs" :key="tab.id">
          <template v-if="currentTab === tab.id">
            <component
              :is="getComponent(tab.component)"
              :account="props.account"
            ></component>
          </template>
        </template>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";
import accountInfo from "@/projects/tenant/views/accountOverview/components/accountDetail/accountInfo.vue";
import TradeHistory from "@/projects/tenant/views/accountOverview/components/accountDetail/accountTrade.vue";
import accountTransfer from "@/projects/tenant/views/accountOverview/components/accountDetail/accountTransfer.vue";
import accountDeposit from "@/projects/tenant/views/accountOverview/components/accountDetail/acccountDeposit.vue";
import accountWithdrawal from "./accountDetail/accountWithdrawal.vue";

const props = defineProps({
  account: {
    type: Object,
    required: true,
  },
});
const tabs = ref([
  {
    name: "Info",
    component: "accountInfo",
    id: 1,
  },
  {
    name: "Trade",
    component: "TradeHistory",
    id: 2,
  },
  {
    name: "Transfer",
    component: "accountTransfer",
    id: 3,
  },
  {
    name: "Deposit",
    component: "accountDeposit",
    id: 4,
  },
  {
    name: "Withdrawal",
    component: "accountWithdrawal",
    id: 5,
  },
]);
const componentMap = {
  accountInfo,
  TradeHistory,
  accountTransfer,
  accountDeposit,
  accountWithdrawal,
};
const currentTab = ref(1);

const handleTabs = (id: number) => {
  currentTab.value = id;
};
const isTabActive = (tab: number) => {
  return tab === tabs.value[0].id;
};
const getComponent = (componentName: string) => {
  return componentMap[componentName];
};
</script>
