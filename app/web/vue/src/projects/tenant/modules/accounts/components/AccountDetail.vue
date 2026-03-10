<template>
  <SiderDetail2
    :save="submit"
    :discard="close"
    :title="detailTitle"
    elId="user_detail_show"
    :isLoading="isLoading"
    :submited="submitted"
    :isDisabled="false"
    :savingTitle="$t('action.saving')"
    :show-footer="false"
    width="85%"
    ref="accountDetailShowRef"
  >
    <LoadingCentralBox v-if="isLoading" />
    <div v-if="!isLoading" class="px-10">
      <div class="fv-row mb-7">
        <ul
          class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
        >
          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.AccountInfo }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.AccountInfo)"
              >{{ $t("fields.info") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              v-if="accountDetails.accountNumber != 0"
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.TradeRecord }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.TradeRecord)"
              >{{ $t("title.trade") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.TransactionRecord }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.TransactionRecord)"
              >{{ $t("title.transfer") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Deposit }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Deposit)"
              >{{ $t("title.deposit") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Withdrawal }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Withdrawal)"
              >{{ $t("title.withdrawal") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Rebate }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Rebate)"
              v-if="tenant != 'jp' && tenant != 'au'"
              >{{ $t("title.rebate") }}</a
            >
          </li>

          <!-- <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.PaymentMethod }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.PaymentMethod)"
              >{{ $t("title.paymentMethod") + " (Account)" }}</a
            >
          </li> -->

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.PaymentMethod }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.PaymentMethod)"
              >{{ $t("title.paymentMethod") + " (Account)" }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Credit }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Credit)"
              >{{ $t("title.creditAdjust") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Config }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Config)"
              >{{ $t("title.config") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Refer }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Refer)"
              >{{ $t("fields.referralCode") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Stat }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Stat)"
              v-if="tenant != 'jp'"
              >{{ $t("fields.stat") }}</a
            >
          </li>
          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.Log }"
              data-bs-toggle="tab"
              href="#"
              @click="changeTab(AccountDetailTab.Log)"
              >{{ $t("title.log") }}</a
            >
          </li>

          <li class="nav-item">
            <a
              class="nav-link text-active-primary pb-4"
              :class="{ active: tab === AccountDetailTab.ApiLog }"
              data-bs-toggle="tab"
              href="#"
              v-if="$can('SuperAdmin')"
              @click="changeTab(AccountDetailTab.ApiLog)"
              >ApiLog</a
            >
          </li>
          <!--end:::Tab item-->
        </ul>
      </div>
      <div v-if="tab === AccountDetailTab.AccountInfo">
        <AccountDetailInfoForm :infos="infos" />
      </div>

      <div v-if="tab === AccountDetailTab.TradeRecord">
        <AccountDetailTrades
          :accountId="accountDetails.id"
          :serviceId="accountDetails.serviceId"
        />
      </div>

      <div v-if="tab === AccountDetailTab.TransactionRecord">
        <AccountDetailTransactions
          :accountId="accountDetails.id"
          :partyId="accountDetails.partyId"
        />
      </div>

      <AccountRebate
        v-if="tab === AccountDetailTab.Rebate"
        :account-details="accountDetails"
      ></AccountRebate>

      <div v-if="tab === AccountDetailTab.Deposit">
        <AccountDetailDeposit :account-details="accountDetails" />
      </div>

      <div v-if="tab === AccountDetailTab.Withdrawal">
        <AccountDetailWithdrawal :account-details="accountDetails" />
      </div>

      <div v-if="tab === AccountDetailTab.PaymentMethod">
        <PaymentMethodDetailForm />
      </div>
      <div v-if="tab === AccountDetailTab.Credit">
        <AccountDetailCredit />
      </div>

      <div v-if="tab === AccountDetailTab.Config && $can('TenantAdmin')">
        <ConfigList
          :rowId="accountDetails.id"
          :category="ConfigCategory.account"
          :siteId="accountDetails.siteId"
          :role="accountDetails.role"
        />
      </div>

      <div v-if="tab === AccountDetailTab.Refer">
        <AccountReferCode :account-details="accountDetails" />
      </div>

      <div
        v-if="
          tab === AccountDetailTab.Stat &&
          $cans(['TenantAdmin', 'WebTenantAccountStat'])
        "
      >
        <AccountDetailStat :account-details="accountDetails" />
      </div>

      <div v-if="tab === AccountDetailTab.Log">
        <AccountDetailLog />
      </div>

      <div v-if="tab === AccountDetailTab.ApiLog && $can('TenantAdmin')">
        <UserApiLog :partyId="accountDetails.partyId" />
      </div>
    </div>
  </SiderDetail2>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { nextTick, provide, ref } from "vue";
import AccountRebate from "./AccountRebate.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountReferCode from "./AccountReferCode.vue";
import AccountDetailLog from "./AccountDetailLog.vue";
import SiderDetail from "@/components/SiderDetail.vue";
import AccountService from "../services/AccountService";
import SiderDetail2 from "@/components/SiderDetail2.vue";
import AccountDetailCredit from "./AccountDetailCredit.vue";
import AccountDetailTrades from "./AccountDetailTrades.vue";
import { ConfigCategory } from "@/core/types/ConfigCategory";
import AccountDetailInfoForm from "./AccountDetailInfoForm.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import PaymentMethodDetailForm from "./PaymentMethodDetailForm.vue";
import AccountDetailTransactions from "./AccountDetailTransactions.vue";
import UserApiLog from "@/projects/tenant/modules/users/components/UserApiLog.vue";
import ConfigList from "@/projects/tenant/modules/system/components/config/ConfigList.vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import AccountDetailStat from "@/projects/tenant/modules/accounts/components/AccountDetailStat.vue";
import AccountDetailDeposit from "@/projects/tenant/modules/accounts/components/AccountDetailDeposit.vue";
import AccountDetailWithdrawal from "@/projects/tenant/modules/accounts/components/AccountDetailWithdrawal.vue";

enum AccountDetailTab {
  AccountInfo = "infos",
  TradeRecord = "trade",
  TransactionRecord = "transaction",
  Rebate = "rebate",
  PaymentService = "payment",
  PaymentMethod = "paymentMethod",
  Withdrawal = "withdraw",
  Deposit = "deposit",
  Documents = "documents",
  Credit = "credit",
  Config = "config",
  Refer = "refer",
  Stat = "stat",
  Log = "log",
  ApiLog = "apiLog",
}

const store = useStore();
const isLoading = ref(true);
const infos = ref({} as any);
const submitted = ref(false);
const accountDetails = ref({} as any);
const detailTitle = ref(`Account Detail: loading...`);
const tenant = ref(store.state.AuthModule.user?.tenancy);
const tab = ref<AccountDetailTab>(AccountDetailTab.AccountInfo);
const accountDetailShowRef = ref<InstanceType<typeof SiderDetail>>();
const userEmail = ref("");

provide(AccountInjectionKeys.ACCOUNT_DETAILS, accountDetails);
provide(AccountInjectionKeys.USER_EMAIL, userEmail);

const show = async (
  id: number,
  selectedTab: string,
  paymentService: Array<any>,
  _accountDetails?: any,
  email?: string
) => {
  isLoading.value = true;
  try {
    accountDetails.value = {} as any;
    infos.value = {} as any;
    userEmail.value = email || "";

    if (_accountDetails) {
      accountDetails.value = _accountDetails;
    } else if (id !== -1) {
      accountDetails.value = await AccountService.getAccountDetailById(id);
    }

    detailTitle.value =
      accountDetails.value.name +
      " Account Detail: " +
      accountDetails.value.tradeAccount?.accountNumber;
    infos.value = accountDetails.value;

    tab.value = selectedTab as AccountDetailTab;
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
    await nextTick();
  }
  accountDetailShowRef.value?.show();
};

const changeTab = (_tab: AccountDetailTab) => {
  tab.value = _tab;
};

const submit = async () => {
  console.log("tbd");
};

const close = () => {
  accountDetailShowRef.value?.hide();
};

defineExpose({ show });
</script>

<style></style>
