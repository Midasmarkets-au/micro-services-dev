<template>
  <div class="d-flex flex-column flex-column-fluid">
    <HeaderMenu :tabs="TabStatus" @changeTab="changeTab" ref="headerMenuRef" />
    <div>
      <div class="card py-5 px-5 mb-3">
        <div class="d-flex flex-wrap justify-content-between gap-3">
          <div class="d-flex flex-wrap gap-3">
            <el-date-picker
              class="w-250px"
              v-model="period"
              type="daterange"
              :start-placeholder="$t('fields.startDate')"
              :end-placeholder="$t('fields.endDate')"
              value-format="YYYY-MM-DD"
            />

            <el-select
              v-if="tab === TabStatus.All"
              v-model="criteria.stateId"
              @change="confirmSearch"
              class="w-150px"
              :placeholder="$t('fields.status')"
              clearable
            >
              <el-option :value="null" :label="$t('fields.all')" />
              <el-option
                v-for="status in WithdrawalStateTypes"
                :key="status"
                :value="status"
                :label="$t(`type.transactionState.${status}`)"
              />
            </el-select>

            <el-input
              v-model="criteria.accountNumber"
              class="w-200px"
              :placeholder="$t('fields.accountNumber')"
              clearable
            >
            </el-input>

            <el-input
              v-model="criteria.email"
              class="w-250px"
              :placeholder="$t('fields.email')"
              clearable
            >
            </el-input>

            <el-input
              v-model="criteria.walletId"
              class="w-150px"
              :placeholder="$t('fields.walletId')"
              clearable
            >
            </el-input>

            <el-input
              v-model="criteria.paymentId"
              class="w-150px"
              :placeholder="$t('fields.paymentId')"
              clearable
            >
            </el-input>

            <el-button plain type="primary" @click="confirmSearch">{{
              $t("action.search")
            }}</el-button>

            <el-button plain type="info" @click="clearSearchFilterCriteria">{{
              $t("action.clear")
            }}</el-button>

            <el-button
              v-if="tab === TabStatus.All"
              type="success"
              @click="submitReportRequest"
              :loading="exporting"
              plain
              >{{ $t("action.export") }}
            </el-button>
          </div>
          <div class="d-flex gap-3"></div>
        </div>
      </div>

      <div class="card">
        <div class="card-header">
          <div
            v-if="tab == TransactionStateType.WithdrawalCreated"
            class="card-title d-flex justify-content-between"
            style="width: 100%"
          >
            <div>Dealing 審核</div>
            <el-button
              v-if="tab === TabStatus.All || tab === TabStatus.Pending"
              type="success"
              @click="submitReportRequest"
              :loading="exporting"
              plain
              >{{ $t("action.export") }}
            </el-button>
          </div>
        </div>
        <div class="card-body py-4">
          <div class="table-responsive">
            <table
              class="table align-middle table-row-dashed fs-6 gy-5"
              id="table_accounts_requests"
            >
              <thead>
                <tr
                  class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                >
                  <th>{{ $t("fields.client") }}</th>
                  <th>{{ $t("fields.info") }}</th>
                  <th v-if="tab == TabStatus.All">{{ $t("fields.status") }}</th>
                  <th>{{ $t("fields.paymentStatus") }}</th>
                  <th>{{ $t("fields.paymentID") }}</th>
                  <th>{{ $t("fields.paymentMethod") }}</th>
                  <th>{{ $t("fields.currency") }}</th>
                  <th>{{ $t("fields.withdrawAmount") }}</th>
                  <th>{{ $t("fields.exchangeRate") }}</th>
                  <th>{{ $t("fields.sourceBalance") }}</th>
                  <th>{{ $t("fields.time") }}</th>
                  <th class="text-center cell-color">
                    {{ $t("fields.paymentInfo") }}
                  </th>
                  <th
                    v-if="tab != TransactionStateType.WithdrawalCreated"
                    class="cell-color text-center"
                  >
                    {{ $t("fields.operatedBy") }}
                  </th>
                  <th
                    class="cell-color text-center"
                    v-if="
                      tab == TransactionStateType.WithdrawalCreated || tab == 0
                    "
                  >
                    {{ $t("action.action") }}
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
                  <td>
                    {{ item.source.salesGroupName }}<br />{{
                      item.source.agentGroupName
                    }}
                  </td>
                  <td v-if="tab == TabStatus.All">
                    {{ $t(`type.transactionState.${item.stateId}`) }}
                  </td>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      <span
                        class="badge"
                        :class="{
                          'badge-primary':
                            item.payment.status === PaymentStatusTypes.Pending,
                          'badge-success':
                            item.payment.status ===
                            PaymentStatusTypes.Completed,
                          'badge-danger':
                            item.payment.status === PaymentStatusTypes.Failed,
                          'badge-warning':
                            item.payment.status ===
                            PaymentStatusTypes.Executing,
                        }"
                        >{{
                          $t(`type.paymentStatus.${item.payment.status}`)
                        }}</span
                      >

                      <el-button
                        :icon="Document"
                        circle
                        @click="historyRef.show(item.id)"
                      />
                    </div>
                  </td>
                  <td
                    @click="
                      viewComments(
                        CommentType.Withdrawal,
                        item.id,
                        item.payment.number.substring(3)
                      )
                    "
                  >
                    {{ item.payment.id
                    }}<i
                      v-if="item.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i>
                  </td>
                  <td>
                    {{ item.payment.paymentServiceName }}
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

                  <td>
                    <div>
                      <BalanceShow
                        :currency-id="item.currencyId"
                        :balance="
                          item.source.accountType == 1
                            ? item.source.balanceInCents
                            : item.source.equityInCents
                        "
                      />
                    </div>
                    <div
                      v-if="
                        item.source.accountType ===
                        TransactionAccountType.Wallet
                      "
                      class="badge badge-primary"
                    >
                      Wallet # {{ item.source.displayNumber }}
                    </div>
                    <div
                      v-if="
                        item.source.accountType ===
                        TransactionAccountType.TradeAccount
                      "
                      class="badge badge-warning"
                      @click="
                        viewComments(
                          CommentType.Account,
                          item.source.id,
                          item.source.displayNumber
                        )
                      "
                    >
                      Account # {{ item.source.displayNumber }}
                      <i
                        v-if="item.source.hasComment"
                        class="fa-regular fa-comment-dots text-info ms-3 cursor-pointer"
                      ></i>
                    </div>
                  </td>

                  <td>
                    <div>
                      <TimeShow :date-iso-string="item.payment.createdOn" />
                    </div>
                    <div
                      v-if="item.payment.createdOn != item.payment.updatedOn"
                    >
                      <TimeShow :date-iso-string="item.payment.updatedOn" />
                    </div>
                  </td>
                  <td class="text-center cell-color">
                    <a
                      href="#"
                      class="btn btn-sm btn-light-info fw-bold ms-2 fs-8 py-1 px-3"
                      @click="showWithdrawInfo(item)"
                    >
                      {{ $t("action.showPaymentInfo") }}
                    </a>
                    <button
                      class="btn btn-primary btn-sm me-1"
                      style="margin-left: 5px"
                      @click="showWithdrawLImitInfo(item)"
                    >
                      {{ $t("fields.fundStatistics") }}
                    </button>
                  </td>
                  <td
                    v-if="tab != TabStatus.Pending"
                    class="cell-color text-center"
                  >
                    <div
                      v-if="
                        item.stateId != TransactionStateType.WithdrawalCreated
                      "
                    >
                      {{ item.operatorName }}
                    </div>
                  </td>

                  <td class="text-center cell-color">
                    <div
                      v-if="
                        item.stateId == TransactionStateType.WithdrawalCreated
                      "
                      class="d-flex text-nowrap"
                    >
                      <button
                        :disabled="
                          (item.source.accountType == 1
                            ? item.source.balanceInCents
                            : item.source.equityInCents) < item.amount
                        "
                        class="btn btn-light btn-primary btn-sm me-1"
                        @click="
                          item.source.accountType ===
                          TransactionAccountTypes.Wallet
                            ? openConfirmPanel(ActionType.Approve, item.id)
                            : openApprovePanel(item.id)
                        "
                      >
                        <template
                          v-if="
                            (item.source.accountType == 1
                              ? item.source.balanceInCents
                              : item.source.equityInCents) < item.amount
                          "
                          >{{ $t("status.notAllowed") }}</template
                        >
                        <template v-else>{{ $t("action.approve") }}</template>
                      </button>

                      <button
                        href="#"
                        class="btn btn-light btn-danger btn-sm"
                        @click="openRejectPanel(item.id)"
                      >
                        {{ $t("action.reject") }}
                      </button>
                      <button
                        href="#"
                        class="btn btn-light btn-light-danger btn-sm ms-1"
                        @click="openConfirmPanel(ActionType.Cancel, item.id)"
                      >
                        {{ $t("action.cancel") }}
                      </button>
                    </div>
                    <div
                      v-if="
                        item.stateId ==
                        TransactionStateType.WithdrawalTenantApproved
                      "
                    >
                      <button
                        v-if="
                          item.payment.status === PaymentStatusTypes.Pending
                        "
                        class="btn btn-light btn-success btn-sm me-3"
                        @click="
                          openConfirmPanel(
                            ActionType.StartExecution,
                            item.payment.id
                          )
                        "
                      >
                        {{ $t("action.startProcess") }}
                      </button>

                      <button
                        v-else-if="
                          item.payment.status === PaymentStatusTypes.Executing
                        "
                        class="btn btn-light btn-info btn-sm me-3"
                        @click="
                          openConfirmPanel(
                            ActionType.CompletePayment,
                            item.id,
                            item.payment.id
                          )
                        "
                      >
                        {{ $t("action.complete") }}
                      </button>
                      <button
                        v-if="$can('TenantAdmin') || $can('SuperAdmin')"
                        class="btn btn-light btn-light-danger btn-sm me-3"
                        @click="openConfirmPanel(ActionType.Refund, item.id)"
                      >
                        {{ $t("action.refund") }}
                      </button>
                    </div>

                    <div
                      v-if="
                        item.stateId ==
                        TransactionStateType.WithdrawalTenantRejected
                      "
                      class="btn btn-light btn-danger btn-sm me-3"
                      @click="openRejectRestorePanel(item.id)"
                    >
                      {{ $t("action.restore") }}
                    </div>
                    <div
                      v-if="
                        item.stateId == TransactionStateType.WithdrawalCanceled
                      "
                      class="btn btn-light btn-danger btn-sm me-3"
                      @click="openCancelRestorePanel(item.id)"
                    >
                      {{ $t("action.restore") }}
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <TableFooter @page-change="pageChange" :criteria="criteria" />
        </div>
      </div>

      <div class="card mt-5" v-if="tab == TabStatus.Pending">
        <div class="card-header">
          <div
            v-if="tab == TransactionStateType.WithdrawalCreated"
            class="card-title d-flex justify-content-between align-items-center"
            style="width: 100%"
          >
            <span>財務部門 審核</span>
            <el-button
              type="primary"
              :disabled="selectedDealingIds.length === 0"
              @click="handleBatchProcess"
            >
              {{ $t("action.batchProcess") }} ({{ selectedDealingIds.length }})
            </el-button>
          </div>
        </div>
        <div class="card-body py-4">
          <div class="table-responsive">
            <table
              class="table align-middle table-row-dashed fs-6 gy-5"
              id="table_accounts_requests"
            >
              <thead>
                <tr
                  class="text-start text-muted fw-bold fs-7 text-uppercase gs-0"
                >
                  <th>
                    <el-checkbox
                      v-model="isAllDealingSelected"
                      :indeterminate="isDealingIndeterminate"
                      @change="(val: any) => handleSelectAllDealing(val as boolean)"
                    />
                  </th>
                  <th>{{ $t("fields.client") }}</th>
                  <th v-if="tab == TabStatus.All">{{ $t("fields.status") }}</th>
                  <th>{{ $t("fields.paymentStatus") }}</th>
                  <th>{{ $t("fields.paymentID") }}</th>
                  <th>{{ $t("fields.paymentMethod") }}</th>
                  <th>{{ $t("fields.currency") }}</th>
                  <th>{{ $t("fields.withdrawAmount") }}</th>
                  <th>{{ $t("fields.sourceBalance") }}</th>
                  <th>{{ $t("fields.createdOn") }}</th>
                  <th class="text-center cell-color">
                    {{ $t("fields.paymentInfo") }}
                  </th>
                  <th class="cell-color text-center">
                    {{ $t("fields.operatedBy") }}
                  </th>
                  <th class="cell-color text-center">
                    {{ $t("action.action") }}
                  </th>
                  <th class="cell-color text-center">
                    {{ $t("action.refund") }}
                  </th>
                </tr>
              </thead>

              <tbody v-if="isDealingLoading || isLoadingAll">
                <LoadingRing />
              </tbody>
              <tbody
                v-else-if="
                  !isDealingLoading && !isLoadingAll && dataDealing.length === 0
                "
              >
                <NoDataBox />
              </tbody>

              <tbody v-else class="fw-semibold text-gray-900">
                <tr v-for="(item, index) in dataDealing" :key="index">
                  <td>
                    <el-checkbox
                      v-if="
                        item.payment.status === PaymentStatusTypes.Pending &&
                        item.payment.paymentServiceName.includes('USDT')
                      "
                      v-model="item.selected"
                      @change="handleDealingSelectionChange"
                    />
                  </td>
                  <td class="d-flex align-items-center">
                    <UserInfo v-if="item.user" :user="item.user" class="me-2" />
                  </td>
                  <td v-if="tab == TabStatus.All">
                    {{ $t(`type.transactionState.${item.stateId}`) }}
                  </td>
                  <td>
                    <div class="d-flex align-items-center gap-2">
                      <span
                        class="badge"
                        :class="{
                          'badge-primary':
                            item.payment.status === PaymentStatusTypes.Pending,
                          'badge-success':
                            item.payment.status ===
                            PaymentStatusTypes.Completed,
                          'badge-danger':
                            item.payment.status === PaymentStatusTypes.Failed,
                          'badge-warning':
                            item.payment.status ===
                            PaymentStatusTypes.Executing,
                        }"
                        >{{
                          $t(`type.paymentStatus.${item.payment.status}`)
                        }}</span
                      >
                      <el-button
                        :icon="Document"
                        circle
                        @click="historyRef.show(item.id)"
                      />
                    </div>
                  </td>
                  <td
                    @click="
                      viewComments(
                        CommentType.Withdrawal,
                        item.id,
                        item.payment.number.substring(3)
                      )
                    "
                  >
                    {{ item.payment.id
                    }}<i
                      v-if="item.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i>
                  </td>
                  <td>
                    {{ item.payment.paymentServiceName }}
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
                    <div>
                      <BalanceShow
                        :currency-id="item.currencyId"
                        :balance="
                          item.source.accountType == 1
                            ? item.source.balanceInCents
                            : item.source.equityInCents
                        "
                      />
                    </div>
                    <div
                      v-if="
                        item.source.accountType ===
                        TransactionAccountType.Wallet
                      "
                      class="badge badge-primary"
                    >
                      Wallet # {{ item.source.displayNumber }}
                    </div>
                    <div
                      v-if="
                        item.source.accountType ===
                        TransactionAccountType.TradeAccount
                      "
                      class="badge badge-warning"
                      @click="
                        viewComments(
                          CommentType.Account,
                          item.source.id,
                          item.source.displayNumber
                        )
                      "
                    >
                      Account # {{ item.source.displayNumber }}
                      <i
                        v-if="item.source.hasComment"
                        class="fa-regular fa-comment-dots text-info ms-3 cursor-pointer"
                      ></i>
                    </div>
                  </td>
                  <td>
                    <div>
                      <TimeShow :date-iso-string="item.payment.createdOn" />
                    </div>
                    <div
                      v-if="item.payment.createdOn != item.payment.updatedOn"
                    >
                      <TimeShow :date-iso-string="item.payment.updatedOn" />
                    </div>
                  </td>
                  <td class="text-center cell-color">
                    <a
                      href="#"
                      class="btn btn-sm btn-light-info fw-bold ms-2 fs-8 py-1 px-3"
                      @click="showWithdrawInfo(item)"
                    >
                      {{ $t("action.showPaymentInfo") }}
                    </a>
                  </td>
                  <td
                    v-if="
                      item.stateId != TransactionStateType.WithdrawalCreated
                    "
                    class="cell-color text-center"
                  >
                    {{ item.operatorName }}
                  </td>

                  <td class="text-center cell-color">
                    <button
                      v-if="item.payment.status === PaymentStatusTypes.Pending"
                      class="btn btn-light btn-success btn-sm me-3"
                      @click="
                        openConfirmPanel(
                          ActionType.StartExecution,
                          item.payment.id
                        )
                      "
                    >
                      {{ $t("action.startProcess") }}
                    </button>

                    <button
                      v-else-if="
                        item.payment.status === PaymentStatusTypes.Executing ||
                        item.payment.status === PaymentStatusTypes.Completed
                      "
                      class="btn btn-light btn-info btn-sm me-3"
                      @click="
                        openConfirmPanel(
                          ActionType.CompletePayment,
                          item.id,
                          item.payment.id
                        )
                      "
                    >
                      {{ $t("action.complete") }}
                    </button>
                  </td>

                  <td class="text-center cell-color">
                    <button
                      v-if="$can('TenantAdmin') || $can('SuperAdmin')"
                      class="btn btn-light btn-light-danger btn-sm me-3"
                      @click="openConfirmPanel(ActionType.Refund, item.id)"
                    >
                      {{ $t("action.refund") }}
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <TableFooter
            @page-change="fetchDealingData"
            :criteria="dealingCriteria"
          />
        </div>
      </div>
    </div>
    <WithdrawInfo ref="WithdrawInfoRef" />
    <BatchProcessModal ref="batchProcessModalRef" @success="fecthAll" />
    <WithdrawLimitInfo ref="WithdrawLimitRef" />
    <CommentsView ref="commentsRef" type="" id="0" />
    <ConfirmBox
      ref="confirmBoxRef"
      :is-loading="isSubmitting"
      :handleConfirm="processTransaction"
      :confirm-color="
        {
          [ActionType.Approve]: 'primary',
          [ActionType.Reject]: 'danger',
          [ActionType.Cancel]: 'success',
          [ActionType.CompletePayment]: 'info',
          [ActionType.Complete]: 'warning',
        }[action]
      "
    />
    <ConfirmCommentBox
      ref="confirmCommentBoxRef"
      :hasComment="true"
    ></ConfirmCommentBox>
    <NewWithdrawalRequestModal ref="newWithdrawalRequestModalRef" />
    <HistoryRecord ref="historyRef" />
  </div>
</template>
<script setup lang="ts">
import moment from "moment/moment";
import { ref, onMounted, inject, watch, provide, InsHTMLAttributes } from "vue";
import NewWithdrawalRequestModal from "@/projects/tenant/modules/Payment/components/modal/NewWithdrawalRequestModal.vue";
import { isDateInDST_US } from "@/core/plugins/TimerService";
import svc from "../services/PaymentService";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import {
  TransactionStateType,
  WithdrawalStatusType,
  WithdrawalStateTypes,
  TransactionAccountType,
} from "@/core/types/StateInfos";
import { TransactionAccountTypes } from "@/core/types/AccountInfos";
import { CommentType } from "@/core/types/CommentType";
import WithdrawInfo from "../modal/WithdrawInfo.vue";
import WithdrawLimitInfo from "../modal/WithdrawLimitInfo.vue";
import BatchProcessModal from "../modal/BatchProcessModal.vue";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { PaymentStatusTypes } from "@/core/types/PaymentTypes";
import { ActionType } from "@/core/types/Actions";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import ConfirmCommentBox from "@/components/ConfirmCommentBox.vue";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { convertTradeTime } from "@/core/helpers/helpers";
import HeaderMenu from "../components/HeaderMenu.vue";
import { Document } from "@element-plus/icons-vue";
import HistoryRecord from "../components/modal/HistoryRecord.vue";
import { normalizeAmountList } from "@/lib/utils";
const historyRef = ref<any>(null);
const newWithdrawalRequestModalRef = ref<any>();
const TabStatus = WithdrawalStatusType;
const isLoadingAll = ref(false);
const isLoading = ref(false);
const isDealingLoading = ref(false);
const isSubmitting = ref(false);
provide("isLoading", isLoading);
const WithdrawInfoRef = ref<InstanceType<typeof WithdrawInfo>>();
const WithdrawLimitRef = ref<InstanceType<typeof WithdrawLimitInfo>>();
const commentsRef = ref<InstanceType<typeof CommentsView>>();
const exporting = ref(false);
const tab = ref<any>(TabStatus.Pending);
const action = ref(ActionType.Cancel);
const processTransaction = ref(() => Promise.resolve());
const headerMenuRef = ref<InstanceType<typeof HeaderMenu>>();
const data = ref(Array<any>());
const dataDealing = ref(<any>[]);

// 批量处理相关状态
const batchProcessModalRef = ref<InstanceType<typeof BatchProcessModal>>();
const selectedDealingIds = ref<number[]>([]);
const isAllDealingSelected = ref(false);
const isDealingIndeterminate = ref(false);

const criteria = ref<any>({
  page: 1,
  size: 10,
  stateId: TransactionStateType.WithdrawalCreated,
  sortField: "createdOn",
});

const dealingCriteria = ref<any>({
  page: 1,
  size: 10,
  stateId: TransactionStateType.WithdrawalTenantApproved,
  sortField: "createdOn",
});
const period = ref([] as any);

const changeTab = (_tab: any) => {
  tab.value = _tab;
  if (tab.value == TabStatus.All) {
    criteria.value.stateId = null;
  } else criteria.value.stateId = tab.value;
  fetchData(1);
  if (tab.value == TabStatus.Pending) {
    fetchDealingData(1);
  }
};

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  const res = await svc.queryWithdrawals(criteria.value);
  criteria.value = res.criteria;
  data.value = res.data;

  isLoading.value = false;
};

const fetchDealingData = async (_page: number) => {
  isDealingLoading.value = true;
  dealingCriteria.value = {
    ...criteria.value,
    stateId: TransactionStateType.WithdrawalTenantApproved,
  };
  dealingCriteria.value.page = _page;
  const res = await svc.queryWithdrawals(dealingCriteria.value);
  dealingCriteria.value = res.criteria;
  dataDealing.value = res.data;
  isDealingLoading.value = false;
};
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
  fetchData(1);
};
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
const formData = ref<CreateReportSpec>({
  name: "",
  type: ReportRequestTypes.WithdrawForTenant,
  query: criteria.value,
});

const submitReportRequest = async () => {
  exporting.value = true;
  formData.value.type =
    tab.value == TabStatus.Pending
      ? ReportRequestTypes.WithdrawPendingForTenant
      : ReportRequestTypes.WithdrawForTenant;

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

const pageChange = (page: number) => {
  fetchData(page);
};

const showWithdrawInfo = async (_item: any) => {
  WithdrawInfoRef.value?.show(_item);
};

const showWithdrawLImitInfo = async (_item: any) => {
  WithdrawLimitRef.value?.show(_item);
};

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};

const openConfirmBox = inject<any>(InjectKeys.OPEN_CONFIRM_MODAL);
const openRejectBox = inject(InjectKeys.OPEN_REJECT_REASON_MODAL);
const confirmCommentBoxRef = ref<InstanceType<typeof ConfirmCommentBox>>();

const openConfirmPanel = (
  _action: ActionType,
  id: number,
  paymentID?: number
) => {
  action.value = _action;
  const _handler = {
    [ActionType.CompletePayment]: () =>
      svc
        .completePaymentById(paymentID)
        .then(() => svc.completeWithdrawalByPaymentId(id)),
    [ActionType.StartExecution]: () => svc.executePaymentById(id),
    [ActionType.Refund]: () => svc.refundWithdrawById(id),
    [ActionType.Approve]: () =>
      svc.approveWithdrawalById(id, { comment: "N/A" }),
    [ActionType.Cancel]: () => svc.cancelWithdrawalById(id),
  }[_action];
  if (!_handler) {
    MsgPrompt.error("fail");
    return;
  }
  openConfirmBox(() =>
    _handler()
      .then(() => MsgPrompt.success())
      .then(() => {
        fecthAll();
      })
  );
};

const openApprovePanel = (id: number) => {
  confirmCommentBoxRef.value?.show?.((form: any) =>
    svc
      .approveWithdrawalById(id, { comment: form.comment })
      .then(() => MsgPrompt.success())
      .then(() => {
        fecthAll();
      })
  );
};

const openRejectPanel = (id: number) => {
  openRejectBox?.((form: any) =>
    svc
      .rejectWithdrawalById(id, { id, reason: form.reason })
      .then(() => MsgPrompt.success())
      .then(() => {
        fetchData(criteria.value.page);
      })
  );
};

const openRejectRestorePanel = (id: number) => {
  openConfirmBox?.(() =>
    svc
      .restoreRejectedWithdrawalById(id)
      .then(() => MsgPrompt.success())
      .then(() => {
        fetchData(criteria.value.page);
      })
  );
};

const openCancelRestorePanel = (id: number) => {
  openConfirmBox?.(() =>
    svc
      .restoreCanceledWithdrawalById(id)
      .then(() => MsgPrompt.success())
      .then(() => {
        fetchData(criteria.value.page);
      })
  );
};

const fecthAll = async () => {
  isLoadingAll.value = true;
  await fetchData(criteria.value.page);
  if (tab.value == TabStatus.Pending) {
    await fetchDealingData(dealingCriteria.value.page);
  }
  isLoadingAll.value = false;
};

// ==================== 批量处理相关方法 ====================

// 处理財務部門審核表格的全选（只选中 Pending 状态的项）
const handleSelectAllDealing = (val: boolean) => {
  dataDealing.value.forEach((item: any) => {
    if (
      item.payment.status === PaymentStatusTypes.Pending &&
      item.payment.paymentServiceName.includes("USDT")
    ) {
      item.selected = val;
    }
  });
  updateDealingSelection();
};

// 处理財務部門審核表格单行选择变化
const handleDealingSelectionChange = () => {
  updateDealingSelection();
};

// 更新財務部門審核表格选择状态（只计算 Pending 状态的项）
const updateDealingSelection = () => {
  const pendingItems = dataDealing.value.filter(
    (item: any) => item.payment.status === PaymentStatusTypes.Pending
  );
  const selectedItems = pendingItems.filter((item: any) => item.selected);
  selectedDealingIds.value = selectedItems.map((item: any) => item.id);

  const totalCount = pendingItems.length;
  const selectedCount = selectedItems.length;

  isAllDealingSelected.value = totalCount > 0 && selectedCount === totalCount;
  isDealingIndeterminate.value =
    selectedCount > 0 && selectedCount < totalCount;
};

// 点击批量处理按钮，打开批量处理 Modal
const handleBatchProcess = () => {
  if (selectedDealingIds.value.length === 0) return;

  // 只传递 Pending 状态且被选中的项
  const selectedItems = dataDealing.value.filter(
    (item: any) =>
      item.selected &&
      item.payment.status === PaymentStatusTypes.Pending &&
      item.payment.paymentServiceName.includes("USDT")
  );
  batchProcessModalRef.value?.show(selectedItems);
};

onMounted(async () => {
  await fecthAll();
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
</style>
