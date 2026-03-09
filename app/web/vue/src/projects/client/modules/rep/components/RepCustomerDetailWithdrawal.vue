<template>
  <div>
    <RepWithdrawalFilter
      ref="filterRef"
      :trigger="'button'"
      :filter-options="filterOptions"
      :service-handler="getFetchDataFunc"
      :default-criteria="defaultCriteria"
    />

    <div
      v-if="filterRef?.filtered"
      @click="reset"
      class="cursor-pointer d-flex align-items-center gap-2 mb-4 fs-5"
    >
      <img src="/images/left-arrow.png" style="width: 10px" />
      <span style="color: #4196f0">Back to all transaction</span>
    </div>
    <!-- <button @click="reset">Back to all transaction</button> -->
    <h2 class="fw-semibold">
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
        filterRef?.isLoading ||
        (filterRef?.data && filterRef?.data.length === 0)
      "
      class="table align-middle table-row-bordered gy-5"
      id="kt_ecommerce_rep_table"
    >
      <tbody v-if="filterRef?.isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!filterRef?.isLoading && filterRef?.data.length === 0">
        <NoDataBox />
      </tbody>
    </table>
    <table v-else class="table align-middle table-row-bordered gy-5">
      <template v-for="(group, index) in groupedItems" :key="index">
        <thead style="border-bottom: #717171 1px solid">
          <tr class="text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0">
            <th
              class="text-start"
              style="
                width: 40%;
                font-size: 16px;
                font-weight: 400;
                color: #212121;
              "
            >
              {{ group.date }}
            </th>

            <th class="text-start">{{ $t("fields.status") }}</th>
            <th class="text-start">{{ $t("fields.currency") }}</th>
            <th class="text-start">{{ $t("fields.exchangeRate") }}</th>
            <th class="text-start">{{ $t("fields.amount") }}</th>
            <th class="text-start">{{ $t("fields.time") }}</th>
          </tr>
        </thead>

        <tbody
          class="fw-semibold overflow-auto"
          style="color: #4d4d4d; white-space: nowrap"
        >
          <tr
            class="text-start"
            v-for="(item, index) in group.objects"
            :key="index"
          >
            <td class="text-start">
              <div class="d-flex gap-5 align-items-center">
                <span class="symbol symbol-25px mx-2">
                  <img
                    src="/images/icons/finance/wallet-withdraw.svg"
                    alt="icon"
                    style="width: 34px; height: 34px"
                  />
                </span>
                <div class="d-flex flex-column">
                  <span class="fs-4">
                    No.
                    {{ item.source.displayNumber }}
                    <span class="fs-9">
                      {{ $t(`type.currency.${item.source.currencyId}`) }}
                    </span>
                  </span>

                  <span class="fs-7">
                    {{ $t("fields.group") }}:
                    {{ item.source.agentGroupName || "***" }}
                  </span>
                </div>
              </div>
            </td>
            <td class="text-start">
              <span
                class="badge badge-info"
                :class="`badge-${
                  {
                    [TransactionStateType.WithdrawalCreated]: 'primary',
                    [TransactionStateType.WithdrawalCompleted]: 'success',
                    [TransactionStateType.WithdrawalTenantRejected]: 'danger',
                    [TransactionStateType.WithdrawalTenantApproved]: 'warning',
                  }[item.stateId] ?? 'info'
                }`"
                >{{
                  $t(`type.transactionState.${item.stateId}`).replace(
                    /^withdrawal\s+/i,
                    ""
                  )
                }}</span
              >
            </td>
            <td class="text-start">
              {{ $t(`type.currency.${item.currencyId}`) }}
            </td>
            <td class="text-start">
              {{ item.exchangeRate }}
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
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import RepWithdrawalFilter from "@/projects/client/modules/rep/components/RepWithdrawalFilter.vue";
import { useStore } from "@/store";
import RepService from "@/projects/client/modules/rep/services/RepService";
import moment from "moment/moment";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import { getLanguage } from "@/core/types/LanguageTypes";

const props = defineProps<{
  accountDetails: any;
}>();

const accountDetails = ref({} as any);

const store = useStore();
const repAccount = computed(() => store.state.RepModule.repAccount);
// const language = store.state.AuthModule.user.language;

const filterRef = ref<any>();
const filterOptions = ["size", "period"];

const initialCriteria = ref<any>({
  page: 1,
  size: 10,
  isClosed: false,
  accountUid: props.accountDetails.uid,
});
const defaultCriteria = ref<any>(initialCriteria.value);
const getFetchDataFunc = (criteria?: any) =>
  RepService.queryClientWithdrawal(criteria);

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
  filterRef.value?.initCriteria();
  await filterRef.value?.fetchData(1);
};
onMounted(() => {
  accountDetails.value = props.accountDetails;
});
</script>

<style scoped></style>
