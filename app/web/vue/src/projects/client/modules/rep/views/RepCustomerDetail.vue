<template>
  <!-- <div v-if="isLoading">Loading</div> -->
  <div v-if="!isLoading" class="card mb-5">
    <div class="card-header border-0">
      <div class="card-title">
        <router-link :to="'/rep/customers'" class="">
          <span class="svg-icon svg-icon-6 me-3">
            <inline-svg src="/images/icons/arrows/left001.svg" />
          </span>
          <span style="color: #7c8fa2; font-size: 14px">{{
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

    <div class="card-header border-0 my-10">
      <div class="row w-100 d-flex align-items-center">
        <div
          class="col-lg-8 d-flex flex-column flex-lg-row align-items-center gap-4 gap-lg-18"
        >
          <div class="d-flex align-items-center">
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
          <div class="d-flex gap-18">
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

            <div
              v-if="accountDetails.role === AccountRoleTypes.Client && isMobile"
            >
              <h4 style="font-size: 14px; font-weight: 400" class="text-gray">
                {{ $t("fields.balance") }}
              </h4>
              <span>
                <BalanceShow
                  :balance="accountDetails.tradeAccount?.balanceInCents"
                  :currency-id="accountDetails.tradeAccount?.currencyId"
                />
                {{
                  $t(`type.currency.${accountDetails.tradeAccount?.currencyId}`)
                }}
              </span>
            </div>
          </div>
        </div>

        <div
          class="col-lg-4 text-center text-lg-start mt-4 mt-lg-0"
          v-if="accountDetails.role === AccountRoleTypes.Client && !isMobile"
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
            {{ $t(`type.currency.${accountDetails.tradeAccount?.currencyId}`) }}
          </span>
        </div>
      </div>
    </div>

    <div class="card-body py-0 py-5">
      <BcrAccountTab
        :tabList="tabList"
        :selected-tab="currentTab"
        @change-tab="
          (_tab) => router.push('/rep/customers/' + accountUid + '/' + _tab)
        "
      />

      <div v-if="currentTab === 'transaction'" class="">
        <RepCustomerDetailTransactions :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'trade-report'" class="">
        <RepCustomerDetailTrades :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'deposit'" class="">
        <RepCustomerDetailDeposit :account-details="accountDetails" />
      </div>

      <div v-if="currentTab === 'withdrawal'" class="">
        <RepCustomerDetailWithdrawal :account-details="accountDetails" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "@/store";
import RepService from "../services/RepService";
import SalesService from "@/projects/client/modules/sales/services/SalesService";
import GlobalService from "../../../services/ClientGlobalService";
import RepCustomerDetailTrades from "../components/RepCustomerDetailTrades.vue";
import RepCustomerDetailTransactions from "../components/RepCustomerDetailTransactions.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import BcrAccountTab from "@/components/BcrAccountTab.vue";
import { useI18n } from "vue-i18n";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
import { PublicSetting } from "@/core/types/ConfigTypes";
import RepCustomerDetailDeposit from "@/projects/client/modules/rep/components/RepCustomerDetailDeposit.vue";
import RepCustomerDetailWithdrawal from "@/projects/client/modules/rep/components/RepCustomerDetailWithdrawal.vue";
import RefreshMTDataButton from "@/projects/client/components/RefreshMTDataButton.vue";

const route = useRoute();
const router = useRouter();
const store = useStore();
const { t } = useI18n();

const accountUid = computed(() => parseInt(route.params.accountId as string));
const repAccount = computed(() => store.state.RepModule.repAccount);
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
  { key: "trade-report", label: t("fields.tradeReport") },
]);

const getClientAccountDetail = async () => {
  // console.log("props.customerAccounts", props.customerAccounts);

  if (props.customerAccounts && props.customerAccounts.length > 0) {
    accountDetails.value = props.customerAccounts.find(
      (x: any) => x.uid === accountUid.value
    );

    if (!accountDetails.value) {
      await router.push("/rep/customers");
    }
  } else {
    try {
      const { data: clientAccounts } = await RepService.queryAccounts({
        uid: accountUid.value,
      });

      accountDetails.value = clientAccounts[0];
    } catch (error) {
      await router.push("/rep/customers");
    }
  }
};

watch(accountUid, async () => {
  if (!accountUid.value || accountUid.value === -1) return;
  if (!props.serviceMap) {
    serviceMap.value = await GlobalService.getServiceMap();
  }
  // console.log("runnded");
  await getClientAccountDetail();
  isLoading.value = false;
});

onMounted(async () => {
  // console.log(props.customerAccounts);
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
