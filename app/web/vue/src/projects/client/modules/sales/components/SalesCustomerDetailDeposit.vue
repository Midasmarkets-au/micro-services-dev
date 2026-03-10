<template>
  <div>
    <SalesDepositFilter
      ref="filterRef"
      :trigger="'button'"
      :filter-options="filterOptions"
      :service-handler="getFetchDataFunc"
      :default-criteria="defaultCriteria"
    />
  </div>

  <div
    v-if="filterRef?.filtered"
    @click="reset"
    class="cursor-pointer d-flex align-items-center gap-2 mb-4 fs-5"
  >
    <img src="/images/left-arrow.png" style="width: 10px" />
    <span style="color: #4196f0">Back to all transaction</span>
  </div>
  <!-- <button @click="reset">Back to all transaction</button> -->
  <h2 class="fs-3 font-medium">
    {{
      $t("action.showing") +
      " " +
      (filterRef?.criteria.total ?? "0") +
      " " +
      $t("title.results")
    }}
  </h2>

  <table
    v-if="
      filterRef?.isLoading || (filterRef?.data && filterRef?.data.length === 0)
    "
    class="table align-middle table-row-bordered fs-6 gy-5"
    id="kt_ecommerce_sales_table"
  >
    <tbody v-if="filterRef?.isLoading">
      <LoadingRing />
    </tbody>
    <tbody v-else-if="!filterRef?.isLoading && filterRef?.data.length === 0">
      <NoDataBox />
    </tbody>
  </table>
  <table v-else class="table align-middle table-row-bordered fs-6 gy-5">
    <template v-for="(group, index) in groupedItems" :key="index">
      <thead>
        <tr class="text-start text-uppercase gs-0">
          <th class="text-start">
            {{ group.date }}
          </th>

          <th class="text-start">{{ $t("fields.status") }}</th>
          <th class="text-start">{{ $t("fields.currency") }}</th>
          <th class="text-start">{{ $t("fields.amount") }}</th>
          <th class="text-start">{{ $t("fields.time") }}</th>
        </tr>
      </thead>

      <tbody class="overflow-auto">
        <tr
          class="text-start"
          v-for="(item, index) in group.objects"
          :key="index"
        >
          <td class="text-start">
            <div class="d-flex gap-5 align-items-center">
              <span class="symbol symbol-25px mx-2">
                <img
                  src="/images/icons/finance/wallet-deposit.svg"
                  alt="icon"
                  style="width: 34px; height: 34px"
                />
              </span>
              <div class="d-flex flex-column">
                <span class="fs-4">
                  No.
                  {{ item.targetTradeAccount.accountNumber }}
                  <span class="fs-9">
                    {{
                      $t(`type.currency.${item.targetTradeAccount.currencyId}`)
                    }}
                  </span>
                </span>

                <span class="fs-7">
                  {{ $t("fields.group") }}:
                  {{ item.targetTradeAccount.group || "***" }}
                </span>
              </div>
            </div>
          </td>
          <td class="text-start">
            <span
              class="badge badge-pending fw-normal"
              :class="{
                'badge-created':
                  item.stateId === TransactionStateType.DepositCreated ||
                  item.stateId === TransactionStateType.DepositPaymentCompleted,
                'badge-completed':
                  item.stateId === TransactionStateType.DepositCompleted,

                'badge-refused':
                  item.stateId === TransactionStateType.DepositTenantRejected,
              }"
              >{{ $t(`type.transactionState.${item.stateId}`) }}</span
            >
          </td>

          <td class="text-start">
            {{ $t(`type.currency.${item.currencyId}`) }}
          </td>

          <td class="text-start">
            <div>
              <BalanceShow
                :balance="item.amount"
                :currency-id="item.currencyId"
              />
            </div>
          </td>
          <td class="text-start">
            <TimeShow :date-iso-string="item.createdOn" format="h:mm a" />
          </td>
        </tr>
      </tbody>
    </template>
  </table>
  <TableFooter
    @page-change="filterRef?.fetchData"
    :criteria="filterRef?.criteria"
  />
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useStore } from "@/store";
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import SalesDepositFilter from "@/projects/client/modules/sales/components/SalesDepositFilter.vue";
import moment from "moment";
import { TransactionStateType } from "@/core/types/StateInfos";
import TimeShow from "@/components/TimeShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { getLanguage } from "@/core/types/LanguageTypes";

const props = defineProps<{
  accountDetails: any;
}>();

const accountDetails = ref({} as any);

const store = useStore();
const filterRef = ref<any>();
// const language = store.state.AuthModule.user.language;

const filterOptions = ["size", "period", "depositStatus"];

const initialCriteria = ref<any>({
  page: 1,
  size: 10,
  isClosed: false,
  accountUid: props.accountDetails.uid,
});
const defaultCriteria = ref<any>(initialCriteria.value);
const getFetchDataFunc = (criteria?: any) =>
  SalesService.queryClientDeposit(criteria);

const groupedItems = computed(() => {
  const groupedObject = filterRef.value?.data.reduce((acc, cur) => {
    const date = moment(cur.createdOn).locale(getLanguage.value);
    let dateKey = date.format("ddd DD MMM YYYY");
    if (!acc[dateKey]) {
      acc[dateKey] = [];
    }
    acc[dateKey].push(cur);
    return acc;
  }, {});

  if (!groupedObject) return [];
  return Object.keys(groupedObject).map((dateKey) => ({
    date: dateKey,
    objects: groupedObject[dateKey],
  }));
});
const reset = async () => {
  filterRef.value?.reset();
};

onMounted(() => {
  accountDetails.value = props.accountDetails;
  // console.log(props.accountDetails);
});
</script>

<style scoped></style>
