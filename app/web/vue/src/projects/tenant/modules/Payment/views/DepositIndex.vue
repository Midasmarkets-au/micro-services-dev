<template>
  <div class="d-flex flex-column flex-column-fluid">
    <HeaderMenu :tabs="TabStatus" @change-tab="changeTab" ref="headerMenuRef" />
    <div class="card py-5 px-5">
      <div class="d-flex flex-wrap justify-content-between gap-3">
        <div class="d-flex flex-wrap gap-3">
          <el-date-picker
            class="w-250px"
            v-model="period"
            type="daterange"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            :disabled="isLoading"
            value-format="YYYY-MM-DD"
          />

          <el-select
            v-if="tab === TabStatus.All"
            v-model="criteria.stateId"
            @change="confirmSearch"
            class="w-150px"
            :placeholder="$t('fields.status')"
            clearable
            :disabled="isLoading"
          >
            <el-option :value="null" :label="$t('fields.all')" />
            <el-option
              v-for="depositStatus in DepositStateTypes"
              :key="depositStatus"
              :value="depositStatus"
              :label="
                $t(`type.transactionState.${depositStatus}`).replace(
                  /^Deposit /,
                  ''
                )
              "
            />
          </el-select>

          <el-input
            v-model="criteria.accountNumber"
            class="w-200px"
            :placeholder="$t('fields.accountNumber')"
            clearable
            :disabled="isLoading"
          >
          </el-input>

          <el-input
            v-model="criteria.email"
            class="w-250px"
            :placeholder="$t('fields.email')"
            clearable
            :disabled="isLoading"
          >
          </el-input>

          <el-button
            plain
            type="primary"
            @click="confirmSearch"
            :disabled="isLoading"
            >{{ $t("action.search") }}
          </el-button>
          <el-button
            plain
            @click="clearSearchFilterCriteria"
            :disabled="isLoading"
            >{{ $t("action.clear") }}
          </el-button>
          <el-button
            v-if="tab === TabStatus.All"
            type="success"
            @click="submitReportRequest"
            :loading="exporting"
            :disabled="isLoading"
            plain
            >{{ $t("action.export") }}
          </el-button>
        </div>

        <div class="d-flex gap-3"></div>
      </div>
    </div>

    <div v-for="(data, index) in dataColection" :key="index">
      <div class="card mt-5" v-if="tab == data.show || data.show == true">
        <div class="card-header">
          <div class="card-title">{{ data.label }}</div>
        </div>
        <div class="card-body py-4">
          <div class="table-responsive">
            <table
              class="table align-middle table-row-dashed fs-5 gy-5"
              id="table_accounts_requests"
            >
              <thead>
                <tr
                  class="text-start table-header-text fw-bold fs-5 text-uppercase gs-0"
                >
                  <th>{{ $t("fields.client") }}</th>
                  <th v-if="tab == TabStatus.All">
                    {{ $t("fields.status") }}
                  </th>
                  <th>
                    {{ $t("fields.paymentStatus") }}
                  </th>
                  <th>{{ $t("fields.paymentId") }}</th>
                  <th>{{ $t("fields.paymentNo") }}</th>
                  <th>{{ $t("title.paymentMethod") }}</th>
                  <th>{{ $t("fields.amount") }}</th>
                  <th>{{ $t("fields.actualAmount") }}</th>
                  <th>{{ $t("fields.exchangeRate") }}</th>
                  <th>{{ $t("fields.tradeAccount") }}</th>
                  <th>{{ $t("fields.group") }}</th>

                  <th class="text-center">{{ $t("fields.createdOn") }}</th>
                  <th class="cell-color text-center">
                    {{ $t("fields.receipt") }}
                  </th>
                  <th class="cell-color text-center">
                    {{ $t("fields.operatedBy") }}
                  </th>
                  <th
                    class="cell-color text-center"
                    v-if="tab != TransactionStateType.DepositCompleted"
                  >
                    {{ $t("action.action") }}
                  </th>
                </tr>
              </thead>
              <tbody v-if="data.isLoading">
                <LoadingRing />
              </tbody>
              <tbody v-else-if="!data.isLoading && data.data.length === 0">
                <NoDataBox />
              </tbody>
              <tbody v-else class="fw-semibold text-gray-900">
                <tr v-for="(item, index) in data.data" :key="index">
                  <td class="d-flex align-items-center">
                    <UserInfo v-if="item.user" :user="item.user" class="me-2" />
                  </td>
                  <td v-if="tab == TabStatus.All">
                    {{
                      $t(`type.transactionState.${item.stateId}`).replace(
                        /^Deposit /,
                        ""
                      )
                    }}
                  </td>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      <el-tag
                        :type="getTagType(item.payment.status)"
                        effect="dark"
                        class="fs-6"
                      >
                        {{ $t(`type.paymentStatus.${item.payment.status}`) }}
                      </el-tag>
                      <el-button
                        :icon="Document"
                        circle
                        @click="historyRef.show(item.id)"
                      />
                    </div>
                  </td>
                  <td>
                    {{ item.payment.id }}
                  </td>
                  <td>
                    {{ item.payment.number.substring(3) }}
                    <TinyCopyBox
                      :val="item.payment.number.substring(3)"
                    ></TinyCopyBox>
                  </td>
                  <td>
                    <div
                      class="d-flex flex-column"
                      v-if="item.payment.referenceNumber !== ''"
                      :class="item.walletAddress != '' ? 'cursor-pointer ' : ''"
                      :style="{
                        color: item.walletAddress != '' ? '#fd7e14' : 'black',
                      }"
                      @click="showWalletAddress(item.walletAddress)"
                    >
                      <span class="fs-7 text-muted fw-semobold mt-1">{{
                        item.payment.paymentServiceName
                      }}</span>
                      <span>{{ item.payment.referenceNumber }}</span>
                    </div>
                    <div v-else class="d-flex flex-column">
                      <span class="fs-7 text-muted fw-semobold mt-1">
                        {{ item.payment.paymentServiceName }}</span
                      >
                      <span>{{ item.payment.referenceNumber }}</span>
                    </div>
                  </td>
                  <td>
                    <BalanceShow
                      :currency-id="item.currencyId"
                      :balance="item.amount"
                    />
                  </td>
                  <td>
                    {{ actualAmount(item.payment.amount) }}
                  </td>
                  <td>{{ item.payment.exchangeRate }}</td>
                  <td>
                    <span
                      v-if="item.targetTradeAccount"
                      @click="
                        viewComments(
                          CommentType.Account,
                          item.targetTradeAccount.id,
                          item.targetTradeAccount.accountNumber
                        )
                      "
                      >{{ item.targetTradeAccount.accountNumber }}
                      <i
                        v-if="item.targetTradeAccount.hasComment"
                        class="fa-regular fa-comment-dots text-primary"
                      ></i
                    ></span>
                  </td>
                  <td>{{ item.targetTradeAccount.group }}</td>

                  <td class="text-center">
                    <TimeShow :date-iso-string="item.payment.createdOn" />
                  </td>
                  <td class="cell-color text-center">
                    <span
                      v-if="
                        item.payment.paymentServiceId ==
                        PaymentPlatformTypes.Poli
                      "
                      class="btn btn-sm btn-light-success fw-bold ms-2 fs-8 py-1 px-3"
                      @click="viewPoliReference(item.id)"
                    >
                      {{ $t("action.viewReference") }}
                    </span>

                    <span
                      v-else-if="item.hasReceipt"
                      href="#"
                      class="btn btn-sm btn-light-primary fw-bold ms-2 fs-8 py-1 px-3"
                      @click="viewWireReceipt(item.id)"
                    >
                      {{ $t("action.viewReceipt") }}
                    </span>

                    <span
                      v-else
                      @click="openUploadDepositReceiptPanel(item.id)"
                      style="cursor: pointer"
                      ><i class="fa-solid fa-arrow-up-from-bracket"></i
                    ></span>
                  </td>
                  <td class="cell-color text-center">
                    {{ item.operatorName }}
                  </td>
                  <td class="cell-color">
                    <div class="d-flex">
                      <button
                        v-for="(button, index) in getButtons(item)"
                        :key="index"
                        class="btn btn-sm me-3"
                        :class="button.type"
                        @click="button.action"
                      >
                        {{ button.label }}
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <TableFooter
            @page-change="data.fetchData"
            :criteria="data.criteria"
          />
        </div>
      </div>
    </div>
  </div>
  <DepositReference ref="depositReferenceRef" />
  <HistoryRecord ref="historyRef" />
  <UploadDepositReceiptModal
    ref="uploadDepositReceiptRef"
    @fetchData="fetchData"
  />
  <CommentsView ref="commentsRef" type="" id="0" />

  <el-dialog
    v-model="showAddress"
    width="700"
    align-center
    style="max-height: 95vh; overflow: auto"
  >
    <div class="d-flex justify-content-center fs-1 mb-5">
      {{ $t("action.deposit") }} {{ $t("tip.walletAddress") }}
    </div>

    <div
      class="d-flex justify-content-center fs-1 pt-3 pb-3"
      style="background-color: cornsilk; border-radius: 100px"
    >
      {{ dialogData }}
    </div>

    <div class="mt-9 d-flex justify-content-center">
      <span v-if="showTip" class="tip fs-8 badge badge-light">Copyed </span>
      <el-button
        class="text-gray cursor-pointer position-relative"
        @click="copyAddress(dialogData)"
      >
        <span
          v-if="showTip"
          class="tip fs-8 badge badge-light"
          :class="{ 'show-tip-animation': showTip }"
          >Copyed </span
        >{{ $t("action.copy") }}</el-button
      >
    </div>
  </el-dialog>
</template>
<script setup lang="ts">
import MsgPrompt from "@/core/plugins/MsgPrompt";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import UploadDepositReceiptModal from "../modal/UploadDepositReceiptModal.vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import DepositReference from "@/projects/tenant/modules/Payment/modal/DepositReference.vue";
import HeaderMenu from "../components/HeaderMenu.vue";
import { ActionType } from "@/core/types/Actions";
import { ref, onMounted, inject, watch, reactive, provide } from "vue";
import { CommentType } from "@/core/types/CommentType";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import {
  TransactionStateType,
  DepositStatusType,
  DepositStateTypes,
} from "@/core/types/StateInfos";
import {
  PaymentStatusTypes,
  PaymentPlatformTypes,
} from "@/core/types/PaymentTypes";
import i18n from "@/core/plugins/i18n";
import Decimal from "decimal.js";
import { convertTradeTime } from "@/core/helpers/helpers";
import { Document } from "@element-plus/icons-vue";
import HistoryRecord from "../components/modal/HistoryRecord.vue";
import Clipboard from "clipboard";

const historyRef = ref<any>(null);

const showTip = ref(false);
const t = i18n.global.t;
const isLoading = ref(false);
provide("isLoading", isLoading);
const callBackLoading = ref(false);
const dealingDataLoading = ref(false);
const exporting = ref(false);

const TabStatus = DepositStatusType;
const tab = ref<any>(TabStatus.Pending);
const headerMenuRef = ref<InstanceType<typeof HeaderMenu>>();
const data = ref(<any>[]);
const dataDealing = ref(<any>[]);
const callBackData = ref(<any>[]);

const period = ref([] as any);
const showAddress = ref(false);
const dialogData = ref(<any>[]);

const commentsRef = ref<InstanceType<typeof CommentsView>>();
const depositReferenceRef = ref<InstanceType<typeof DepositReference>>();
const uploadDepositReceiptRef =
  ref<InstanceType<typeof UploadDepositReceiptModal>>();

const criteria = ref<any>({
  page: 1,
  size: 10,
  stateId: tab.value,
  sortField: "createdOn",
});

const dealingCriteria = ref<any>({
  page: 1,
  size: 10,
  stateIds: [
    TransactionStateType.DepositPaymentCompleted,
    TransactionStateType.DepositCallbackTimeOut,
  ],
  sortField: "createdOn",
});

const callBackCriteria = ref<any>({
  page: 1,
  size: 10,
  stateId: TransactionStateType.DepositCallbackComplete,
  sortField: "createdOn",
});

const changeTab = (_tab: any) => {
  tab.value = _tab;
  if (tab.value == TabStatus.All) {
    criteria.value.stateId = null;
  } else criteria.value.stateId = tab.value;
  fetchAll();
};

const fetchAll = (page?: number) => {
  const pageNumber = page ?? 1;
  if (tab.value == TabStatus.Pending) {
    fetchData(pageNumber);
    fetchDealingData(pageNumber);
    fetchCallBackData(pageNumber);
  } else {
    fetchData(pageNumber);
  }
};

const actualAmount = (amount: any) => {
  return new Decimal(amount)
    .div(100)
    .toFixed(0)
    .replace(/\B(?=(\d{3})+(?!\d))/g, ",");
};

const fetchData = async (_page?: number) => {
  if (_page) criteria.value.page = _page;

  isLoading.value = true;
  const res = await PaymentService.queryDeposits(criteria.value);
  criteria.value = res.criteria;
  data.value = res.data;
  isLoading.value = false;
};

const fetchDealingData = async (_page?: number) => {
  if (_page) dealingCriteria.value.page = _page;

  dealingDataLoading.value = true;
  const res = await PaymentService.queryDeposits(dealingCriteria.value);
  dealingCriteria.value = res.criteria;
  dataDealing.value = res.data;
  dealingDataLoading.value = false;
};

const fetchCallBackData = async (_page?: number) => {
  if (_page) callBackCriteria.value.page = _page;

  callBackLoading.value = true;
  const res = await PaymentService.queryDeposits(callBackCriteria.value);
  callBackCriteria.value = res.criteria;
  callBackData.value = res.data;
  callBackLoading.value = false;
};

const dataColection = reactive(<any>[
  {
    label: "財務部門 審核",
    show: true,
    data: data,
    isLoading: isLoading,
    fetchData: fetchData,
    criteria: criteria,
  },
  {
    label: "Dealing 審核",
    show: TabStatus.Pending,
    data: dataDealing,
    isLoading: dealingDataLoading,
    fetchData: fetchDealingData,
    criteria: dealingCriteria,
  },
  {
    label: "第三方 - 完成支付",
    show: TabStatus.Pending,
    data: callBackData,
    isLoading: callBackLoading,
    fetchData: fetchCallBackData,
    criteria: callBackCriteria,
  },
]);

watch(
  () => period.value,
  (periodVal) => {
    if (periodVal && periodVal.length > 0) {
      const [from, to] = convertTradeTime(periodVal[0], periodVal[1]);
      criteria.value.from = from;
      criteria.value.to = to;
    } else {
      criteria.value.from = null;
      criteria.value.to = null;
    }
  }
);
const confirmSearch = () => {
  if (tab.value != TabStatus.All) {
    headerMenuRef.value?.changeTab(TabStatus.All);
  } else {
    fetchData(1);
  }
};

const clearSearchFilterCriteria = () => {
  criteria.value = {
    page: 1,
    size: 10,
    SortField: "updatedOn",
    stateId: tab.value,
  };
  if (tab.value == TabStatus.All) {
    criteria.value.stateId = null;
  }
  period.value = [];
  fetchAll();
};

const openUploadDepositReceiptPanel = (_id: number) => {
  uploadDepositReceiptRef.value?.show(_id);
};

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};

const formData = ref<CreateReportSpec>({
  name: "",
  type: ReportRequestTypes.DepositForTenant,
  query: criteria.value,
});

const submitReportRequest = async () => {
  exporting.value = true;
  const [from, to] = period.value ?? [null, null];
  const [createdFrom, createdTo] = convertTradeTime(from, to);

  formData.value.query = {
    from: createdFrom,
    to: createdTo,
    stateId: criteria.value.stateId,
  };
  try {
    const media = await TenantGlobalService.createReportRequestDownload(
      formData.value
    );
    await TenantGlobalService.downloadFileByGuid(media.guid, media.fileName);
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    exporting.value = false;
  }
};
const viewPoliReference = async (id) => {
  const res = await PaymentService.getDepositReferenceById(id);
  depositReferenceRef.value?.show(res);
};

const openFilePreviewModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_FILE_MODAL
);

const viewWireReceipt = async (id) => {
  const res = await PaymentService.getDepositReceiptById(id);
  const media = {
    guid: res[0],
  };
  openFilePreviewModal?.(media);
};

const getTagType = (status: number) => {
  switch (status) {
    case PaymentStatusTypes.Pending:
      return "primary";
    case PaymentStatusTypes.Completed:
      return "success";
    case PaymentStatusTypes.Failed:
      return "danger";
    case PaymentStatusTypes.Executing:
      return "warning";
    default:
      return "primary";
  }
};

const openConfirmModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const openConfirmPanel = (
  _action: ActionType,
  id: number,
  paymentID?: number
) => {
  openConfirmModal?.(() => {
    return {
      [ActionType.Approve]: () => PaymentService.approveDepositById(id),
      [ActionType.Reject]: () => PaymentService.rejectDepositById(id),
      [ActionType.Cancel]: () => PaymentService.cancelDepositById(id),
      [ActionType.CompletePayment]: () =>
        PaymentService.completeDepositByPaymentId(id),
      [ActionType.Complete]: () =>
        PaymentService.completeCallBackByPaymentId(id),
      [ActionType.RestoreRejected]: () =>
        PaymentService.restoreRejectedDepositByPaymentId(id),
      [ActionType.RestoreCanceled]: () =>
        PaymentService.restoreCanceledDepositByPaymentId(id),
    }
      [_action]()
      .then(() => {
        fetchAll(criteria.value.page);
      });
  });
};

const buttonTypes = {
  comepletePayment: (item) => ({
    label: t("action.completePayment"),
    type: "btn-info",
    action: () => openConfirmPanel(ActionType.CompletePayment, item.id),
  }),
  approvePayment: (item) => ({
    label: t("action.approve"),
    type: "btn-primary",
    action: () => openConfirmPanel(ActionType.Approve, item.id),
  }),
  rejectPayment: (item) => ({
    label: t("action.reject"),
    type: "btn-danger",
    action: () => openConfirmPanel(ActionType.Reject, item.id),
  }),
  cancelPayment: (item) => ({
    label: t("action.cancel"),
    type: "btn-light-danger",
    action: () => openConfirmPanel(ActionType.Cancel, item.id),
  }),
  restoreRejectPayment: (item) => ({
    label: t("action.restore"),
    type: "btn-light-danger",
    action: () => openConfirmPanel(ActionType.RestoreRejected, item.id),
  }),
  restoreCancelPayment: (item) => ({
    label: t("action.restore"),
    type: "btn-light-danger",
    action: () => openConfirmPanel(ActionType.RestoreCanceled, item.id),
  }),
  completeCallbackPayment: (item) => ({
    label: t("action.complete"),
    type: "btn-info",
    action: () => openConfirmPanel(ActionType.Complete, item.id),
  }),
};
import Can from "@/core/plugins/ICan";
const $cans = Can.cans;

function getButtons(item: any) {
  let configs: any[] = [];

  switch (item.stateId) {
    case TransactionStateType.DepositCreated:
      configs.push(buttonTypes["comepletePayment"](item));
      configs.push(buttonTypes["cancelPayment"](item));
      break;

    case TransactionStateType.DepositPaymentCompleted:
      configs.push(buttonTypes["approvePayment"](item));
      if ($cans(["TenantAdmin"])) {
        configs.push(buttonTypes["rejectPayment"](item));
      }
      break;

    case TransactionStateType.DepositCallbackTimeOut:
      configs.push(buttonTypes["approvePayment"](item));
      if ($cans(["TenantAdmin"])) {
        configs.push(buttonTypes["rejectPayment"](item));
      }
      break;

    case TransactionStateType.DepositTenantRejected:
      configs.push(buttonTypes["restoreRejectPayment"](item));
      break;

    case TransactionStateType.DepositCanceled:
      configs.push(buttonTypes["restoreCancelPayment"](item));
      break;

    case TransactionStateType.DepositCallbackComplete:
      configs.push(buttonTypes["completeCallbackPayment"](item));
      configs.push(buttonTypes["cancelPayment"](item));
      break;
  }
  return configs;
}

const showWalletAddress = (_walletAddress: any) => {
  showAddress.value = true;
  dialogData.value = _walletAddress;
};

const copyAddress = (_walletAddress) => {
  Clipboard.copy(_walletAddress as string);
  showTip.value = true;
  setTimeout(() => {
    showTip.value = false;
  }, 1000);
};

onMounted(() => {
  fetchAll();
});
</script>

<style scoped>
.table-responsive {
  overflow-x: scroll !important;
}
.table {
  min-width: 100%;
  width: max-content;
}
.table td,
.table th {
  white-space: nowrap;
}
.cell-color {
  background-color: rgb(255, 255, 224);
}
.nav-line-tabs .nav-item .nav-link {
  color: black;
}
.table-header-text {
  color: #76787d;
}
.tip {
  opacity: 0;
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  transition: all 1s ease-in-out;
  pointer-events: none; /* This prevents the tip from being clickable */
}
.show-tip-animation {
  animation: tip-show 1s forwards;
}
@keyframes tip-show {
  0% {
    opacity: 0;
    bottom: 100%;
  }
  40%,
  60% {
    opacity: 1;
    bottom: 160%;
  }
  100% {
    opacity: 0;
    bottom: 160%;
  }
}
</style>
