<template>
  <div>
    <div v-if="!repAccount">{{ $t("action.noRepAccount") }}</div>
    <div v-else>
      <div class="sub-menu d-flex">
        <router-link to="/rep" class="sub-menu-item">{{
          $t("title.customer")
        }}</router-link>
        <router-link to="/rep/trade" class="sub-menu-item active">{{
          $t("title.trade")
        }}</router-link>
        <router-link to="/rep/transaction" class="sub-menu-item">{{
          $t("title.transfer")
        }}</router-link>
        <router-link to="/rep/deposit" class="sub-menu-item">{{
          $t("title.deposit")
        }}</router-link>
        <router-link to="/rep/withdrawal" class="sub-menu-item">{{
          $t("title.withdrawal")
        }}</router-link>
        <!-- <router-link to="/rep/lead" class="sub-menu-item">{{
          $t("title.repLeadSystem")
        }}</router-link> -->
      </div>
    </div>

    <div class="card round-bl-br">
      <TradeFilter
        ref="tradeFilterRef"
        :trigger="'button'"
        :filter-options="filterOptions"
        :service-handler="getFetchDataFunc"
        :default-criteria="defaultCriteria"
      />
    </div>
    <div class="card mt-2 round-tl-tr">
      <div class="card-header">
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
          class="table align-middle table-row-bordered gy-5"
          id="kt_ecommerce_rep_table"
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
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch, computed, nextTick } from "vue";
import { useStore } from "@/store";
import RepService from "../services/RepService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import { useRoute, useRouter } from "vue-router";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { handleTradeBuySellDisplay } from "@/core/helpers/helpers";
const isLoading = ref(true);
const route = useRoute();
const router = useRouter();
const store = useStore();
const repAccount = computed(() => store.state.RepModule.repAccount);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = [
  "closed",
  "service",
  "period",
  "symbol",
  "accountNumber",
];

const initialCriteria = ref<any>({
  page: 1,
  size: 10,
  isClosed: false,
  // openedFrom: moment().startOf("day").toISOString(),
  // openedTo: moment().endOf("day").toISOString(),
});
const defaultCriteria = ref<any>(initialCriteria.value);

watch(repAccount, async () => {
  defaultCriteria.value = initialCriteria.value;
  await parseRoute();
  await nextTick();
  if (repAccount.value) {
    tradeFilterRef.value?.initCriteria();
    tradeFilterRef.value?.fetchData(1);
  }
});

const getFetchDataFunc = (criteria?: any) =>
  RepService.queryTradeReportsOfRep(criteria);

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
