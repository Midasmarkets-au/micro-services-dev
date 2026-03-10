<template>
  <!--begin::Row-->
  <IBLayout activeMenuItem="trade">
    <IBAccountsSelector v-if="!agentAccount" />
    <div class="h-full" v-if="agentAccount" style="border: 0 !important">
      <div v-if="!isMobile" class="card h-full">
        <div class="px-6">
          <TradeFilter
            ref="tradeFilterRef"
            :trigger="'button'"
            :filter-options="filterOptions"
            :service-handler="getFetchDataFunc"
            :default-criteria="defaultCriteria"
            :type="'trade'"
          />
          <div
            v-if="tradeFilterRef?.filtered"
            @click="reset"
            class="cursor-pointer d-flex align-items-center gap-2 mb-4 fs-5"
          >
            <img src="/images/left-arrow.png" style="width: 10px" />
            <span style="color: #4196f0">{{ $t("action.reset") }}</span>
          </div>
        </div>
        <div class="ib_content">
          <!--这段代码没有使用-->
          <div class="card-header" v-if="false">
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
                <span class="me-2" style="color: #4196f0"> </span>
                <span class="svg-icon svg-icon-3">
                  <inline-svg src="/images/icons/general/download.svg" />
                </span>
              </button>
            </div>
          </div>
          <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
            <table
              class="table align-middle table-row-bordered gy-2 mt-2"
              id="kt_ecommerce_sales_table"
            >
              <thead>
                <tr class="text-start gs-0">
                  <!-- <th class="text-start">{{ $t("fields.select") }}</th> -->
                  <th>{{ $t("fields.accountNumber") }}</th>
                  <th>{{ $t("fields.ticket") }}</th>
                  <th>{{ $t("fields.symbol") }}</th>
                  <th>{{ $t("fields.type") }}</th>
                  <th>{{ $t("fields.volume") }}</th>
                  <th>{{ $t("fields.openTime") }}</th>
                  <th>{{ $t("fields.openPrice") }}</th>
                  <th>{{ $t("fields.s/l") }}</th>
                  <th>{{ $t("fields.tp") }}</th>

                  <template v-if="tradeFilterRef?.criteria.isClosed">
                    <th>
                      {{ $t("fields.closeTime") }}
                    </th>
                    <th>
                      {{ $t("fields.closePrice") }}
                    </th>
                  </template>
                  <th>{{ $t("fields.commission") }}</th>
                  <th>{{ $t("fields.swaps") }}</th>
                  <th>{{ $t("fields.p/l") }}</th>
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
              <tbody v-else class="font-medium">
                <tr
                  class="text-start"
                  v-for="(trade, index) in tradeFilterRef?.getData()"
                  :key="index"
                >
                  <td>{{ trade.accountNumber }}</td>
                  <td>{{ trade.ticket }}</td>
                  <!-- <td class="text-center">{{ transaction.tp }}</td> -->
                  <td>{{ trade.symbol }}</td>
                  <td>
                    <span>
                      {{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}
                    </span>
                  </td>
                  <td>{{ trade.volume }}</td>
                  <td>
                    <TimeShow
                      type="GMTinFields"
                      :date-iso-string="trade.openAt"
                    />
                  </td>
                  <td>
                    {{ handleTradeFormatted(trade.openPrice, trade.digits) }}
                  </td>
                  <td>{{ trade.sl }}</td>
                  <td>{{ trade.tp }}</td>
                  <template v-if="tradeFilterRef?.criteria.isClosed">
                    <td>
                      <TimeShow
                        v-if="tradeFilterRef?.criteria.isClosed"
                        type="GMTinFields"
                        :date-iso-string="trade.closeAt"
                      />
                      <span v-else>-</span>
                    </td>
                    <td>
                      {{ handleTradeFormatted(trade.closePrice, trade.digits) }}
                    </td>
                  </template>
                  <td>{{ trade.commission }}</td>
                  <td>{{ trade.swaps }}</td>
                  <td
                    :class="{
                      'text-danger': trade.profit < 0,
                      'text-success': trade.profit > 0,
                    }"
                  >
                    {{ handleTradeFormatted(trade.profit, 2) }}
                  </td>
                </tr>
                <tr style="line-height: 60px">
                  <td>{{ $t("title.subTotal") }}</td>
                  <td colspan="3"></td>
                  <td>
                    {{ tradeFilterRef?.criteria.pageTotalVolume ?? 0 }}
                  </td>
                  <td colspan="6" v-if="tradeFilterRef?.criteria.isClosed"></td>
                  <td v-else colspan="4"></td>

                  <td>
                    {{ tradeFilterRef?.criteria.pageTotalCommission ?? 0 }}
                  </td>
                  <td>
                    {{ tradeFilterRef?.criteria.pageTotalSwap ?? 0 }}
                  </td>
                  <td
                    :class="{
                      'text-danger':
                        tradeFilterRef?.criteria.pageTotalProfit < 0,
                      'text-success':
                        tradeFilterRef?.criteria.pageTotalProfit > 0,
                    }"
                  >
                    {{ tradeFilterRef?.criteria.pageTotalProfit ?? 0 }}
                  </td>
                </tr>
                <tr class="bg-light" style="line-height: 60px">
                  <td>{{ $t("title.total") }}</td>
                  <td colspan="3"></td>
                  <td>
                    {{ tradeFilterRef?.criteria.totalVolume ?? 0 }}
                  </td>
                  <td colspan="6" v-if="tradeFilterRef?.criteria.isClosed"></td>
                  <td v-else colspan="4"></td>

                  <td>
                    {{ tradeFilterRef?.criteria.totalCommission ?? 0 }}
                  </td>
                  <td>
                    {{ tradeFilterRef?.criteria.totalSwap ?? 0 }}
                  </td>
                  <td
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
      <div v-else>
        <div class="card p-2">
          <TradeFilter
            ref="tradeFilterRef"
            :trigger="'button'"
            :filter-options="mobileFilterOptions"
            :service-handler="getFetchDataFunc"
            :default-criteria="defaultCriteria"
            :type="'trade'"
          />
        </div>
        <TradeCardMobile />
        <TableFooter
          @page-change="tradeFilterRef?.fetchData"
          :criteria="tradeFilterRef?.criteria"
        />
      </div>
    </div>
  </IBLayout>
</template>

<script lang="ts" setup>
import { ref, onMounted, provide } from "vue";
import IBLayout from "../components/IBLayout.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import svc from "../services/IbService";
import TimeShow from "@/components/TimeShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import IBAccountsSelector from "../components/IBAccountsSelector.vue";
import TradeFilter from "../../../components/TradeFilter.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { isMobile } from "@/core/config/WindowConfig";
import { useStore } from "@/store";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import {
  handleTradeBuySellDisplay,
  handleTradeFormatted,
} from "@/core/helpers/helpers";
import TradeCardMobile from "@/projects/client/components/trade/TradeCardMobile.vue";
const store = useStore();
const agentAccount = ref(store.state.AgentModule.agentAccount);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
provide("tradeFilterRef", tradeFilterRef);
provide("isCustomer", ref(false));
const filterOptions = [
  "closed",
  "service",
  "period",
  "symbol",
  "accountNumber",
];

const mobileFilterOptions = [
  "closed",
  "service",
  "period",
  "symbol",
  "accountNumber",
];

const reset = async () => {
  tradeFilterRef.value?.initCriteria();
  await tradeFilterRef.value?.fetchData(1);
};

const projectConfig: PublicSetting = store.state.AuthModule.config;
const initialCriteria = {
  page: 1,
  size: 25,
  isClosed: false,
  from: null,
  to: null,
  serviceId: 30,
};

const defaultCriteria = ref(initialCriteria);

const getFetchDataFunc = (criteria?: any) => {
  return svc.queryTradeReportsOfAgent(criteria);
};

onMounted(async () => {
  moibleNavScroller(".ib-menu", ".scroll-to");
});
</script>
