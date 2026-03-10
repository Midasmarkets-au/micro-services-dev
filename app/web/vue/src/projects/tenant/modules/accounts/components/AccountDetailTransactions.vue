<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("title.transferHistory") }}</div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.client") }}</th>
            <th class="">{{ $t("fields.source") }}</th>
            <th class="">{{ $t("fields.target") }}</th>
            <th class="">{{ $t("fields.amount") }}</th>
            <th class="">{{ $t("fields.createdOn") }}</th>
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
            <td class="d-flex align-items-center">
              <UserInfo v-if="item.user" :user="item.user" class="me-2" />
            </td>
            <!--              <td class="">{{ item.id }}</td>-->

            <td class="">
              <div>
                {{
                  item.sourceAccountType == 1
                    ? $t(`type.transactionAccount.${item.sourceAccountType}`) +
                      " " +
                      (item.sourceAccountType === TransactionAccountType.Wallet
                        ? $t(`type.currency.840`)
                        : item.sourceAccountNumber)
                    : $t(`type.transactionAccount.${item.sourceAccountType}`) +
                      " " +
                      (item.sourceAccountType === TransactionAccountType.Wallet
                        ? $t(`type.currency.${item.currencyId}`)
                        : item.sourceAccountNumber)
                }}
              </div>
              <div>
                <BalanceShow
                  class="fw-bold"
                  :balance="item.sourceAccountBalanceInCents"
                  :currency-id="
                    item.sourceAccountType == 1 ? 840 : item.currencyId
                  "
                />

                <!-- <BalanceShow
                  class="fw-bold"
                  :balance="item.sourceAccountBalanceInCents"
                  :currency-id="item.currencyId"
                /> -->
              </div>
            </td>

            <td class="">
              <div>
                {{
                  $t(`type.transactionAccount.${item.targetAccountType}`) +
                  " " +
                  (item.targetAccountType ===
                  TransactionAccountType.TradeAccount
                    ? item.targetAccountNumber
                    : $t(`type.currency.${item.currencyId}`))
                }}
              </div>
              <div>
                <BalanceShow
                  class="fw-bold"
                  :balance="item.targetAccountBalanceInCents"
                  :currency-id="item.currencyId"
                />
              </div>
            </td>

            <td class="">
              <BalanceShow
                class="fw-bold"
                :balance="item.amount"
                :currency-id="item.currencyId"
              />
            </td>
            <td class=""><TimeShow :date-iso-string="item.createdOn" /></td>

            <td class="text-center"></td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import AccountService from "../services/AccountService";
import { TransactionAccountType } from "@/core/types/StateInfos";
import TableFooter from "@/components/TableFooter.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BalanceShow from "@/components/BalanceShow.vue";
import { ActionType } from "@/core/types/Actions";
import TimeShow from "@/components/TimeShow.vue";

const props = defineProps<{
  accountId: number;
  partyId: number;
}>();

const isLoading = ref(true);

const criteria = ref({
  accountId: props.accountId,
} as any);

const transactions = ref(Array<any>());

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  try {
    const responseBody = await AccountService.queryTransactions(criteria.value);
    criteria.value = responseBody.criteria;
    transactions.value = responseBody.data;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(async () => {
  await fetchData(1);
});

const pageChange = (newPage: number) => {
  fetchData(newPage);
};
</script>

<style scoped></style>
