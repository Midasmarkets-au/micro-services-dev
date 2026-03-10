<template>
  <div class="card">
    <div class="card-header">
      <div v-if="tab == TransactionStateType.DepositCreated" class="card-title">
        財務部門 審核
      </div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.client") }}</th>
            <th class="">
              {{ $t("fields.paymentStatus") }}
            </th>
            <th class="">{{ $t("fields.paymentId") }}</th>
            <th class="">{{ $t("fields.paymentNo") }}</th>
            <th class="">{{ $t("title.paymentMethod") }}</th>
            <th class="">{{ $t("fields.currency") }}</th>
            <th class="">{{ $t("fields.amount") }}</th>
            <th class="">{{ $t("fields.tradeAccount") }}</th>

            <th class="text-center">{{ $t("fields.createdOn") }}</th>
            <th class="cell-color text-center">{{ $t("fields.receipt") }}</th>
            <th
              v-if="tab != TransactionStateType.DepositCreated"
              class="cell-color text-center"
            >
              {{ $t("fields.operatedBy") }}
            </th>
            <th
              class="cell-color text-center"
              v-if="tab == TransactionStateType.DepositCreated || tab == 0"
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
            <td class="d-flex align-items-center">
              <UserInfo v-if="item.user" :user="item.user" class="me-2" />
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
                    item.payment.status === PaymentStatusTypes.Failed,
                  'badge-warning':
                    item.payment.status === PaymentStatusTypes.Executing,
                }"
                >{{ $t(`type.paymentStatus.${item.payment.status}`) }}</span
              >
            </td>
            <td class="">
              {{ item.payment.id }}
            </td>
            <td
              class=""
              @click="
                viewComments(
                  CommentType.Deposit,
                  item.id,
                  item.payment.number.substring(3)
                )
              "
            >
              {{ item.payment.number.substring(3)
              }}<i
                v-if="item.hasComment"
                class="fa-regular fa-comment-dots text-primary"
              ></i>
            </td>
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
            <td class="">
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

            <td class="text-center">
              <TimeShow :date-iso-string="item.payment.createdOn" />
            </td>
            <td class="cell-color text-center">
              <span
                v-if="
                  item.payment.paymentServiceId == PaymentPlatformTypes.Poli
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
            <td
              v-if="item.stateId != TransactionStateType.DepositCreated"
              class="cell-color text-center"
            >
              {{ item.operatorName }}
            </td>
            <!-- BUTTON -->
            <td
              class="cell-color text-center"
              v-if="
                item.stateId == TransactionStateType.DepositCreated || tab == 0
              "
            >
              <!-- 財務審核 -->
              <button
                v-if="item.payment.status == PaymentStatusTypes.Executing"
                class="btn btn-light btn-info btn-sm me-3"
                @click="
                  openConfirmPanel(
                    ActionType.CompletePayment,
                    item.id,
                    item.payment.id
                  )
                "
              >
                {{ $t("action.completePayment") }}
              </button>
              <template
                v-if="
                  item.payment.status == PaymentStatusTypes.Executing ||
                  item.payment.status == PaymentStatusTypes.Pending ||
                  item.payment.status == PaymentStatusTypes.Failed
                "
              >
                <button
                  class="btn btn-light btn-light-danger btn-sm me-3"
                  @click="
                    openConfirmPanel(ActionType.Cancel, item.id, item.paymentId)
                  "
                >
                  {{ $t("action.cancel") }}
                </button>
              </template>
              <!-- Dealing 審核 -->
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>

  <DepositReference ref="depositReferenceRef" />

  <UploadDepositReceiptModal
    ref="uploadDepositReceiptRef"
    @fetchData="fetchData"
  />

  <CommentsView ref="commentsRef" type="" id="0" />
</template>
<script setup lang="ts">
import { ref, onMounted, watch, inject, computed } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import { TransactionStateType } from "@/core/types/StateInfos";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { ActionType } from "@/core/types/Actions";
import BalanceShow from "@/components/BalanceShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import DepositReference from "@/projects/tenant/modules/Payment/modal/DepositReference.vue";
import UploadDepositReceiptModal from "../../modal/UploadDepositReceiptModal.vue";
import {
  PaymentStatusTypes,
  PaymentPlatformTypes,
} from "@/core/types/PaymentTypes";
import { useRoute } from "vue-router";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import { ReportRequestTypes } from "@/core/types/ReportRequestTypes";
import { isDateInDST_US } from "@/core/plugins/TimerService";
import TenantGlobalService, {
  CreateReportSpec,
} from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import moment from "moment";

const props = defineProps<{
  tab: any;
  criteria: any;
}>();

const tab = computed(() => {
  return props.tab;
});
const criteria = computed(() => {
  return props.criteria;
});

const isLoading = ref(false);
const exporting = ref(false);
const route = useRoute();
const commentsRef = ref<InstanceType<typeof CommentsView>>();

const refresher = inject<any>("refresher");
watch(refresher, () => {
  fetchData(1);
});
const period = ref([] as any);

const defaultTime = ref<[Date, Date]>([
  new Date(2000, 1, 1, 0, 0, 0),
  new Date(2000, 2, 1, 23, 59, 59),
]);

// current for WS use when routing to this page, see details in WSNotify.vue
watch(
  () => route.query.id,
  () => {
    if (route.query.id) {
      fetchData();
    }
  }
);
watch(
  () => tab.value,
  () => {
    if (tab.value == 0) criteria.value.stateId = null;
    else criteria.value.stateId = tab.value;
    fetchData();
  }
);

const data = ref(Array<any>());
const confirmBoxRef = ref<InstanceType<typeof ConfirmBox>>();
const depositReferenceRef = ref<InstanceType<typeof DepositReference>>();

const uploadDepositReceiptRef =
  ref<InstanceType<typeof UploadDepositReceiptModal>>();

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

const viewPoliReference = async (id) => {
  const res = await PaymentService.getDepositReferenceById(id);
  depositReferenceRef.value?.show(res);
};

// const criteria = ref({
//   page: 1,
//   size: 10,
//   stateId: tab.value,
//   SortField: "updatedOn",
// });

onMounted(() => {
  fetchData(criteria.value.page);
});

const fetchData = async (_page?: number) => {
  if (_page) criteria.value.page = _page;

  isLoading.value = true;
  const res = await PaymentService.queryDeposits(criteria.value);
  criteria.value = res.criteria;
  data.value = res.data;
  isLoading.value = false;
};

const pageChange = (page: number) => {
  fetchData(page);
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
        PaymentService.completeDepositByPaymentId(id),
    }
      [_action]()
      .then(() => {
        fetchData(criteria.value.page);
        refresher.value = !refresher.value;
      });
  });
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
  const [from, to] = period.value;
  const isDST = isDateInDST_US();
  criteria.value.createdFrom = from
    ? moment(from).format(`YYYY-MM-DD[T]${isDST ? 21 : 22}:00:00.000[Z]`)
    : null;
  criteria.value.createdTo = to
    ? moment(to).format(`YYYY-MM-DD[T]${isDST ? 20 : 21}:59:59.000[Z]`)
    : null;

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
</script>

<style scoped>
.cell-color {
  background-color: rgb(255, 255, 224);
}
</style>
