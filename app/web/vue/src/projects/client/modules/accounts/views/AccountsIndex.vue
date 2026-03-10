<template>
  <div class="card mb-2" :class="isMobile ? 'mx-3' : ''">
    <div class="card-header border-0">
      <div class="card-title fw-bold">{{ $t("title.tradeAccounts") }}</div>
      <div class="card-toolbar">
        <button
          class="btn btn-light-secondary text-primary btn-sm"
          style="
            box-shadow: 4px 4px 6px 0px rgba(113, 113, 113, 0.25) !important;
          "
          @click="CreateLiveAccountViewRef?.show()"
        >
          {{ $t("action.createTradeAccount") }}
        </button>
      </div>
    </div>
    <!-- <div class="separator mx-auto" style="width: 95%"></div> -->
    <div class="card-body">
      <div class="d-flex justify-content-center" v-if="isLoading">
        <LoadingRing />
      </div>
      <div v-else class="">
        <div :class="isMobile ? 'd-flex overflow-auto gap-4' : 'row'">
          <NoDataCentralBox
            v-if="pendingApplications.length === 0 && liveAccounts.length === 0"
          />
          <div
            class="col-12 col-md-4 d-flex justify-content-center mb-5"
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
            class="col-12 col-md-4 d-flex justify-content-center mb-5"
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
              @on-change-leverage="onChangeLeverage(item?.tradeAccount)"
              @on-reset-password="onResetPassword(item?.tradeAccount)"
              :button-handler="
                () => goToDetailPage(item?.tradeAccount?.accountNumber)
              "
              :button-text="
                item.hasTradeAccount
                  ? $t('action.details')
                  : $t('status.pendingTtd') + '...'
              "
              class="m-auto m-md-0"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
  <div class="card account-box" :class="isMobile ? 'mx-3' : ''">
    <div class="card-header border-0">
      <div class="card-title fw-bold">{{ $t("title.demoAccounts") }}</div>
      <div class="card-toolbar">
        <button
          class="btn btn-sm btn-light-secondary text-primary"
          style="
            box-shadow: 4px 4px 6px 0px rgba(113, 113, 113, 0.25) !important;
          "
          @click="CreateDemoAccountViewRef?.show()"
        >
          {{ $t("action.createDemoAccount") }}
        </button>
      </div>
    </div>
    <!-- <div class="separator mx-auto" style="width: 95%"></div> -->
    <div class="card-body">
      <NoDataCentralBox v-if="demoAccounts.length === 0 && !isLoading" />
      <div class="d-flex justify-content-center" v-if="isLoading">
        <LoadingRing />
      </div>
      <div v-else class="">
        <div :class="isMobile ? 'd-flex overflow-auto gap-4' : 'row'">
          <div
            class="col-12 col-md-4 d-flex justify-content-center mb-5"
            v-for="(item, index) in demoAccounts"
            :key="index"
          >
            <TradeAccountCard
              :serviceMap="serviceMap"
              :item="item"
              type="demo"
              :showWebtrader="
                Object.values(ServiceTypes).includes(item.serviceId)
              "
              :button-text="$t('title.demo')"
              class="m-auto m-md-0"
            />
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
  <ChangeLeverageForm ref="changeLeverageFormRef" />
  <ResetPasswordForm ref="resetPasswordFormRef" />
</template>
<script lang="ts" setup>
import { ServiceTypes } from "@/core/types/ServiceTypes";
import { ref, onMounted, nextTick } from "vue";
import CreateDemoAccountView from "../components/modal/CreateDemoAccountView.vue";
import CreateLiveAccountView from "../components/modal/CreateLiveAccountView.vue";
import {
  ApplicationStatusType,
  ApplicationType,
} from "@/core/types/ApplicationInfos";
import AccountService from "../services/AccountService";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import { ServiceMapType } from "@/core/types/ServiceTypes";
import ChangeLeverageForm from "../components/modal/ChangeLeverageForm.vue";
import ResetPasswordForm from "../components/modal/ResetPasswordForm.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import TradeAccountCard from "@/components/TradeAccountCard.vue";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { useRouter } from "vue-router";
import { isMobile } from "@/core/config/WindowConfig";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import {
  AccountRoleTypes,
  AccountStatusTypes,
} from "@/core/types/AccountInfos";

const isLoading = ref(true);
const serviceMap = ref<ServiceMapType>();
const pendingApplications = ref(Array<any>());
const liveAccounts = ref(Array<any>());
const demoAccounts = ref(Array<any>());

const CreateLiveAccountViewRef =
  ref<InstanceType<typeof CreateLiveAccountView>>();

const CreateDemoAccountViewRef =
  ref<InstanceType<typeof CreateDemoAccountView>>();

const changeLeverageFormRef = ref<InstanceType<typeof ChangeLeverageForm>>();
const resetPasswordFormRef = ref<InstanceType<typeof ResetPasswordForm>>();

const router = useRouter();

const getLiveAccounts = async () => {
  try {
    const res = await AccountService.queryAccounts({
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

const goToDetailPage = (accountNumber: number) => {
  router.push("/account/" + accountNumber);
};

const onResetPassword = (_tradeAccount) => {
  resetPasswordFormRef.value?.show(_tradeAccount);
};

onMounted(async () => {
  isLoading.value = true;
  serviceMap.value = await GlobalService.getServiceMap();
  await getPendingApplication();
  await getLiveAccounts();
  await getDemoAccounts();

  isLoading.value = false;

  await nextTick();
  reinitializeComponents();
});
</script>
<style scoped></style>
