<template>
  <!-- <div v-if="isLoading">Loading</div> -->
  <div v-if="!isLoading">
    <div class="card">
      <div class="card-header border-0">
        <div class="card-title-noicon">
          <router-link :to="'/ib/customers'" class="d-flex align-items-center">
            <span class="svg-icon svg-icon-6 me-3">
              <inline-svg src="/images/icons/arrows/left001.svg" />
            </span>
            <span class="text-gray" style="font-size: 18px">{{
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

      <div class="card-body border-0" v-if="!isMobile">
        <div class="row w-100 d-flex align-items-center">
          <div
            class="col-xl-3 d-flex flex-column flex-lg-row align-items-center gap-4 gap-lg-10"
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
                <span class="fs-5" style="color: #3a3e44">
                  {{ accountDetails.user.email }}
                </span>
              </div>
            </div>
            <div
              class="flex-1 gap-3 d-flex flex-column"
              style="border-right: 1px dashed #adcbff; padding-right: 10px"
            >
              <!---角色-->
              <div>
                <h4 class="text-gray" style="font-size: 14px; font-weight: 400">
                  {{ $t("fields.role") }}
                </h4>

                <template
                  v-if="
                    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                      accountDetails.role
                    )
                  "
                >
                  <span class="fs-5 font-medium" style="color: #333333">
                    {{ $t(`fields.ib`) }}</span
                  >
                </template>
                <template v-else>
                  <span class="fs-5 font-medium" style="color: #333333">
                    {{ $t(`type.accountRole.${accountDetails.role}`) }}
                  </span>
                </template>
              </div>
              <div>
                <template
                  v-if="
                    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                      accountDetails.role
                    )
                  "
                >
                  <h4
                    class="text-gray"
                    style="font-size: 14px; font-weight: 400"
                  >
                    {{ $t("fields.accountUid") }}
                  </h4>
                  <span class="fs-6 font-medium">
                    {{ accountDetails.uid }}
                  </span>
                </template>
                <template v-else>
                  <h4
                    class="text-gray"
                    style="font-size: 14px; font-weight: 400"
                  >
                    {{ $t("fields.accountNo") }}
                  </h4>
                  <span class="fs-5 font-medium" style="color: #333333">
                    {{ accountDetails.tradeAccount?.accountNumber }}
                  </span>
                </template>
              </div>
            </div>
          </div>
          <div
            class="col-lg-3 text-center text-lg-center mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 16px" class="mb-8 text-gray">
              {{ $t("fields.balance") }}
            </h4>
            <span style="font-size: 28px; line-height: 28px; color: #000f32">
              <BalanceShow
                :balance="accountDetails.tradeAccount?.balanceInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
          <div
            class="col-lg-3 text-center text-lg-center mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 16px" class="mb-8 text-gray">
              {{ $t("fields.equity") }}
            </h4>
            <span style="font-size: 28px; line-height: 28px; color: #000f32">
              <BalanceShow
                :balance="accountDetails.tradeAccount?.equityInCents"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
          <div
            class="col-lg-3 text-center text-lg-center mt-4 mt-lg-0"
            v-if="accountDetails.role === AccountRoleTypes.Client"
          >
            <h4 style="font-size: 16px" class="mb-8 text-gray">
              {{ $t("fields.credit") }}
            </h4>
            <span style="font-size: 28px; line-height: 28px; color: #000f32">
              <BalanceShow
                :balance="accountDetails.credit * 100"
                :currency-id="accountDetails.tradeAccount?.currencyId"
              />
            </span>
          </div>
        </div>
      </div>
      <div class="card-header border-0 my-10" v-if="isMobile">
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
          </div>
          <div class="row mt-4">
            <div class="col-6">
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

            <div class="col-6">
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
          </div>
          <div
            v-if="accountDetails.role === AccountRoleTypes.Client"
            class="d-flex gap-6 mt-4"
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
    <div>
      <div class="py-0 py-5">
        <div class="card card-header card-header-bottom">
          <BcrAccountTab
            :tabList="tabList"
            :selected-tab="currentTab"
            @change-tab="
              (_tab) => router.push('/ib/customers/' + accountUid + '/' + _tab)
            "
          />
        </div>
        <div class="card card-body mt-2 round-tl-tr">
          <div v-if="currentTab === 'deposit'" class="overflow-auto">
            <IBCustomerDetailDeposit :account-details="accountDetails" />
          </div>

          <div v-if="currentTab === 'withdrawal'" class="overflow-auto">
            <IBCustomerDetailWithdrawal :account-details="accountDetails" />
          </div>

          <div v-if="currentTab === 'transaction'" class="overflow-auto">
            <IBCustomerDetailTransactions :account-details="accountDetails" />
          </div>

          <div v-if="currentTab === 'trade-report'">
            <IBCustomerDetailTrades :account-details="accountDetails" />
          </div>

          <div v-if="currentTab === 'rebate-settings'" class="overflow-auto">
            <IBCustomerDetailRebateSetting :accountRole="accountDetails.role" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useStore } from "@/store";
import svc from "../services/IbService";
import GlobalService from "../../../services/ClientGlobalService";
import IBCustomerDetailTrades from "../components/IBCustomerDetailTrades.vue";
import IBCustomerDetailTransactions from "../components/IBCustomerDetailTransactions.vue";
import IBCustomerDetailRebates from "../components/IBCustomerDetailRebates.vue";
import IBCustomerDetailRebateSetting from "../components/IBCustomerDetailRebateSetting.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import BcrAccountTab from "@/components/BcrAccountTab.vue";
import { useI18n } from "vue-i18n";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { isMobile } from "@/core/config/WindowConfig";
import { PublicSetting } from "@/core/types/ConfigTypes";
import IBCustomerDetailDeposit from "@/projects/client/modules/ib/components/IBCustomerDetailDeposit.vue";
import IBCustomerDetailWithdrawal from "@/projects/client/modules/ib/components/IBCustomerDetailWithdrawal.vue";
import RefreshMTDataButton from "@/projects/client/components/RefreshMTDataButton.vue";
const route = useRoute();
const router = useRouter();
const store = useStore();
const { t } = useI18n();

const accountUid = computed(() => parseInt(route.params.accountId as string));

const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);

const serviceMap = ref({} as any);

const props = defineProps<{
  serviceMap?: any;
  customerAccounts: any;
}>();

const accountDetails = ref();
const isLoading = ref(true);

const tabList = ref([
  { key: "deposit", label: t("title.deposit") },
  { key: "withdrawal", label: t("title.withdrawal") },
  { key: "transaction", label: t("title.transfer") },
  { key: "trade-report", label: t("fields.tradeReport") },
]);

const currentTab = computed(
  () => (route.params.part as string) || tabList.value[0].key
);

const getClientAccountDetail = async () => {
  if (props.customerAccounts) {
    accountDetails.value = props.customerAccounts.find(
      (x: any) => x.uid === accountUid.value
    );
    if (!accountDetails.value) {
      await router.push("/ib/customers");
    }
  } else {
    try {
      const { data: clientAccounts } =
        await svc.queryAgentClientAccountsByAgent({
          uid: accountUid.value,
        });

      accountDetails.value = clientAccounts[0];
    } catch (error) {
      await router.push("/ib/customers");
    }
  }

  if (
    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
      accountDetails.value.role
    )
  ) {
    tabList.value.push(
      ...[{ key: "rebate-settings", label: t("fields.rebateSettings") }]
    );
  }

  if (projectConfig.value?.rebateEnabled) {
    tabList.value.push({
      key: "rebate-report",
      label: t("fields.rebateReport"),
    });
  }
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
  if (props.serviceMap) {
    serviceMap.value = props.serviceMap;
  }
  await getClientAccountDetail();
  console.log("accountDetails", accountDetails.value);
  serviceMap.value = await GlobalService.getServiceMap();
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
