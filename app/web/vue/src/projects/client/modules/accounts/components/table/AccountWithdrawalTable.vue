<template>
  <div class="">
    <div class="d-flex align-items-center justify-content-between">
      <TradeFilter
        ref="filterRef"
        :default-criteria="defaultCriteria"
        :service-handler="getWithdrawHandler"
        :filter-options="filterOptions"
        :trigger="'button'"
      />

      <div v-if="!isMobile" class="d-flex gap-2">
        <button class="btn btn-secondary" @click.prevent="openWithdrawalModal">
          {{ $t("action.withdraw") }}
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
        v-if="filterRef?.data && filterRef?.data.length !== 0 && !isMobile"
        class="table align-middle table-row-bordered gy-5"
        id="kt_ecommerce_sales_table"
      >
        <template v-for="(group, index) in groupedItems" :key="index">
          <thead>
            <tr class="text-start fs-7 text-uppercase gs-0">
              <th class="text-start col-5 date-title">
                {{ group.date }}
              </th>

              <th class="text-start col-2">{{ $t("fields.status") }}</th>
              <th class="text-start col-2">{{ $t("fields.payment") }}</th>
              <th class="text-start col-2">{{ $t("fields.amount") }}</th>
              <th class="text-start col-3">{{ $t("fields.createdOn") }}</th>
              <th class="text-start col-3">{{ $t("fields.updatedOn") }}</th>
              <!--              <th class="text-start col-2"></th>-->
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
                        src="/images/icons/finance/wallet-deposit.png"
                        alt=""
                      />
                    </span>

                    <i
                      class="fa-solid fa-arrow-up-from-line"
                      style="color: #edba54"
                    ></i>
                    <span>{{ item.paymentMethodName }}</span>
                  </label>
                  <el-button
                    size="small"
                    plain
                    type="danger"
                    class="ms-2"
                    v-if="
                      item.stateId === TransactionStateType.WithdrawalCreated
                    "
                    @click="cancelWithdrawal(item)"
                    >{{ $t("action.cancel") }}</el-button
                  >
                </div>
              </td>
              <td class="text-start">
                <span
                  class="badge badge-pending fw-normal"
                  :class="{
                    'badge-pending':
                      item.stateId === TransactionStateType.WithdrawalCreated ||
                      item.stateId ===
                        TransactionStateType.WithdrawalPaymentCompleted,
                    'badge-completed':
                      item.stateId === TransactionStateType.WithdrawalCompleted,

                    'badge-refused':
                      item.stateId ===
                      TransactionStateType.WithdrawalTenantRejected,
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
              <td class="text-start">
                <TimeShow :date-iso-string="item.updatedOn" format="HH:mm A" />
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

  <CreateWithdrawalModal
    ref="createWithdrawalModalRef"
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
import WalletService from "@/projects/client/modules/wallet/services/WalletService";
import { isMobile } from "@/core/config/WindowConfig";
import CreateWithdrawalModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { useRoute } from "vue-router";
import { getLanguage } from "@/core/types/LanguageTypes";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;
const openConfirmBoxModel = inject(
  ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);

const isLoading = ref(false);
const createWithdrawalModalRef =
  ref<InstanceType<typeof CreateWithdrawalModal>>();

const defaultCriteria = ref({
  page: 1,
  size: 10,
  sourceAccountType: TransactionAccountType.TradeAccount,
  targetAccountType: TransactionAccountType.TradeAccount,
});

const route = useRoute();

// const language = store.state.AuthModule.user.language;

const filterRef = ref<InstanceType<typeof TradeFilter>>();
const filterOptions = ["period", "withdrawalState", "size"];

const getAccountDetails = inject(AccountInjectionKeys.GET_ACCOUNT_DETAILS);
const accountDetails = ref<any>(null);

watch(
  () => route.params.accountNumber,
  () => {
    accountDetails.value = getAccountDetails?.();
    filterRef.value?.fetchData(1);
  }
);

const getWithdrawHandler = async (_criteria: any) => {
  await nextTick();
  return await WalletService.queryAccountWithdrawalV2(
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

const openWithdrawalModal = () => {
  const isAccount = true;
  createWithdrawalModalRef.value?.show(isAccount, accountDetails.value);
};

const cancelWithdrawal = async (item: any) => {
  await openConfirmBoxModel?.(
    () => handleCancel(item),
    undefined, // explicitly pass undefined
    {
      confirmText: t("tip.areYouSureToCancel"),
    }
  );
};

const handleCancel = async (item: any) => {
  isLoading.value = true;
  try {
    await WalletService.cancelWithdrawal(item.hashId);
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
  } finally {
    await filterRef.value?.fetchData(1);
    isLoading.value = false;
  }
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
