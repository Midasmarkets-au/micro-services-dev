<template>
  <div id="monthlyTable">
    <div
      class="gap-2 mb-5 sub-menu d-inline-flex"
      v-if="props.showActionButtons !== false"
    >
      <div
        class="sub-menu-item"
        :class="{ active: currentNav === 'withdraw' }"
        @click.prevent="showModal('withdraw')"
      >
        {{ $t("action.withdraw") }}
      </div>
      <div
        class="sub-menu-item"
        :class="{ active: currentNav === 'transfer' }"
        @click.prevent="showModal('transfer')"
      >
        {{ $t("action.transferOut") }}
      </div>
    </div>
    <div class="py-0 d-flex justify-content-between">
      <div class="tab-content w-100">
        <div class="tab-pane fade active show" id="kt_table_widget_5_tab_1">
          <div class="card round-bl-br mb-2">
            <div
              class="d-flex justify-between align-items-center card-header py-3"
            >
              <!-- <el-tabs v-model="currentTab" @tab-change="changeTab()">
                <el-tab-pane
                  v-for="item in walletOptions"
                  :key="item.value"
                  :label="item.label"
                  :name="item.value"
                  :disabled="isLoading"
                />
              </el-tabs> -->
              <div class="tabs-nav d-flex gap-3">
                <button
                  class="btn btn-light"
                  v-for="item in walletOptions"
                  :key="item.value"
                  :label="item.label"
                  @click="changeTab(item.value)"
                  :class="{
                    'btn-primary': currentTab === item.value,
                    'bg-white': currentTab !== item.value,
                  }"
                >
                  {{ item.label }}
                </button>
              </div>
              <div class="date-from d-flex gap-2 align-items-center">
                <el-date-picker
                  class="w-125px"
                  v-model="startDate"
                  type="date"
                  value-format="YYYY-MM-DD"
                  :placeholder="$t('fields.startDate')"
                  :disabled="isLoading"
                />
                <el-date-picker
                  class="w-125px"
                  v-model="endDate"
                  type="date"
                  value-format="YYYY-MM-DD"
                  :placeholder="$t('fields.endDate')"
                  :disabled="isLoading"
                />
                <div>
                  <el-button
                    size="large"
                    @click="fetchData(1)"
                    :loading="isLoading"
                    >{{ $t("action.search") }}</el-button
                  >
                  <el-button
                    size="large"
                    @click="resetFilter()"
                    :disabled="isLoading"
                    >{{ $t("action.reset") }}</el-button
                  >
                </div>
              </div>
            </div>
            <!-- <div class="d-flex gap-2 align-items-center mt-1 mb-3">
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
              <div>
                <el-button @click="fetchData(1)" :loading="isLoading">{{
                  $t("action.search")
                }}</el-button>
                <el-button @click="resetFilter()" :disabled="isLoading">{{
                  $t("action.reset")
                }}</el-button>
              </div>
            </div> -->
          </div>
          <div class="card card-body round-tl-tr">
            <div class="fw-500 fs-2 mb-3">
              {{ $t("action.showing") }}
              {{ data.length }}
              {{ $t("title.results") }}
            </div>

            <table
              v-if="isLoading || (data && data.length === 0)"
              class="table align-middle table-row-bordered gy-5"
              id="kt_ecommerce_sales_table"
            >
              <tbody v-if="isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!isLoading && data.length === 0">
                <NoDataBox />
              </tbody>
            </table>

            <table
              v-else
              class="table align-middle table-row-bordered gy-5"
              id="kt_ecommerce_sales_table"
            >
              <template v-for="(group, index) in groupedItems" :key="index">
                <thead>
                  <tr class="text-start text-uppercase gs-0">
                    <th class="text-start col-3">
                      {{ group.date }}
                    </th>
                    <th class="text-start col-2"></th>
                    <th class="text-start col-2">{{ $t("fields.status") }}</th>
                    <th v-if="currentTab !== 600 && currentTab !== 700">
                      {{ $t("fields.currency") }}
                    </th>
                    <th class="text-start col-2">{{ $t("fields.amount") }}</th>
                    <th class="text-start col-1">{{ $t("fields.time") }}</th>
                  </tr>
                </thead>

                <tbody>
                  <tr
                    class="text-start"
                    v-for="(item, index) in group.objects"
                    :key="index"
                  >
                    <td>
                      <div class="d-flex align-items-center gap-2">
                        <img
                          style="width: 17px; height: 17px"
                          :src="`/images/icons/finance/wallet-${
                            {
                              [MatterTypes.Deposit]: 'deposit',
                              [MatterTypes.Withdrawal]: 'withdraw',
                              [MatterTypes.Rebate]: 'rebate',
                              [MatterTypes.InternalTransfer]: 'transfer',
                              [MatterTypes.Refund]: 'refund',
                              [MatterTypes.WalletAdjust]: 'refund',
                              [MatterTypes.TransferRewards]: {
                                in: 'deposit',
                                out: 'withdraw',
                              }[item.flowType],
                            }[currentTab] ?? 'transfer'
                          }.svg`"
                          alt=""
                        />
                        <label
                          class="d-inline-block"
                          v-if="currentTab != MatterTypes.WalletAdjust"
                        >
                          {{
                            {
                              [MatterTypes.InternalTransfer]:
                                $t("title.transferOut") +
                                `->(  ${
                                  item.targetAccountNumber == 0
                                    ? $t("fields.wallet")
                                    : item.targetAccountNumber
                                })`,

                              [MatterTypes.Deposit]: $t(
                                "tip.depositInToTradeAccount"
                              ),

                              [MatterTypes.Withdrawal]: $t("action.withdraw"),

                              [MatterTypes.Rebate]:
                                $t("fields.rebate") +
                                " (" +
                                $t("fields.ticket") +
                                `  ${item.ticket}` +
                                ")",

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
                        <label v-else>
                          {{ adjustTypes[item.sourceType] }}
                        </label>
                      </div>
                    </td>
                    <td class="text-center">
                      <button
                        v-if="
                          item.stateId ===
                          TransactionStateType.WithdrawalCreated
                        "
                        class="btn btn-sm btn-secondary"
                        @click="openConfirmBoxPanel(item.id)"
                      >
                        <span>{{ $t("action.cancel") }}</span>
                      </button>
                    </td>
                    <td class="text-start">
                      <span
                        v-if="
                          [
                            TransactionStateType.TransferCompleted,
                            TransactionStateType.DepositCompleted,
                            TransactionStateType.WithdrawalCompleted,
                          ].includes(item.stateId)
                        "
                        class="statusBadgeCompleted"
                        >{{ $t("type.transactionState." + item.stateId) }}</span
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
                        >{{ $t("type.transactionState." + item.stateId) }}</span
                      >
                      <span v-else class="alpha-tag">{{
                        $t("type.transactionState." + item.stateId)
                      }}</span>
                    </td>
                    <td
                      class="text-start"
                      v-if="currentTab !== 600 && currentTab !== 700"
                    >
                      {{ $t(`type.currency.${item.currencyId}`) }}
                    </td>
                    <td class="text-start">
                      <a
                        href="#"
                        class="text-dark fw-normal text-hover-primary mb-1 fs-6"
                      >
                        <span
                          >{{
                            {
                              [MatterTypes.Deposit]: "+",
                              [MatterTypes.Withdrawal]: "-",
                            }[currentTab]
                          }}
                        </span>
                        <BalanceShow
                          :currency-id="item.currencyId"
                          :balance="item.amount"
                        />
                      </a>
                    </td>
                    <td class="text-start text-muted fw-semobold">
                      <TimeShow
                        :date-iso-string="item.createdOn"
                        format="h:mm a"
                      />
                    </td>
                  </tr>
                </tbody>
              </template>
            </table>
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
import CreateTransferModal from "./modal/CreateTransferModal.vue";
import TradeFilter from "@/projects/client/components/TradeFilter.vue";
import { MatterTypesOptions, MatterTypes } from "@/core/types/MatterTypes";
import CreateWithdrawModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";
import store from "@/store";

const props = defineProps<{
  selectedWallet: any;
  showActionButtons?: boolean;
}>();

const endDate = ref("");
const startDate = ref("");
const { t } = i18n.global;
const selectedId = ref(-1);
const data = ref<any[]>([]);
const isLoading = ref(false);
const criteria = ref<any>({} as any);
const currentNav = ref("withdraw");
const currentTab = ref(MatterTypes.Withdrawal);
const filterRef = ref<InstanceType<typeof TradeFilter>>();
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const CreateWithdrawRef = ref<InstanceType<typeof CreateWithdrawModal>>();
const CreateTransferRef = ref<InstanceType<typeof CreateTransferModal>>();
const user = store.state.AuthModule.user;

// 根据用户角色过滤 walletOptions
const walletOptions = computed(() => {
  const options = MatterTypesOptions.value;
  return options;
  // 只有当用户角色包含 IB 或 Sales 时才显示 Rebate tab
  // const hasIBOrSales =
  //   user.roles && (user.roles.includes("IB") || user.roles.includes("Sales"));
  // if (!hasIBOrSales) {
  //   return options.filter((option) => option.value !== MatterTypes.Rebate);
  // }
  // return options;
});
const adjustTypes = ref({
  0: t("fields.adjust"),
  1: t("fields.walletAdjustSalesRebate"),
  2: t("fields.eventRebate"),
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
    // currencyId: props.selectedWallet.currencyId,
  };
};

const resetFilter = () => {
  startDate.value = "";
  endDate.value = "";
  criteria.value.from = null;
  criteria.value.to = null;
  fetchData(1);
};

const changeTab = (value: number) => {
  currentTab.value = value;
  criteria.value = {
    page: 1,
    size: 10,
    to: null,
    from: null,
    matterType: currentTab.value,
    //currencyId: props.selectedWallet.currencyId,
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
  currentNav.value = _modalName;
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
</script>

<style>
#monthlyTable .alpha-tag {
  background-color: rgba(255, 205, 147, 0.3);
  color: #ffb82f;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 700;
}

#monthlyTable .statusBadgeCompleted {
  background-color: rgba(97, 200, 166, 0.3);
  color: #009262;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 700;
}

#monthlyTable .statusBadgeReject {
  background-color: rgba(232, 158, 152, 0.3);
  color: #e02b1d;
  border-radius: 5px;
  padding: 5px 10px;
  font-size: 10px;
  font-weight: 700;
}

#monthlyTable .transactionBtn {
  padding: 8px 24px;
  background: #f5f7fa;
  border-radius: 8px;
  font-style: normal;
  font-weight: 400;
  font-size: 16px;
  color: #002957;
}

#monthlyTable .transactionBtn:hover {
  background: #ffd400;
}
</style>
