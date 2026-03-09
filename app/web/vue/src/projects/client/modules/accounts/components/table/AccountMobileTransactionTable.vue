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
      <!-- mobile section table -->
      <div class="">
        <div v-for="(group, index) in groupedItems" :key="index">
          <div
            class="px-4 py-2"
            style="
              background: #eef5fc;
              color: #212121;
              font-size: 14px;
              font-style: normal;
              font-weight: 600;
            "
          >
            <div
              class="text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0"
            >
              <span
                class="text-start col-12"
                style="font-size: 16px; font-weight: 400; color: #212121"
              >
                {{ group.date }}
              </span>
            </div>
          </div>

          <div class="fw-semi-bold" style="color: #4d4d4d">
            <div
              class="px-1"
              v-for="(item, index) in group.objects"
              :key="index"
            >
              <div class="d-flex border-bottom py-3">
                <div class="col-6">
                  <div class="d-flex align-items-start">
                    <label class="d-flex align-items-center">
                      <span
                        :class="{
                          ['is-this-account']:
                            item.sourceAccountNumber ===
                            accountDetails.tradeAccount.accountNumber,
                        }"
                        >{{ item.sourceAccountNumber }}</span
                      >
                      <span class="symbol symbol-30px mx-1">
                        <i class="bi bi-arrow-right"></i>
                      </span>

                      <span
                        :class="{
                          ['is-this-account']:
                            item.targetAccountNumber ===
                            accountDetails.tradeAccount.accountNumber,
                        }"
                        >{{ item.targetAccountNumber }}</span
                      >
                    </label>
                  </div>

                  <div class="d-flex justify-content-start gap-1 fs-7 mt-2">
                    <div class="">
                      <span
                        class="badge badge-info fw-normal"
                        :class="{
                          'badge-pending':
                            item.stateId ===
                              TransactionStateType.TransferCreated ||
                            item.stateId ===
                              TransactionStateType.TransferAwaitingApproval,
                          'badge-completed':
                            item.stateId ===
                            TransactionStateType.TransferCompleted,

                          'badge-refused':
                            item.stateId ===
                            TransactionStateType.TransferRejected,
                        }"
                        >{{ $t(`type.transactionState.${item.stateId}`) }}</span
                      >
                    </div>
                  </div>
                </div>

                <div
                  class="col-3 d-flex justify-content-center align-items-start"
                >
                  <BalanceShow
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </div>
                <div
                  class="col-3 d-flex justify-content-center align-items-start"
                >
                  <TimeShow :date-iso-string="item.statedOn" format="HH:mm A" />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
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
      item.currencyId === accountDetails.value.tradeAccount.currencyId &&
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

const fetchData = async () => {
  isLoading.value = true;
  await filterRef.value?.fetchData(1);
  isLoading.value = false;
};

onMounted(async () => {
  isLoading.value = true;
  accountDetails.value = getAccountDetails?.();
  await filterRef.value?.fetchData(1);
  isLoading.value = false;
});

defineExpose({
  fetchData,
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
