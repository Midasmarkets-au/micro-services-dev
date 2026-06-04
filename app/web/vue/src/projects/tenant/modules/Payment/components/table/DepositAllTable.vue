<template>
  <div class="card">
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th colspan="8"></th>
          </tr>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">{{ $t("fields.client") }}</th>
            <th class="">{{ $t("fields.status") }}</th>
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
            <th class="cell-color text-center">
              {{ $t("fields.operatedBy") }}
            </th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && deposits.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in deposits" :key="index">
            <td class="d-flex align-items-center">
              <UserInfo v-if="item.user" :user="item.user" class="me-2" />
            </td>
            <td class="">
              {{
                $t(`type.transactionState.${item.stateId}`).replace(
                  /^Deposit /,
                  ""
                )
              }}
            </td>
            <td class="">
              {{ $t(`type.paymentStatus.${item.payment.status}`) }}
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
                  [
                    PaymentPlatformTypes.WireDeposit,
                    PaymentPlatformTypes.WholeSaleWireDeposit,
                  ].includes(item.payment.paymentServiceId) && item.hasReceipt
                "
                href="#"
                class="btn btn-sm btn-light-primary fw-bold ms-2 fs-8 py-1 px-3"
                @click="viewWireReceipt(item.id)"
              >
                {{ $t("action.viewReceipt") }}
              </span>
              <span
                class="btn btn-sm btn-light-success fw-bold ms-2 fs-8 py-1 px-3"
                @click="viewPoliReference(item.id)"
                v-else-if="
                  item.payment.paymentServiceId == PaymentPlatformTypes.Poli
                "
              >
                {{ $t("action.viewReference") }}
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
import { ref, onMounted, watch, inject } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import { TransactionStateType } from "@/core/types/StateInfos";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { ActionType } from "@/core/types/Actions";
import BalanceShow from "@/components/BalanceShow.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import DepositReceipt from "@/projects/tenant/modules/Payment/modal/DepositReceipt.vue";
import DepositReference from "@/projects/tenant/modules/Payment/modal/DepositReference.vue";
import UploadDepositReceiptModal from "../../modal/UploadDepositReceiptModal.vue";
import {
  PaymentStatusTypes,
  PaymentPlatformTypes,
} from "@/core/types/PaymentTypes";
import { useRoute } from "vue-router";
import TenantGlobalInjectionKeys from "@/core/types/TenantGlobalInjectionKeys";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import PaymentInjectionKeys from "@/projects/tenant/modules/Payment/types/PaymentInjectionKeys";
import { isDateInDST_US } from "@/core/plugins/TimerService";
import moment from "moment";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const isLoading = ref(false);
const route = useRoute();
const commentsRef = ref<InstanceType<typeof CommentsView>>();

// current for WS use when routing to this page, see details in WSNotify.vue
watch(
  () => route.query.id,
  () => {
    if (route.query.id) {
      fetchData();
    }
  }
);

const refresher = inject<any>("refresher");
const topCriteria = inject(PaymentInjectionKeys.DEPOSIT_CRITERIA);
watch(refresher, () => {
  criteria.value = {
    ...topCriteria.value,
  };
  fetchData(1);
});

const deposits = ref(Array<any>());
const depositReferenceRef = ref<InstanceType<typeof DepositReference>>();

const openFilePreviewModal = inject<any>(
  TenantGlobalInjectionKeys.OPEN_FILE_MODAL
);
const viewWireReceipt = async (id) => {
  const res = await PaymentService.getDepositReceiptById(id);
  console.log(res);
  const media = {
    guid: res[0],
  };
  openFilePreviewModal?.(media);
};

const viewPoliReference = async (id) => {
  const res = await PaymentService.getDepositReferenceById(id);
  depositReferenceRef.value?.show(res);
};

const uploadDepositReceiptRef =
  ref<InstanceType<typeof UploadDepositReceiptModal>>();

const criteria = ref({
  page: 1,
  size: 10,
  SortField: "updatedOn",
});

onMounted(() => {
  fetchData(criteria.value.page);
});

const fetchData = async (_page?: number) => {
  if (_page) criteria.value.page = _page;

  isLoading.value = true;
  // await PaymentService.queryDepositsV2(criteria.value);
  const res = await PaymentService.queryDeposits(criteria.value);
  criteria.value = res.criteria;
  deposits.value = res.data;
  isLoading.value = false;
};

const pageChange = (page: number) => {
  fetchData(page);
};

const openUploadDepositReceiptPanel = (_id: number) => {
  uploadDepositReceiptRef.value?.show(_id);
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
