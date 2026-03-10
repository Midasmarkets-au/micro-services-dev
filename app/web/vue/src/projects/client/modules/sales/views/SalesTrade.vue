<template>
  <!-- <div>
    <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
    <div v-else>
      <SalesCenterMenu activeMenuItem="trade" />
    </div> -->
  <SalesLayout activeMenuItem="trade">
    <div class="card pl-3 round-bl-br">
      <TradeFilter
        ref="tradeFilterRef"
        :trigger="'button'"
        :filter-options="filterOptions"
        :service-handler="getFetchDataFunc"
        :default-criteria="defaultCriteria"
        :type="'trade'"
        class="ms-2"
      />
    </div>
    <div class="card mt-2 round-tl-tr flex-1" v-if="!isMobile">
      <div class="card-header">
        <div class="card-title">
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
        <div class="card-toolbar">
          <button
            v-if="false && tradeFilterRef?.data.length !== 0"
            class="border-0 bg-transparent d-flex align-items-center"
          >
            <span class="me-2" style="color: #4196f0">
              <!--              {{ $t("action.export") }}-->
            </span>
            <span class="svg-icon svg-icon-3">
              <inline-svg src="/images/icons/general/download.svg" />
            </span>
          </button>
        </div>
      </div>
      <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
        <table
          class="table align-middle table-row-bordered gy-2"
          id="kt_ecommerce_sales_table"
        >
          <thead>
            <tr class="text-start gs-0">
              <!-- <th class="text-start">{{ $t("fields.select") }}</th> -->
              <th class="text-center">{{ $t("fields.accountNumber") }}</th>
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
            >
              <td class="text-center">{{ trade.accountNumber }}</td>
              <td class="text-center">{{ trade.ticket }}</td>
              <!-- <td class="text-center">{{ transaction.tp }}</td> -->
              <td class="text-center">{{ trade.symbol }}</td>
              <td class="text-center">
                <span>
                  {{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}
                </span>
              </td>
              <td class="text-center">{{ trade.volume }}</td>
              <td class="text-center">
                <TimeShow type="GMTinFields" :date-iso-string="trade.openAt" />
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
                    type="GMTinFields"
                    :date-iso-string="trade.closeAt"
                  />
                  <span v-else>-</span>
                </td>
                <td class="text-center">
                  {{ handleTradeFormatted(trade.closePrice, trade.digits) }}
                </td>
              </template>
              <td class="text-center">
                {{ $filters.digits(trade.commission) }}
              </td>
              <td class="text-center">{{ $filters.digits(trade.swaps) }}</td>
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
              <td colspan="3"></td>
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
              <td colspan="3"></td>
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
        <TableFooter
          @page-change="tradeFilterRef?.fetchData"
          :criteria="tradeFilterRef?.criteria"
        />
      </div>
    </div>
    <div v-else>
      <TradeCardMobile />
      <TableFooter
        @page-change="tradeFilterRef?.fetchData"
        :criteria="tradeFilterRef?.criteria"
      />
    </div>
  </SalesLayout>
  <!-- </div> -->
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed, nextTick, provide } from "vue";
import { useStore } from "@/store";
import SalesService from "../services/SalesService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import { useRoute, useRouter } from "vue-router";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";
import {
  handleTradeBuySellDisplay,
  handleTradeFormatted,
} from "@/core/helpers/helpers";
import { isMobile } from "@/core/config/WindowConfig";
import TradeCardMobile from "@/projects/client/components/trade/TradeCardMobile.vue";
import SalesLayout from "../components/SalesLayout.vue";
const isLoading = ref(true);
const route = useRoute();
const router = useRouter();
const store = useStore();
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["closed", "service", "period", "symbol", "target"];
provide("tradeFilterRef", tradeFilterRef);
provide("isCustomer", ref(false));
const initialCriteria = ref<any>({
  page: 1,
  size: 25,
  isClosed: true,
  from: "",
  to: "",
  serviceId: ConfigRealServiceSelections.value[0].id,
});

const defaultCriteria = ref<any>(initialCriteria.value);

watch(salesAccount, async () => {
  defaultCriteria.value = initialCriteria.value;
  await parseRoute();
  await nextTick();
  if (salesAccount.value) {
    tradeFilterRef.value?.initCriteria();
    tradeFilterRef.value?.fetchData(1);
  }
});

const getFetchDataFunc = (criteria?: any) =>
  SalesService.queryTradeReportsOfSales(criteria);

const parseRoute = async () => {
  if (!route.query?.accountNumber) return;
  defaultCriteria.value.accountNumber = route.query.accountNumber as string;
  setTimeout(() => router.push({ query: {} }), 3000);
  await nextTick();
};

onMounted(async () => {
  isLoading.value = true;
  await parseRoute();
  isLoading.value = false;
});
</script>

<style scoped>
.card-body,
.sub-menu {
  overflow-x: auto;
  width: 100%;
  white-space: nowrap;
}
@media (max-width: 768px) {
  .table {
    font-size: 12px !important;
  }
}
</style>
