<template>
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
    <tbody v-else-if="!isLoading && data.length === 0">
      <NoDataBox />
    </tbody>

    <tbody v-else class="fw-semibold text-gray-900">
      <tr v-for="(item, index) in data" :key="index">
        <td class="d-flex align-items-center">
          <UserInfo v-if="item.user" :user="item.user" class="me-2" />
        </td>
        <!--              <td class="">{{ item.id }}</td>-->

        <td class="">
          <div>
            {{
              $t(`type.transactionAccount.${item.sourceAccountType}`) +
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
              :currency-id="item.currencyId"
            />
          </div>
        </td>

        <td class="">
          <div>
            {{
              $t(`type.transactionAccount.${item.targetAccountType}`) +
              " " +
              (item.targetAccountType === TransactionAccountType.TradeAccount
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
</template>
<script lang="ts" setup>
import { ref, onMounted, watch } from "vue";
import AccountOverviewService from "@/projects/tenant/views/accountOverview/services/AccountOverviewServices";
// import i18n from "@/core/plugins/i18n";
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
  size: 10,
  accountId: props.account.id,
} as any);

const fetchData = async (_page: number) => {
  if (props.account.accountNumber === undefined) return;
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountOverviewService.queryTransactions(criteria.value);
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
