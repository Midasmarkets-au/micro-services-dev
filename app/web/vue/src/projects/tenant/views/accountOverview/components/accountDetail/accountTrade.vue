<template>
  <table
    class="table align-middle table-row-dashed fs-6 gy-5"
    id="table_accounts_requests"
  >
    <thead>
      <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
        <th class="text-start w-70px">{{ $t("fields.ticket") }}</th>
        <th class="w-70px text-end">{{ $t("fields.tp") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.symbol") }}</th>
        <th class="min-w-40px text-end">{{ $t("fields.buy/Sell") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.openPrice") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.openTime") }}</th>
        <th class="min-w-40px text-end">{{ $t("fields.volume") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.profit") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.closePrice") }}</th>
        <th class="min-w-60px text-end">{{ $t("fields.closeTime") }}</th>
      </tr>
    </thead>
    <tbody v-if="isLoading">
      <LoadingRing />
    </tbody>
    <tbody v-else-if="!isLoading && data.length === 0">
      <NoDataBox />
    </tbody>

    <tbody v-else class="fw-semibold text-gray-900">
      <tr class="text-center" v-for="(transaction, index) in data" :key="index">
        <td class="text-end">{{ transaction.ticket }}</td>
        <td class="text-end">{{ transaction.tp }}</td>
        <td class="text-end">{{ transaction.symbol }}</td>
        <td class="text-end">{{ transaction.buySell }}</td>
        <td class="text-end">{{ transaction.openPrice }}</td>
        <td class="text-end">
          <TimeShow :date-iso-string="transaction.openTime" />
        </td>
        <td class="text-end">{{ transaction.volume }}</td>
        <td
          class="text-end"
          :class="{
            'text-success': transaction.profit >= 0,
            'text-danger': transaction.profit < 0,
          }"
        >
          {{ transaction.profit }}
        </td>

        <td v-if="transaction.closePrice != null" class="text-end">
          {{ transaction.closePrice }}
        </td>
        <td v-if="transaction.closeTime != null" class="text-end">
          <TimeShow :date-iso-string="transaction.closeTime" />
        </td>
      </tr>
    </tbody>
  </table>
  <TableFooter @page-change="pageChange" :criteria="criteria" />
</template>
<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import AccountOverviewService from "@/projects/tenant/views/accountOverview/services/AccountOverviewServices";
import i18n from "@/core/plugins/i18n";
import { handleTradeBuySellDisplay } from "@/core/helpers/helpers";

const isLoading = ref(false);
const data = ref<any>([]);
const { t } = i18n.global;

const props = defineProps({
  account: {
    type: Object,
    required: true,
  },
});

const criteria = ref({
  size: 10,
  accountId: props.account.id,
  accountNumber: props.account.accountNumber,
  serviceId: props.account.serviceId,
} as any);

const fetchData = async (_page: number) => {
  if (
    props.account.accountNumber === 0 ||
    props.account.accountNumber === undefined
  )
    return;
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountOverviewService.queryTrades(criteria.value);
    criteria.value = res.criteria;
    // data.value = res;
    data.value = res.data.map((item: any) => ({
      ticket: item.ticket,
      symbol: item.symbol,
      tp: item.tp,
      volume: item.volume,
      profit: item.profit,
      openTime: item.openAt,
      closeTime: item.closeAt,
      openPrice: item.openPrice,
      closePrice: item.closePrice,
      buySell: t(`type.cmd.${handleTradeBuySellDisplay(item)}`),
    }));
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

watch(
  () => props.account,
  (newAccount, oldAccount) => {
    if (newAccount && newAccount.id !== oldAccount?.id) {
      resetCriteria();
      fetchData(1);
    }
  },
  { deep: true }
);

const resetCriteria = () => {
  criteria.value = {
    size: 10,
    accountId: props.account.id,
    accountNumber: props.account.accountNumber,
    serviceId: props.account.serviceId,
  };
};

onMounted(() => {
  fetchData(1);
});

const pageChange = (page: number) => {
  criteria.value.page = page;
  fetchData(page);
};
</script>
