<template>
  <div class="ib-menu">
    <div class="sub-menu d-flex flex-row">
      <router-link
        to="/ib/index"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.index }"
        >{{ $t("title.overview") }}</router-link
      >
      <router-link
        to="/ib/new-customers"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.newCustomers }"
        >{{ $t("title.newCustomers") }}</router-link
      >
      <router-link
        to="/ib/customers"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.customers }"
        >{{ $t("title.customer") }}</router-link
      >
      <router-link
        to="/ib/trade"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.trade }"
        >{{ $t("title.trade") }}</router-link
      >
      <router-link
        to="/ib/deposit"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.deposit }"
        >{{ $t("title.deposit") }}</router-link
      >
      <router-link
        to="/ib/withdrawal"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.withdrawal }"
        >{{ $t("title.withdrawal") }}</router-link
      >
      <router-link
        to="/ib/rebate"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.rebate }"
        >{{ $t("title.rebate") }}</router-link
      >
      <router-link
        to="/ib/iblink"
        class="sub-menu-item"
        :class="{ active: props.activeMenuItem === menuItem.link }"
      >
        {{ $t("title.ibLink") }}
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";

const menuItem = {
  index: "index",
  newCustomers: "newCustomers",
  customers: "customers",
  trade: "trade",
  deposit: "deposit",
  withdrawal: "withdrawal",
  rebate: "rebate",
  link: "link",
  report: "report",
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
  //width: 100%;
  white-space: nowrap;
}
</style>
