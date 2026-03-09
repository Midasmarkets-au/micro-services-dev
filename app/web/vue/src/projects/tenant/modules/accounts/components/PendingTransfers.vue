<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">{{ $t("title.pendingTransfers") }}</div>
        <div class="card-toolbar">
          <router-link to="/funding/transfer">
            {{ $t("title.viewMore") }}</router-link
          >
        </div>
      </div>
      <div class="card-body py-4">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.id") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.amount") }}</th>
              <th class="">{{ $t("fields.sender") }}</th>
              <th class="">{{ $t("fields.receiver") }}</th>
              <th class="min-w-125px">{{ $t("fields.createdOn") }}</th>
              <!-- <th class="text-center min-w-150px">
                  {{ $t("action.action") }}
                </th> -->
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && transactions.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, index) in transactions" :key="index">
              <td class="">{{ item.id }}</td>
              <td class="">{{ $t(`type.currency.${item.currencyId}`) }}</td>
              <td class="">
                <BalanceShow
                  :balance="item.amount"
                  :currency-id="item.currencyId"
                />
              </td>
              <td class="">
                <div>
                  {{ $t(`type.transactionAccount.${item.sourceAccountType}`) }}
                </div>
                <div>ID: {{ item.sourceAccountId }}</div>
              </td>

              <td class="">
                <div>
                  {{ $t(`type.transactionAccount.${item.targetAccountType}`) }}
                </div>
                <div>ID: {{ item.targetAccountId }}</div>
              </td>
              <td class=""><TimeShow :date-iso-string="item.createdOn" /></td>

              <td class="text-center"></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";

import AccountService from "../services/AccountService";
import { TransactionStateType } from "@/core/types/StateInfos";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";

const isLoading = ref(true);
const transactions = ref(Array<any>());
const criteria = ref({
  page: 1,
  size: 5,
  stateId: TransactionStateType.TransferAwaitingApproval,
});

onMounted(async () => {
  try {
    const res = await AccountService.queryTransactions(criteria.value);
    criteria.value = res.criteria;
    transactions.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
});
</script>

<style scoped></style>
