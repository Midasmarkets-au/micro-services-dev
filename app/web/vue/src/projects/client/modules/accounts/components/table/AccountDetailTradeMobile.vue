<template>
  <div class="card p-0" style="border: 0 !important">
    <TradeFilter
      ref="tradeFilterRef"
      :trigger="'button'"
      :filter-options="filterOptions"
      :service-handler="getFetchDataFunc"
      :default-criteria="defaultCriteria"
      :type="'trade'"
    />
    <TradeCardMobile />
  </div>
</template>

<script setup lang="ts">
import { ref, watch, provide } from "vue";
import svc from "../../services/AccountService";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import TradeCardMobile from "@/projects/client/components/trade/TradeCardMobile.vue";
const props = defineProps<{
  currentAccount: any;
}>();

const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
provide("tradeFilterRef", tradeFilterRef);
provide("isCustomer", ref(true));
const filterOptions = ["closed", "symbol", "period"];

watch(
  () => props.currentAccount,
  () => {
    tradeFilterRef.value?.fetchData(1);
  }
);

const reset = async () => {
  await tradeFilterRef.value?.initCriteria();
  await tradeFilterRef.value?.fetchData(1);
};

const defaultCriteria = {
  page: 1,
  size: 10,
  symbol: "",
  isClosed: false,
};

const getFetchDataFunc = (_criteria?: any) =>
  svc.getTradesByTradeAccountUid(props.currentAccount.uid, _criteria);
</script>

<style scoped>
.ib-trd-fixed-container {
  width: 100%;
  overflow-x: auto;
  white-space: nowrap;
  padding: 5px;
  margin-bottom: 5px;
  /* border-radius: 10px; */
  /* border-radius:  */
  /* border: 1px dashed #ddd; */
}
.el-collapse {
  --el-collapse-header-height: auto; /* Change to your desired height */
}
</style>
