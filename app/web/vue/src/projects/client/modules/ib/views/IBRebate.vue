<template>
  <!--begin::Row-->
  <IBLayout activeMenuItem="rebate">
    <div
      class="h-full d-flex flex-column"
      v-if="agentAccount"
      style="border: 0 !important"
    >
      <div
        class="card mb-2 pt-md-0 round-bl-br"
        :class="isMobile ? 'p-2' : 'pl-3 pt-2'"
      >
        <TradeFilter
          ref="rebateFilterRef"
          :trigger="'button'"
          :filter-options="filterOptions"
          :total-value-handler="getSumUpValue"
          :service-handler="getFetchDataFunc"
          :default-criteria="defaultCriteria"
          type="rebate"
          class="ms-2"
        />

        <div
          v-if="rebateFilterRef?.filtered"
          @click="reset"
          class="cursor-pointer d-flex align-items-center gap-2 fs-5"
        >
          <img src="/images/left-arrow.png" style="width: 10px" />
          <span style="color: #4196f0">{{ $t("action.reset") }}</span>
        </div>
      </div>
      <div v-if="isMobile">
        <RebateMobile />
        <TableFooter
          @page-change="rebateFilterRef?.fetchData"
          :criteria="rebateFilterRef?.criteria"
        />
      </div>
      <div v-else class="card round-tl-tr flex-1" style="white-space: nowrap">
        <div class="card-header">
          <div class="card-title-noicon">
            <h2 class="font-medium fs-2">
              {{
                $t("action.showing") +
                " " +
                (rebateFilterRef?.criteria.total ?? "0") +
                " " +
                $t("title.results")
              }}
            </h2>
          </div>
        </div>
        <div class="card-body pt-0 overflow-auto">
          <table
            class="table align-middle table-row-bordered gy-3"
            id="kt_ecommerce_sales_table"
          >
            <thead>
              <tr class="text-start gs-0">
                <th>{{ $t("fields.accountNumber") }}</th>
                <th>{{ $t("fields.symbol") }}</th>
                <th>{{ $t("fields.ticket") }}</th>
                <th>{{ $t("fields.currency") }}</th>
                <th>{{ $t("fields.volume") }}</th>
                <th>{{ $t("fields.amountIn") }}</th>
                <th>{{ $t("fields.status") }}</th>
                <th>{{ $t("fields.createdOn") }}</th>
                <th>{{ $t("fields.closeTime") }}</th>
              </tr>
            </thead>
            <tbody v-if="rebateFilterRef?.isLoading">
              <LoadingRing />
            </tbody>
            <tbody
              v-else-if="
                !rebateFilterRef?.isLoading && rebateFilterRef?.data.length == 0
              "
            >
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr
                v-for="(item, index) in rebateFilterRef?.getData()"
                :key="index"
              >
                <td>
                  <div>{{ item.trade?.accountName }}</div>
                  <div>{{ item.trade?.accountNumber }}</div>
                </td>
                <td>{{ item.trade?.symbol }}</td>
                <td>{{ item.trade?.ticket }}</td>
                <td class="text-start">
                  {{ $t(`type.currency.${item.trade.currencyId}`) }}
                </td>
                <td>{{ item.trade?.volume / 100 }}</td>

                <td>
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </td>
                <td>
                  {{ $t(`type.transactionState.${item.stateId}`) }}
                </td>
                <td>
                  <TimeShow
                    :date-iso-string="item.createdOn"
                    type="exactTime"
                  />
                </td>
                <td>
                  <TimeShow
                    :date-iso-string="item.trade?.closeAt"
                    type="exactTime"
                  />
                </td>
              </tr>
              <tr class="">
                <td>{{ $t("title.subTotal") }}</td>
                <td colspan="2"></td>
                <td>
                  {{ rebateFilterRef?.criteria.pageTotalVolume / 100 ?? 0 }}
                </td>
                <td>
                  <BalanceShow
                    :balance="
                      normalizeAmountList(
                        rebateFilterRef?.criteria.pageTotalAmount
                      ) ?? 0
                    "
                    :currency-id="agentAccount.currencyId"
                  />
                </td>
                <td colspan="4"></td>
              </tr>
              <tr class="bg-light">
                <td>{{ $t("title.total") }}</td>
                <td colspan="2"></td>
                <td>
                  {{ rebateFilterRef?.criteria.totalVolume / 100 ?? 0 }}
                </td>
                <td>
                  <BalanceShow
                    :balance="
                      normalizeAmountList(
                        rebateFilterRef?.criteria.totalAmount
                      ) ?? 0
                    "
                    :currency-id="agentAccount.currencyId"
                  />
                </td>
                <td colspan="4"></td>
              </tr>
              <tr></tr>
            </tbody>
          </table>
          <TableFooter
            @page-change="rebateFilterRef?.fetchData"
            :criteria="rebateFilterRef?.criteria"
          />
        </div>
      </div>
    </div>
  </IBLayout>
  <!-- <IBReportModal ref="IBReportModalRef" /> -->

  <!--end::Row-->
</template>

<script lang="ts" setup>
import { ref, onMounted, provide } from "vue";
import IbService from "../services/IbService";
import BalanceShow from "@/components/BalanceShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { isMobile } from "@/core/config/WindowConfig";
import IbReportService from "@/projects/client/modules/ib/services/IbReportService";
import { useStore } from "@/store";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import IBLayout from "../components/IBLayout.vue";
//import headerMenu from "../components/menu/headerMenu.vue";
import IBReportModal from "../components/modal/IBReportModal.vue";
import RebateMobile from "../components/rebate/RebateMobile.vue";
import { normalizeAmountList } from "@/lib/utils";
const store = useStore();
const agentAccount = ref(store.state.AgentModule.agentAccount);
const rebateFilterRef = ref<InstanceType<typeof TradeFilter>>();
provide("agentAccount", agentAccount);
provide("rebateFilterRef", rebateFilterRef);
const filterOptions = ["symbol", "period", "accountNumber"];
const initialCriteria = {
  page: 1,
  size: 20,
};

const IBReportModalRef = ref<InstanceType<typeof IBReportModal>>();
const defaultCriteria = ref(initialCriteria);

const reset = async () => {
  rebateFilterRef.value?.initCriteria();
  await rebateFilterRef.value?.fetchData(1);
};

const getFetchDataFunc = (criteria: any) =>
  IbService.queryRebateReportsOfAgent(criteria);

const getSumUpValue = (criteria: any) =>
  IbReportService.getRebateTotalValue(criteria);

const showReportModal = () => {
  IBReportModalRef.value?.show();
};

onMounted(async () => {
  moibleNavScroller(".ib-menu", ".scroll-to");
});
</script>

<style scoped>
.tabBtn {
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 14px;
  border-radius: 5px 5px 0 0;
  width: 150px;
  height: 40px;
  margin-right: 6px;
  cursor: pointer;
  border: 2px solid #ffd400;
  margin-bottom: -2px;
  color: white;
}

.tabActive {
  color: black;

  background-color: #ffd400;
}

.tabLine {
  width: 98%;
  height: 3px;
  background-color: #ffd400;
  margin-top: -1px;
}

.IB-rebate-account-type-select {
  width: 300px;
  height: 56px;
  padding: 16px 20px;
  border: 1px solid #b1b1b1;
  border-radius: 4px;
}
</style>
