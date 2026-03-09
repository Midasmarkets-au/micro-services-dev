<template>
  <div id="monthlyMobileTable" class="">
    <div class="card-header border-0 min-h-100 my-4 px-5">
      <span class="fw-400 fs-2">{{ $t("title.transHistory") }}</span>
    </div>

    <div class="card card-body py-3 d-flex justify-content-between mx-7">
      <div class="w-100">
        <div>
          <div>
            <el-tabs type="card" v-model="currentTab" @tab-change="changeTab()">
              <el-tab-pane
                v-for="item in walletOptions"
                :key="item.value"
                :label="item.label"
                :name="item.value"
                :disabled="isLoading"
              />
            </el-tabs>
          </div>
          <div class="d-flex gap-2 align-items-center mt-1 mb-3">
            <el-date-picker
              class="w-150px"
              v-model="startDate"
              type="date"
              value-format="YYYY-MM-DD"
              :placeholder="$t('fields.startDate')"
              :disabled="isLoading"
            />
            <el-date-picker
              class="w-150px"
              v-model="endDate"
              type="date"
              value-format="YYYY-MM-DD"
              :placeholder="$t('fields.endDate')"
              :disabled="isLoading"
            />
          </div>
          <div class="mb-3">
            <el-button @click="fetchData(1)" :loading="isLoading">{{
              $t("action.search")
            }}</el-button>
            <el-button @click="resetFilter()" :disabled="isLoading">{{
              $t("action.reset")
            }}</el-button>
          </div>
          <div class="fw-400 fs-2 mb-3">
            {{ $t("action.showing") }}
            {{ data.length }}
            {{ $t("title.results") }}
          </div>
        </div>

        <table
          v-if="isLoading || data.length === 0"
          class="table align-middle table-row-bordered gy-5"
        >
          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && data.length === 0">
            <NoDataBox />
          </tbody>
        </table>

        <div v-else class="">
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
                class="px-5"
                v-for="(item, index) in group.objects"
                :key="index"
              >
                <div class="d-flex border-bottom py-3">
                  <div class="col-6">
                    <div class="d-flex align-items-center gap-1">
                      <img
                        style="width: 25px; height: 25px"
                        :src="`/images/icons/finance/wallet-${
                          {
                            [MatterTypes.Deposit]: 'deposit',
                            [MatterTypes.Withdrawal]: 'withdraw',
                            [MatterTypes.Rebate]: 'rebate',
                            [MatterTypes.InternalTransfer]: 'transfer',
                            [MatterTypes.Refund]: 'refund',
                            [MatterTypes.WalletAdjust]: 'refund',
                            [MatterTypes.TransferRewards]: 'refund',
                          }[currentTab] ?? 'transfer'
                        }.png`"
                        alt=""
                      />
                      <label class="fs-7">
                        {{
                          {
                            [MatterTypes.InternalTransfer]: {
                              false: $t("action.transferIn"), // +
                              //`  (${item.sourceName})`,
                              true:
                                $t("action.transferOut") +
                                `  (${item.targetAccountNumber})`,
                            }[item.source === 0],

                            [MatterTypes.Deposit]: $t(
                              "tip.depositInToTradeAccount"
                            ),

                            [MatterTypes.Withdrawal]: $t("action.withdraw"),

                            [MatterTypes.Rebate]:
                              $t("fields.rebate") + `  (${item.sourceName})`,

                            [MatterTypes.Refund]: $t("fields.refundDeduct"),
                            [MatterTypes.WalletAdjust]: $t(
                              "fields.walletAdjustSalesRebate"
                            ),
                            [MatterTypes.TransferRewards]: $t(
                              "fields.TransferRewards"
                            ),
                          }[currentTab]
                        }}
                      </label>
                    </div>

                    <div class="d-flex justify-content-between gap-1 fs-7 mt-1">
                      <div class="">
                        <span
                          v-if="
                            [
                              TransactionStateType.TransferCompleted,
                              TransactionStateType.DepositCompleted,
                              TransactionStateType.WithdrawalCompleted,
                              TransactionStateType.WalletAdjustCompleted,
                            ].includes(item.stateId)
                          "
                          class="statusBadgeCompleted"
                          >{{
                            $t("type.transactionState." + item.stateId)
                          }}</span
                        >
                        <span
                          v-else-if="
                            [
                              TransactionStateType.TransferRejected,
                              TransactionStateType.DepositTenantRejected,
                              TransactionStateType.DepositCentralRejected,
                              TransactionStateType.WithdrawalTenantRejected,
                              TransactionStateType.DepositFailed,
                              TransactionStateType.WithdrawalFailed,
                            ].includes(item.stateId)
                          "
                          class="statusBadgeReject"
                          >{{
                            $t("type.transactionState." + item.stateId)
                          }}</span
                        >
                        <span v-else class="alpha-tag">{{
                          $t("type.transactionState." + item.stateId)
                        }}</span>
                      </div>

                      <div class="">
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
                    class="col-3 d-flex justify-content-center align-items-center"
                  >
                    <a
                      href="#"
                      class="text-gray-800 text-hover-primary mb-1 fs-7"
                    >
                      <span
                        >{{
                          {
                            [MatterTypes.Deposit]: "+",
                            [MatterTypes.Withdrawal]: "-",
                            [MatterTypes.Rebate]: "+",
                          }[currentTab]
                        }}
                      </span>
                      <BalanceShow
                        :currency-id="item.currencyId"
                        :balance="item.amount"
                      />
                    </a>
                  </div>
                  <div
                    class="col-3 fs-7 d-flex justify-content-center align-items-center"
                  >
                    <TimeShow
                      :date-iso-string="item.postedOn"
                      format="h:mm a"
                    />
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
  </div>

  <ConfirmBox
    :title="$t('title.cancelWithdrawal')"
    ref="confirmBoxRef"
    :is-loading="isLoading"
    :handleConfirm="cancelDepositHandler"
    :confirmation-prompt="$t('tip.areYouSureToCancelThisWithdrawal')"
  />

  <CreateWithdrawModal ref="CreateWithdrawRef" @on-created="fetchData(1)" />
  <CreateTransferModal ref="CreateTransferRef" @on-created="fetchData(1)" />
</template>

<script lang="ts" setup>
import moment from "moment";
import i18n from "@/core/plugins/i18n";
import TimeShow from "@/components/TimeShow.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { computed, ref, watch, onMounted } from "vue";
import WalletService from "../services/WalletService";
import { getLanguage } from "@/core/types/LanguageTypes";
import { TransactionStateType } from "@/core/types/StateInfos";
import CreateWithdrawModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";
import CreateTransferModal from "./modal/CreateTransferModal.vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { MatterTypesOptions, MatterTypes } from "@/core/types/MatterTypes";
import store from "@/store";

const props = defineProps<{
  selectedWallet: any;
}>();

const endDate = ref("");
const startDate = ref("");
const { t } = i18n.global;
const selectedId = ref(-1);
const data = ref<any[]>([]);
const isLoading = ref(false);
const criteria = ref<any>({} as any);
const currentTab = ref(MatterTypes.Withdrawal);
const filterRef = ref<InstanceType<typeof TradeFilter>>();
const CreateWithdrawRef = ref<InstanceType<typeof CreateWithdrawModal>>();
const CreateTransferRef = ref<InstanceType<typeof CreateTransferModal>>();
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const user = store.state.AuthModule.user;

// 根据用户角色过滤 walletOptions
const walletOptions = computed(() => {
  const options = MatterTypesOptions.value;
  // 只有当用户角色包含 IB 或 Sales 时才显示 Rebate tab
  const hasIBOrSales =
    user.roles && (user.roles.includes("IB") || user.roles.includes("Sales"));
  if (!hasIBOrSales) {
    return options.filter((option) => option.value !== MatterTypes.Rebate);
  }
  return options;
});

const groupedItems = computed(() => {
  const groupedObject = data.value.reduce((acc, cur) => {
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

const resetPage = () => {
  currentTab.value = MatterTypes.Withdrawal;
  startDate.value = "";
  endDate.value = "";
  criteria.value = {
    page: 1,
    size: 10,
    to: null,
    from: null,
    matterType: currentTab.value,
    currencyId: props.selectedWallet.currencyId,
  };
};

const resetFilter = () => {
  startDate.value = "";
  endDate.value = "";
  criteria.value.from = null;
  criteria.value.to = null;
  fetchData(1);
};

const changeTab = () => {
  criteria.value = {
    page: 1,
    size: 10,
    to: null,
    from: null,
    matterType: currentTab.value,
    currencyId: props.selectedWallet.currencyId,
  };
  criteria.value.from = startDate.value;
  criteria.value.to = endDate.value;
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;

  if (startDate.value) {
    criteria.value.from = moment(startDate.value).startOf("day").format();
  } else {
    criteria.value.from = null;
  }
  if (endDate.value) {
    criteria.value.to = moment(endDate.value).endOf("day").format();
  } else {
    criteria.value.to = null;
  }

  let res;
  delete criteria.value.matterType;

  try {
    switch (currentTab.value) {
      case MatterTypes.Withdrawal:
        res = await WalletService.queryWalletWithdrawV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      case MatterTypes.InternalTransfer:
        res = await WalletService.queryWalletTransferV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      case MatterTypes.WalletAdjust:
        res = await WalletService.queryWalletAdjustV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      case MatterTypes.Refund:
        res = await WalletService.queryWalletRefundV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      case MatterTypes.Rebate:
        res = await WalletService.queryWalletRebateV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      case MatterTypes.TransferRewards:
        res = await WalletService.queryWalletTransferRewardsV2(
          props.selectedWallet.hashId,
          criteria.value
        );
        break;
      default:
        break;
    }
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const showModal = (_modalName: string) => {
  switch (_modalName) {
    case "withdraw":
      CreateWithdrawRef.value?.show(false, props.selectedWallet);
      break;
    case "transfer":
      CreateTransferRef.value?.show(props.selectedWallet.id);
      break;
  }
};

const openConfirmBoxPanel = (_id: number) => {
  selectedId.value = _id;
  confirmBoxRef.value?.show();
};

const cancelDepositHandler = async () => {
  isLoading.value = true;
  try {
    await WalletService.cancelWithdrawal(selectedId.value);
    MsgPrompt.success(t("tip.widthdrawCancelSuccess")).then(() => {
      confirmBoxRef.value?.hide();
      filterRef.value?.fetchData(1);
    });
  } catch (error) {
    MsgPrompt.error(error).then(() => confirmBoxRef.value?.hide());
  } finally {
    isLoading.value = false;
  }
};

watch(
  () => props.selectedWallet,
  async () => {
    await resetPage();
    fetchData(1);
  }
);

onMounted(async () => {
  await resetPage();
  fetchData(1);
});

defineExpose({
  showModal,
});
</script>

<style>
#monthlyMobileTable .alpha-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ff8a00;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 400;
}

#monthlyMobileTable .statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 400;
}

#monthlyMobileTable .statusBadgeReject {
  background-color: rgba(232, 158, 152, 0.3);
  color: #e02b1d;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 400;
}

#monthlyMobileTable .transactionBtn {
  padding: 8px 24px;
  background: #f5f7fa;
  border-radius: 8px;
  font-style: normal;
  font-weight: 400;
  font-size: 16px;
  color: #002957;
}

#monthlyMobileTable .transactionBtn:hover {
  background: #ffd400;
}
</style>
