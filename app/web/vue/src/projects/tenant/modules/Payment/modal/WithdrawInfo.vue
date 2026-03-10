<template>
  <SimpleForm
    ref="formRef"
    :is-loading="isLoading"
    :title="$t('fields.paymentInfo')"
    :save-title="$t('action.copy')"
    :submit="copyPaymentInfo"
    :discard-title="$t('action.close')"
    :min-height="775"
  >
    <LoadingCentralBox class="h-100" v-if="isLoading" />
    <div v-else class="p-9 h-100">
      <div class="d-flex justify-content-end mb-3">
        <button
          class="btn btn-light btn-danger btn-sm"
          @click="openConfirmPanel()"
          :disabled="isLoading"
        >
          Create OFA
        </button>
      </div>
      <!--      style="width: 0; height: 0; overflow: hidden"   -->
      <div class="" ref="copyTable" style="position: relative; height: 720px">
        <!--        <div style="position: absolute; width: 100%">-->
        <div style="position: absolute; width: 100%; top: -10000px">
          Dear Tony: <br />
          This is {{ wireForm.user.name }}({{ wireForm.user.nativeName }})'s
          Withdrawal request form, the system has been updated. Please find the
          attached file. <br />
          <b
            >Withdrawal Amount (Platform):
            <BalanceShow
              :balance="wireForm.amount"
              :currency-id="wireForm.fromCurrencyId"
            />
            <br />
            Rate: {{ wireForm.exchangeRate }}<br />
            <span style="color: red"
              >Withdrawal Amount (Actual):
              <BalanceShow
                :balance="wireForm.amount * wireForm.exchangeRate"
                :currency-id="wireForm.toCurrencyId"
            /></span>
          </b>
          <br />
          <br />
          Please check it. Thanks.
          <br />
          <br />
          Regards,<br />
          {{ user.name }}
          <br />
          <br />
        </div>
        <div style="position: absolute; width: 100%">
          <!--        <div style="position: absolute; width: 100%; top: 200px">-->
          <table>
            <tr>
              <td colspan="2" style="text-align: center; font-weight: bold">
                {{ $t("title.withdrawInfo") }}
              </td>
            </tr>
            <tr>
              <td>{{ $t("fields.createdOn") }}</td>
              <td><TimeShow :date-iso-string="wireForm.createdOn" /></td>
            </tr>
            <tr>
              <td>{{ $t("fields.status") }}</td>
              <td>
                {{ $t(`type.paymentStatus.${wireForm.status}`) }}
              </td>
            </tr>
            <tr>
              <td>{{ $t("fields.refId") }}</td>
              <td>{{ wireForm.refId }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.nativeName") }}</td>
              <td>{{ wireForm.user.nativeName }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.email") }}</td>
              <td>{{ wireForm.user.email }}</td>
            </tr>

            <template
              v-if="
                wireForm.source.accountType ===
                TransactionAccountTypes.TradeAccount
              "
            >
              <tr>
                <td>{{ $t("fields.accountNumber") }}</td>
                <td>{{ wireForm.source.displayNumber }}</td>
              </tr>

              <tr>
                <td>{{ $t("fields.salesCode") }}</td>
                <td>{{ wireForm.accountInfo.code }}</td>
              </tr>

              <tr>
                <td>{{ $t("fields.ibGroup") }}</td>
                <td>{{ wireForm.accountInfo.group }}</td>
              </tr>
            </template>
            <tr>
              <td>
                <span
                  v-if="
                    wireForm.source.accountType ===
                    TransactionAccountTypes.Wallet
                  "
                >
                  {{ $t("fields.walletBalance") }}
                </span>
                <span
                  v-else-if="
                    wireForm.source.accountType ===
                    TransactionAccountTypes.TradeAccount
                  "
                >
                  {{ $t("fields.accountBalance") }}
                </span>
              </td>
              <td>
                <BalanceShow
                  :currency-id="wireForm.user.currency"
                  :balance="wireForm.user.balanceInCents"
                />

                ( {{ $t("type.currency." + wireForm.user.currency) }} )
              </td>
            </tr>

            <tr>
              <td>
                {{ $t("fields.withdrawAmount") }}
              </td>
              <td>
                <BalanceShow
                  :currency-id="wireForm.fromCurrencyId"
                  :balance="wireForm.amount"
                />
                ( {{ $t("type.currency." + wireForm.fromCurrencyId) }} )
              </td>
            </tr>
            <tr>
              <td>
                {{ $t("fields.exchangeRate") }}
              </td>
              <td>{{ wireForm.exchangeRate }}</td>
            </tr>
            <tr>
              <td>
                {{ $t("fields.actualAmount") }}
              </td>
              <td>
                <BalanceShow
                  :currency-id="wireForm.toCurrencyId"
                  :balance="wireForm.amount * wireForm.exchangeRate"
                />
                ( {{ $t("type.currency." + wireForm.toCurrencyId) }} )
              </td>
            </tr>
          </table>
          <div style="margin: 10px 0"></div>
          <table>
            <tr>
              <td colspan="2" style="text-align: center; font-weight: bold">
                {{ $t("fields.paymentMethod") }}
              </td>
            </tr>
            <tr>
              <td>{{ $t("fields.method") }}</td>
              <td>{{ wireForm.payment.paymentServiceName }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.countryOfBank") }}</td>
              <td>
                {{ phonesData[wireForm.bankInfo.bankCountry]?.name }}
              </td>
            </tr>
            <tr>
              <td>{{ $t("fields.bankName") }}</td>
              <td>{{ wireForm.bankInfo.bankName }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.branchName") }}</td>
              <td>{{ wireForm.bankInfo.branchName }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.city") }}</td>
              <td>{{ wireForm.bankInfo.city }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.state") }}</td>
              <td>{{ wireForm.bankInfo.state }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.accountHolder") }}</td>
              <td>{{ wireForm.bankInfo.holder }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.accountNo") }}</td>
              <td>{{ wireForm.bankInfo.accountNo }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.confirmAccountNo") }}</td>
              <td>{{ wireForm.bankInfo.confirmAccountNo }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.bsb") }}</td>
              <td>{{ wireForm.bankInfo.bsb }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.swiftCode") }}</td>
              <td>{{ wireForm.bankInfo.swiftCode }}</td>
            </tr>
            <tr>
              <td>{{ $t("fields.usdtWalletAddress") }}</td>
              <td>{{ wireForm.bankInfo.walletAddress }}</td>
            </tr>
          </table>
          <div style="margin: 10px 0"></div>
          <table>
            <tr>
              <td colspan="2" style="text-align: center; font-weight: bold">
                {{ $t("title.userAccounts") }}
              </td>
            </tr>
            <tr>
              <td style="text-align: center; font-weight: bold">
                {{ $t("fields.accountNo") }}
              </td>
              <td style="text-align: center; font-weight: bold">
                {{ $t("fields.balance") }}
              </td>
            </tr>
            <tr v-for="(acc, idx) in wireForm.userTradeAccounts" :key="idx">
              <td>{{ acc.accountNumber }}</td>
              <td>
                <BalanceShow
                  :currency-id="acc.currencyId"
                  :balance="acc.balanceInCents"
                />
              </td>
            </tr>
          </table>
        </div>
      </div>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref, inject } from "vue";
import phonesData from "@/core/data/phonesData";
import SimpleForm from "@/components/SimpleForm.vue";
import { useI18n } from "vue-i18n";
import Clipboard from "clipboard";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import BalanceShow from "@/components/BalanceShow.vue";
import { useStore } from "@/store";
import TimeShow from "@/components/TimeShow.vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import { TransactionAccountTypes } from "@/core/types/AccountInfos";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";

const withdrawId = ref(0);
const wireForm = ref<any>({});
const isLoading = ref(true);
const formRef = ref<InstanceType<typeof SimpleForm> | null>(null);
const store = useStore();
const user = store.state.AuthModule.user;
const t = useI18n().t;
const openConfirmBox = inject<any>(InjectKeys.OPEN_CONFIRM_MODAL);

const openConfirmPanel = () => {
  const _handler = () =>
    PaymentService.createOFA({
      id: withdrawId.value,
    });

  openConfirmBox(async () => {
    try {
      await _handler().then(() => MsgPrompt.success());
    } catch (error) {
      MsgPrompt.error(error);
    }
  });
};

const show = async (_item: any) => {
  isLoading.value = true;
  formRef.value?.show();

  // 1. get exchange rate
  const fromCurrencyId = _item.currencyId;

  // 2. get withdrawal bank info
  const source = _item.source;
  withdrawId.value = _item.id;

  const withdrawalInfo: any = await PaymentService.getWithdrawalInfosById(
    _item.id
  );

  const res = await AccountService.queryAccounts({
    partyId: _item.partyId,
    hasTradeAccount: true,
  });

  const accountInfo =
    res.data.find(
      (x) =>
        x.tradeAccount && x.tradeAccount.accountNumber == source.displayNumber
    ) ?? {};

  const userTradeAccounts = res.data.map((x) => x.tradeAccount);

  wireForm.value = {
    refId: _item.payment.id,
    createdOn: _item.createdOn,
    status: _item.payment.status,
    amount: _item.amount,
    fee: "0",
    user: {
      name:
        _item.displayName || `${_item.user?.firstName} ${_item.user?.lastName}`,
      nativeName: _item.user?.nativeName,
      email: _item.user?.email,
      currency: _item.currencyId,
      balance: _item.source.balance,
      balanceInCents: _item.source.balanceInCents,
    },
    payment: _item.payment,
    // bankInfo: _item.supplements.Withdraw.Reference.Request,
    bankInfo: withdrawalInfo.Reference.Request,
    fromCurrencyId,
    toCurrencyId: withdrawalInfo.TargetCurrencyId,
    userTradeAccounts,
    exchangeRate: withdrawalInfo.ExchangeRate,
    accountInfo,
    source,
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
  width: 50%;
  border: 1px solid #808080;
  text-align: left;
  padding: 8px;
}

tr:nth-child(even) {
  background-color: #f5f5f5;
}
</style>
