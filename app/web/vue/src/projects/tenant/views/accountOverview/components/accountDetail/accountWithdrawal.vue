<template>
  <table
    class="table align-middle table-row-dashed fs-6 gy-5"
    id="table_accounts_requests"
  >
    <thead>
      <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
        <th class="">{{ $t("fields.status") }}</th>
        <th class="">{{ $t("fields.paymentStatus") }}</th>
        <th class="">{{ $t("fields.paymentID") }}</th>
        <th class="">{{ $t("fields.currency") }}</th>
        <th class="">{{ $t("fields.withdrawAmount") }}</th>
        <th class="">{{ $t("fields.sourceBalance") }}</th>
        <th class="">{{ $t("fields.createdOn") }}</th>
        <th class="cell-color text-center">{{ $t("action.action") }}</th>
      </tr>
    </thead>
    <tbody v-if="isLoading">
      <LoadingRing />
    </tbody>
    <tbody v-else-if="!isLoading && data.length === 0">
      <NoDataBox />
    </tbody>

    <tbody v-else class="fw-semibold text-gray-900">
      <tr v-for="(item, index) in data" :key="index">
        <td class="">
          {{
            $t(`type.transactionState.${item.stateId}`).replace(
              /^Withdrawal\s+/,
              ""
            )
          }}
        </td>
        <td>
          <span
            class="badge"
            :class="{
              'badge-primary':
                item.payment.status === PaymentStatusTypes.Pending,
              'badge-success':
                item.payment.status === PaymentStatusTypes.Completed,
              'badge-danger': item.payment.status === PaymentStatusTypes.Failed,
              'badge-warning':
                item.payment.status === PaymentStatusTypes.Executing,
            }"
            >{{ $t(`type.paymentStatus.${item.payment.status}`) }}</span
          >
        </td>
        <td class="">{{ item.id }}</td>
        <td class="">
          {{ $t(`type.currency.${item.currencyId}`) }}
        </td>
        <td class="">
          <BalanceShow :currency-id="item.currencyId" :balance="item.amount" />
        </td>
        <td class="">
          <BalanceShow
            :currency-id="item.currencyId"
            :balance="item.source.balanceInCents"
          />
          <div
            v-if="item.source.accountType === TransactionAccountType.Wallet"
            class="badge badge-primary"
          >
            Wallet
          </div>
          <div
            v-if="
              item.source.accountType === TransactionAccountType.TradeAccount
            "
            class="badge badge-warning"
          >
            Account ({{ item.source.displayNumber }})
          </div>
        </td>
        <td class="">
          <TimeShow :date-iso-string="item.payment.createdOn" />
        </td>
      </tr>
    </tbody>
  </table>
  <TableFooter @page-change="pageChange" :criteria="criteria" />
</template>
<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import AccountOverviewService from "@/projects/tenant/views/accountOverview/services/AccountOverviewServices";
// import i18n from "@/core/plugins/i18n";
import { PaymentStatusTypes } from "@/core/types/PaymentTypes";
import { TransactionAccountType } from "@/core/types/StateInfos";
const isLoading = ref(false);
const data = ref<any>([]);
// const { t } = i18n.global;

const props = defineProps({
  account: {
    type: Object,
    required: true,
  },
});

const criteria = ref({
  page: 1,
  size: 10,
  accountId: props.account.id,
} as any);

const fetchData = async (_page: number) => {
  if (props.account.accountNumber === undefined) return;
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountOverviewService.queryWithdrawals(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

watch(
  () => props.account,
  (newAccount, oldAccount) => {
    if (newAccount && newAccount.id !== oldAccount?.id) {
      resetCriteria();
      fetchData(1);
    }
  },
  { deep: true }
);

const resetCriteria = () => {
  criteria.value = {
    size: 10,
    accountId: props.account.id,
    accountNumber: props.account.accountNumber,
    serviceId: props.account.serviceId,
  };
};

onMounted(() => {
  fetchData(1);
});

const pageChange = (page: number) => {
  criteria.value.page = page;
  fetchData(page);
};
</script>
