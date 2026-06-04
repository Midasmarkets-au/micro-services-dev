<template>
  <div class="">
    <div class="card">
      <div class="card-header">
        <div class="card-title">Dealing 審核</div>
      </div>
      <div class="card-body py-4">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.client") }}</th>
              <th class="">{{ $t("fields.paymentStatus") }}</th>
              <th class="">{{ $t("fields.paymentID") }}</th>
              <th class="">{{ $t("fields.paymentMethod") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.withdrawAmount") }}</th>
              <th class="">{{ $t("fields.sourceBalance") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="text-center cell-color">
                {{ $t("fields.paymentInfo") }}
              </th>
              <th class="cell-color text-center">
                {{ $t("action.action") }}
              </th>
              <th class="cell-color text-center">
                {{ $t("action.cancel") }}
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
              <td>
                <span
                  class="badge"
                  :class="{
                    'badge-primary':
                      item.payment.status === PaymentStatusTypes.Pending,
                    'badge-success':
                      item.payment.status === PaymentStatusTypes.Completed,
                    'badge-danger':
                      item.payment.status === PaymentStatusTypes.Failed,
                    'badge-warning':
                      item.payment.status === PaymentStatusTypes.Executing,
                  }"
                  >{{ $t(`type.paymentStatus.${item.payment.status}`) }}</span
                >
              </td>
              <td
                class=""
                @click="
                  viewComments(
                    CommentType.Withdrawal,
                    item.id,
                    item.payment.number.substring(3)
                  )
                "
              >
                {{ item.payment.id }}
                <i
                  v-if="item.hasComment"
                  class="fa-regular fa-comment-dots text-primary"
                ></i>
              </td>
              <td class="">
                {{ item.payment.paymentServiceName }}
              </td>
              <td class="">
                {{ $t(`type.currency.${item.currencyId}`) }}
              </td>
              <td class="">
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.amount"
                />
              </td>
              <td class="">
                <div>
                  <BalanceShow
                    :currency-id="item.currencyId"
                    :balance="item.source.balanceInCents"
                  />
                </div>
                <div
                  v-if="
                    item.source.accountType === TransactionAccountTypes.Wallet
                  "
                  class="badge badge-primary"
                >
                  Wallet # {{ item.source.displayNumber }}
                </div>

                <div
                  v-if="
                    item.source.accountType ===
                    TransactionAccountTypes.TradeAccount
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
              <td class="">
                <TimeShow :date-iso-string="item.payment.createdOn" />
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
              <td class="text-center cell-color">
                <button
                  :disabled="item.source.balanceInCents < item.amount"
                  class="btn btn-light btn-primary btn-sm me-3"
                  @click="
                    item.source.accountType === TransactionAccountTypes.Wallet
                      ? openConfirmPanel(ActionType.Approve, item.id)
                      : openApprovePanel(item.id)
                  "
                >
                  <template v-if="item.source.balanceInCents < item.amount">{{
                    $t("status.notAllowed")
                  }}</template>
                  <template v-else>{{ $t("action.approve") }}</template>
                </button>

                <button
                  href="#"
                  class="btn btn-light btn-danger btn-sm"
                  @click="openRejectPanel(item.id)"
                >
                  {{ $t("action.reject") }}
                </button>
              </td>
              <td class="text-center cell-color">
                <button
                  href="#"
                  class="btn btn-light btn-light-danger btn-sm"
                  @click="openConfirmPanel(ActionType.Cancel, item.id)"
                >
                  {{ $t("action.cancel") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>
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
    <WithdrawInfo ref="WithdrawInfoRef" />
    <CommentsView ref="commentsRef" type="" id="0" />
    <ConfirmCommentBox
      ref="confirmCommentBoxRef"
      :hasComment="true"
    ></ConfirmCommentBox>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import svc from "../../services/PaymentService";
import { TransactionStateType } from "@/core/types/StateInfos";
import ConfirmBox from "@/components/ConfirmBox.vue";
import ConfirmCommentBox from "@/components/ConfirmCommentBox.vue";
import { ActionType } from "@/core/types/Actions";
import WithdrawInfo from "../../modal/WithdrawInfo.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { PaymentStatusTypes } from "@/core/types/PaymentTypes";
import { useRoute } from "vue-router";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import { TransactionAccountTypes } from "@/core/types/AccountInfos";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import PaymentInjectionKeys from "@/projects/tenant/modules/Payment/types/PaymentInjectionKeys";

const isLoading = ref(false);
const isSubmitting = ref(false);
const { t } = useI18n();
const transactions = ref(Array<any>());
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const confirmCommentBoxRef = ref<InstanceType<typeof ConfirmCommentBox>>();
const WithdrawInfoRef = ref<InstanceType<typeof WithdrawInfo>>();
const commentsRef = ref<InstanceType<typeof CommentsView>>();

const criteria = ref({
  page: 1,
  size: 10,
  stateId: TransactionStateType.WithdrawalCreated,
});

const route = useRoute();
watch(
  () => route.query.id,
  () => {
    if (route.query.id) {
      fetchData(1);
    }
  }
);

onMounted(async () => {
  await fetchData(criteria.value.page);
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  // const res = await svc.queryWithdrawals(criteria.value);
  const res = await svc.queryWithdrawals(criteria.value);
  criteria.value = res.criteria;
  transactions.value = res.data;
  isLoading.value = false;
};

const pageChange = (page: number) => {
  fetchData(page);
};

const processTransaction = ref(() => Promise.resolve());
const action = ref(ActionType.Cancel);
const openConfirmBox = inject(InjectKeys.OPEN_CONFIRM_MODAL);
const openRejectBox = inject(InjectKeys.OPEN_REJECT_REASON_MODAL);
const refresher = inject<any>("refresher");
const topCriteria = inject(PaymentInjectionKeys.WITHDRAWAL_CRITERIA);
watch(refresher, () => {
  criteria.value = {
    ...topCriteria.value,
    stateId: TransactionStateType.WithdrawalCreated,
  };
  fetchData(1);
});

const openConfirmPanel = (_action: ActionType, id: number) => {
  action.value = _action;
  const _handler = {
    [ActionType.Approve]: () =>
      svc.approveWithdrawalById(id, { comment: "N/A" }),
    [ActionType.Cancel]: () => svc.cancelWithdrawalById(id),
  }[_action];
  if (!_handler) {
    MsgPrompt.error(t("tip.fail"));
    return;
  }
  openConfirmBox?.(() =>
    _handler()
      .then(() => MsgPrompt.success())
      .then(() => {
        fetchData(criteria.value.page);
        refresher.value = !refresher.value;
      })
  );
};

const openApprovePanel = (id: number) => {
  confirmCommentBoxRef.value?.show?.((form: any) =>
    svc
      .approveWithdrawalById(id, { comment: form.comment })
      .then(() => MsgPrompt.success())
      .then(() => {
        fetchData(criteria.value.page);
        refresher.value = !refresher.value;
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
        refresher.value = !refresher.value;
      })
  );
};

const showWithdrawInfo = async (_item: any) => {
  // const data = await processWithdrawInfoData(_item);
  WithdrawInfoRef.value?.show(_item);
};

const viewComments = (type: CommentType, id: number, title: string) => {
  commentsRef.value?.show(type, id, title);
};
</script>

<style scoped>
.cell-color {
  background-color: rgb(255, 255, 224);
}
</style>
