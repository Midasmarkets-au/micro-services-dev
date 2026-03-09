<template>
  <div class="">
    <TableFilter
      ref="filterRef"
      :default-criteria="defaultCriteria"
      :service-handler="getTransactionHandler"
      :filter-options="filterOptions"
      :trigger="'button'"
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
      id="kt_ecommerce_sales_table"
    >
      <tbody v-if="filterRef?.isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!filterRef?.isLoading && filterRef?.data.length === 0">
        <NoDataBox />
      </tbody>
    </table>
    <div v-else>
      <table
        v-if="!isMobile"
        class="table align-middle table-row-bordered gy-5"
      >
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

          <tbody class="fw-semibold overflow-auto" style="white-space: nowrap">
            <tr
              class="text-start"
              v-for="(item, index) in group.objects"
              :key="index"
            >
              <td class="text-start">
                <div class="d-flex align-items-center">
                  <label
                    class="d-flex align-items-center"
                    v-if="
                      item.sourceAccountType ===
                        TransactionAccountType.TradeAccount &&
                      item.targetAccountType ===
                        TransactionAccountType.TradeAccount
                    "
                  >
                    <span class="symbol symbol-25px me-2">
                      <img src="/images/icons/finance/AccountTransfer.png" />
                    </span>

                    <span> {{ $t("tip.transferBetweenTradeAccounts") }} </span>
                  </label>

                  <div class="d-flex align-items-center gap-5" v-else>
                    <div
                      :class="{
                        ['is-this-account']:
                          item.sourceAccount.accountNumber ===
                          props.accountDetails?.tradeAccount?.accountNumber,
                      }"
                      class="d-flex gap-5 align-items-center"
                    >
                      <div class="d-flex flex-column">
                        <span class="fs-4">
                          No.
                          {{ item.sourceAccount.accountNumber }}
                          <span class="fs-9">
                            {{
                              $t(
                                `type.currency.${item.sourceAccount.currencyId}`
                              )
                            }}
                          </span>
                        </span>

                        <span class="fs-7">
                          {{ $t("fields.group") }}:
                          {{ item.sourceAccount.group || "***" }}
                        </span>
                      </div>
                    </div>
                    <span class="symbol symbol-25px mx-2">
                      <img
                        src="/images/icons/finance/wallet-withdraw.svg"
                        alt="icon"
                        style="width: 34px; height: 34px"
                      />
                    </span>

                    <div
                      :class="{
                        ['is-this-account']:
                          item.targetAccount.accountNumber ===
                          props.accountDetails?.tradeAccount?.accountNumber,
                      }"
                      class="d-flex gap-5 align-items-center"
                    >
                      <div class="d-flex flex-column">
                        <span class="fs-4">
                          No.
                          {{ item.targetAccount.accountNumber }}
                          <span class="fs-9">
                            {{
                              $t(
                                `type.currency.${item.targetAccount.currencyId}`
                              )
                            }}
                          </span>
                        </span>

                        <span class="fs-7">
                          {{ $t("fields.group") }}:
                          {{ item.targetAccount.group || "***" }}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
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
                {{ $t(`type.currency.${item.currencyId}`) }}
              </td>
              <td class="text-start">
                <BalanceShow
                  :balance="item.amount"
                  :currency-id="item.currencyId"
                />
              </td>
              <td class="text-start">
                <TimeShow :date-iso-string="item.createdOn" format="h:mm a" />
              </td>
            </tr>
          </tbody>
        </template>
      </table>
      <!-- mobile section -->
      <table
        v-if="isMobile"
        class="table align-middle table-row-bordered gy-5"
        id="kt_ecommerce_sales_table"
      >
        <template v-for="(group, index) in groupedItems" :key="index">
          <thead>
            <tr class="px-4 py-2" style="background: #eef5fc">
              <div class="text-start text-uppercase gs-0">
                <span
                  class="text-start col-12 text-gray"
                  style="font-size: 16px; font-weight: 400"
                >
                  {{ group.date }}
                </span>
              </div>
            </tr>
          </thead>
          <tbody>
            <tr
              class="text-start"
              v-for="(item, index) in group.objects"
              :key="index"
            >
              <td class="text-start">
                <div class="d-flex border-bottom py-3">
                  <div
                    class="col-6"
                    v-if="
                      item.sourceAccountType ===
                        TransactionAccountType.Wallet &&
                      item.targetAccountType ===
                        TransactionAccountType.TradeAccount
                    "
                  >
                    <div class="d-flex align-items-center gap-1">
                      <img
                        src="/images/icons/finance/AccountTransfer.png"
                        alt=""
                        style="width: 16px; height: 16px"
                      />
                      <label class="fs-7">
                        {{ $t("tip.transferInFromWallet") }}
                      </label>
                    </div>

                    <div class="d-flex justify-content-end gap-1 fs-7">
                      <div class="">
                        <span
                          class="badge badge-info mt-1 fw-normal"
                          :class="{
                            'badge-pending':
                              item.status ===
                                TransactionStateType.TransferCreated ||
                              item.status ===
                                TransactionStateType.TransferAwaitingApproval,
                            'badge-completed':
                              item.status ===
                              TransactionStateType.TransferApproved,

                            'badge-refused':
                              item.status ===
                              TransactionStateType.TransferRejected,
                          }"
                          >{{ $t(`type.transactionState.${item.status}`) }}
                        </span>
                      </div>

                      <div class="">
                        <span
                          v-if="
                            item.stateId === TransactionStateType.DepositCreated
                          "
                          class="cursor-pointer badge badge-secondary fs-8 fw-normal"
                          @click="showInstruction(item.id)"
                        >
                          <span>{{ $t("action.detail") }}</span>
                        </span>

                        <span
                          v-if="
                            item.stateId ===
                            TransactionStateType.WithdrawalCreated
                          "
                          class="cursor-pointer badge badge-secondary fs-8 fw-normal"
                          @click="openConfirmBoxPanel(item.id)"
                        >
                          <span>{{ $t("action.cancel") }}</span>
                        </span>
                      </div>
                    </div>
                  </div>

                  <div
                    class="col-6"
                    v-else-if="
                      item.sourceAccountType ===
                        TransactionAccountType.TradeAccount &&
                      item.targetAccountType === TransactionAccountType.Wallet
                    "
                  >
                    <div class="d-flex align-items-center gap-1">
                      <img
                        src="/images/icons/finance/AccountTransfer.png"
                        alt=""
                        style="width: 16px; height: 16px"
                      />
                      <label class="fs-7">
                        {{ $t("tip.transferOutToWallet") }}
                      </label>
                    </div>

                    <div class="d-flex justify-content-end gap-1 fs-7">
                      <div class="">
                        <span
                          class="badge badge-info mt-1 fw-normal"
                          :class="{
                            'badge-pending':
                              item.status ===
                                TransactionStateType.TransferCreated ||
                              item.status ===
                                TransactionStateType.TransferAwaitingApproval,
                            'badge-completed':
                              item.status ===
                              TransactionStateType.TransferApproved,

                            'badge-refused':
                              item.status ===
                              TransactionStateType.TransferRejected,
                          }"
                          >{{ $t(`type.transactionState.${item.status}`) }}
                        </span>
                      </div>
                    </div>
                  </div>

                  <div
                    class="col-6"
                    v-else-if="
                      item.sourceAccountType ===
                        TransactionAccountType.TradeAccount &&
                      item.targetAccountType ===
                        TransactionAccountType.TradeAccount
                    "
                  >
                    <div class="d-flex align-items-center gap-1">
                      <img
                        src="/images/icons/finance/AccountTransfer.png"
                        alt=""
                        style="width: 16px; height: 16px"
                      />
                      <label class="fs-7">
                        {{ $t("tip.transferBetweenTradeAccounts") }}
                      </label>
                    </div>

                    <div class="d-flex justify-content-end gap-1 fs-7">
                      <div class="">
                        <span
                          class="badge badge-info mt-1 fw-normal"
                          :class="{
                            'badge-pending':
                              item.status ===
                                TransactionStateType.TransferCreated ||
                              item.status ===
                                TransactionStateType.TransferAwaitingApproval,
                            'badge-completed':
                              item.status ===
                              TransactionStateType.TransferApproved,

                            'badge-refused':
                              item.status ===
                              TransactionStateType.TransferRejected,
                          }"
                          >{{ $t(`type.transactionState.${item.stateId}`) }}
                        </span>
                      </div>
                    </div>
                  </div>

                  <div
                    class="col-3 d-flex justify-content-center align-items-center"
                  >
                    <div class="text-gray-800 text-hover-primary mb-1 fs-7">
                      <BalanceShow
                        :balance="item.amount"
                        :currency-id="item.currencyId"
                      />
                    </div>
                  </div>
                  <div
                    class="col-3 fs-7 d-flex justify-content-center align-items-center"
                  >
                    <TimeShow
                      :date-iso-string="item.createdOn"
                      format="h:mm a"
                    />
                  </div>
                </div>
              </td>
            </tr>
          </tbody>
        </template>
      </table>
    </div>
    <TableFooter
      @page-change="filterRef?.fetchData"
      :criteria="filterRef?.criteria"
    />
  </div>
</template>
<script setup lang="ts">
import { ref, watch, computed, onMounted } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import SalesService from "../services/SalesService";
import { useStore } from "@/store";
import { useRoute } from "vue-router";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import TableFilter from "@/projects/client/components/TradeFilter.vue";
import moment from "moment";
import TimeShow from "@/components/TimeShow.vue";
import {
  TransactionAccountType,
  TransactionStateType,
} from "@/core/types/StateInfos";
import BalanceShow from "@/components/BalanceShow.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { getLanguage } from "@/core/types/LanguageTypes";

const props = defineProps<{
  accountDetails: any;
}>();
const filterRef = ref<InstanceType<typeof TableFilter>>();
const filterOptions = ["transferState", "period"];

const defaultCriteria = ref({
  page: 1,
  size: 10,
  sourceAccountType: TransactionAccountType.TradeAccount,
  targetAccountType: TransactionAccountType.TradeAccount,
});

const store = useStore();
const route = useRoute();

// const language = store.state.AuthModule.user.language;

const accountUid = ref(parseInt((route.params.accountId as string) || "-1"));

watch(
  () => route.params.accountId,
  (newVal) => {
    accountUid.value = parseInt((newVal as string) || "-1");
    if (accountUid.value !== -1) filterRef.value?.fetchData(1);
  }
);

const getTransactionHandler = (criteria: any) =>
  SalesService.queryClientTransaction(accountUid.value, criteria);

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
  const finalArray = Object.keys(groupedObject).map((dateKey) => ({
    date: dateKey,
    objects: groupedObject[dateKey],
  }));
  return finalArray;
});

const reset = async () => {
  filterRef.value?.initCriteria();
  await filterRef.value?.fetchData(1);
};
onMounted(() => {
  // console.log(props.accountDetails);
});

// const ccc = () => {
//   console.log(groupedItems.value);
// };
</script>

<style scoped lang="scss">
.is-this-account {
  box-sizing: border-box;
  border-radius: 10px;
  padding: 0 5px;
  width: 100%;
  height: 100%;
  border: 2px #ffce32 dashed;
}
</style>
