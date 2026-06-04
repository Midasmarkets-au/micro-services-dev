<template>
  <div class="">
    <div class="card">
      <div class="card-body py-4">
        <table
          class="table align-middle table-row-dashed fs-6 gy-5"
          id="table_accounts_requests"
        >
          <thead>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th colspan="7"></th>
              <th colspan="2" class="text-center"></th>
            </tr>
            <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
              <th class="">{{ $t("fields.client") }}</th>
              <th class="">{{ $t("fields.status") }}</th>
              <th class="">{{ $t("fields.paymentStatus") }}</th>
              <th class="">{{ $t("fields.paymentID") }}</th>
              <th class="">{{ $t("fields.paymentMethod") }}</th>
              <th class="">{{ $t("fields.currency") }}</th>
              <th class="">{{ $t("fields.withdrawAmount") }}</th>
              <th class="">{{ $t("fields.sourceBalance") }}</th>
              <th class="text-center">
                {{ $t("fields.paymentInfo") }}
              </th>
              <th class="">{{ $t("fields.createdOn") }}</th>
              <th class="cell-color text-center">
                {{ $t("fields.operatedBy") }}
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
              <td>{{ $t(`type.transactionState.${item.stateId}`) }}</td>
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
                {{ item.payment.id
                }}<i
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
                    item.source.accountType === TransactionAccountType.Wallet
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
              <td class="text-center">
                <a
                  href="#"
                  class="btn btn-sm btn-light-info fw-bold ms-2 fs-8 py-1 px-3"
                  @click="showWithdrawInfo(item)"
                >
                  {{ $t("action.showPaymentInfo") }}
                </a>
              </td>
              <td class="">
                <TimeShow :date-iso-string="item.payment.createdOn" />
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

    <WithdrawInfo ref="WithdrawInfoRef" />
    <CommentsView ref="commentsRef" type="" id="0" />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted, watch, inject } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import svc from "../../services/PaymentService";
import {
  TransactionAccountType,
  TransactionStateType,
} from "@/core/types/StateInfos";
import ConfirmBox from "@/components/ConfirmBox.vue";
import { ActionType } from "@/core/types/Actions";
import WithdrawInfo from "../../modal/WithdrawInfo.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import { PaymentStatusTypes } from "@/core/types/PaymentTypes";
import { useRoute } from "vue-router";
import CommentsView from "@/projects/tenant/components/CommentView.vue";
import { CommentType } from "@/core/types/CommentType";
import PaymentInjectionKeys from "@/projects/tenant/modules/Payment/types/PaymentInjectionKeys";

const isLoading = ref(false);
const transactions = ref(Array<any>());
const WithdrawInfoRef = ref<InstanceType<typeof WithdrawInfo>>();
const commentsRef = ref<InstanceType<typeof CommentsView>>();

const criteria = ref({
  page: 1,
  size: 10,
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

const refresher = inject<any>("refresher");
const topCriteria = inject(PaymentInjectionKeys.WITHDRAWAL_CRITERIA);
watch(refresher, () => {
  criteria.value = {
    ...topCriteria.value,
  };
  fetchData(1);
});

onMounted(async () => {
  await fetchData(criteria.value.page);
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  isLoading.value = true;
  const res = await svc.queryWithdrawals(criteria.value);
  // const res = await svc.queryWithdrawsV2(criteria.value);
  criteria.value = res.criteria;
  transactions.value = res.data;
  isLoading.value = false;
};

const pageChange = (page: number) => {
  fetchData(page);
};

const showWithdrawInfo = async (_item: any) => {
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
