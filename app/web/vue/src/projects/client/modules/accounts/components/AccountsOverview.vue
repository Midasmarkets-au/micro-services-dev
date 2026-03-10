<template>
  <div>
    <div class="card mb-1 round-bl-br">
      <div class="card-header border-0 account-height">
        <div class="card-title fw-bold">
          <h3 v-if="currentTab === 'RealAccounts'">
            {{ $t("title.tradeAccounts") }}
          </h3>
          <h3 class="" v-else>{{ $t("title.demoAccounts") }}</h3>
        </div>

        <div v-if="isMobile" class="card-toolbar">
          <button
            class="btn btn-sm btn-primary d-flex align-items-center gap-2"
            style="
              box-shadow: 4px 4px 6px 0px rgba(113, 113, 113, 0.25) !important;
            "
            @click="
              currentTab === 'RealAccounts'
                ? CreateLiveAccountViewRef?.show()
                : CreateDemoAccountViewRef?.show()
            "
          >
            <inline-svg src="/images/icons/general/plus01.svg" />
            <span>{{ $t("action.createNew") }}</span>
          </button>
        </div>
      </div>
      <div class="d-flex align-items-center justify-content-between !min-h-0">
        <div class="">
          <div class="nav account-tabs nav-pills nav-pills-custom ml-8">
            <button
              class="account-tab nav-item"
              :style="{
                cursor: !Can.cans(['Client']) ? 'not-allowed' : 'pointer',
                color: !Can.cans(['Client']) ? '#b5b5c3' : '#000',
              }"
              :disabled="!Can.cans(['Client'])"
              :class="{ active: currentTab === 'RealAccounts' }"
              data-bs-toggle="pill"
              @click="currentTab = 'RealAccounts'"
              href="#dashboard-real-accounts"
            >
              {{ $t("title.realAccount") }}
            </button>
            <button
              class="account-tab nav-item"
              :class="{ active: currentTab === 'DemoAccounts' }"
              data-bs-toggle="pill"
              @click="currentTab = 'DemoAccounts'"
              href="#dashboard-demo-accounts"
            >
              {{ $t("fields.demoAccount") }}
            </button>
          </div>
        </div>

        <div v-if="!isMobile" class="card-toolbar mr-3 mb-1">
          <button
            v-if="currentTab === 'RealAccounts'"
            class="btn btn-sm btn-light-secondary text-primary btn-bordered"
            @click="CreateLiveAccountViewRef?.show()"
          >
            {{ $t("action.createTradeAccount") }}
          </button>
          <!-- && $can('SuperAdmin') -->
          <button
            v-if="
              currentTab === 'RealAccounts' &&
              $can('Wholesale') &&
              $can('SuperAdmin')
            "
            class="ms-5 btn btn-sm btn-light-secondary text-primary btn-bordered"
            @click="CreateWholesaleReferralViewRef?.show()"
          >
            {{ $t("fields.referral") }}
          </button>

          <button
            v-if="currentTab === 'DemoAccounts'"
            class="btn btn-sm btn-light-secondary text-primary btn-bordered"
            @click="CreateDemoAccountViewRef?.show()"
          >
            {{ $t("action.createDemoAccount") }}
          </button>
        </div>
      </div>
    </div>
    <div class="card round-tl-tr">
      <div class="card-body" style="padding-bottom: 10px">
        <div
          class="d-flex justify-content-center"
          :class="{
            'account-no-data': props.showEventShop,
            'account-no-client': !props.showEventShop && !isMobile,
          }"
          v-if="isLoading"
        >
          <LoadingRing />
        </div>
        <div
          v-else
          class="tab-content"
          :class="{
            'account-no-data': props.showEventShop,
            'account-no-client': !props.showEventShop && !isMobile,
          }"
        >
          <div
            class="tab-pane fade"
            :class="{
              'show active': Can.cans(['Client']),
            }"
            id="dashboard-real-accounts"
          >
            <table class="mx-auto" v-if="isLoading">
              <tbody>
                <LoadingRing />
              </tbody>
            </table>

            <table
              class="mx-auto"
              v-else-if="
                pendingApplications.length === 0 && liveAccounts.length === 0
              "
            >
              <tbody>
                <NoDataBox />
              </tbody>
            </table>

            <div v-else class="">
              <div class="row flex-nowrap overflow-auto gx-2">
                <div
                  class="col-12 col-md-6 d-flex justify-content-center mb-5"
                  v-for="(item, index) in pendingApplications"
                  :key="index"
                >
                  <TradeAccountCard
                    :serviceMap="serviceMap"
                    :item="item"
                    type="application"
                    class="m-auto m-md-0"
                    :button-text="$t('status.pending') + '...'"
                  />
                </div>

                <div
                  class="col-12 col-md-6 d-flex justify-content-center mb-5"
                  v-for="(item, index) in liveAccounts"
                  :key="index"
                >
                  <TradeAccountCard
                    type="account"
                    :showWebtrader="
                      Object.values(ServiceTypes).includes(item.serviceId)
                    "
                    :serviceMap="serviceMap"
                    :item="item"
                    :wholesaleEnable="wholesaleEnable"
                    @on-change-leverage="onChangeLeverage(item.tradeAccount)"
                    @on-reset-password="onResetPassword(item.tradeAccount)"
                    :button-handler="
                      item.hasTradeAccount
                        ? () => openDirectDepositModal(item)
                        : null
                    "
                    :button-text="
                      item.hasTradeAccount
                        ? $t('action.deposit')
                        : $t('status.pendingTtd') + '...'
                    "
                    class="m-auto m-md-0"
                  />
                </div>
              </div>
            </div>
          </div>

          <div
            class="tab-pane fade"
            :class="{
              'show active': !Can.cans(['Client']),
            }"
            id="dashboard-demo-accounts"
          >
            <table class="mx-auto" v-if="isLoading">
              <tbody>
                <LoadingRing />
              </tbody>
            </table>

            <table class="mx-auto" v-else-if="demoAccounts.length === 0">
              <tbody>
                <NoDataBox />
              </tbody>
            </table>

            <div v-else class="row flex-nowrap" style="overflow: auto">
              <div
                class="col-12 col-md-6 d-flex justify-content-center mb-5"
                v-for="(item, index) in demoAccounts"
                :key="index"
              >
                <TradeAccountCard
                  :serviceMap="serviceMap"
                  :item="item"
                  type="demo"
                  :button-text="$t('title.demo')"
                  class="m-auto m-md-0"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <CreateDemoAccountView
      ref="CreateDemoAccountViewRef"
      @demo-account-created="getDemoAccounts"
    />
    <CreateLiveAccountView
      ref="CreateLiveAccountViewRef"
      @live-account-created="getPendingApplication"
    />
    <CreateWholesaleReferralView
      ref="CreateWholesaleReferralViewRef"
      @live-account-created="getPendingApplication"
    />
    <ChangeLeverageForm ref="changeLeverageFormRef" />
    <ResetPasswordForm ref="resetPasswordFormRef" />
    <CreateDepositModalV2 ref="createDepositModalRefV2" />
  </div>
</template>
<script lang="ts" setup>
import { useStore } from "@/store";
import { nextTick, onMounted, ref } from "vue";
import CreateDemoAccountView from "./modal/CreateDemoAccountView.vue";
import CreateLiveAccountView from "./modal/CreateLiveAccountView.vue";
import CreateWholesaleReferralView from "./modal/CreateWholesaleReferral.vue";
import {
  ApplicationStatusType,
  ApplicationType,
} from "@/core/types/ApplicationInfos";
import AccountService from "../services/AccountService";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { ServiceMapType, ServiceTypes } from "@/core/types/ServiceTypes";
import ChangeLeverageForm from "./modal/ChangeLeverageForm.vue";
import ResetPasswordForm from "./modal/ResetPasswordForm.vue";
import TradeAccountCard from "@/components/TradeAccountCard.vue";
import CreateDepositModalV2 from "@/projects/client/components/funding/CreateDepositModal.vue";
import { reinitializeComponents } from "@/core/plugins/plugins";
import Can from "@/core/plugins/ICan";
import { useRouter } from "vue-router";
import { isMobile } from "@/core/config/WindowConfig";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";

const store = useStore();
const isLoading = ref(true);
const router = useRouter();
const serviceMap = ref<ServiceMapType>();
const pendingApplications = ref(Array<any>());
const liveAccounts = ref(Array<any>());
const demoAccounts = ref(Array<any>());
const CreateLiveAccountViewRef =
  ref<InstanceType<typeof CreateLiveAccountView>>();

const CreateWholesaleReferralViewRef =
  ref<InstanceType<typeof CreateWholesaleReferralView>>();

const CreateDemoAccountViewRef =
  ref<InstanceType<typeof CreateDemoAccountView>>();

const changeLeverageFormRef = ref<InstanceType<typeof ChangeLeverageForm>>();
const resetPasswordFormRef = ref<InstanceType<typeof ResetPasswordForm>>();
const createDepositModalRefV2 =
  ref<InstanceType<typeof CreateDepositModalV2>>();
const wholesaleEnable = ref(false);

const currentTab = ref(Can.cans(["Client"]) ? "RealAccounts" : "DemoAccounts");

const props = defineProps({
  showEventShop: Boolean,
});
const getLiveAccounts = async () => {
  try {
    const res = await AccountService.queryAccounts({
      // if the client is IB, only shows the accounts that have trade account
      // if the client is not IB, shows all accounts
      // hasTradeAccount: Can.cans(["IB"]) ? true : undefined,
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
    liveAccounts.value = res.data;
  } catch (error) {
    // console.log(error);
  }
};

const getDemoAccounts = async () => {
  try {
    const { data } = await AccountService.getDemoAccounts();
    demoAccounts.value = data;
  } catch (error) {
    // console.log(error);
  }
};

const getPendingApplication = async () => {
  const { data } = await AccountService.queryApplications({
    statuses: [
      ApplicationStatusType.AwaitingApproval,
      ApplicationStatusType.Approved,
    ],
    type: ApplicationType.TradeAccount,
  });
  pendingApplications.value = data;
};

const onChangeLeverage = (_tradeAccount) => {
  changeLeverageFormRef.value?.show(_tradeAccount);
};

const onResetPassword = (_tradeAccount) => {
  resetPasswordFormRef.value?.show(_tradeAccount);
};

const openDirectDepositModal = (account: any) => {
  createDepositModalRefV2.value?.show(account);
};

// const goToWebTrader = () => {
//   router.push("/webtrader");
// };

onMounted(async () => {
  isLoading.value = true;
  // serviceMap.value = await GlobalService.getServiceMap();
  wholesaleEnable.value = store.state.AuthModule.config.wholesaleEnabled;

  if (Can.cans(["Client"])) {
    await getPendingApplication();
    await getLiveAccounts();
  }
  serviceMap.value = await GlobalService.getServiceMap();
  await getDemoAccounts();

  isLoading.value = false;

  await nextTick();
  reinitializeComponents();
});
</script>
<style scoped lang="scss">
.account-height {
  min-height: 64px;
  @media (max-width: 1512px) {
    min-height: 57px;
  }
}
.account-no-data {
  min-height: 244px;
  @media (max-width: 1512px) {
    min-height: 264px;
  }
}

.account-no-client {
  min-height: 556px;
  @media (max-width: 1512px) {
    min-height: 576px;
  }
}
</style>
