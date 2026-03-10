<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">{{ $t("title.withdrawal") }}</div>
        <div class="card-toolbar">
          <button
            class="btn btn-sm btn-success fs-6"
            @click="openCreateWithdrawalPanel"
          >
            <i class="fa-solid fa-plus fa-xl" style="color: #ffffff"></i>
            {{ $t("action.createWithdrawal") }}
          </button>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.paymentStatus") }}</th>
              <th class="">{{ $t("fields.paymentID") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.withdrawAmount") }}</th>
              <th class="">{{ $t("fields.sourceBalance") }}</th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="text-center">{{ $t("fields.operatedBy") }}</th>
              <th class="cell-color text-center">{{ $t("action.action") }}</th>
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
              <td class="">
                {{
                  $t(`type.transactionState.${item.stateId}`).replace(
                    /^Withdrawal\s+/,
                    ""
                  )
                }}
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
              <td class="">{{ item.id }}</td>
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
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.source.balanceInCents"
                />
                <div
                  v-if="
                    item.source.accountType === TransactionAccountType.Wallet
                  "
                  class="badge badge-primary"
                >
                  Wallet
                </div>
                <div
                  v-if="
                    item.source.accountType ===
                    TransactionAccountType.TradeAccount
                  "
                  class="badge badge-warning"
                >
                  Account ({{ item.source.displayNumber }})
                </div>
              </td>
              <td class="">
                <TimeShow :date-iso-string="item.payment.createdOn" />
              </td>
              <td class="text-center">
                {{ item.operatorName }}
              </td>

              <td class="text-center cell-color">
                <el-dropdown trigger="click">
                  <el-button type="primary" class="btn btn-secondary btn-sm">
                    {{ $t("action.action")
                    }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item @click="showWithdrawInfo(item)">
                        {{ $t("action.showPaymentInfo") }}
                      </el-dropdown-item>
                      <template
                        v-if="
                          item.stateId ===
                          TransactionStateType.WithdrawalCreated
                        "
                      >
                        <el-dropdown-item
                          divided
                          @click="openConfirmPanel(ActionType.Approve, item.id)"
                        >
                          {{ $t("action.approve") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          @click="openConfirmPanel(ActionType.Reject, item.id)"
                        >
                          {{ $t("action.reject") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          divided
                          @click="openConfirmPanel(ActionType.Cancel, item.id)"
                        >
                          {{ $t("action.cancel") }}
                        </el-dropdown-item>
                      </template>
                      <template
                        v-if="
                          item.stateId ===
                          TransactionStateType.WithdrawalTenantApproved
                        "
                      >
                        <el-dropdown-item
                          divided
                          v-if="
                            item.payment.status === PaymentStatusTypes.Pending
                          "
                          @click="
                            openConfirmPanel(
                              ActionType.StartExecution,
                              item.payment.id
                            )
                          "
                        >
                          {{ $t("action.startProcessing") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          divided
                          v-else-if="
                            item.payment.status === PaymentStatusTypes.Executing
                          "
                          @click="
                            openConfirmPanel(
                              ActionType.CompletePayment,
                              item.id,
                              item.payment.id
                            )
                          "
                        >
                          {{ $t("action.completePayment") }}
                        </el-dropdown-item>
                      </template>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="pageChange" :criteria="criteria" />
      </div>
    </div>
    <WithdrawInfo ref="withdrawInfoRef" />
    <CreateWithdrawalForAccount
      ref="createWithdrawalForAccountRef"
      @fetchData="fetchData"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import {
  TransactionAccountType,
  TransactionStateType,
} from "@/core/types/StateInfos";
import { ActionType } from "@/core/types/Actions";
import WithdrawInfo from "@/projects/tenant/modules/Payment/modal/WithdrawInfo.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { PaymentStatusTypes } from "@/core/types/PaymentTypes";
import { useRoute } from "vue-router";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";
import CreateWithdrawalForAccount from "@/projects/tenant/modules/accounts/components/modal/CreateWithdrawalForAccount.vue";
import { ArrowDown } from "@element-plus/icons-vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import svc from "@/projects/tenant/modules/Payment/services/PaymentService";

const isLoading = ref(false);
const { t } = useI18n();
const transactions = ref(Array<any>());
const withdrawInfoRef = ref<InstanceType<typeof WithdrawInfo>>();
const createWithdrawalForAccountRef =
  ref<InstanceType<typeof CreateWithdrawalForAccount>>();

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const criteria = ref({
  page: 1,
  size: 10,
  accountId: accountDetails.value?.id,
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
  const res = await PaymentService.queryWithdrawals(criteria.value);
  criteria.value = res.criteria;
  transactions.value = res.data;
  isLoading.value = false;
};

const pageChange = (page: number) => {
  fetchData(page);
};

const action = ref(ActionType.Cancel);
const openConfirmBox = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);

const openConfirmPanel = (
  _action: ActionType,
  id: number,
  paymentId?: number
) => {
  action.value = _action;
  const _handler = {
    [ActionType.Approve]: () => PaymentService.approveWithdrawalById(id),
    [ActionType.Reject]: () => PaymentService.rejectWithdrawalById(id),
    [ActionType.Cancel]: () => PaymentService.cancelWithdrawalById(id),
    [ActionType.StartExecution]: () => PaymentService.executePaymentById(id),
    [ActionType.CompletePayment]: async () => {
      if (!paymentId) {
        MsgPrompt.error("No PaymentId specified");
        return;
      }
      await PaymentService.completePaymentById(paymentId);
      await PaymentService.completeWithdrawalByPaymentId(id);
    },
  }[_action];
  if (!_handler) {
    MsgPrompt.error(t("tip.fail"));
    return;
  }
  openConfirmBox?.(
    () =>
      _handler()
        .then(() => MsgPrompt.success())
        .then(() => fetchData(criteria.value.page)),
    void 0,
    {
      confirmColor: _action === ActionType.Reject ? "danger" : "primary",
    }
  );
};
const showWithdrawInfo = async (_item: any) => {
  withdrawInfoRef.value?.show(_item);
};

const openCreateWithdrawalPanel = () => {
  createWithdrawalForAccountRef.value?.show();
};
</script>

<style scoped></style>
