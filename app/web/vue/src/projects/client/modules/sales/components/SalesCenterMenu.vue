<template>
  <div class="ib-menu">
    <div class="sub-menu d-flex">
      <!-- <router-link
        to="/sales/dashboard"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.dashboard }"
        >{{ $t("title.dashboard") }}</router-link
      > -->

      <router-link
        to="/sales/customers"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.customers }"
        >{{ $t("title.customer") }}</router-link
      >
      <router-link
        to="/sales/new-customers"
        class="sub-menu-item"
        :class="{
          active: props.activeMenuItem === menuItem.newCustomers,
        }"
        >{{ $t("title.newCustomers") }}</router-link
      >
      <router-link
        to="/sales/trade"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.trade }"
        >{{ $t("title.trade") }}</router-link
      >
      <router-link
        to="/sales/transaction"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.transaction }"
        >{{ $t("title.transfer") }}</router-link
      >
      <router-link
        to="/sales/deposit"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.deposit }"
        >{{ $t("title.deposit") }}</router-link
      >
      <router-link
        to="/sales/withdrawal"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.withdrawal }"
        >{{ $t("title.withdrawal") }}</router-link
      >
      <router-link
        to="/sales/lead"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.lead }"
        >{{ $t("title.salesLeadSystem") }}==={{ menuItem.lead }}</router-link
      >
      <router-link
        to="/sales/link"
        class="sub-menu-item"
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
