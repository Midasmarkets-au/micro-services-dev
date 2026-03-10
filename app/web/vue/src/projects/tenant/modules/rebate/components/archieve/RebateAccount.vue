<template>
  <ul
    class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
  >
    <!--begin:::Tab item-->
    <li class="nav-item">
      <a
        class="nav-link text-active-primary pb-4 active"
        data-bs-toggle="tab"
        href="#Client"
        @click="changeTab(AccountRoleTypes.Client)"
        >客戶</a
      >
    </li>
    <!--end:::Tab item-->
    <!--begin:::Tab item-->
    <li class="nav-item">
      <a
        class="nav-link text-active-primary pb-4"
        data-bs-toggle="tab"
        href="#Broker"
        @click="changeTab(AccountRoleTypes.Broker)"
        >一級代理</a
      >
    </li>
    <!--end:::Tab item-->

    <!--begin:::Tab item-->
    <li class="nav-item">
      <a
        class="nav-link text-active-primary pb-4"
        data-bs-toggle="tab"
        href="#Agent"
        @click="changeTab(AccountRoleTypes.IB)"
        >二級代理</a
      >
    </li>
    <!--end:::Tab item-->
  </ul>

  <ClientList v-if="tab == AccountRoleTypes.Client" />
</template>

<script lang="ts" setup>
import { ref, onMounted, nextTick } from "vue";
// import AccountService from "../services/AccountService";
import TimeShow from "@/components/TimeShow.vue";
import i18n from "@/core/plugins/i18n";
import TableFooter from "@/components/TableFooter.vue";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { ArrowDown } from "@element-plus/icons-vue";
import { Search } from "@element-plus/icons-vue";
import ClientList from "./ClientList.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";

const { t } = i18n.global;

const isLoading = ref(true);
const liveAccounts = ref(Array<any>());
const tab = ref(AccountRoleTypes.Client);

const criteria = ref<any>({
  page: 1,
  size: 20,
  numPages: 1,
  total: 0,
  hasTradeAccount: true,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    // const responseBody = await AccountService.queryUserAccounts(criteria.value);
    // criteria.value = responseBody.criteria;
    // liveAccounts.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
    await nextTick();
    reinitializeComponents();
  }
};

onMounted(async () => {
  isLoading.value = true;
  await fetchData(1);
  isLoading.value = false;
});

const changeTab = (_tab: AccountRoleTypes) => {
  tab.value = _tab;
};
</script>
