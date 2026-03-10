<template>
  <div class="">
    <TradeFilter
      ref="tradeFilterRef"
      :trigger="'button'"
      :filter-options="filterOptions"
      :service-handler="getFetchDataFunc"
      :default-criteria="defaultCriteria"
    />

    <div
      v-if="tradeFilterRef?.filtered"
      @click="reset"
      class="cursor-pointer d-flex align-items-center gap-2 fs-5"
    >
      <img src="/images/left-arrow.png" style="width: 10px" />
      <span style="color: #4196f0">{{ $t("action.reset") }}</span>
    </div>

    <div class="card-header px-0">
      <div class="card-title">
        <h2 class="fw-semibold" v-if="true">
          {{
            $t("action.showing") +
            " " +
            (tradeFilterRef?.criteria.total ?? "0") +
            " " +
            $t("title.results")
          }}
        </h2>
      </div>
      <div class="card-toolbar">
        <button
          v-if="false && tradeFilterRef?.data.length !== 0"
          class="border-0 bg-transparent d-flex align-items-center"
        >
          <span class="me-2" style="color: #4196f0"> </span>
          <span class="svg-icon svg-icon-3">
            <inline-svg src="/images/icons/general/download.svg" />
          </span>
        </button>
      </div>
    </div>
    <div class="ib-trd-fixed-container">
      <table
        class="table align-middle table-row-bordered fs-6 gy-5"
        id="kt_ecommerce_rep_table"
      >
        <thead>
          <tr class="text-start gs-0">
            <th class="text-center">{{ $t("fields.ticket") }}</th>
            <th class="text-center">{{ $t("fields.symbol") }}</th>
            <th class="text-center">{{ $t("fields.type") }}</th>
            <th class="text-center">{{ $t("fields.volume") }}</th>
            <th class="text-center">{{ $t("fields.openTime") }}</th>
            <th class="text-center">{{ $t("fields.openPrice") }}</th>
            <th class="text-center">{{ $t("fields.s/l") }}</th>
            <th class="text-center">{{ $t("fields.tp") }}</th>
            <template v-if="tradeFilterRef?.criteria.isClosed">
              <th class="text-center">
                {{ $t("fields.closeTime") }}
              </th>
              <th class="text-center">
                {{ $t("fields.closePrice") }}
              </th>
            </template>
            <th class="text-center">{{ $t("fields.commission") }}</th>
            <th class="text-center">{{ $t("fields.swaps") }}</th>
            <th class="text-center">{{ $t("fields.p/l") }}</th>
          </tr>
        </thead>
        <tbody v-if="tradeFilterRef?.isLoading">
          <LoadingRing />
        </tbody>
        <tbody
          v-else-if="
            !tradeFilterRef?.isLoading && tradeFilterRef?.data.length == 0
          "
        >
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            class="text-start"
            v-for="(trade, index) in tradeFilterRef?.getData()"
            :key="index"
            @click="
              transactionClicked(
                trade.ticket,
                trade.profit,
                trade.volume,
                trade.commission,
                trade.swaps
              )
            "
          >
            <td class="text-center">{{ trade.ticket }}</td>
            <td class="text-center">{{ trade.symbol }}</td>
            <td class="text-center">
              {{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}
            </td>
            <td class="text-center">{{ trade.volume }}</td>
            <td class="text-center">
              <TimeShow type="inFields" :date-iso-string="trade.openAt" />
            </td>
            <td class="text-center">
              {{ trade.openPrice }}
            </td>
            <td class="text-center">{{ trade.sl }}</td>
            <td class="text-center">{{ trade.tp }}</td>
            <template v-if="tradeFilterRef?.criteria.isClosed">
              <td class="text-center">
                <TimeShow
                  v-if="tradeFilterRef?.criteria.isClosed"
                  type="inFields"
                  :date-iso-string="trade.closeAt"
                />
                <span v-else>-</span>
              </td>
              <td class="text-center">
                {{ trade.closePrice }}
              </td>
            </template>
            <td class="text-center">{{ trade.commission }}</td>
            <td class="text-center">{{ trade.swaps }}</td>
            <td
              class="text-center"
              :class="{
                'text-danger': trade.profit < 0,
                'text-success': trade.profit > 0,
              }"
            >
              {{ trade.profit }}
            </td>
          </tr>

          <tr class="">
            <td class="text-center">{{ $t("title.subTotal") }}</td>
            <td colspan="2"></td>
            <td class="text-center">
              {{ tradeFilterRef?.criteria.pageTotalVolume ?? 0 }}
            </td>
            <td :colspan="tradeFilterRef?.criteria.isClosed ? 6 : 4"></td>

            <td class="text-center">
              {{ tradeFilterRef?.criteria.pageTotalCommission ?? 0 }}
            </td>
            <td class="text-center">
              {{ tradeFilterRef?.criteria.pageTotalSwap ?? 0 }}
            </td>
            <td
              class="text-center"
              :class="{
                'text-danger': tradeFilterRef?.criteria.pageTotalProfit < 0,
                'text-success': tradeFilterRef?.criteria.pageTotalProfit > 0,
              }"
            >
              {{ tradeFilterRef?.criteria.pageTotalProfit ?? 0 }}
            </td>
          </tr>
          <tr class="bg-light">
            <td class="text-center">{{ $t("title.total") }}</td>
            <td colspan="2"></td>
            <td class="text-center">
              {{ tradeFilterRef?.criteria.totalVolume ?? 0 }}
            </td>
            <td :colspan="tradeFilterRef?.criteria.isClosed ? 6 : 4"></td>

            <td class="text-center">
              {{ tradeFilterRef?.criteria.totalCommission ?? 0 }}
            </td>
            <td class="text-center">
              {{ tradeFilterRef?.criteria.totalSwap ?? 0 }}
            </td>
            <td
              class="text-center"
              :class="{
                'text-danger': tradeFilterRef?.criteria.totalProfit < 0,
                'text-success': tradeFilterRef?.criteria.totalProfit > 0,
              }"
            >
              {{ tradeFilterRef?.criteria.totalProfit ?? 0 }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <TableFooter
      @page-change="tradeFilterRef?.fetchData"
      :criteria="tradeFilterRef?.criteria"
    />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, watch, computed } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import RepService from "../services/RepService";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { handleTradeBuySellDisplay } from "@/core/helpers/helpers";

defineProps<{
  accountDetails: any;
}>();

const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["closed", "size", "period", "symbol", "order"];

const store = useStore();
const route = useRoute();

const currentSelectedRep = computed(() => store.state.RepModule.repAccount);
const accountUid = ref(-1);

const selectedItems = ref(Array<any>());

watch(
  () => route.params.accountId,
  (newVal) => {
    accountUid.value = parseInt((newVal as string) || "-1");
    if (accountUid.value !== -1) tradeFilterRef.value?.fetchData(1);
  }
);

const defaultCriteria = {
  page: 1,
  size: 10,
  symbol: "",
  isClosed: false,
};

const getFetchDataFunc = (criteria?: any) =>
  RepService.queryClientTrade(accountUid.value, criteria);

onMounted(() => {
  accountUid.value = parseInt((route.params.accountId as string) || "-1");
});
const transactionClicked = (
  ticket: number,
  profit: number,
  volume: number,
  commission: number,
  swaps: number
) => {
  const index = selectedItems.value.findIndex((x: any) => x.ticket === ticket);
  if (index > -1) {
    selectedItems.value.splice(index, 1);
  } else {
    selectedItems.value.push({
      ticket,
      profit,
      volume,
      commission,
      swaps,
    });
  }
};

const reset = async () => {
  tradeFilterRef.value?.initCriteria();
  await tradeFilterRef.value?.fetchData(1);
};
</script>

<style scoped>
.ib-trd-fixed-container {
  width: 100%;
  overflow-x: auto;
  white-space: nowrap;
  padding: 5px;
  margin-bottom: 5px;
}
</style>
