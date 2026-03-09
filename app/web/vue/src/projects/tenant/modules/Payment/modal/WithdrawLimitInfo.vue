<template>
  <SimpleForm
    ref="formRef"
    :is-loading="isLoading"
    :title="$t('fields.fundStatistics')"
    :save-title="$t('action.copy')"
    :submit="copyPaymentInfo"
    :minWidth="700"
    :discard-title="$t('action.close')"
  >
    <LoadingCentralBox class="h-100" v-if="isLoading" />
    <div v-else class="h-100">
      <div class="" ref="copyTable" style="position: relative">
        <table>
          <thead>
            <tr>
              <td colspan="5" style="text-align: center; font-weight: bold">
                <UserInfo
                  v-if="wireForm.user"
                  :user="wireForm.user"
                  class="me-2"
                />
              </td>
            </tr>
            <tr>
              <td style="text-align: center; font-weight: bold">
                {{ $t("fields.transactionType") }}
              </td>
              <td style="text-align: center; font-weight: bold">
                {{ $t("title.paymentMethod") }}
              </td>
              <td style="text-align: right; font-weight: bold">
                {{ $t("fields.totalDeposits") }}
              </td>
              <td style="text-align: right; font-weight: bold">
                {{ $t("fields.Withdrawals") }}
              </td>
              <td style="text-align: right; font-weight: bold">
                {{ $t("fields.WithdrawLimit") }}
              </td>
              <td
                style="
                  text-align: center;
                  white-space: nowrap;
                  font-weight: bold;
                "
              >
                {{ $t("fields.action") }}
              </td>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(acc, idx) in wireForm.depositWidthDrawSummary"
              :key="idx"
            >
              <td style="text-align: center">
                {{
                  acc.type === "Deposit"
                    ? $t("type.title.deposit")
                    : acc.type === "Withdrawal"
                    ? $t("type.title.withdrawal")
                    : ""
                }}
              </td>
              <td style="text-align: center">
                <!-- {{ $t(`type.fundType.${acc.fundType}`) }} -->
                {{ acc.paymentGroupName }}
              </td>
              <td style="text-align: right">
                <BalanceShow
                  :currency-id="acc.currencyId"
                  :balance="acc.depositSumUsd"
                />
              </td>
              <td style="text-align: right">
                <BalanceShow
                  :currency-id="acc.currencyId"
                  :balance="acc.withdrawSumUsd"
                />
              </td>
              <td style="text-align: right">
                <BalanceShow
                  :currency-id="acc.currencyId"
                  :balance="acc.needWithdrawAmountUsd"
                />
              </td>
              <td style="text-align: center">
                <button
                  class="btn btn-bordered btn-sm btn-radius btn-primary me-3"
                  @click="showDetailInfo(acc)"
                >
                  {{ $t("title.details") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </SimpleForm>
  <WithdrawLimitDetail ref="WithdrawLimitDetailRef" />
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import { useI18n } from "vue-i18n";
import Clipboard from "clipboard";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BalanceShow from "@/components/BalanceShow.vue";
import { useStore } from "@/store";
import TimeShow from "@/components/TimeShow.vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import WithdrawLimitDetail from "./WithdrawLimitDetail.vue";
const WithdrawLimitDetailRef = ref<InstanceType<typeof WithdrawLimitDetail>>();
const wireForm = ref<any>({});
const isLoading = ref(true);
const formRef = ref<InstanceType<typeof SimpleForm> | null>(null);
const store = useStore();
const t = useI18n().t;
const showDetailInfo = async (_item: any) => {
  console.log("item", _item);
  WithdrawLimitDetailRef.value?.show(_item);
};
const show = async (_item: any) => {
  isLoading.value = true;
  formRef.value?.show();
  const res = await PaymentService.getDepositwithdrawSummary(_item.partyId);
  wireForm.value = {
    user: _item.user,
    depositWidthDrawSummary: res,
  };
  isLoading.value = false;
};

const copyTable = ref<HTMLDivElement | null>(null);

const copyPaymentInfo = async () => {
  if (copyTable.value) {
    Clipboard.copy(copyTable.value);
    MsgPrompt.success("Copy success");
    return;
  }
  MsgPrompt.error("Copy failed");
};

const hide = () => {
  formRef.value?.hide();
};

defineExpose({
  hide,
  show,
});
</script>
<style scoped>
table {
  left: -10000px;
  top: -10000px;
  font-family: arial, sans-serif;
  border-collapse: collapse;
  width: 100%;
  border: 1px solid #808080;
}

td,
th {
  border: 1px solid #808080;
  text-align: left;
  padding: 8px;
}

tr:nth-child(even) {
  background-color: #f5f5f5;
}
</style>
