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
        <button class="btn btn-secondary" @click.prevent="openDepositPanel">
          {{ $t("action.deposit") }}
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
      class="table align-middle table-row-bordered gy-5"
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
        v-if="filterRef?.data && filterRef?.data.length !== 0"
        class="table align-middle table-row-bordered gy-5"
        id="kt_ecommerce_sales_table"
      >
        <template v-for="(group, index) in groupedItems" :key="index">
          <thead>
            <tr class="text-start text-uppercase gs-0">
              <th class="text-start col-5 date-title">
                {{ group.date }}
              </th>

              <th class="text-start col-2">{{ $t("fields.status") }}</th>
              <th class="text-start col-2">{{ $t("fields.payment") }}</th>
              <th class="text-start col-2">{{ $t("fields.amount") }}</th>
              <th class="text-start col-3">{{ $t("fields.createdOn") }}</th>
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
                    <span class="symbol symbol-30px mx-2">
                      <img
                        src="/images/icons/finance/wallet-deposit.svg"
                        alt=""
                      />
                    </span>

                    <i
                      class="fa-solid fa-arrow-up-from-line"
                      style="color: #edba54"
                    ></i>
                    <span>{{ item.paymentMethodName }}</span>
                    <button
                      v-if="
                        item.stateId === TransactionStateType.DepositCreated
                      "
                      @click="showInstruction(item)"
                      class="btn btn-secondary btn-sm ms-5 d-flex align-items-center"
                    >
                      <i class="fa-regular fa-file"></i>
                      <span>{{ $t("action.uploadReceipt") }}</span>
                    </button>
                  </label>

                  <i
                    class="fa-solid fa-arrow-up-from-line"
                    style="color: #edba54"
                  ></i>
                </div>
              </td>
              <td class="text-start">
                <span
                  class="badge badge-pending fw-normal"
                  :class="{
                    'badge-pending':
                      item.stateId === TransactionStateType.DepositCreated ||
                      item.stateId ===
                        TransactionStateType.DepositPaymentCompleted,
                    'badge-completed':
                      item.stateId === TransactionStateType.DepositCompleted,

                    'badge-refused':
                      item.stateId ===
                      TransactionStateType.DepositTenantRejected,
                  }"
                  >{{ $t(`type.transactionState.${item.stateId}`) }}</span
                >
              </td>
              <td class="text-start">
                {{ $t(`type.paymentStatus.${item.paymentStatus}`) }}
              </td>
              <td class="text-start">
                <a
                  href="#"
                  class="text-dark fw-normal text-hover-primary mb-1 fs-6"
                >
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </a>
              </td>
              <td class="text-start">
                <TimeShow :date-iso-string="item.createdOn" format="HH:mm A" />
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
  <UploadReceipt ref="uploadReceiptRef" />
  <CreateDepositModal
    ref="depositFormRef"
    @on-created="filterRef?.fetchData(1)"
  />
</template>

<script lang="ts" setup>
import moment from "moment";
import { computed, inject, nextTick, onMounted, ref, watch } from "vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import {
  TransactionAccountType,
  TransactionStateType,
} from "@/core/types/StateInfos";
import CreateDepositModal from "@/projects/client/components/funding/CreateDepositModal.vue";
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import { isMobile } from "@/core/config/WindowConfig";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { useRoute } from "vue-router";
import { getLanguage } from "@/core/types/LanguageTypes";
import UploadReceipt from "@/projects/client/components/funding/depositModal/UploadReceipt.vue";

const route = useRoute();
const isLoading = ref(false);
const accountDetails = ref<any>(null);
const filterOptions = ["period", "depositState", "size"];
const filterRef = ref<InstanceType<typeof TradeFilter>>();
const uploadReceiptRef = ref<InstanceType<typeof UploadReceipt>>();
const depositFormRef = ref<InstanceType<typeof CreateDepositModal>>();
const getAccountDetails = inject(AccountInjectionKeys.GET_ACCOUNT_DETAILS);

const defaultCriteria = ref({
  page: 1,
  size: 10,
  sourceAccountType: TransactionAccountType.TradeAccount,
  targetAccountType: TransactionAccountType.TradeAccount,
});

const getTransactionHandler = async (_criteria: any) => {
  await nextTick();
  return await WalletService.queryDepositV2(
    accountDetails.value.uid,
    _criteria
  );
};

const groupedItems = computed<any>(() => {
  const groupedObject: Array<any> = filterRef.value?.data.reduce((acc, cur) => {
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

const openDepositPanel = () => {
  depositFormRef.value?.show(accountDetails.value);
};

const showInstruction = (data: any) => {
  uploadReceiptRef.value?.show(data, accountDetails.value.uid);
};

const reset = async () => {
  filterRef.value?.initCriteria();
  await filterRef.value?.fetchData(1);
};

watch(
  () => route.params.accountNumber,
  () => {
    accountDetails.value = getAccountDetails?.();
    filterRef.value?.fetchData(1);
  }
);

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
