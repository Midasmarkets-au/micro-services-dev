<template>
  <div class="card" v-if="isLoading">
    <LoadingCentralBox :isTable="false" />
  </div>
  <div class="" v-else>
    <div>
      <AccountsCard
        :accountsList="accountsList"
        :currentAccount="currentAccount"
        :tab="tab"
        @deposit-submit="() => depositRef?.fetchData()"
        @withdrawal-submit="() => withdrawalRef?.fetchData()"
        @transfer-submit="() => transferRef?.fetchData()"
      />
    </div>
    <div class="card px-5 round-bl-br">
      <BcrAccountTab
        :tab-list="tabList"
        :selected-tab="tab"
        @change-tab="
          (_tab) =>
            router.push(
              '/account/' +
                currentAccount?.tradeAccount?.accountNumber +
                '/' +
                _tab
            )
        "
      />
    </div>
    <div class="card mt-2 px-5 round-tl-tr">
      <template v-if="tab === 'transaction'">
        <AccountTransactionTable
          v-if="!isMobile"
          :accountsList="accountsList"
        />

        <AccountMobileTransactionTable
          class="mt-5"
          v-if="isMobile"
          ref="transferRef"
          :accountsList="accountsList"
          :currentAccount="currentAccount"
        />
      </template>

      <template v-if="tab === 'deposit'">
        <AccountMobileDepositTable v-if="isMobile" ref="depositRef" />
        <AccountDepositTable v-if="!isMobile" />
      </template>

      <template v-if="tab === 'withdrawal'">
        <AccountMobileWithdrawalTable v-if="isMobile" ref="withdrawalRef" />
        <AccountWithdrawalTable v-if="!isMobile" />
      </template>

      <template v-if="tab === 'trade-report'">
        <AccountDetailTrades
          v-if="!isMobile"
          :currentAccount="currentAccount"
        />
        <AccountDetailTradeMobile v-else :currentAccount="currentAccount" />
      </template>
      <AccountReferLink
        v-if="tab === 'refer-link'"
        :currentAccount="currentAccount"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from "vue-router";
import i18n from "@/core/plugins/i18n";
import { ref, watch, onMounted, computed, provide } from "vue";
import LoadingRing from "@/components/LoadingRing.vue";
import AccountService from "../services/AccountService";
import AccountsCard from "../components/AccountsCard.vue";
import BcrAccountTab from "@/components/BcrAccountTab.vue";
import AccountTransactionTable from "../components/table/AccountTransactionTable.vue";
import AccountDetailTrades from "../components/AccountDetailTrades.vue";
import AccountDetailTradeMobile from "../components/table/AccountDetailTradeMobile.vue";
import AccountDepositTable from "@/projects/client/modules/accounts/components/table/AccountDepositTable.vue";
import AccountReferLink from "../components/AccountReferLink.vue";
import AccountWithdrawalTable from "@/projects/client/modules/accounts/components/table/AccountWithdrawalTable.vue";
import AccountMobileTransactionTable from "@/projects/client/modules/accounts/components/table/AccountMobileTransactionTable.vue";
import AccountMobileDepositTable from "@/projects/client/modules/accounts/components/table/AccountMobileDepositTable.vue";
import AccountMobileWithdrawalTable from "../components/table/AccountMobileWithdrawalTable.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { useStore } from "@/store";

const route = useRoute();
const { t } = i18n.global;
const store = useStore();

const isLoading = ref(true);
const currentAccount = ref({} as any);
const accountsList = ref(Array<any>());
const router = useRouter();
const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);

const tabList = computed(() => [
  { key: "deposit", label: t("title.deposit") },
  { key: "withdrawal", label: t("title.withdrawal") },
  { key: "transaction", label: t("title.transfer") },
  { key: "trade-report", label: t("title.tradeReport") },
]);

const tab = computed(
  () => (route.params.part as string) || tabList.value[0].key
);

const depositRef = ref();
const withdrawalRef = ref();
const transferRef = ref();

const fetchAccountInfo = async () => {
  isLoading.value = true;

  try {
    const res = await AccountService.queryAccounts({
      hasTradeAccount: true,
      status: AccountStatusTypes.Activate,
      roles: [
        AccountRoleTypes.Client,
        AccountRoleTypes.SuperAdmin,
        AccountRoleTypes.TenantAdmin,
        AccountRoleTypes.Wholesale,
        AccountRoleTypes.Guest,
      ],
    });
    accountsList.value = res.data;
    currentAccount.value = await getCurrentAccountFromUrlParams();
    if (!currentAccount.value) {
      await router.push("/account");
    }
    // if (
    //   projectConfig.value?.newYearEvent &&
    //   currentAccount.value.selfReferCode != null &&
    //   currentAccount.value.selfReferCode != ""
    // ) {
    //   tabList.value.push({
    //     key: "refer-link",
    //     label: t("title.newYearReferLink"),
    //   });
    // }
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const getCurrentAccountFromUrlParams = async () => {
  let account = accountsList.value.find(
    (item) => item.tradeAccount?.accountNumber == route.params.accountNumber
  );
  if (account) return account;
  const res = await AccountService.queryAccounts({
    accountNumber: route.params.accountNumber,
  });
  if (res.data.length > 0) {
    return res.data[0];
  }
  return null;
};

provide(AccountInjectionKeys.GET_ACCOUNT_DETAILS, () => currentAccount.value);

onMounted(() => {
  console.log("de");
  fetchAccountInfo();
});

watch(
  () => route.params.accountNumber,
  async () => {
    isLoading.value = true;
    if (!/^\/account/.test(route.path)) {
      return;
    }
    currentAccount.value = await getCurrentAccountFromUrlParams();
    if (!currentAccount.value) {
      await router.push("/account");
    }
    isLoading.value = false;
  }
);
</script>

<style scoped></style>
