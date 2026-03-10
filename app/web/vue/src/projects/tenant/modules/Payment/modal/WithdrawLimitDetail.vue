<template>
  <SimpleForm
    ref="formRef"
    :is-loading="isLoading"
    :title="$t('title.details')"
    :minWidth="1000"
    :discard-title="$t('action.close')"
    :disableFooter="true"
  >
    <LoadingCentralBox class="h-100" v-if="isLoading" />
    <div v-else class="h-100">
      <table
        class="table align-middle table-row-bordered gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted text-uppercase gs-0">
            <th>{{ $t("fields.client") }}</th>
            <!-- <th>{{ $t("fields.info") }}</th> -->
            <th>{{ $t("fields.transactionType") }}</th>
            <th>{{ $t("title.paymentMethod") }}</th>
            <!-- <th v-if="tab == TabStatus.All">{{ $t("fields.status") }}</th> -->
            <th>{{ $t("fields.paymentID") }}</th>
            <th>{{ $t("fields.paymentMethod") }}</th>
            <th>{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.amount") }}</th>
            <th>{{ $t("fields.exchangeRate") }}</th>
            <!-- <th>{{ $t("fields.sourceBalance") }}</th> -->
            <th>{{ $t("fields.time") }}</th>
            <!-- <th class="text-center cell-color">
              {{ $t("fields.paymentInfo") }}
            </th> -->
            <th
              v-if="tab != TransactionStateType.WithdrawalCreated"
              class="cell-color text-center"
            >
              {{ $t("fields.operatedBy") }}
            </th>
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
            <td>
              <UserInfo v-if="item.user" :user="item.user" class="me-2" />
            </td>
            <!-- <td>
              {{ item?.source?.agentGroupName }}<br />{{
                item?.source?.agentGroupName
              }}
            </td> -->
            <td>
              {{
                item.type === "Deposit"
                  ? $t("type.title.deposit")
                  : item.type === "Withdrawal"
                  ? $t("type.title.withdrawal")
                  : ""
              }}
            </td>
            <td>
              <!-- {{ $t(`type.fundType.${item.fundType}`) }} -->
              {{ item.fundTypeName }}
            </td>
            <td v-if="tab == TabStatus.All">
              {{ $t(`type.transactionState.${item.stateId}`) }}
            </td>
            <td>
              {{ item?.paymentId }}
            </td>
            <td>
              {{ item?.paymentName }}
            </td>
            <td>
              {{ $t(`type.currency.${item.currencyId}`) }}
            </td>
            <td>
              <BalanceShow
                :currency-id="item.currencyId"
                :balance="item.amount"
              />
            </td>

            <td>
              {{ item.exchangeRate }}
            </td>
            <!-- <td>
              <div>
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="
                    item?.source?.accountType == 1
                      ? item?.source?.balanceInCents
                      : item?.source?.equityInCents
                  "
                />
              </div>
              <div
                v-if="
                  item?.source?.accountType === TransactionAccountType.Wallet
                "
                class="badge badge-primary"
              >
                Wallet # {{ item?.source?.displayNumber }}
              </div>
              <div
                v-if="
                  item?.source?.accountType ===
                  TransactionAccountType.TradeAccount
                "
                class="badge badge-warning"
                @click="
                  viewComments(
                    CommentType.Account,
                    item?.source?.id,
                    item?.source?.displayNumber
                  )
                "
              >
                Account # {{ item?.source?.displayNumber }}
                <i
                  v-if="item?.source?.hasComment"
                  class="fa-regular fa-comment-dots text-info ms-3 cursor-pointer"
                ></i>
              </div>
            </td> -->

            <td>
              <div>
                <TimeShow :date-iso-string="item?.createdOn" />
              </div>
              <!-- <div v-if="item?.createdOn != item?.updatedOn">
                <TimeShow :date-iso-string="item?.updatedOn" />
              </div> -->
            </td>
            <!-- <td class="text-center cell-color">
              <a
                href="#"
                class="btn btn-sm btn-light-info fw-bold ms-2 fs-8 py-1 px-3"
                @click="showWithdrawInfo(item)"
              >
                {{ $t("action.showPaymentInfo") }}
              </a>
            </td> -->
            <td v-if="tab != TabStatus.Pending" class="cell-color text-center">
              <div
                v-if="item.stateId != TransactionStateType.WithdrawalCreated"
              >
                {{ item.operatorName }}
              </div>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { useI18n } from "vue-i18n";
import BalanceShow from "@/components/BalanceShow.vue";
import { useStore } from "@/store";
import TimeShow from "@/components/TimeShow.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import svc from "../services/PaymentService";

import {
  TransactionStateType,
  WithdrawalStatusType,
  WithdrawalStateTypes,
  TransactionAccountType,
} from "@/core/types/StateInfos";
const TabStatus = WithdrawalStatusType;
const tab = ref<any>(TabStatus.Pending);
const wireForm = ref<any>({});
const isLoading = ref(true);
const formRef = ref<InstanceType<typeof SimpleForm> | null>(null);
const store = useStore();
const user = store.state.AuthModule.user;
const t = useI18n().t;
const data = ref(Array<any>());
const show = async (_item: any) => {
  isLoading.value = true;
  formRef.value?.show();
  wireForm.value = _item;
  // 1. get exchange rate

  isLoading.value = false;
  fetchData(1);
};

const copyTable = ref<HTMLDivElement | null>(null);
const criteria = ref<any>({
  page: 1,
  size: 10,
  stateId: TransactionStateType.WithdrawalCreated,
  sortField: "createdOn",
});
const hide = () => {
  formRef.value?.hide();
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  const res = await svc.getDepositwithdrawDetail(
    wireForm.value.partyId,
    wireForm.value.paymentGroupId,
    criteria.value
  );
  criteria.value = res.criteria;
  data.value = res.data;
  isLoading.value = false;
};
const pageChange = (page: number) => {
  fetchData(page);
};
defineExpose({
  hide,
  show,
});
</script>
<style scoped></style>
