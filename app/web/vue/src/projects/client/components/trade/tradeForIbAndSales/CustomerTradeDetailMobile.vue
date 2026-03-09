<template>
  <TradeFilter
    ref="tradeFilterRef"
    :trigger="'button'"
    :filter-options="filterOptions"
    :service-handler="getFetchDataFunc"
    :default-criteria="defaultCriteria"
  />
  <TradeCardMobile />
</template>
<script setup lang="ts">
import { ref, onMounted, watch, provide } from "vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import IbService from "@/projects/client/modules/ib/services/IbService";
import SaleService from "@/projects/client/modules/sales/services/SalesService";
import { useRoute } from "vue-router";
import TradeCardMobile from "../TradeCardMobile.vue";
const props = defineProps<{
  accountDetails: any;
  accountType: string;
}>();

const route = useRoute();
const accountUid = ref(-1);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const isCustomer = ref(true);
provide("tradeFilterRef", tradeFilterRef);
provide("isCustomer", isCustomer);
const filterOptions = ["closed", "period", "symbol"];
const defaultCriteria = {
  page: 1,
  size: 10,
  symbol: "",
  isClosed: false,
};

watch(
  () => route.params.accountId,
  (newVal) => {
    accountUid.value = parseInt((newVal as string) || "-1");
    if (accountUid.value !== -1) tradeFilterRef.value?.fetchData(1);
  }
);

const getFetchDataFunc = (criteria?: any) =>
  props.accountType === "ib"
    ? IbService.queryTradesOfTradeAccountOfAgent(accountUid.value, criteria)
    : SaleService.queryClientTrade(accountUid.value, criteria);

onMounted(() => {
  accountUid.value = parseInt((route.params.accountId as string) || "-1");
});
</script>
