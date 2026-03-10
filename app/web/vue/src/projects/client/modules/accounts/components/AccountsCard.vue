<template>
  <div v-if="isLoading" id="accountDetailOverview" class="card mb-5 mb-xl-8">
    <LoadingCentralBox />
  </div>
  <div v-if="!isLoading" id="accountDetailOverview" class="card mb-5 mb-xl-8">
    <div class="card-header border-0">
      <router-link to="/account" class="card-label fs-5 text-muted mt-5"
        ><i class="fa-solid fa-angle-left me-2"></i
        >{{ $t("action.backToAccountList") }}
      </router-link>
      <div class="card-toolbar">
        <span class="svg-icon svg-icon-3">
          <div>
            <button
              type="button"
              class="setting-btn btn btn-sm btn-icon btn-active-white"
              data-kt-menu-trigger="click"
              data-kt-menu-attach="parent"
              data-kt-menu-placement="bottom-end"
              data-kt-menu-flip="bottom"
              @click="boxClicked = true"
            >
              <span class="svg-icon svg-icon-2">
                <inline-svg src="/images/icons/general/gen063.svg"
              /></span>
            </button>

            <div
              class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary menu-arrow-gray-400 py-2 fs-6 w-200px"
              data-kt-menu="true"
            >
              <div class="menu-item px-2">
                <div
                  class="menu-link"
                  :class="{
                    'menu-disabled': passwordChangeActivity,
                  }"
                  @click="onResetPassword"
                >
                  {{ $t("action.changePassword") }}
                </div>
              </div>

              <div class="menu-item px-2">
                <div
                  class="menu-link"
                  :class="{
                    'menu-disabled': leverageChangeActivity,
                  }"
                  @click="onChangeLeverage"
                >
                  {{ $t("action.changeLeverage") }}
                </div>
              </div>

              <!-- !$can('Wholesale')|| -->
              <div
                class="menu-item px-2"
                v-if="$can('SuperAdmin') || wholesaleEnable"
              >
                <div class="menu-link">
                  <router-link
                    :to="
                      '/account/' + currentAccountNumber + '/apply-wholesale'
                    "
                  >
                    {{ $t("action.upgradeWholesale") }}
                  </router-link>
                </div>
              </div>
            </div>
          </div>
        </span>
      </div>
    </div>

    <!--begin::Body-->
    <div class="card-body pt-0">
      <div class="row">
        <div class="col-sm-4 mb-3 mb-sm-0" :class="{ 'border-end': !isMobile }">
          <div>
            <img
              v-if="props.currentAccount.tradeAccount.currencyId"
              :src="
                '/images/currency/' +
                props.currentAccount.tradeAccount.currencyId +
                '.svg'
              "
              class="me-2 w-25px"
              alt=""
              style="border-radius: 50%"
            /><span>{{ $t("fields.currentAccount") }}</span>
          </div>
          <div
            class="d-flex align-items-center mt-5 pb-2 mobile_border"
            :class="{ '': isMobile }"
          >
            <CopyBox
              :val="currentAccountNumber + ''"
              :showText="false"
              :hasIcon="true"
            />

            <el-select v-model="currentAccountNumber" class="w-150px">
              <el-option
                v-for="item in props.accountsList"
                :key="item.tradeAccount.accountNumber"
                :label="item.tradeAccount.accountNumber"
                :value="item.tradeAccount.accountNumber"
              />
            </el-select>
            <router-link
              class="ms-3"
              :to="newUrl"
              style="font-weight: 400; font-size: 14px; color: #002957"
              >{{ $t("action.change") }}</router-link
            >
          </div>
        </div>

        <div
          :class="{
            'col-sm-5 ps-5 pt-5 pt-md-0': !isMobile,
            'col-sm-5 ps-5 pt-1 pt-md-0': isMobile,
          }"
        >
          <div
            :class="{
              'd-flex justify-content-between mb-2': isMobile,
              'row mb-5': !isMobile,
            }"
          >
            <div class="col-sm-6 col-xl-4">
              <label>{{ $t("fields.type") }}</label>
              <div class="info">
                {{ $t(`type.account.${props.currentAccount.type}`) }}
              </div>
            </div>
            <div
              class="col-sm-6 col-xl-4 d-flex flex-column justify-content-between"
            >
              <label>{{ $t("fields.leverage") }}</label>
              <div class="info">
                {{ props.currentAccount.tradeAccount.leverage + ":1" }}
              </div>
              <div class="new-info" v-if="leverageChangeActivity">
                <span class="text-lowercase fs-8 text-info"
                  >({{
                    $t("status.pending") +
                    ": " +
                    leverageChangeActivity.supplement.leverage +
                    ":1"
                  }})</span
                >
              </div>
            </div>
          </div>
          <div
            :class="{
              'd-flex justify-content-between ': isMobile,
              'row ': !isMobile,
            }"
          >
            <div class="col-sm-6 col-xl-4">
              <label>{{ $t("fields.server") }}</label>
              <div class="info">
                {{
                  serviceMap[props.currentAccount.tradeAccount.serviceId]
                    .serverName
                }}
              </div>
            </div>
            <div class="col-sm-6 col-xl-4">
              <label>{{ $t("fields.platform") }}</label>
              <div class="info">
                {{
                  serviceMap[props.currentAccount.tradeAccount.serviceId]
                    .platformName
                }}
              </div>
            </div>
          </div>
        </div>
        <div v-if="isMobile" class="col-sm-5 ps-5 pt-2 pt-md-0">
          <div class="d-flex justify-content-between align-items-center mb-5">
            <div class="col-sm-6 col-xl-4">
              <label style="font-size: 14px; font-weight: 400; color: #717171">
                {{ $t("fields.balance") }}
              </label>
              <div
                style="
                  font-weight: 500;
                  font-size: 16px;
                  line-height: 28px;
                  /* identical to box height, or 140% */

                  color: #000000;
                "
              >
                <BalanceShow
                  :balance="props.currentAccount.tradeAccount?.balanceInCents"
                  :currency-id="props.currentAccount.tradeAccount?.currencyId"
                />
                {{
                  $t(
                    `type.currency.${props.currentAccount.tradeAccount?.currencyId}`
                  )
                }}
              </div>
            </div>
          </div>
        </div>
        <div v-else class="col-sm-3">
          <label style="font-size: 14px; font-weight: 400; color: #717171">
            {{ $t("fields.balance") }}
          </label>
          <div
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
              :balance="props.currentAccount.tradeAccount?.balanceInCents"
              :currency-id="props.currentAccount.tradeAccount?.currencyId"
            />
            {{
              $t(
                `type.currency.${props.currentAccount.tradeAccount?.currencyId}`
              )
            }}
          </div>

          <label
            class="pt-2"
            style="font-size: 14px; font-weight: 400; color: #717171"
          >
            {{ $t("fields.credit") }}
          </label>
          <div
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
              :balance="props.currentAccount.tradeAccount?.creditInCents"
              :currency-id="props.currentAccount.tradeAccount?.currencyId"
            />
            {{
              $t(
                `type.currency.${props.currentAccount.tradeAccount?.currencyId}`
              )
            }}
          </div>
        </div>
        <div
          v-if="isMobile"
          class="d-flex align-items-center justify-content-between text-nowrap gap-1"
        >
          <button class="btn btn-primary" @click.prevent="openDepositPanel">
            {{ t(`action.deposit`) }}
          </button>

          <button class="btn btn-primary" @click.prevent="openWithdrawalModal">
            {{ t(`title.withdrawal`) }}
          </button>
          <button
            v-if="enableTransfer(permissions) === true"
            class="btn btn-primary"
            @click.prevent="openTransferToAccountPanel"
          >
            {{ t(`action.transfer`) }}
          </button>
        </div>
      </div>
    </div>
    <!--end::Body-->
  </div>

  <CreateDepositModal
    ref="depositFormRef"
    @on-created="handleEmits('depositSubmit')"
  />
  <CreateWithdrawalModal
    ref="createWithdrawalModalRef"
    @on-created="handleEmits('withdrawalSubmit')"
  />

  <TransferToAccountForm
    ref="transferToAccountFormRef"
    @on-created="handleEmits('transferSubmit')"
  />
  <ChangeLeverageForm
    ref="changeLeverageFormRef"
    @submit="getAccountChangePasswordAndLeverageActivities"
  />
  <ResetPasswordForm
    ref="resetPasswordFormRef"
    @submit="getAccountChangePasswordAndLeverageActivities"
  />
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import {
  ref,
  onMounted,
  nextTick,
  watch,
  computed,
  inject,
  defineEmits,
} from "vue";
import ResetPasswordForm from "./modal/ResetPasswordForm.vue";
import ChangeLeverageForm from "./modal/ChangeLeverageForm.vue";
import { reinitializeComponents } from "@/core/plugins/plugins";
import { ServiceMapType } from "@/core/types/ServiceTypes";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useRoute } from "vue-router";
import AccountService from "@/projects/client/modules/accounts/services/AccountService";
import {
  ApplicationStatusType,
  ApplicationType,
} from "@/core/types/ApplicationInfos";
import { isMobile } from "@/core/config/WindowConfig";
import { PublicSetting } from "@/core/types/ConfigTypes";
import { AccountTypes } from "@/core/types/AccountInfos";
import CopyBox from "@/components/CopyBox.vue";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";
import i18n from "@/core/plugins/i18n";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import CreateDepositModal from "@/projects/client/components/funding/CreateDepositModal.vue";
import CreateWithdrawalModal from "@/projects/client/components/funding/CreateWithdrawModal.vue";
import TransferToAccountForm from "./modal/TransferToAccountForm.vue";
import { CreditCard, Wallet, Money } from "@element-plus/icons-vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { enableTransfer } from "@/core/helpers/permissionHelpers";
const depositFormRef = ref<InstanceType<typeof CreateDepositModal>>();
const createWithdrawalModalRef =
  ref<InstanceType<typeof CreateWithdrawalModal>>();

const transferToAccountFormRef =
  ref<InstanceType<typeof TransferToAccountForm>>();

const getAccountDetails = inject(AccountInjectionKeys.GET_ACCOUNT_DETAILS);
const accountDetails = ref<any>(null);
const permissions = computed(() => accountDetails?.value?.permission ?? null);
const { t } = i18n.global;
const store = useStore();
const user = store.state.AuthModule.user;
const config = user.configurations;
const projectConfig = computed<PublicSetting>(
  () => store.state.AuthModule.config
);
const emits = defineEmits([
  "depositSubmit",
  "withdrawalSubmit",
  "transferSubmit",
]);
const handleEmits = (emit) => {
  emits(emit);
};
const props = defineProps<{
  accountsList: Array<any>;
  currentAccount: any;
  tab: string;
}>();

const wholesaleEnable = ref(false);
const route = useRoute();
const leverageChangeActivity = ref<any>();
const passwordChangeActivity = ref<any>();
const isLoading = ref(true);
const boxClicked = ref(false);
const serviceMap = ref<ServiceMapType>();
const currentAccountNumber = ref(
  props.currentAccount.tradeAccount?.accountNumber
);

const disabledTransfer = () => {
  var isDisabled = false;
  config.forEach((item: any) => {
    if (item.key === "DisableTransfer") {
      isDisabled = item.value;
    }
  });

  return isDisabled;
};

const newUrl = computed(
  () => `/account/${currentAccountNumber.value}/${props.tab}`
);
watch(
  () => route.params.accountNumber,
  async () => {
    currentAccountNumber.value = parseInt(route.params.accountNumber as string);

    await getAccountChangePasswordAndLeverageActivities();
  }
);
const resetPasswordFormRef = ref<InstanceType<typeof ResetPasswordForm>>();
const changeLeverageFormRef = ref<InstanceType<typeof ChangeLeverageForm>>();

const onResetPassword = () => {
  if (passwordChangeActivity.value) {
    return;
  }
  resetPasswordFormRef.value?.show(props.currentAccount.tradeAccount);
};

const onChangeLeverage = () => {
  if (leverageChangeActivity.value) {
    return;
  }
  changeLeverageFormRef.value?.show(props.currentAccount.tradeAccount);
};

const getAccountChangePasswordAndLeverageActivities = async () => {
  //
  const leverageRes = await AccountService.queryApplications({
    referenceId: props.currentAccount.uid,
    type: ApplicationType.TradeAccountChangeLeverage,
    status: ApplicationStatusType.AwaitingApproval,
  });
  if (leverageRes.data.length > 0) {
    leverageChangeActivity.value = leverageRes.data[0];
  }

  const passwordRes = await AccountService.queryApplications({
    referenceId: props.currentAccount.uid,
    type: ApplicationType.TradeAccountChangePassword,
    status: ApplicationStatusType.AwaitingApproval,
  });

  if (passwordRes.data.length > 0) {
    passwordChangeActivity.value = passwordRes.data[0];
  }
};

const openDepositPanel = () => {
  depositFormRef.value?.show(accountDetails.value);
};

const openWithdrawalModal = () => {
  createWithdrawalModalRef.value?.show(true, accountDetails.value);
};

const openTransferToAccountPanel = () => {
  const filteredAccountList = props.accountsList.filter(
    (item) =>
      item.role === AccountRoleTypes.Client &&
      item.currencyId === accountDetails.value.tradeAccount.currencyId &&
      item.fundType == accountDetails.value.fundType &&
      item.tradeAccount.accountNumber !==
        accountDetails.value.tradeAccount.accountNumber
  );

  transferToAccountFormRef.value?.show(
    filteredAccountList,
    accountDetails.value.tradeAccount.accountNumber,
    accountDetails.value.uid,
    accountDetails.value.tradeAccount.currencyId,
    accountDetails.value.fundType,
    accountDetails.value.tradeAccount.balance,
    accountDetails.value
  );
};

onMounted(async () => {
  wholesaleEnable.value = store.state.AuthModule.config.wholesaleEnabled;
  accountDetails.value = getAccountDetails?.();
  try {
    isLoading.value = true;
    serviceMap.value = await GlobalService.getServiceMap();

    await getAccountChangePasswordAndLeverageActivities();
    await nextTick();
    reinitializeComponents();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
});
</script>

<style scoped lang="scss">
#accountDetailOverview label {
  font-weight: 500;
  font-size: 12px;
  color: #717171;
}

#accountDetailOverview .info {
  font-weight: 400;
  font-size: 16px;
  color: #212121;
}

#accountDetailOverview.new-info {
  font-weight: 400;
  font-size: 16px;
  color: #002957;
}

@media (max-width: 767.98px) {
  .mobile_border {
    border-bottom: solid 1px #e4e6ef;
  }
}
.menu-column {
  width: max-content !important;
  min-width: 275px;
  & .menu-item {
    .menu-link {
      .menu-title {
        color: #081735;
        .title-type {
          color: #868d98;
        }
      }
      color: #081735;
      &::after {
        content: none !important;
        display: none !important;
      }
    }
  }
}
</style>
