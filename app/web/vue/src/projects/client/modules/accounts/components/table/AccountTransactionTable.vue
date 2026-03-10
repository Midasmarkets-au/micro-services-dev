<template>
  <div class="">
    <div class="d-flex align-items-center justify-content-between">
      <TradeFilter
        ref="filterRef"
        :default-criteria="defaultCriteria"
        :service-handler="getTransactionHandler"
        :filter-options="filterOptions"
        :trigger="'button'"
      />

      <div v-if="!isMobile" class="d-flex gap-2">
        <button
          v-if="
            props.accountsList.length > 1 &&
            enableTransfer(permissions) === true
          "
          class="btn btn-secondary"
          @click.prevent="openTransferToAccountPanel"
        >
          {{ $t("action.transferOut") }}
        </button>
      </div>
    </div>
    <div
      v-if="filterRef?.filtered"
      @click="reset"
      class="cursor-pointer d-flex align-items-center gap-2 mb-4 fs-5"
    >
      <img src="/images/left-arrow.png" style="width: 10px" />
      <span style="color: #4196f0">Back to all transaction</span>
    </div>
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
        isLoading ||
        (filterRef?.data && filterRef?.data.length === 0)
      "
      class="table align-middle table-row-bordered fs-6 gy-5"
      id="kt_ecommerce_sales_table"
    >
      <tbody v-if="filterRef?.isLoading || isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!filterRef?.isLoading && filterRef?.data.length === 0">
        <NoDataBox />
      </tbody>
    </table>

    <template v-else>
      <table
        v-if="filterRef?.data && filterRef?.data.length !== 0 && !isMobile"
        class="table align-middle table-row-bordered fs-6 gy-5"
        id="kt_ecommerce_sales_table"
      >
        <template v-for="(group, index) in groupedItems" :key="index">
          <thead>
            <tr class="text-start text-uppercase gs-0">
              <th class="text-start col-6 date-title">
                {{ group.date }}
              </th>

              <th class="text-start col-3">{{ $t("fields.status") }}</th>
              <th class="text-start col-2">{{ $t("fields.amount") }}</th>
              <th class="text-start col-1">{{ $t("fields.createdOn") }}</th>
            </tr>
          </thead>

          <tbody>
            <tr
              class="text-start"
              v-for="(item, index) in group.objects"
              :key="index"
            >
              <td class="text-start">
                <div class="d-flex align-items-center">
                  <label class="d-flex align-items-center">
                    <span
                      :class="{
                        ['is-this-account']:
                          item.sourceAccountNumber ===
                          accountDetails.tradeAccount.accountNumber,
                      }"
                      >No. {{ item.sourceAccountNumber }}</span
                    >
                    <span class="symbol symbol-30px mx-2">
                      <img
                        src="/images/icons/finance/wallet-withdraw.svg"
                        alt=""
                      />
                    </span>

                    <i
                      class="fa-solid fa-arrow-up-from-line"
                      style="color: #edba54"
                    ></i>
                    <span
                      :class="{
                        ['is-this-account']:
                          item.targetAccountNumber ===
                          accountDetails.tradeAccount.accountNumber,
                      }"
                      >No. {{ item.targetAccountNumber }}</span
                    >
                  </label>

                  <i
                    class="fa-solid fa-arrow-up-from-line"
                    style="color: #edba54"
                  ></i>
                </div>
              </td>
              <td class="text-start">
                <span
                  class="badge badge-info fw-normal"
                  :class="{
                    'badge-pending':
                      item.stateId === TransactionStateType.TransferCreated ||
                      item.stateId ===
                        TransactionStateType.TransferAwaitingApproval,
                    'badge-completed':
                      item.stateId === TransactionStateType.TransferCompleted,

                    'badge-refused':
                      item.stateId === TransactionStateType.TransferRejected,
                  }"
                  >{{ $t(`type.transactionState.${item.stateId}`) }}</span
                >
              </td>
              <td class="text-start">
                <a
                  href="#"
                  class="text-dark fw-normal text-hover-primary mb-1 fs-6"
                >
                  {{
                    item.targetAccountNumber ===
                    accountDetails.tradeAccount.accountNumber
                      ? "+"
                      : "-"
                  }}
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </a>
              </td>
              <td class="text-start">
                <TimeShow :date-iso-string="item.statedOn" format="HH:mm A" />
              </td>
            </tr>
          </tbody>
        </template>
      </table>
    </template>

    <TableFooter
      @page-change="filterRef?.fetchData"
      :criteria="filterRef?.criteria"
    />
  </div>

  <TransferToAccountForm
    ref="transferToAccountFormRef"
    @on-created="filterRef?.fetchData"
  />
</template>

<script lang="ts" setup>
import moment from "moment";
import { computed, inject, nextTick, onMounted, ref, watch } from "vue";
import TransferToAccountForm from "../modal/TransferToAccountForm.vue";
import AccountService from "../../services/AccountService";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import {
  TransactionAccountType,
  TransactionStateType,
} from "@/core/types/StateInfos";
import { isMobile } from "@/core/config/WindowConfig";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { useRoute } from "vue-router";
import { getLanguage } from "@/core/types/LanguageTypes";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { enableTransfer } from "@/core/helpers/permissionHelpers";
const isLoading = ref(false);
const transferToAccountFormRef =
  ref<InstanceType<typeof TransferToAccountForm>>();

const props = defineProps<{
  accountsList: Array<any>;
}>();

const defaultCriteria = ref({
  page: 1,
  size: 10,
  sourceAccountType: TransactionAccountType.TradeAccount,
  targetAccountType: TransactionAccountType.TradeAccount,
});

const route = useRoute();

// const language = store.state.AuthModule.user.language;

const filterRef = ref<InstanceType<typeof TradeFilter>>();
// const filterOptions = ["transactionStatus", "period"];
const filterOptions = ["period", "transferState", "size"];

const getAccountDetails = inject(AccountInjectionKeys.GET_ACCOUNT_DETAILS);
const accountDetails = ref<any>(null);
const permissions = computed(() => accountDetails?.value?.permission ?? null);
watch(
  () => route.params.accountNumber,
  () => {
    accountDetails.value = getAccountDetails?.();
    filterRef.value?.fetchData(1);
  }
);

const getTransactionHandler = async (_criteria: any) => {
  await nextTick();
  return await AccountService.getTransactionsByTradeAccountUid(
    accountDetails.value.uid,
    _criteria
  );
};

const groupedItems = computed<any>(() => {
  const groupedObject: Array<any> = filterRef.value?.data.reduce((acc, cur) => {
    const date = moment(cur.statedOn).locale(getLanguage.value);
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

const openTransferToAccountPanel = () => {
  const filteredAccountList = props.accountsList.filter(
    (item) =>
      item.role === AccountRoleTypes.Client &&
      //item.currencyId === accountDetails.value.tradeAccount.currencyId &&
      item.fundType == accountDetails.value.fundType &&
      item.tradeAccount.accountNumber !==
        accountDetails.value.tradeAccount.accountNumber
  );
  transferToAccountFormRef.value?.show(
    filteredAccountList,
    accountDetails.value.tradeAccount.accountNumber,
    accountDetails.value.uid,
    accountDetails.value.tradeAccount.currencyId,
    accountDetails.value.fundType,
    accountDetails.value.tradeAccount.balance,
    accountDetails.value
  );
};

const reset = async () => {
  filterRef.value?.initCriteria();
  await filterRef.value?.fetchData(1);
};

onMounted(async () => {
  isLoading.value = true;
  accountDetails.value = getAccountDetails?.();
  await filterRef.value?.fetchData(1);
  isLoading.value = false;
});
</script>

<style scoped lang="scss">
.date-title {
  font-size: 16px;
  font-weight: 400;
  color: #212121;
}
.date-title-mobile {
  font-size: 14px;
  font-style: normal;
  font-weight: 600;
  color: #212121;
}
.svg-container {
  transition: transform 0.3s ease-in-out;
}

.arrow.rotate-up .svg-container {
  transform: rotate(-180deg);
}

.collapse-content {
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease-in-out;
}

.is-this-account {
  box-sizing: border-box;
  border-radius: 10px;
  padding: 0 5px;
  height: 100%;
  border: 2px #ffce32 dashed;
}
</style>
