<template>
  <div>
    <div class="d-flex">
      <TradeFilter
        v-if="!isMobile"
        ref="filterRef"
        :trigger="'button'"
        :filter-options="filterOptions"
        :service-handler="getFetchDataFunc"
        :default-criteria="defaultCriteria"
      />
    </div>

    <div>
      <div>
        <div class="card-header px-0 flex-column flex-lg-row">
          <div class="card-title">
            <h2 class="fw-semibold">
              {{
                $t("action.showing") +
                " " +
                (filterRef?.criteria.total ?? "0") +
                " " +
                $t("title.results")
              }}
            </h2>
          </div>
          <div class="card-toolbar">
            <button
              v-if="false"
              class="border-0 bg-transparent d-flex align-items-center"
            >
              <span class="me-2" style="color: #4196f0"> </span>
              <span class="svg-icon svg-icon-3">
                <inline-svg src="/images/icons/general/download.svg" />
              </span>
            </button>
          </div>
        </div>

        <div
          class="ib-trd-fixed-container overflow-auto"
          style="white-space: nowrap"
        >
          <table
            class="table align-middle table-row-bordered fs-6 gy-5"
            id="kt_ecommerce_sales_table"
          >
            <thead>
              <tr class="text-center gs-0">
                <th class="text-center">{{ $t("fields.symbol") }}</th>
                <th class="text-center">{{ $t("fields.volume") }}</th>
                <th class="text-center">{{ $t("fields.amountIn") }}</th>
                <th class="text-center">{{ $t("fields.status") }}</th>
                <th class="text-center">{{ $t("fields.createdOn") }}</th>
                <th class="text-center">{{ $t("fields.holdUntilOn") }}</th>
              </tr>
            </thead>
            <tbody v-if="filterRef?.isLoading">
              <LoadingRing />
            </tbody>
            <tbody
              v-else-if="!filterRef?.isLoading && filterRef?.data.length == 0"
            >
              <NoDataBox />
            </tbody>
            <tbody v-else class="fs-6">
              <tr
                class="text-center"
                v-for="(item, index) in filterRef?.getData()"
                :key="index"
              >
                <td class="text-center">{{ item.trade?.symbol }}</td>
                <td class="text-center">{{ item.trade?.volume / 100 }}</td>

                <td class="text-center">
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </td>

                <td class="text-center">
                  {{ $t(`type.transactionState.${item.stateId}`) }}
                </td>

                <td class="text-center">
                  <TimeShow :date-iso-string="item.createdOn" />
                </td>
                <td class="text-center">
                  <TimeShow :date-iso-string="item.holdUntilOn" />
                </td>
              </tr>
              <tr class="">
                <td class="text-center">{{ $t("title.subTotal") }}</td>
                <td class="text-center">
                  {{ filterRef?.criteria.pageTotalVolume / 100 ?? 0 }}
                </td>
                <td class="text-center">
                  <BalanceShow
                    :balance="filterRef?.criteria.pageTotalAmount ?? 0"
                    :currency-id="agentAccount.currencyId"
                  />
                </td>
                <td colspan="3"></td>
              </tr>
              <tr class="bg-light">
                <td class="text-center">{{ $t("title.total") }}</td>
                <td class="text-center">
                  {{ filterRef?.criteria.totalVolume / 100 ?? 0 }}
                </td>
                <td class="text-center">
                  <BalanceShow
                    :balance="filterRef?.criteria.totalAmount ?? 0"
                    :currency-id="agentAccount.currencyId"
                  />
                </td>
                <td colspan="3"></td>
              </tr>
            </tbody>
          </table>
        </div>
        <TableFooter
          @page-change="filterRef?.fetchData"
          :criteria="filterRef?.criteria"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import { computed, watch, ref } from "vue";
import IbService from "../services/IbService";
import TimeShow from "@/components/TimeShow.vue";
import { isMobile } from "@/core/config/WindowConfig";
import BalanceShow from "@/components/BalanceShow.vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";

const store = useStore();
const route = useRoute();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);

const accountUid = ref(parseInt((route.params.accountId as string) || "-1"));

const defaultCriteria = ref({
  page: 1,
  size: 10,
  sourceAccountUid: accountUid.value,
});

watch(
  () => route.params.accountId,
  (newVal) => {
    accountUid.value = parseInt((newVal as string) || "-1");
    if (accountUid.value !== -1) filterRef.value?.fetchData(1);
  }
);

const filterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["period", "size", "symbol"];

const getFetchDataFunc = (_criteria: any) =>
  IbService.queryRebateReportsOfAgent({
    ..._criteria,
    sourceAccountUid: accountUid.value,
  });
</script>

<style scoped lang="scss"></style>
