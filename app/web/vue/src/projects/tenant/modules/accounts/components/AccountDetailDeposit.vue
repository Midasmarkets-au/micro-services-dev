<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">{{ $t("title.accountDeposit") }}</div>
        <div class="card-toolbar">
          <button
            class="btn btn-sm btn-success fs-6"
            @click="openCreateDepositPanel"
          >
            <i class="fa-solid fa-plus fa-xl" style="color: #ffffff"></i>
            {{ $t("action.createDeposit") }}
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
              <th class="">
                {{ $t("fields.deposit") + " " + $t("fields.status") }}
              </th>
              <th class="">{{ $t("fields.paymentStatus") }}</th>
              <th class="">{{ $t("fields.paymentId") }}</th>
              <th class="">{{ $t("title.paymentMethod") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.amount") }}</th>
              <th class="text-center">{{ $t("fields.createdOn") }}</th>
              <th class="text-center">{{ $t("fields.operatedBy") }}</th>
              <th class="cell-color text-center">{{ $t("action.action") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && items.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold">
            <tr
              v-for="(item, index) in items"
              :key="index"
              :class="{
                'text-gray-400':
                  item.stateId === TransactionStateType.DepositCanceled,
              }"
            >
              <td>
                {{
                  $t(`type.transactionState.${item.stateId}`).replace(
                    /^Deposit\s+/,
                    ""
                  )
                }}
              </td>
              <td class="">
                <span
                  class="badge"
                  :class="{
                    'badge-primary':
                      item.payment.status === PaymentStatusTypes.Pending,
                    'badge-success':
                      item.payment.status === PaymentStatusTypes.Completed,
                    'badge-danger':
                      item.payment.status === PaymentStatusTypes.Rejected,
                    'badge-warning':
                      item.payment.status === PaymentStatusTypes.Executing,
                  }"
                  >{{ $t(`type.paymentStatus.${item.payment.status}`) }}</span
                >
              </td>
              <td class="">{{ item.payment.id }}</td>
              <td class="">
                {{ item.payment.paymentServiceName }}
              </td>
              <td class="">{{ $t(`type.currency.${item.currencyId}`) }}</td>
              <td class="">
                <BalanceShow
                  :currency-id="item.currencyId"
                  :balance="item.amount"
                />
              </td>

              <td class="text-center">
                <TimeShow :date-iso-string="item.createdOn" />
              </td>

              <td class="text-center">
                {{ item.operatorName }}
              </td>

              <td class="cell-color text-center">
                <el-dropdown trigger="click">
                  <el-button type="primary" class="btn btn-secondary btn-sm">
                    {{ $t("action.action")
                    }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                  </el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <template
                        v-if="
                          [
                            PaymentPlatformTypes.WireDeposit,
                            PaymentPlatformTypes.WholeSaleWireDeposit,
                          ].includes(item.payment.paymentServiceId)
                        "
                      >
                        <el-dropdown-item
                          v-if="item.hasReceipt"
                          @click="viewWireReceipt(item.id)"
                        >
                          {{ $t("action.viewReceipt") }}
                        </el-dropdown-item>

                        <el-dropdown-item
                          v-else
                          @click="openUploadDepositReceiptPanel(item.id)"
                        >
                          {{ $t("action.uploadReceipt") }}
                        </el-dropdown-item>
                      </template>

                      <el-dropdown-item
                        v-else-if="
                          PaymentPlatformTypes.Poli ==
                          item.payment.paymentServiceId
                        "
                        @click="viewPoliReference(item.id)"
                      >
                        {{ $t("action.viewReference") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        divided
                        v-if="
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

                      <el-dropdown-item
                        divided
                        v-if="
                          item.stateId ===
                          TransactionStateType.DepositPaymentCompleted
                        "
                        @click="openConfirmPanel(ActionType.Approve, item.id)"
                      >
                        {{ $t("action.approve") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        v-if="
                          $cans(['SuperAdmin']) &&
                          item.stateId ===
                            TransactionStateType.DepositPaymentCompleted
                        "
                        @click="openConfirmPanel(ActionType.Reject, item.id)"
                      >
                        {{ $t("action.reject") }}
                      </el-dropdown-item>

                      <el-dropdown-item
                        divided
                        v-if="
                          [
                            PaymentStatusTypes.Executing,
                            PaymentStatusTypes.Pending,
                          ].includes(item.payment.status)
                        "
                        @click="
                          openConfirmPanel(
                            ActionType.Complete,
                            item.id,
                            item.payment.id
                          )
                        "
                      >
                        {{ $t("action.cancel") }}
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </td>
            </tr>
          </tbody>
        </table>

        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
    <DepositReference ref="depositReferenceRef" />
    <CreateDepositForAccount ref="createDepositRef" @fetchData="fetchData" />
    <UploadDepositReceiptModal
      ref="uploadDepositReceiptRef"
      @fetchData="fetchData"
    />
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted, ref } from "vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import {
  PaymentPlatformTypes,
  PaymentStatusTypes,
} from "@/core/types/PaymentTypes";
import BalanceShow from "@/components/BalanceShow.vue";
import DepositReference from "@/projects/tenant/modules/Payment/modal/DepositReference.vue";
import UploadDepositReceiptModal from "@/projects/tenant/modules/Payment/modal/UploadDepositReceiptModal.vue";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { ActionType } from "@/core/types/Actions";
import { TransactionStateType } from "@/core/types/StateInfos";
import { ArrowDown } from "@element-plus/icons-vue";
import CreateDepositForAccount from "@/projects/tenant/modules/accounts/components/modal/CreateDepositForAccount.vue";
const props = defineProps<{
  accountDetails: any;
}>();

const isLoading = ref(false);
const isSubmitting = ref(false);

const items = ref(Array<any>());
const depositReferenceRef = ref<InstanceType<typeof DepositReference>>();
const createDepositRef = ref<InstanceType<typeof CreateDepositForAccount>>();
const uploadDepositReceiptRef =
  ref<InstanceType<typeof UploadDepositReceiptModal>>();
const criteria = ref({
  page: 1,
  size: 10,
  accountId: props.accountDetails.id,
});

const openFilePreviewModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_FILE_MODAL
);

const openConfirmBox = inject(TenantGlobalInjectionKeys.OPEN_CONFIRM_MODAL);
const fetchData = async (_page) => {
  criteria.value.page = _page;
  isLoading.value = true;
  try {
    const res = await PaymentService.queryDeposits(criteria.value);
    items.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    MsgPrompt.error(e);
  } finally {
    isLoading.value = false;
  }
};

const viewWireReceipt = async (id) => {
  const res = await PaymentService.getDepositReceiptById(id);
  const media = {
    guid: res[0],
  };
  openFilePreviewModal?.(media);
};

const viewPoliReference = async (id) => {
  const res = await PaymentService.getDepositReferenceById(id);
  depositReferenceRef.value?.show(res);
};

const openUploadDepositReceiptPanel = (id) => {
  uploadDepositReceiptRef.value?.show(id);
};

const openCreateDepositPanel = () => {
  createDepositRef.value?.show();
};

const openConfirmPanel = (
  _action: ActionType,
  id: number,
  paymentId?: number
) => {
  const _handler = {
    [ActionType.Approve]: () => PaymentService.approveDepositById(id),
    [ActionType.Reject]: () => PaymentService.rejectDepositById(id),
    [ActionType.Cancel]: () => PaymentService.cancelDepositById(id),
    [ActionType.CompletePayment]: async () => {
      if (!paymentId) {
        MsgPrompt.error("No PaymentId specified");
        return;
      }
      await PaymentService.completePaymentById(paymentId);
      await PaymentService.completeDepositByPaymentId(id);
    },
    [ActionType.Complete]: () => PaymentService.completeDepositByPaymentId(id),
  }[_action];
  if (!_handler) {
    MsgPrompt.error("No handler, action not supported");
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

onMounted(() => {
  fetchData(1);
});
</script>

<style scoped></style>
