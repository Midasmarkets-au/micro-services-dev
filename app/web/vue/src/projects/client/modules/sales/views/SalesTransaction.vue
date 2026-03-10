<template>
  <!-- <div>
    <div v-if="!salesAccount">{{ $t("action.noSalesAccount") }}</div>
    <div v-else>
      <SalesCenterMenu activeMenuItem="transaction" />
    </div> -->
  <SalesLayout activeMenuItem="transaction">
    <div class="card pl-3 round-bl-br">
      <TradeFilter
        ref="tradeFilterRef"
        :trigger="'button'"
        :filter-options="filterOptions"
        :service-handler="getFetchDataFunc"
        :default-criteria="defaultCriteria"
        type="transfer"
        class="ms-2"
      />
    </div>
    <div v-if="isMobile">
      <TransactionMobile />
      <TableFooter
        @page-change="tradeFilterRef?.fetchData"
        :criteria="tradeFilterRef?.criteria"
      />
    </div>
    <div v-else class="card mt-2 round-tl-tr flex-1">
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
          <div class="d-flex mt-1 ms-5 fs-5">
            <div class="me-2">{{ $t("title.total") }}:</div>

            <BalanceShow
              :balance="tradeFilterRef?.criteria.totalAmount"
              :currency="tradeFilterRef?.data[0]?.currencyId"
            />
          </div>
        </div>
        <div class="card-toolbar">
          <el-switch
            class="me-50px"
            v-model="isSelectingClient"
            @change="onIsSelectingClientChange"
            width="65"
            size="large"
            inline-prompt
            style="
              --el-switch-on-color: #0053ad;
              --el-switch-off-color: #ffc420;
            "
            :active-text="$t('fields.client')"
            :inactive-text="$t('fields.ib')"
          />
          <!-- <button
            v-if="false && tradeFilterRef?.data.length !== 0"
            class="border-0 bg-transparent d-flex align-items-center"
          >
            <span class="me-2" style="color: #4196f0"> </span>
            <span class="svg-icon svg-icon-3">
              <inline-svg src="/images/icons/general/download.svg" />
            </span>
          </button> -->
        </div>
      </div>
      <div class="card-body pt-0 overflow-auto" style="white-space: nowrap">
        <table
          class="table align-middle table-row-bordered gy-3"
          id="kt_ecommerce_sales_table"
        >
          <thead>
            <tr class="text-start gs-0">
              <th>{{ $t("fields.user") }}</th>
              <th>{{ $t("fields.email") }}</th>
              <th>{{ $t("fields.currency") }}</th>
              <th class="text-center">{{ $t("fields.sourceAccount") }}</th>
              <th class="text-center">{{ $t("fields.targetAccount") }}</th>
              <th>{{ $t("fields.amount") }}</th>
              <th>{{ $t("fields.status") }}</th>
              <th>{{ $t("fields.time") }}</th>
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
              v-for="(item, index) in tradeFilterRef?.getData()"
              :key="index"
            >
              <td>
                <div class="d-md-flex align-items-center">
                  <UserAvatar
                    :avatar="item.user.avatar"
                    :name="item.user.displayName"
                    size="40px"
                    class="me-3"
                    side="client"
                    rounded
                  />
                  <span>
                    {{
                      item.user.nativeName ||
                      item.user.displayName ||
                      `${item.user.firstName} ${item.user.lastNameName}`
                    }}
                  </span>
                </div>
              </td>

              <td class="text-start">
                {{ item.user.email }}
              </td>

              <td class="text-start">
                {{ $t(`type.currency.${item.currencyId}`) }}
              </td>

              <td class="text-center">
                <div class="d-flex flex-column align-items-center">
                  <span class="text-gray-800">
                    No.
                    <span class="">
                      {{ item.targetAccount.accountNumber }} ({{
                        $t(`type.currency.${item.targetAccount.currencyId}`)
                      }})</span
                    >
                  </span>

                  <span class="fs-7">
                    Group:
                    <span class="">
                      {{ item.targetAccount.group || "***" }}</span
                    >
                  </span>
                </div>
              </td>

              <td class="text-center">
                <div class="d-flex flex-column align-items-center">
                  <span class="text-gray-800">
                    No.
                    <span class="">
                      {{ item.sourceAccount.accountNumber }} ({{
                        $t(`type.currency.${item.sourceAccount.currencyId}`)
                      }})</span
                    >
                  </span>

                  <span class="fs-7">
                    Group:
                    <span class="">
                      {{ item.sourceAccount.group || "***" }}</span
                    >
                  </span>
                </div>
              </td>

              <td class="text-start">
                <BalanceShow
                  :balance="item.amount"
                  :currencyId="item.currencyId"
                />
              </td>
              <td class="text-start">
                <span
                  class="badge"
                  :class="`badge-${
                    {
                      [TransactionStateType.TransferCreated]: 'primary',
                      [TransactionStateType.TransferCompleted]: 'success',
                      [TransactionStateType.TransferRejected]: 'danger',
                      [TransactionStateType.TransferApproved]: 'warning',
                    }[item.stateId] ?? 'info'
                  }`"
                  >{{
                    $t(`type.transactionState.${item.stateId}`).replace(
                      /^transfer\s+/i,
                      ""
                    )
                  }}</span
                >
              </td>

              <td class="text-start">
                <TimeShow type="inFields" :date-iso-string="item.createdOn" />
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter
          @page-change="tradeFilterRef?.fetchData"
          :criteria="tradeFilterRef?.criteria"
        />
      </div>
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
import BalanceShow from "@/components/BalanceShow.vue";
import { useRoute, useRouter } from "vue-router";
import SalesCenterMenu from "../components/SalesCenterMenu.vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import UserAvatar from "@/components/UserAvatar.vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import { moibleNavScroller } from "@/core/utils/mobileNavScroller";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import TransactionMobile from "../components/transaction/TransactionMobile.vue";
import { isMobile } from "@/core/config/WindowConfig";
import SalesLayout from "../components/SalesLayout.vue";
const store = useStore();
const route = useRoute();
const router = useRouter();
const isLoading = ref(true);
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const tradeFilterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["period", "target"];

const isSelectingClient = ref(true);
const onIsSelectingClientChange = () => {
  //
  tradeFilterRef.value.criteria.role = isSelectingClient.value
    ? AccountRoleTypes.Client
    : AccountRoleTypes.IB;
  tradeFilterRef.value?.fetchData(1);
};
provide("tradeFilterRef", tradeFilterRef);
provide("isSelectingClient", isSelectingClient);
provide("onIsSelectingClientChange", onIsSelectingClientChange);

const initialCriteria = ref<any>({
  page: 1,
  size: isMobile ? 15 : 25,
  isClosed: false,
  accountNumber: route.query?.accountNumber as string,
  // openedFrom: moment().startOf("day").toISOString(),
  // openedTo: moment().endOf("day").toISOString(),
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
  SalesService.queryTransactionReportsOfSales(criteria);

const parseRoute = async () => {
  if (!route.query?.accountNumber) return;
  defaultCriteria.value.accountNumber = route.query.accountNumber as string;
  setTimeout(() => router.push({ query: {} }), 3000);
  await nextTick();
};

onMounted(async () => {
  moibleNavScroller(".sub-menu", ".scroll-to");
  moibleNavScroller(".ib-menu", ".scroll-to");
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
