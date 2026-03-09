<template>
  <div class="">
    <!-- <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.awaiting },
          ]"
          data-bs-toggle="tab"
          href="#application_pending"
          @click="changeTab(TabStatus.awaiting)"
          >{{ $t("status.awaiting") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.approved },
          ]"
          data-bs-toggle="tab"
          href="#application_approved"
          @click="changeTab(TabStatus.approved)"
          >{{ $t("status.approved") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.complete },
          ]"
          data-bs-toggle="tab"
          href="#application_approved"
          @click="changeTab(TabStatus.complete)"
          >{{ $t("status.completed") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.rejected },
          ]"
          data-kt-countup-tabs="true"
          data-bs-toggle="tab"
          href="#application_rejected"
          @click="changeTab(TabStatus.rejected)"
          >{{ $t("status.rejected") }}</a
        >
      </li>

      <li class="nav-item">
        <a
          :class="[
            'nav-link text-active-primary pb-4',
            { active: tab === TabStatus.All },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="changeTab(TabStatus.All)"
          >{{ $t("status.all") }}(Export)</a
        >
      </li>
    </ul> -->
    <HeaderMenu :tabs="TabStatus" @change-tab="changeTab" ref="headerMenuRef" />
    <div class="card mb-3">
      <div class="card-header">
        <div class="card-title">
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
            @change="fetchData(1)"
            class="w-200px ms-3"
            :placeholder="$t('fields.status')"
            clearable
          >
            <el-option :value="null" :label="$t('fields.all')" />
            <el-option
              v-for="status in TransferStateTypes"
              :key="status"
              :value="status"
              :label="$t(`type.transactionState.${status}`)"
            />
          </el-select>
          <el-input
            v-model="criteria.email"
            class="w-250px ms-3"
            :placeholder="$t('fields.email')"
            clearable
          >
          </el-input>
          <el-button plain type="primary" class="ms-3" @click="search">{{
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
        <div class="card-toolbar">
          <el-button @click="showAutoTransfer">Auto Transfer Setting</el-button>
        </div>
      </div>
    </div>

    <div class="card">
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
                <th class="">{{ $t("fields.client") }}</th>
                <th class="">{{ $t("fields.id") }}</th>
                <th v-if="tab === TabStatus.All" class="">
                  {{ $t("fields.status") }}
                </th>
                <th class="">{{ $t("fields.source") }}</th>
                <th class="">{{ $t("fields.target") }}</th>
                <th class="">{{ $t("fields.amount") }}</th>
                <th class="">{{ $t("fields.createdOn") }}</th>
                <th>
                  {{ $t("fields.operatedBy") }}
                </th>
                <th
                  class="text-center min-w-150px"
                  v-if="tab == TabStatus.awaiting"
                >
                  {{ $t("action.action") }}
                </th>
              </tr>
            </thead>

            <tbody v-if="isLoading">
              <LoadingRing />
            </tbody>
            <tbody v-else-if="!isLoading && transactions.length === 0">
              <NoDataBox />
            </tbody>

            <tbody v-else class="fw-semibold text-gray-900">
              <tr v-for="(item, index) in transactions" :key="index">
                <td class="d-flex align-items-center">
                  <UserInfo v-if="item.user" :user="item.user" class="me-2" />
                </td>
                <td class="">
                  <span
                    @click="
                      viewComments(CommentType.Transfer, item.id, item.id)
                    "
                    >{{ item.id }}
                    <i
                      v-if="item.hasComment"
                      class="fa-regular fa-comment-dots text-primary"
                    ></i
                  ></span>
                </td>
                <td v-if="tab === TabStatus.All">
                  {{ $t(`type.transactionState.${item.stateId}`) }}
                </td>
                <td class="">
                  <div class="d-flex align-items-center gap-2">
                    <span>{{
                      $t(`type.transactionAccount.${item.sourceAccountType}`) +
                      ": "
                    }}</span>
                    <span
                      @click="
                        viewComments(
                          CommentType.Account,
                          item.sourceAccountId,
                          item.sourceAccountNumber
                        )
                      "
                    >
                      {{
                        item.sourceAccountType === TransactionAccountType.Wallet
                          ? $t(`type.currency.840`)
                          : item.sourceAccountNumber
                      }}<i
                        v-if="item.sourceAccountHasComment"
                        class="fa-regular fa-comment-dots text-primary ms-1"
                      ></i>
                    </span>

                    <el-button
                      :icon="Document"
                      circle
                      @click="historyRef.show(item.id)"
                    />
                  </div>
                  <div>
                    <BalanceShow
                      class="fw-bold"
                      :balance="item.sourceAccountBalanceInCents"
                      :currency-id="
                        item.sourceAccountType === TransactionAccountType.Wallet
                          ? 840
                          : item.currencyId
                      "
                    />
                  </div>
                </td>

                <td class="">
                  <div>
                    <span>{{
                      $t(`type.transactionAccount.${item.targetAccountType}`) +
                      " "
                    }}</span>
                    <span
                      @click="
                        viewComments(
                          CommentType.Account,
                          item.targetAccountId,
                          item.targetAccountNumber
                        )
                      "
                      >{{
                        item.targetAccountType ===
                        TransactionAccountType.TradeAccount
                          ? item.targetAccountNumber
                          : $t(`type.currency.${item.currencyId}`)
                      }}<i
                        v-if="item.targetAccountHasComment"
                        class="fa-regular fa-comment-dots text-primary ms-1"
                      ></i
                    ></span>
                  </div>
                  <div>
                    <BalanceShow
                      class="fw-bold"
                      :balance="item.targetAccountBalanceInCents"
                      :currency-id="item.currencyId"
                    />
                  </div>
                </td>

                <td class="">
                  <BalanceShow
                    class="fw-bold"
                    :balance="item.amount"
                    :currency-id="item.currencyId"
                  />
                </td>

                <td><TimeShow :date-iso-string="item.createdOn" /></td>
                <td>{{ item.operatorName }}</td>
                <td class="text-center">
                  <button
                    v-if="tab === TabStatus.awaiting"
                    :disabled="item.sourceAccountBalanceInCents < item.amount"
                    class="btn btn-light btn-primary btn-sm me-3"
                    @click="openConfirmPanel(ActionType.Approve, item.id)"
                  >
                    <template
                      v-if="item.sourceAccountBalanceInCents < item.amount"
                      >{{ $t("status.notAllowed") }}</template
                    >
                    <template v-else>{{ $t("action.approve") }}</template>
                  </button>

                  <button
                    v-if="tab === TabStatus.approved"
                    :disabled="item.sourceAccountBalanceInCents < item.amount"
                    class="btn btn-light btn-info btn-sm me-3"
                    @click="openConfirmPanel(ActionType.Complete, item.id)"
                  >
                    {{ $t("action.complete") }}
                  </button>

                  <button
                    v-if="tab === TabStatus.awaiting"
                    class="btn btn-light btn-danger btn-sm"
                    @click="openConfirmPanel(ActionType.Reject, item.id)"
                  >
                    {{ $t("action.reject") }}
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>
  </div>
  <HistoryRecord ref="historyRef" />
  <UserDetails ref="userDetailRef"></UserDetails>
  <CommentsView ref="commentsRef" type="" id="0" />
  <AutoTransferSetting ref="autoTransferRef" />
</template>
<script setup lang="ts">
import { ref, onMounted, inject, watch, provide } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import AutoTransferSetting from "@/projects/tenant/modules/Payment/components/modal/AutoTransferSetting.vue";
import svc from "../services/PaymentService";
import TimeShow from "@/components/TimeShow.vue";
import {
  TransactionAccountType,
  TransactionStateType,
  TransferStateTypes,
} from "@/core/types/StateInfos";
import { ActionType } from "@/core/types/Actions";
import BalanceShow from "@/components/BalanceShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import UserDetails from "@/projects/tenant/components/UserDetails.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import {
  convertTradeTime,
  handleCriteriaTradeTime,
} from "@/core/helpers/helpers";
import HeaderMenu from "../components/HeaderMenu.vue";
import { Document } from "@element-plus/icons-vue";
import HistoryRecord from "../components/modal/HistoryRecord.vue";

const historyRef = ref<any>(null);
const isLoading = ref(false);
provide("isLoading", isLoading);
const commentsRef = ref<InstanceType<typeof CommentsView>>();
const autoTransferRef = ref(null);
enum TabStatus {
  awaiting = 1,
  approved = 2,
  rejected = 3,
  completed = 4,
  All = 0,
}

const tab = ref(TabStatus.awaiting);
const transactions = ref(Array<any>());
const userDetailRef = ref<InstanceType<typeof UserDetails>>();
const headerMenuRef = ref<InstanceType<typeof HeaderMenu>>();
const criteria = ref<any>({
  page: 1,
  size: 10,
  stateId: TransactionStateType.TransferAwaitingApproval,
});

onMounted(async () => {
  await fetchData(criteria.value.page);
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  const res = await svc.queryTransactions(criteria.value);
  transactions.value = res.data.map((item) => ({
    ...item,
    user: {
      partyId: item.partyId,
      ...item.user,
    },
  }));
  criteria.value = res.criteria;
  isLoading.value = false;
};

const changeTab = (_tab: TabStatus) => {
  if (tab.value === _tab) return;
  tab.value = _tab;
  criteria.value.stateId = {
    [TabStatus.awaiting]: TransactionStateType.TransferAwaitingApproval,
    [TabStatus.approved]: TransactionStateType.TransferApproved,
    [TabStatus.rejected]: TransactionStateType.TransferRejected,
    [TabStatus.completed]: TransactionStateType.TransferCompleted,
  }[_tab];
  fetchData(criteria.value.page);
};

const pageChange = (page: number) => {
  fetchData(page);
};

const showAutoTransfer = () => {
  autoTransferRef.value?.show();
};

const openConfirmModel = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

const openConfirmPanel = (_action: ActionType, id: number) => {
  const handler = {
    [ActionType.Approve]: () => svc.approveTransactionById(id),
    [ActionType.Reject]: () => svc.rejectTransactionById(id),
    [ActionType.Cancel]: () => svc.cancelTransactionById(id),
    [ActionType.Complete]: () => svc.completeTransactionById(id),
  }[_action];

  if (!handler) {
    MsgPrompt.error("Invalid action");
    return;
  }
  openConfirmModel?.(() =>
    handler()
      .then(() => MsgPrompt.success("Success"))
      .then(() => fetchData(criteria.value.page))
  );
};

const period = ref([] as any);

watch(
  () => period.value,
  (periodVal) => {
    handleCriteriaTradeTime(periodVal, criteria);
  }
);
const search = () => {
  headerMenuRef.value?.changeTab(TabStatus.All);
  fetchData(1);
};
const clearSearchFilterCriteria = () => {
  criteria.value = { page: 1, size: 10 };
  criteria.value.stateId = {
    [TabStatus.awaiting]: TransactionStateType.TransferAwaitingApproval,
    [TabStatus.approved]: TransactionStateType.TransferApproved,
    [TabStatus.rejected]: TransactionStateType.TransferRejected,
    [TabStatus.complete]: TransactionStateType.TransferCompleted,
  }[tab.value];
  period.value = [];
  fetchData(1);
};
const exporting = ref(false);

const formData = ref<CreateReportSpec>({
  name: "",
  type: ReportRequestTypes.TransactionForTenant,
  query: criteria.value,
});
const submitReportRequest = async () => {
  exporting.value = true;
  const [from, to] = period.value ?? [null, null];

  const [createdFrom, createdTo] = convertTradeTime(from, to);

  formData.value.query = { ...criteria.value };
  formData.value.query.from = createdFrom;
  formData.value.query.to = createdTo;
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
const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};
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
</style>
