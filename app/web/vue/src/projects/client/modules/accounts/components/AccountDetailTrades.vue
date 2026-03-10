<template>
  <div class="p-0" style="border: 0 !important">
    <TradeFilter
      ref="tradeFilterRef"
      :trigger="'button'"
      :filter-options="filterOptions"
      :service-handler="getFetchDataFunc"
      :default-criteria="defaultCriteria"
      :type="'trade'"
    />

    <div class="card-header px-0">
      <div class="card-title">
        <div>
          <div
            v-if="tradeFilterRef?.filtered"
            @click="reset"
            class="cursor-pointer d-flex align-items-center gap-2 mb-4 fs-5"
          >
            <img src="/images/left-arrow.png" style="width: 10px" />
            <span style="color: #4196f0">Back to all transaction</span>
          </div>
          <div>
            <h2 class="fw-semibold">
              {{
                $t("action.showing") +
                " " +
                (tradeFilterRef?.criteria.total ?? "0") +
                " " +
                $t("title.results")
              }}
            </h2>
          </div>
        </div>
      </div>

      <div class="card-toolbar">
        <button
          v-if="tradeFilterRef?.data.length !== 0 && false"
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
        class="table align-middle table-row-bordered gy-5"
        id="kt_ecommerce_sales_table"
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
            !tradeFilterRef?.isLoading && tradeFilterRef?.data.length === 0
          "
        >
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            class="text-start"
            v-for="(trade, index) in tradeFilterRef?.getData()"
            :key="index"
          >
            <td class="text-center">{{ trade.ticket }}</td>
            <td class="text-center">{{ trade.symbol }}</td>
            <td class="text-center">
              <span>
                {{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}
              </span>
            </td>
            <td class="text-center">{{ trade.volume }}</td>
            <td class="text-center">
              <TimeShow type="inFields" :date-iso-string="trade.openAt" />
            </td>
            <td class="text-center">
              {{ handleTradeFormatted(trade.openPrice, trade.digits) }}
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
                <template v-if="tradeFilterRef?.criteria.isClosed">
                  {{
                    handleTradeFormatted(trade.closePrice, trade.digits)
                  }}</template
                >
                <template v-else>-</template>
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
              {{ handleTradeFormatted(trade.profit, 2) }}
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
          <tr></tr>
        </tbody>
      </table>
    </div>

    <TableFooter
      @page-change="tradeFilterRef?.fetchData"
      :criteria="tradeFilterRef?.criteria"
    />
    <!-- <div class="btn btn-primary">{{ selectedItems }}</div> -->
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import svc from "../services/AccountService";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import {
  handleTradeBuySellDisplay,
  handleTradeFormatted,
} from "@/core/helpers/helpers";
const props = defineProps<{
  currentAccount: any;
}>();

const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["closed", "period", "symbol"];

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
</style>
