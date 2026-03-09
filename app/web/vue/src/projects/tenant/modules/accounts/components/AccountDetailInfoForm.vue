<template>
  <div>
    <div class="card">
      <div class="card-header">
        <div class="card-title">
          <div>{{ $t("fields.info") }}</div>
        </div>
        <div class="card-toolbar">
          <el-button type="primary" plain :icon="User" @click="onUserClicked">
            {{ $t("title.userDetails") }}
          </el-button>
          <el-button
            type="success"
            plain
            :icon="Refresh"
            @click="refreshData(true)"
          >
            {{ $t("action.refresh") }}
          </el-button>
        </div>
      </div>
      <div class="card-body py-4">
        <div class="d-flex gap-4">
          <div class="borders">
            <div class="head">{{ $t("fields.userName") }}</div>
            <div class="content">{{ accountDetails?.name }}</div>
          </div>

          <div class="borders">
            <div class="head">Party ID</div>
            <div class="content">{{ accountDetails?.partyId }}</div>
          </div>

          <div class="borders">
            <div class="head">UID</div>
            <div class="content">{{ accountDetails?.uid }}</div>
          </div>

          <div class="borders">
            <div class="head">Account ID</div>
            <div class="content">{{ accountDetails?.id }}</div>
          </div>

          <div class="borders">
            <div class="head">{{ $t("fields.accountNumber") }}</div>
            <div class="content">
              <span
                v-if="accountDetails?.accountNumber == 0"
                class="text-danger"
              >
                {{ $t("fields.noTradeAccount") }}
              </span>
              <span v-else>
                <span>{{ accountDetails?.accountNumber }}</span>
                <div v-if="initialPasswordData" class="mt-2">
                  <div class="d-flex flex-column gap-1 text-start">
                    <small class="text-muted">
                      {{ $t("fields.initialTradePassword") }}:
                      <span class="fw-bold text-primary">{{
                        initialPasswordData.initialMainPassword
                      }}</span>
                    </small>
                    <small class="text-muted">
                      {{ $t("fields.initialInvestorPassword") }}:
                      <span class="fw-bold text-primary">{{
                        initialPasswordData.initialInvestorPassword
                      }}</span>
                    </small>
                  </div>
                </div>
              </span>
            </div>
          </div>

          <div class="borders">
            <div class="head">{{ $t("fields.accountStatus") }}</div>
            <div class="content">
              {{ $t(`type.accountStatus.${accountDetails?.status}`) }}
            </div>
          </div>
        </div>
        <el-divider />

        <div class="px-2">
          <p class="title">{{ $t("fields.basicInfo") }}</p>
          <div class="d-flex gap-4">
            <div class="borders">
              <div class="head">{{ $t("fields.fundType") }}</div>
              <div class="content">
                {{ $t(`type.fundType.${accountDetails?.fundType}`) }}
              </div>
            </div>
            <div class="borders">
              <div class="head">{{ $t("fields.accountRole") }}</div>
              <div class="content">
                {{ $t(`type.accountRole.${accountDetails?.role}`) }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.site") }}</div>
              <div class="content">
                {{ $t(`type.siteType.${accountDetails?.siteId}`) }}
              </div>
            </div>
            <div class="borders">
              <div class="head">{{ $t("fields.accountType") }}</div>
              <div class="content">
                {{ $t(`type.account.${accountDetails?.type}`) }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.group") }}</div>
              <div class="content">
                {{ accountDetails?.group }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.createdOn") }}</div>
              <div class="content">
                <TimeShow
                  :date-iso-string="accountDetails?.createdOn"
                  format="YYYY-MM-DD"
                />
              </div>
            </div>
          </div>
        </div>

        <el-divider v-if="accountDetails.hasTradeAccount == true" />

        <div v-if="accountDetails.hasTradeAccount == true" class="mt-6 px-2">
          <p class="title">{{ $t("fields.tradingAccountInfo") }}</p>
          <div class="d-flex gap-4">
            <div class="borders">
              <div class="head">{{ $t("fields.accountNumber") }}</div>
              <div class="content">
                {{ accountDetails?.accountNumber }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.currency") }}</div>
              <div class="content">
                {{
                  t(`type.currency.${accountDetails.tradeAccount?.currencyId}`)
                }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.platform") }}</div>
              <div class="content">
                <!-- {{ ServiceTypes[accountDetails.tradeAccount?.serviceId] }} -->
                {{
                  serviceMap[accountDetails.tradeAccount?.serviceId]?.serverName
                }}
              </div>
            </div>

            <div class="borders">
              <div class="head">MT Group</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.group }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.lastLogin") }}</div>
              <div class="content">
                <TimeShow
                  v-if="
                    accountDetails.tradeAccount.tradeAccountStatus?.lastLoginOn
                  "
                  :date-iso-string="
                    accountDetails.tradeAccount.tradeAccountStatus?.lastLoginOn
                  "
                  format="YYYY-MM-DD"
                />
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.createdOn") }}</div>
              <div class="content">
                <TimeShow
                  :date-iso-string="accountDetails.tradeAccount?.createdOn"
                  format="YYYY-MM-DD"
                />
              </div>
            </div>
          </div>

          <div class="d-flex gap-4 mt-4">
            <div class="borders">
              <div class="head">{{ $t("fields.leverage") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.leverage }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.balance") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.balance }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.equity") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.equity }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.margin") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.margin }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.marginLevel") }}</div>
              <div class="content">
                {{
                  accountDetails.tradeAccount.tradeAccountStatus?.marginLevel
                }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.marginFree") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.marginFree }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.credit") }}</div>
              <div class="content">
                {{ accountDetails.tradeAccount.tradeAccountStatus?.credit }}
              </div>
            </div>

            <div class="borders">
              <div class="head">{{ $t("fields.interestRate") }}</div>
              <div class="content">
                {{
                  accountDetails.tradeAccount.tradeAccountStatus?.interestRate
                }}
              </div>
            </div>
          </div>
        </div>
        <el-divider />
        <div class="px-2">
          <p class="title">{{ $t("fields.relationship") }}</p>
          <div>
            <div v-if="accountDetails.role != 100" class="d-flex gap-4">
              <div class="borders">
                <div class="head">{{ $t("fields.referCode") }}</div>
                <div class="content">{{ accountDetails?.referCode }}</div>
              </div>

              <div class="borders">
                <div class="head">{{ $t("fields.directIbId") }}</div>
                <div class="content">
                  {{ accountDetails?.agentAccountId }}
                </div>
              </div>

              <div class="borders">
                <div class="head">{{ $t("fields.directSalesId") }}</div>
                <div class="content">
                  {{ accountDetails?.salesAccountId }}
                </div>
              </div>
            </div>
            <div v-if="accountDetails?.referPathUids" class="mt-4">
              <div class="refer-path-border">
                <div class="head">{{ $t("fields.referralPath") }}</div>
                <div class="content d-flex mt-1">
                  <div
                    v-for="(item, index) in accountDetails?.referPathUids"
                    :key="index"
                    class="d-flex align-items-center gap-1 flex-wrap"
                  >
                    <el-tag type="info" size="small" effect="plain">
                      {{ item }}
                    </el-tag>
                    <el-icon
                      v-if="index != accountDetails.referPathUids.length - 1"
                      ><ArrowRight
                    /></el-icon>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
        <el-divider />
        <div class="px-2" v-if="$cans(['TenantAdmin', 'AccountAdmin'])">
          <p class="title">{{ $t("action.changePassword") }}</p>
          <div class="row row-cols-6 mb-3">
            <el-button
              class="col"
              @click.prevent="openPasswordChangePanel('main')"
              :disabled="!tradeAccount"
            >
              {{ $t("action.changeTradePassword") }}
            </el-button>
            <el-button
              class="col"
              @click.prevent="openPasswordChangePanel('investor')"
              :disabled="!tradeAccount"
            >
              {{ $t("action.changeInvestorPassword") }}
            </el-button>
            <el-button
              class="col"
              @click.prevent="openPasswordChangePanel('login')"
            >
              {{ $t("action.changeLoginUserPassword") }}
            </el-button>
          </div>
          <div class="row row-cols-6">
            <el-button
              class="col"
              @click.prevent="openResetToInitialPanel('main')"
              :disabled="!tradeAccount"
            >
              {{ $t("action.resetToInitialPassword") }}
            </el-button>
            <el-button
              class="col"
              @click.prevent="openPasswordHistoryDialog"
              :disabled="!accountDetails"
            >
              {{ $t("action.viewPasswordHistory") }}
            </el-button>
          </div>
        </div>
        <el-divider />
        <div class="px-2">
          <p class="title">{{ $t("action.action") }}</p>
          <div class="row row-cols-6">
            <!-- <el-button class="col" @click.prevent="openPasswordChangePanel">
              {{ $t("action.changePassword") }}
            </el-button> -->
            <el-button class="col" @click.prevent="openChangeSalesOrAgentForm">
              <div class="d-flex flex-column">
                <span>{{ $t("action.changeSalesOrIb") }}</span>
              </div>
            </el-button>

            <el-button class="col" @click.prevent="openEditAccountInfoPanel(3)">
              {{ $t("action.changeStatus") }}
            </el-button>

            <el-button @click="openEditAccountInfoPanel(0)">
              {{ $t("fields.changeSiteId") }}
            </el-button>
            <el-button @click="openEditAccountInfoPanel(1)">
              {{ $t("fields.changeAccountType") }}
            </el-button>
          </div>
          <div class="row row-cols-6 mt-4">
            <el-button
              class="col"
              @click.prevent="editGroup()"
              v-if="accountDetails.role == AccountRoleTypes.IB"
            >
              {{ $t("fields.changeGroup") }}
            </el-button>
            <el-button
              class="col"
              v-if="
                accountDetails.role == AccountRoleTypes.Client &&
                accountDetails.hasTradeAccount == true
              "
              @click.prevent="openEditAccountInfoPanel(4)"
            >
              {{ $t("action.changeLeverage") }}
            </el-button>

            <el-button
              v-if="accountDetails.role == AccountRoleTypes.Client"
              class="col"
              @click.prevent="openChangeAdjustPanel"
            >
              {{ $t("action.changeAdjust") }}
            </el-button>
            <el-button
              v-if="accountDetails.role == AccountRoleTypes.Client"
              class="col"
              @click.prevent="openChangeCreditPanel"
            >
              {{ $t("action.changeCredit") }}
            </el-button>
            <el-button @click="openEditAccountInfoPanel(2)">
              {{ $t("fields.changeFundType") }}
            </el-button>
            <div
              v-if="accountDetails.role != AccountRoleTypes.Client"
              class="d-flex justify-content-center"
            >
              <el-switch
                v-model="levelRule"
                inline-prompt
                size="large"
                style="
                  --el-switch-on-color: #137640;
                  --el-switch-off-color: #c03636;
                "
                active-text=" Level Rule On "
                inactive-text=" Level Rule Off "
                inactive-value="0"
                active-value="1"
                @click.prevent="HasLevelRule"
              >
              </el-switch>
            </div>
          </div>
        </div>
        <el-divider />
        <div class="px-2">
          <p class="title">{{ $t("fields.permission") }}</p>
          <AccountPermission />
        </div>

        <el-divider />
        <div class="px-2">
          <p class="title">{{ $t("title.options") }}</p>
          <AccountTags :accountDetails="accountDetails" />
        </div>
      </div>
    </div>
    <ChangePasswordForm ref="changePasswordFormRef" />
    <ChangeCreditForm ref="changeCreditFormRef" />
    <ChangeAdjustForm ref="changeAdjustFormRef" />
    <ChangeGroupForm ref="editGroupFormRef" />
    <PasswordHistoryDialog ref="passwordHistoryDialogRef" />
    <ChangeSalesOrAgentForm
      ref="changeSalesOrAgentFormRef"
      @re-assign-finished="onReAssignFinished"
    />

    <EditAccountInfo ref="editAccountInfoRef" @refresh="refreshData" />
    <EditSalesAndAgentModal
      ref="editSalesAndAgentModalRef"
      @update="refreshData"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, inject, watchEffect } from "vue";
import {
  ConfigAllAccountTypeSelections,
  AccountStatusOptions,
  AccountRoleTypes,
} from "@/core/types/AccountInfos";
import { backendConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import { ConfigFundTypeSelections } from "@/core/types/FundTypes";
import AccountService from "../services/AccountService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import i18n from "@/core/plugins/i18n";
import ChangePasswordForm from "./ChangePasswordForm.vue";
import ChangeCreditForm from "@/projects/tenant/modules/Payment/components/ChangeCreditForm.vue";
import ChangeAdjustForm from "@/projects/tenant/modules/Payment/components/ChangeAdjustForm.vue";
import ChangeGroupForm from "./form/ChangeGroupForm.vue";
import PasswordHistoryDialog from "./PasswordHistoryDialog.vue";
import ChangeSalesOrAgentForm from "@/projects/tenant/modules/accounts/components/ChangeSalesOrAgentForm.vue";
import EditSalesAndAgentModal from "@/projects/tenant/modules/accounts/components/modal/EditSalesAndAgentModal.vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";
import AccountTags from "./form/AccountTags.vue";
import { ElNotification } from "element-plus";
import { Refresh, ArrowRight, User } from "@element-plus/icons-vue";
import EditAccountInfo from "./modal/EditAccountInfo.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountPermission from "./form/AccountPermission.vue";
import Can from "@/core/plugins/ICan";
const $cans = Can.cans;
const { t } = i18n.global;
const emits = defineEmits<{
  (event: "change-status", status): void;
}>();

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const editAccountInfoRef = ref<InstanceType<typeof EditAccountInfo>>();

const changePasswordFormRef = ref<InstanceType<typeof ChangePasswordForm>>();
const changeCreditFormRef = ref<InstanceType<typeof ChangeCreditForm>>();
const changeAdjustFormRef = ref<InstanceType<typeof ChangeAdjustForm>>();
const passwordHistoryDialogRef =
  ref<InstanceType<typeof PasswordHistoryDialog>>();
const changeSalesOrAgentFormRef =
  ref<InstanceType<typeof ChangeSalesOrAgentForm>>();
const editSalesAndAgentModalRef =
  ref<InstanceType<typeof EditSalesAndAgentModal>>();

const handleRefreshTableData = inject(
  AccountInjectionKeys.HANDLE_REFRESH_ACCOUNT_CLIENT_TABLE
);

const services = ref(Array<any>());
const serviceMap = ref({} as any);
const initialPasswordData = ref<any>(null);
const serverSelections = computed(() =>
  services.value
    .filter((service: any) => service.id === accountDetails.value.serviceId)
    .map((filteredItem) => filteredItem.groups)
    .reduce((acc, groups) => {
      return acc.concat(
        groups.map((group, index) => ({ label: group, value: index }))
      );
    }, [])
);

let editAccountOptions = [] as any[];
watchEffect(() => {
  // Update editAccountOptions when serverSelections changes
  editAccountOptions = [
    {
      label: t("fields.changeSiteId"),
      vModel: accountDetails.value.siteId,
      type: "siteId",
      options: ConfigSiteTypesSelections.value,
    },
    {
      label: t("fields.changeAccountType"),
      vModel: accountDetails.value.type,
      type: "accountType",
      options: ConfigAllAccountTypeSelections.value,
    },
    {
      label: t("fields.changeFundType"),
      vModel: accountDetails.value.fundType,
      type: "fundType",
      options: ConfigFundTypeSelections.value,
    },
    {
      label: t("title.changeStatus"),
      vModel: accountDetails.value.status,
      type: "status",
      options: AccountStatusOptions,
    },
    {
      label: t("action.changeLeverage"),
      vModel: accountDetails.value.tradeAccount?.tradeAccountStatus?.leverage,
      type: "leverage",
      options: backendConfigLeverageSelections,
    },
  ];
});

const openEditAccountInfoPanel = (type: number) => {
  editAccountInfoRef.value?.show(
    accountDetails.value,
    editAccountOptions[type]
  );
};
const openUserDetailSider = inject<(partyId: number) => any>(
  InjectKeys.OPEN_USER_DETAILS,
  () => null
);

const onUserClicked = () => {
  if (accountDetails.value.partyId) {
    openUserDetailSider?.(accountDetails.value.partyId);
  }
};

const editGroupFormRef = ref<InstanceType<typeof ChangeGroupForm>>();

const editGroup = () => {
  editGroupFormRef.value?.show(
    accountDetails.value.agentAccountId,
    accountDetails.value.group
  );
};

const onReAssignFinished = () => {
  handleRefreshTableData?.();
};

const successNotification = (message: string) => {
  ElNotification({
    message: message,
    type: "success",
  });
};
const refreshData = async (notification?: boolean) => {
  accountDetails.value = await AccountService.refreshMTDataById(
    accountDetails.value.id
  ).then((res) => {
    if (notification) {
      successNotification("Refresh Success");
    }
    return res;
  });
};

const tradeAccount = computed(() => accountDetails.value.tradeAccount);
const levelRule = ref(accountDetails.value.hasLevelRule.toString());
const referPath = ref([]);

const referPathConstruct = () => {
  referPath.value = accountDetails.value.referPath.split(".");
};

const HasLevelRule = async () => {
  accountDetails.value.hasLevelRule =
    accountDetails.value.hasLevelRule == 1 ? 0 : 1;
  try {
    await AccountService.updateAccountLevelRule(
      accountDetails.value.id,
      accountDetails.value.hasLevelRule
    ).then((res) => {
      successNotification("Level Rule Update Success");
      levelRule.value = accountDetails.value.hasLevelRule.toString();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const openPasswordChangePanel = (type: string) => {
  console.log("accountDetails.value", accountDetails.value);
  changePasswordFormRef.value?.show(
    tradeAccount.value,
    accountDetails.value.id,
    type,
    "change"
  );
};

const openResetToInitialPanel = (type: string) => {
  console.log("openResetToInitialPanel", accountDetails.value);
  changePasswordFormRef.value?.show(
    tradeAccount.value,
    accountDetails.value.id,
    type,
    "reset"
  );
};

const openPasswordHistoryDialog = () => {
  if (!accountDetails.value?.id) {
    ElNotification({
      message: t("error.accountNotFound"),
      type: "error",
    });
    return;
  }
  passwordHistoryDialogRef.value?.show(accountDetails.value.id);
};

const openChangeCreditPanel = () => {
  changeCreditFormRef.value?.show(accountDetails.value.accountNumber);
};
const openChangeAdjustPanel = () => {
  changeAdjustFormRef.value?.show(accountDetails.value.accountNumber);
};

const openChangeSalesOrAgentForm = () =>
  editSalesAndAgentModalRef.value?.show(accountDetails.value);

onMounted(async () => {
  services.value = await AccountService.getServices();
  serviceMap.value = services.value.reduce((acc: any, service: any) => {
    acc[service.id] = {
      serverName: service.name,
      platform: service.platform,
      platformName: t(`type.platform.${service.platform}`),
    };
    return acc;
  }, {});
  referPathConstruct();
  // 获取初始密码信息
  if (tradeAccount.value && accountDetails.value?.id) {
    try {
      initialPasswordData.value = await AccountService.getInitialTradePassword(
        accountDetails.value.id
      );
    } catch (error) {
      console.error("Failed to fetch initial password:", error);
      initialPasswordData.value = null;
    }
  }
});
</script>

<style scoped>
.borders {
  /* collapse border */
  text-align: center;
  width: 150px;
  border: 1px solid #ccc;
  padding: 10px;
  margin-left: 2px;
  margin-right: 2px;
  border-radius: 5px;
}
.refer-path-border {
  text-align: center;
  padding: 10px;
  margin-left: 2px;
  margin-right: 2px;
  border-radius: 5px;
  border: 1px solid #ccc;
  width: fit-content;
}
.btn {
  text-align: center;
  width: 150px;
  padding: 10px;
  margin-left: 2px;
  margin-right: 2px;
  border-radius: 5px;
}
.head {
  font-weight: bold;
  font-size: 14px;
  padding-bottom: 2px;
}
.title {
  font-weight: bold;
  font-size: 16px;
}
</style>
