<template>
  <div class="ib-menu card h-full">
    <div class="ib-menu-lg d-flex flex-lg-column">
      <!-- <router-link
        to="/sales/statistics"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.statistics }"
        >{{ $t("sales.statisticsTable") }}</router-link
      > -->
      <router-link
        to="/sales/customers"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.customers }"
        >{{ $t("title.customer") }}</router-link
      >
      <router-link
        to="/sales/new-customers"
        class="ib-menu-item"
        :class="{
          active: props.activeMenuItem === menuItem.newCustomers,
        }"
        >{{ $t("title.newCustomers") }}</router-link
      >
      <router-link
        to="/sales/trade"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.trade }"
        >{{ $t("title.trade") }}</router-link
      >
      <router-link
        to="/sales/transaction"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.transaction }"
        >{{ $t("title.transfer") }}</router-link
      >
      <router-link
        to="/sales/deposit"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.deposit }"
        >{{ $t("title.deposit") }}</router-link
      >
      <router-link
        to="/sales/withdrawal"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.withdrawal }"
        >{{ $t("title.withdrawal") }}</router-link
      >
      <router-link
        to="/sales/lead"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.lead }"
        >{{ $t("title.salesLeadSystem") }}</router-link
      >
      <router-link
        to="/sales/link"
        class="ib-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.link }"
      >
        {{ $t("title.salesLink") }}
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";

const menuItem = {
  statistics: "statistics",
  dashboard: "dashboard",
  trade: "trade",
  transaction: "transaction",
  deposit: "deposit",
  withdrawal: "withdrawal",
  lead: "lead",
  link: "link",
  customers: "customers",
  newCustomers: "new-customers",
};

const props = defineProps<{
  activeMenuItem: any;
}>();

onMounted(() => {
  moibleNavScroller(".sub-menu", ".active");
  moibleNavScroller(".ib-menu", ".active");
});
</script>

<style scoped lang="scss">
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
</style>
