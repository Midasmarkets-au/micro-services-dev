<template>
  <!-- <div v-if="isLoading">Loading</div> -->
  <div v-if="!isLoading" class="card mb-5">
    <div class="card-header border-0">
      <div class="card-title-noicon">
        <router-link :to="'/sales/customers'" class="d-flex align-items-center">
          <span class="svg-icon svg-icon-6 me-3">
            <inline-svg src="/images/icons/arrows/left001.svg" />
          </span>
          <span class="text-gray fs-3">{{
            $t("action.backToCustomerList")
          }}</span>
        </router-link>
      </div>
      <div class="ms-auto">
        <RefreshMTDataButton
          v-if="accountDetails?.uid"
          :account-id="accountDetails.uid"
          @refreshed="handleRefreshed"
        />
      </div>
    </div>

    <div class="card-header border-0 my-10" v-if="!isMobile">
      <div class="row w-100 d-flex align-items-center">
        <div
          class="col-xl-8 d-flex flex-column flex-lg-row align-items-center gap-3"
        >
          <div class="d-flex align-items-center">
            <UserAvatar
              v-if="accountDetails.user"
              :avatar="accountDetails.user.avatar"
              :name="accountDetails.user.lastName"
              side="client"
              size="54px"
              class="me-3"
              rounded
            />
            <div>
              <h2 class="fw-semibold fs-4">
                {{ accountDetails.user.nativeName }}
              </h2>
              <span class="" style="font-size: 14px; color: #717171">
                {{ accountDetails.user.email }}
              </span>
            </div>
          </div>
          <div class="d-flex gap-10">
            <div>
              <template
                v-if="
                  [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                    accountDetails.role
                  )
                "
              >
                <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                  {{ $t("fields.accountUid") }}
                </h4>
                <span class="">
                  {{ accountDetails.uid }}
                </span>
              </template>
              <template v-else>
                <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                  {{ $t("fields.accountNo") }}
                </h4>
                <span class="">
                  {{ accountDetails.tradeAccount?.accountNumber }}
                </span></template
              >
            </div>

            <div>
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.role") }}
              </h4>

              <template
                v-if="
                  [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                    accountDetails.role
                  )
                "
              >
                {{ $t(`fields.ib`) }}
              </template>
              <template v-else>
                {{ $t(`type.accountRole.${accountDetails.role}`) }}</template
              >
            </div>

            <div>
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.group") }}
              </h4>

              <span>{{ accountDetails.group }}</span>
            </div>

            <div>
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.code") }}
              </h4>

              <span>{{ accountDetails.code }}</span>
            </div>
          </div>
        </div>
        <div
          class="col-xl-4 d-flex flex-row"
          style="justify-content: space-around"
        >
          <div
            class="text-center text-lg-start mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.balance") }}
            </h4>
            <span
              class=""
              style="
                font-weight: 500;
                font-size: 20px;
                line-height: 28px;
                /* identical to box height, or 140% */

                color: #000000;
              "
            >
              <BalanceShow
                :balance="accountDetails.tradeAccount?.balanceInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>

          <div
            class="text-center text-lg-start mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.equity") }}
            </h4>
            <span
              class=""
              style="
                font-weight: 500;
                font-size: 20px;
                line-height: 28px;
                /* identical to box height, or 140% */

                color: #000000;
              "
            >
              <BalanceShow
                :balance="accountDetails.tradeAccount?.equityInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>

          <div
            class="text-center text-lg-start mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.credit") }}
            </h4>
            <span
              class=""
              style="
                font-weight: 500;
                font-size: 20px;
                line-height: 28px;
                /* identical to box height, or 140% */

                color: #000000;
              "
            >
              <BalanceShow
                :balance="accountDetails.credit * 100"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
        </div>
      </div>
    </div>
    <div class="card-header border-0 mb-3" v-else>
      <div class="d-flex align-items-center mb-3">
        <UserAvatar
          v-if="accountDetails.user"
          :avatar="accountDetails.user.avatar"
          :name="accountDetails.user.lastName"
          side="client"
          size="80px"
          class="me-3"
          rounded
        />
        <div>
          <h2 class="fw-semibold fs-4">
            {{ accountDetails.user.nativeName }}
          </h2>
          <span class="" style="font-size: 14px; color: #717171">
            {{ accountDetails.user.email }}
          </span>
        </div>
      </div>

      <div class="w-100">
        <div class="d-flex gap-4 justify-content-between">
          <div>
            <template
              v-if="
                [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                  accountDetails.role
                )
              "
            >
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.accountUid") }}
              </h4>
              <span>
                {{ accountDetails.uid }}
              </span>
            </template>
            <template v-else>
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.accountNo") }}
              </h4>
              <span>
                {{ accountDetails.tradeAccount?.accountNumber }}
              </span></template
            >
          </div>

          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.role") }}
            </h4>

            <template
              v-if="
                [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                  accountDetails.role
                )
              "
            >
              {{ $t(`fields.ib`) }}
            </template>
            <template v-else>
              {{ $t(`type.accountRole.${accountDetails.role}`) }}</template
            >
          </div>

          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.group") }}
            </h4>

            <span>{{ accountDetails.group }}</span>
          </div>
          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.code") }}
            </h4>

            <span>{{ accountDetails.code }}</span>
          </div>
        </div>

        <div
          class="mt-4 d-flex gap-6"
          v-if="accountDetails.role === AccountRoleTypes.Client"
        >
          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.balance") }}
            </h4>
            <span>
              <BalanceShow
                :balance="accountDetails.tradeAccount?.balanceInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.equity") }}
            </h4>
            <span>
              <BalanceShow
                :balance="accountDetails.tradeAccount?.equityInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
          <div>
            <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
              {{ $t("fields.credit") }}
            </h4>
            <span>
              <BalanceShow
                :balance="accountDetails.credit * 100"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="mt-2">
    <div class="card card-header card-header-bottom">
      <BcrAccountTab
        :tabList="tabList"
        :selected-tab="currentTab"
        @change-tab="
          (_tab) => router.push('/sales/customers/' + accountUid + '/' + _tab)
        "
      />
    </div>
    <div class="card card-body mt-2 round-tl-tr">
      <div v-if="currentTab === 'transaction'" class="">
        <SalesCustomerDetailTransactions :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'trade-report'" class="">
        <SalesCustomerDetailTrades :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'deposit'" class="">
        <SalesCustomerDetailDeposit :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'withdrawal'" class="">
        <SalesCustomerDetailWithdrawal :account-details="accountDetails" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "@/store";
import SalesService from "../services/SalesService";
import GlobalService from "../../../services/ClientGlobalService";
import SalesCustomerDetailTrades from "../components/SalesCustomerDetailTrades.vue";
import SalesCustomerDetailTransactions from "../components/SalesCustomerDetailTransactions.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import BcrAccountTab from "@/components/BcrAccountTab.vue";
import { useI18n } from "vue-i18n";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
import { PublicSetting } from "@/core/types/ConfigTypes";
import SalesCustomerDetailDeposit from "@/projects/client/modules/sales/components/SalesCustomerDetailDeposit.vue";
import SalesCustomerDetailWithdrawal from "@/projects/client/modules/sales/components/SalesCustomerDetailWithdrawal.vue";
import RefreshMTDataButton from "@/projects/client/components/RefreshMTDataButton.vue";
const route = useRoute();
const router = useRouter();
const store = useStore();
const { t } = useI18n();

const accountUid = computed(() => parseInt(route.params.accountId as string));
const salesAccount = computed(() => store.state.SalesModule.salesAccount);
const currentTab = computed(
  () => (route.params.part as string) || "transaction"
);
const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);

const serviceMap = ref({} as any);

const props = defineProps<{
  serviceMap?: any;
  customerAccounts?: any;
}>();

const accountDetails = ref();
const isLoading = ref(true);

const tabList = ref([
  { key: "transaction", label: t("title.transfer") },
  { key: "deposit", label: t("title.deposit") },
  { key: "withdrawal", label: t("title.withdrawal") },
]);

const getClientAccountDetail = async () => {
  if (props.customerAccounts && props.customerAccounts.length > 0) {
    accountDetails.value = props.customerAccounts.find(
      (x: any) => x.uid === accountUid.value
    );
    console.log("first if runned");
    if (!accountDetails.value) {
      await router.push("/sales/customers");
    }
  } else {
    try {
      const { data: clientAccounts } = await SalesService.queryAccounts({
        uid: accountUid.value,
      });
      console.log("second if runned");
      accountDetails.value = clientAccounts[0];
      console.log("accountDetails", accountDetails.value);
    } catch (error) {
      await router.push("/sales/customers");
    }
  }
  if (accountDetails.value.tradeAccount?.accountNumber !== 0) {
    tabList.value.push({ key: "trade-report", label: t("fields.tradeReport") });
  }
  console.log("accountDetails", accountDetails.value);
};

watch(accountUid, async () => {
  if (!accountUid.value || accountUid.value === -1) return;
  if (!props.serviceMap) {
    serviceMap.value = await GlobalService.getServiceMap();
  }
  await getClientAccountDetail();
  isLoading.value = false;
});

onMounted(async () => {
  serviceMap.value = props.serviceMap ?? (await GlobalService.getServiceMap());
  await getClientAccountDetail();
  isLoading.value = false;
});

const handleRefreshed = async (data: any) => {
  // 刷新成功后，重新获取客户详情
  await getClientAccountDetail();
};
</script>

<style scoped lang="scss">
.tabBtn {
  display: flex;
  justify-content: center;
  align-items: center;
  font-size: 14px;
  border-radius: 5px 5px 0 0;
  width: 150px;
  height: 40px;
  margin-right: 6px;
  cursor: pointer;
  border: 2px solid #ffd400;
  border-bottom: 0;
  margin-bottom: -2px;
}

.tabActive {
  background-color: #ffd400;
}

.tabLine {
  width: 98%;
  height: 3px;
  background-color: #ffd400;
  margin-top: -1px;
  margin-left: 10px;
}
@media (max-width: 768px) {
  .card-body {
    padding: 0.75rem !important;
  }
}
</style>
