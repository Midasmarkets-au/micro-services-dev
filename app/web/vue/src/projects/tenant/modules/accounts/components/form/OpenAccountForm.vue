<template>
  <div>
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="120px"
      class="demo-ruleForm"
      size="default"
      status-icon
    >
      <div class="amount-tip mb-5">
        <inline-svg src="/images/icons/general/gen066.svg" />
        <span class="ms-2 me-1"
          >Either use <b> Refer Code</b> or <b>IB Group</b> to designate a
          client to the specified IB</span
        >
      </div>
      <el-form-item v-if="!hasReferCode" :label="$t('fields.role')" prop="role">
        <el-select
          v-model="formData.role"
          @change="onRoleChanged"
          :disabled="
            pendingApplication.status !== ApplicationStatusType.AwaitingApproval
          "
        >
          <el-option
            v-for="item in accountRoleSelections"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </el-form-item>

      <div class="row" v-else>
        <div class="col-lg-6">
          <el-form-item :label="$t('fields.role')" prop="role">
            <el-select
              v-model="formData.role"
              @change="onRoleChanged"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
            >
              <el-option
                v-for="item in accountRoleSelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
          <div v-if="showRoleNotMatchError" class="text-danger text-end">
            The role does not match the designated role from referral code!
          </div>
        </div>
        <div class="col-lg-6">
          <el-form-item
            v-if="roleDesignated"
            label-width="230px"
            :label="$t('fields.roleDesignatedByReferCode')"
          >
            <el-input
              disabled
              :value="$t(`type.accountRole.${roleDesignated}`)"
            />
          </el-form-item>
        </div>
      </div>

      <div class="row" style="border-top: 1px dashed #ccc; height: 300px">
        <div
          class="col-lg-6 d-flex flex-column justify-content-between"
          style="border-right: 1px dashed #ccc"
        >
          <div class="h-100 d-flex flex-column justify-content-center">
            <el-form-item
              :label="$t('fields.accountNo')"
              prop="accountNumber"
              class="mt-3"
              clearable
              v-if="$can('TenantAdmin')"
            >
              <el-input
                v-model="formData.accountNumber"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              >
              </el-input>
            </el-form-item>
            <el-form-item :label="$t('fields.fundType')" prop="fundType">
              <el-select
                v-model="formData.fundType"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              >
                <el-option
                  v-for="item in ConfigFundTypeSelections"
                  :key="item.value"
                  :label="t(`type.fundType.${item.value}`)"
                  :value="item.value"
                />
              </el-select>
            </el-form-item>

            <el-form-item :label="t('fields.accountType')" prop="accountType">
              <el-select
                v-model="formData.accountType"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              >
                <el-option
                  v-for="item in ConfigAllAccountTypeSelections"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                /> </el-select
            ></el-form-item>

            <el-form-item :label="t('fields.currency')" prop="currencyId">
              <el-select
                v-model="formData.currencyId"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              >
                <el-option
                  v-for="item in ConfigCurrencySelectionsCustom"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                /> </el-select
            ></el-form-item>

            <el-form-item
              v-if="formData.role === AccountRoleTypes.Client"
              :label="t('fields.site')"
              prop="siteId"
            >
              <el-input v-model="siteId" disabled></el-input>
            </el-form-item>

            <template
              v-if="
                [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
                  formData.role
                )
              "
            >
              <el-form-item
                :label="$t('fields.ibSelfGroup')"
                prop="agentSelfGroup"
                required
              >
                <el-input
                  clearable
                  v-model="formData.agentSelfGroup"
                  :disabled="
                    pendingApplication.status !==
                    ApplicationStatusType.AwaitingApproval
                  "
                >
                  <template #append>
                    <el-button :icon="Search as any" />
                  </template>
                </el-input>
              </el-form-item>
            </template>
          </div>

          <el-form-item>
            <el-button
              type="primary"
              plain
              @click="submitForm()"
              :loading="isLoading"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
              >{{ $t("action.submit") }}
            </el-button>
            <el-button
              plain
              @click="resetForm"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
              >{{ $t("action.reset") }}</el-button
            >
          </el-form-item>
        </div>

        <div
          class="col-lg-6 d-flex flex-column justify-content-center"
          v-if="
            formData.role == AccountRoleTypes.IB ||
            formData.role == AccountRoleTypes.Broker ||
            formData.role == AccountRoleTypes.Client
          "
        >
          <el-form-item
            v-if="formData.role == AccountRoleTypes.Client"
            style="margin: 0"
          >
            <div style="margin-right: 0; margin-left: auto">
              <span class="me-3"
                ><strong>Rate Opt:</strong>
                <span class="badge badge-success ms-1">{{
                  pcValue.rateOption
                }}</span></span
              >
              <span class="me-3"
                ><strong>Pips:</strong>
                <span class="badge badge-success ms-1">{{
                  pcValue.pips
                }}</span></span
              >
              <span
                ><strong>Commission:</strong>
                <span class="badge badge-success ms-1">{{
                  pcValue.commission
                }}</span></span
              >
            </div>
          </el-form-item>
          <el-form-item :label="$t('fields.referCode')" prop="referCode">
            <el-input
              clearable
              v-model="formData.referCode"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
            />
          </el-form-item>

          <el-form-item
            v-if="
              [
                AccountRoleTypes.IB,
                AccountRoleTypes.Broker,
                AccountRoleTypes.Client,
              ].includes(formData.role)
            "
            :label="$t('fields.ibGroup')"
            prop="group"
          >
            <el-autocomplete
              ref="groupAutoCompleteRef"
              class="w-100"
              clearable
              v-model="formData.group"
              @select="onGroupOrSalesSelect"
              :fetch-suggestions="queryAgentGroupAsync"
              :disabled="
                hasReferCode ||
                pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
              "
            >
              <template #append>
                <el-button :icon="Search as any" />
              </template>
            </el-autocomplete>
          </el-form-item>

          <el-form-item
            v-if="
              [
                AccountRoleTypes.IB,
                AccountRoleTypes.Broker,
                AccountRoleTypes.Client,
              ].includes(formData.role)
            "
            :label="$t('fields.ibInfo')"
            prop="ibInfo"
          >
            <el-input
              :value="formData.accountName + ' ( ' + formData.accountUid + ' )'"
              disabled
            />
          </el-form-item>

          <el-form-item
            :label="$t('fields.salesCode')"
            prop="salesCode"
            required
          >
            <el-autocomplete
              ref="salesAutoCompleteRef"
              @select="onGroupOrSalesSelect"
              class="w-100"
              clearable
              v-model="formData.salesCode"
              :fetch-suggestions="querySalesGroupAsync"
              :disabled="
                hasReferCode ||
                pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
              "
            >
              <template #append>
                <el-button :icon="Search as any" />
              </template>
            </el-autocomplete>
          </el-form-item>

          <el-form-item
            v-if="
              [
                AccountRoleTypes.IB,
                AccountRoleTypes.Broker,
                AccountRoleTypes.Client,
              ].includes(formData.role)
            "
            :label="$t('fields.salesInfo')"
            prop="salesInfo"
          >
            <el-input
              :value="
                formData.salesName + ' ( ' + formData.salesAccountUid + ' )'
              "
              disabled
            />
          </el-form-item>
        </div>

        <div class="col-lg-6 d-flex flex-column justify-content-center" v-else>
          <template v-if="[AccountRoleTypes.Sales].includes(formData.role)">
            <el-form-item label="Rep Account" prop="repAccount">
              <el-select
                clearable
                v-model="formData.repAccountId"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              >
                <el-option
                  v-for="item in repAccounts"
                  :key="item.id"
                  :label="
                    item.label
                    // 'Account Uid' + ' - ' + item.uid
                  "
                  :value="item.value"
                />
              </el-select>
            </el-form-item>

            <el-form-item
              :label="$t('fields.salesSelfCode')"
              prop="salesSelfGroup"
            >
              <el-input
                clearable
                v-model="formData.salesSelfGroup"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              />
            </el-form-item>

            <el-form-item label="Sales Self Group" prop="salesDividedGroup">
              <el-input
                clearable
                v-model="formData.salesDividedGroup"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              />
            </el-form-item>
          </template>
          <template v-if="[AccountRoleTypes.Rep].includes(formData.role)">
            <el-form-item
              :label="$t('fields.salesSelfCode')"
              prop="salesSelfGroup"
            >
              <el-input
                clearable
                v-model="formData.salesSelfGroup"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              />
            </el-form-item>

            <el-form-item label="Sales Self Group" prop="salesDividedGroup">
              <el-input
                clearable
                v-model="formData.salesDividedGroup"
                :disabled="
                  pendingApplication.status !==
                  ApplicationStatusType.AwaitingApproval
                "
              />
            </el-form-item>
          </template>
        </div>
      </div>
    </el-form>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from "vue-i18n";
import { Search } from "@element-plus/icons-vue";
import { inject, onMounted, reactive, ref, watch } from "vue";
import { FormInstance, FormRules } from "element-plus";
import { AccountGroupTypes } from "@/core/types/AccountGroupTypes";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";
import { ApplicationApproveSupplement } from "@/core/models/Application";
import { ReferralServiceTypes } from "@/core/types/ReferralServiceTypes";
import { ConfigFundTypeSelections, FundTypes } from "@/core/types/FundTypes";
import { TenantWithReferredAccountInfoResponseModel } from "@/core/models/Referral";
// import MsgPrompt from "@/core/plugins/MsgPrompt";
import GlobalService from "@/projects/tenant/services/TenantGlobalService";
import UserService from "@/projects/tenant/modules/users/services/UserService";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { ElNotification } from "element-plus";
import {
  AccountRoleTypes,
  AccountTypes,
  ConfigAllAccountTypeSelections,
  getAccountRoleSelectionsByKeys,
} from "@/core/types/AccountInfos";

import AccountService, {
  generateAutoCompleteHandler,
} from "@/projects/tenant/modules/accounts/services/AccountService";

import {
  ConfigCurrencySelections,
  CurrencyTypes,
} from "@/core/types/CurrencyTypes";
import { useStore } from "@/store";
const props = defineProps<{
  siteId: any;
}>();

const store = useStore();
const user = store.state.AuthModule.user;
var siteName = user.tenancy;
const emits = defineEmits<{
  (event: "application-submitted", createdAccount: object): void;
  (event: "roleChanged", role: AccountRoleTypes): void;
}>();

const { t } = useI18n();
const siteId = ref(t("type.siteType." + props.siteId));
const isLoading = ref(false);
const hasReferCode = ref(false);
const ruleFormRef = ref<FormInstance>();
const pendingApplication = inject<any>(
  AccountInjectionKeys.APPLICATION_DETAILS
);
const referCodeAccountTypes = ref({} as any);
const pcValue = ref({} as any);
const salesAutoCompleteRef = ref<any>();
const groupAutoCompleteRef = ref<any>();

const showRoleNotMatchError = ref(false);
// 过滤 USC 货币（如果 hasUSCAccout == true）
const filterUSCCurrency = (selections: any[]) => {
  console.log("selections", selections);
  if (pendingApplication.value?.user?.hasUSCAccount === true) {
    return selections.filter((currency) => currency.value !== 841); // 841 = USC
  }
  return selections;
};

const ConfigCurrencySelectionsCustom = ref<any[]>(
  filterUSCCurrency([...ConfigCurrencySelections.value])
);
const onRoleChanged = () => {
  console.log("onRoleChanged", formData.value.role);
  emits("roleChanged", formData.value.role);
  // 对于Sales、IB、Rep角色，只显示货币840
  if (
    [
      AccountRoleTypes.Sales,
      AccountRoleTypes.IB,
      AccountRoleTypes.Rep,
    ].includes(formData.value.role)
  ) {
    formData.value.accountType = AccountTypes.Standard;
    formData.value.currencyId = ConfigCurrencySelections.value.some(
      (currency) => currency.value === 840
    )
      ? 840
      : ConfigCurrencySelections.value[0].value;
    ConfigCurrencySelectionsCustom.value = filterUSCCurrency(
      ConfigCurrencySelections.value.filter((currency) => {
        return currency.value === 840;
      })
    );
  } else {
    ConfigCurrencySelectionsCustom.value = filterUSCCurrency(
      ConfigCurrencySelections.value
    );
  }
  if (roleDesignated.value && roleDesignated.value !== formData.value.role) {
    // MsgPrompt.info(
    //   "Your selected role does not match the role from referral code"
    // );
    ElNotification({
      title: "Warning",
      message: "Your selected role does not match the role from referral code",
      type: "warning",
    });
    showRoleNotMatchError.value = true;
    return;
  }
  showRoleNotMatchError.value = false;
};

const formData = ref<any>({});
const initFormData = () => {
  formData.value = {
    ...pendingApplication.value.supplement,
    fundType: processFundType(),
    accountNumber: "random",
  };
};

const processFundType = () => {
  if (siteName == "au" || siteName == "jp") {
    formData.value.fundType = FundTypes.Wire;
  } else {
    formData.value.fundType = FundTypes.Ips;
  }
  return formData.value.fundType;
};

const initIbInfoFormData = () => {
  formData.value.group = "";
  formData.value.accountName = "";
  formData.value.accountId = "";
};

const initSalesInfoFormData = () => {
  formData.value.salesCode = "";
  formData.value.salesName = "";
  formData.value.salesAccountId = "";
};

const validInput = (value: string) =>
  value !== "" && value !== undefined && value !== null;

const validateSalesCode = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;
  if (!validInput(value)) {
    callback(new Error("Please input sales code"));
    initSalesInfoFormData();
    isLoading.value = false;
    return;
  }

  if (value?.length < 3) {
    callback(new Error(t("tip.salesCodeLengthShouldBeAtLeast3")));
    isLoading.value = false;
    return;
  }

  const { data } = await AccountService.queryAccounts({
    code: value,
    roles: [AccountRoleTypes.Sales],
  });

  const exists = data?.length > 0;

  if (!exists) {
    callback(new Error(t("tip.salesInfoNotFound")));
    isLoading.value = false;
    return;
  }

  // initIbInfoFormData();
  formData.value.salesName =
    data[0].name == " " ? formData.value.salesName : data[0].name;
  formData.value.salesAccountId = data[0].id;
  isLoading.value = false;
};

const validateSalesSelfGroup = async (rule: any, value: any, callback: any) => {
  if (formData.value.role !== AccountRoleTypes.Sales) return;
  isLoading.value = true;
  if (!validInput(value)) {
    callback(new Error("Please input sales code"));
    initSalesInfoFormData();
    isLoading.value = false;
    return;
  }

  if (value?.length < 3) {
    callback(new Error(t("tip.salesCodeLengthShouldBeAtLeast3")));
    isLoading.value = false;
    return;
  }

  const { data } = await AccountService.queryAccounts({
    code: value,
    roles: [AccountRoleTypes.Sales],
  });

  const exists = data?.length > 0;

  if (exists) {
    callback(new Error(t("tip.salesCodeExists")));
    isLoading.value = false;
    return;
  }
  isLoading.value = false;
};

const validateIbGroup = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;
  if (!validInput(value)) {
    isLoading.value = false;
    [AccountRoleTypes.Sales].includes(formData.value.role) &&
      callback(new Error("Please input group code"));
    initIbInfoFormData();
    return;
  }

  if (value?.length < 3) {
    callback(new Error(t("tip.ibGroupLengthShouldBeAtLeast3")));
    isLoading.value = false;
    return;
  }

  const { data } = await AccountService.queryAccounts({
    group: value,
    roles: [AccountRoleTypes.IB, AccountRoleTypes.Broker],
  });

  const exists = data?.length > 0;

  if (!exists) {
    callback(new Error(t("tip.ibGroupNotFound")));
    isLoading.value = false;
    return;
  }

  console.log("data", data[0]);
  formData.value.accountName = data[0].name;
  formData.value.accountId = data[0].id;
  formData.value.salesName = data[0].salesAccount?.name;
  formData.value.salesCode = data[0].salesAccount?.code;
  formData.value.salesAccountId = data[0].salesAccount?.id;
  isLoading.value = false;
};

const validateIbSelfGroup = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;
  if (!validInput(value)) {
    isLoading.value = false;
    [AccountRoleTypes.IB, AccountRoleTypes.Broker].includes(
      formData.value.role
    ) && callback(new Error("Please input group code"));
    console.log("validateIbSelfGroup");
    initIbInfoFormData();
    return;
  }

  if (value?.length < 3) {
    callback(new Error(t("tip.ibGroupLengthShouldBeAtLeast3")));
    isLoading.value = false;
    return;
  }

  const { data } = await AccountService.queryAccounts({
    group: value,
    roles: [AccountRoleTypes.IB, AccountRoleTypes.Broker],
  });

  const exists = data?.length > 0;

  if (exists) {
    callback(new Error(t("tip.ibGroupExists")));
    isLoading.value = false;
    return;
  }

  isLoading.value = false;
};

const referredCodeInfo = ref<TenantWithReferredAccountInfoResponseModel>(
  {} as TenantWithReferredAccountInfoResponseModel
);

const roleDesignated = ref<any>();
const getAccountRoleDesignatedByReferCode = (
  _referredAccount: TenantWithReferredAccountInfoResponseModel
) =>
  ({
    [ReferralServiceTypes.Broker]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Agent]: AccountRoleTypes.IB,
    [ReferralServiceTypes.Client]: AccountRoleTypes.Client,
    // for legacy data
    [200]: AccountRoleTypes.IB,
    [300]: AccountRoleTypes.IB,
    [400]: AccountRoleTypes.Client,
  }[_referredAccount.serviceType] ?? AccountRoleTypes.Client);

const getReferralCodeSuppliment = async (callback: any) => {
  try {
    const res = await GlobalService.getReferralInfoByReferralCode(
      formData.value.referCode
    );

    referCodeAccountTypes.value =
      res.data?.summary?.allowAccountTypes ??
      res.data?.summary?.Schema ??
      res.data?.summary?.schema ??
      [];

    if (Object.keys(referCodeAccountTypes.value).length !== 0) {
      const accountSchema = referCodeAccountTypes.value.find(
        (item: any) => item.accountType === formData.value.accountType
      );
      if (accountSchema) {
        pcValue.value.pips = accountSchema.pips;
        pcValue.value.commission = accountSchema.commission;
        pcValue.value.rateOption = accountSchema.optionName;
      } else {
        pcValue.value.pips = "-";
        pcValue.value.commission = "-";
        pcValue.value.rateOption = "-";
      }
    } else {
      pcValue.value.pips = "-";
      pcValue.value.commission = "-";
      pcValue.value.rateOption = "-";
    }
  } catch (e) {
    callback(new Error(t("tip.referralCodeInvalid")));
    hasReferCode.value = false;
  }
};

const validateReferCode = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;
  if (!validInput(value)) {
    hasReferCode.value = false;
    isLoading.value = false;
    return;
  }
  try {
    hasReferCode.value = true;
    const _referredAccountInfo = await GlobalService.getReferralCodeAccountInfo(
      formData.value.referCode
    );
    await getReferralCodeSuppliment();

    roleDesignated.value =
      getAccountRoleDesignatedByReferCode(_referredAccountInfo);

    referredCodeInfo.value = _referredAccountInfo;

    formData.value.salesName = _referredAccountInfo.salesName;
    formData.value.salesCode = _referredAccountInfo.salesAccountCode;
    formData.value.salesAccountId = _referredAccountInfo.salesAccountId;
    if (formData.value.salesAccountId > 0) {
      const res = await AccountService.getAccountDetailById(
        formData.value.salesAccountId
      );
      formData.value.salesAccountUid = res.uid;
    }

    formData.value.accountName = _referredAccountInfo.accountName;
    formData.value.group = _referredAccountInfo.accountGroupName;
    formData.value.accountId = _referredAccountInfo.accountId;
    if (formData.value.accountId > 0) {
      const res = await AccountService.getAccountDetailById(
        formData.value.accountId
      );
      formData.value.accountUid =
        res.accountUid == 0 ? formData.value.accountUid : res.accountNumber;
    }
  } catch (e) {
    callback(new Error(t("tip.referralCodeInvalid")));
    hasReferCode.value = false;
  } finally {
    isLoading.value = false;
  }
};

const validateAccountNumber = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;

  if (!validInput(value) || value === "random") {
    isLoading.value = false;
    return;
  }
  if (!/^\d+$/.test(value)) {
    callback(new Error("Account number must contain only digits."));
    isLoading.value = false;

    return;
  }
  try {
    await AccountService.validateAccountNumber(value);
  } catch (e) {
    callback(new Error(e.response.data));
  } finally {
    console.log("value", value);
    isLoading.value = false;
  }
};

const rules = reactive<FormRules>({
  fundType: [
    { required: true, message: "Please select the account fund type" },
  ],
  accountType: [
    { required: true, message: "Please select the account account type" },
  ],
  role: [{ required: true, message: "Please select the account role" }],
  salesCode: [{ asyncValidator: validateSalesCode, trigger: "blur" }],
  group: [{ asyncValidator: validateIbGroup, trigger: "blur" }],
  referCode: [{ asyncValidator: validateReferCode, trigger: "blur" }],
  agentSelfGroup: [{ asyncValidator: validateIbSelfGroup, trigger: "blur" }],
  salesSelfGroup: [{ asyncValidator: validateSalesSelfGroup, trigger: "blur" }],
  accountNumber: [
    { required: true, message: "Account number is required", trigger: "blur" },
    { asyncValidator: validateAccountNumber, trigger: "blur" },
  ],
  // accountNumber: [{ asyncValidator: validateAccountNumber, trigger: "blur" }],
});

const submitForm = async () => {
  if (!ruleFormRef.value) return;

  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  // For Develop

  try {
    isLoading.value = true;

    const temp = { ...formData.value };
    formData.value = { ...temp };

    if (formData.value.accountNumber) {
      if (/^\d+$/.test(formData.value.accountNumber)) {
        formData.value.accountNumber = parseInt(formData.value.accountNumber);
      } else {
        formData.value.accountNumber = null;
      }
    }
    const approveSubmitForm: ApplicationApproveSupplement = {
      role: formData.value.role,
      leverage: formData.value.leverage,
      platform: formData.value.platform,
      fundType: formData.value.fundType,
      serviceId: formData.value.serviceId,
      referCode: formData.value.referCode,
      currencyId: formData.value.currencyId,
      accountType: formData.value.accountType,
      repAccountId: formData.value.repAccountId,
      agentAccountId: formData.value.accountId,
      salesAccountId: formData.value.salesAccountId,
      salesSelfGroup: formData.value.salesSelfGroup,
      agentSelfGroup: formData.value.agentSelfGroup,
      salesDividedGroup: formData.value.salesDividedGroup,
      accountNumber: formData.value.accountNumber,
    };
    const createdAccount = await UserService.approveApplication(
      pendingApplication.value.id,
      approveSubmitForm
    );

    if (formData.value.role !== AccountRoleTypes.Client) {
      await UserService.completeApplication(pendingApplication.value.id, {});
    }
    ElNotification.success({
      title: "Success",
      message: t("tip.bcrAccountApproveSuccess"),
      type: "success",
    });
    emits("application-submitted", createdAccount);
  } catch (e) {
    ElNotification.error({
      title: "Error",
      message: e.response.data.message
        ? e.response.data.message
        : "Error Submit",
      type: "error",
    });
    return false;
  }
  isLoading.value = false;
  // MsgPrompt.success(t("tip.bcrAccountApproveSuccess")).then(() => {
  //   emits("application-submitted", createdAccount);
  //   isLoading.value = false;
  // });
};

const onGroupOrSalesSelect = () => {
  groupAutoCompleteRef.value.blur();
  salesAutoCompleteRef.value.blur();
};

const repAccounts = ref(Array<any>());
const getRepAccounts = async () => {
  const { data } = await AccountService.queryAccounts({
    roles: [AccountRoleTypes.Rep],
    size: 1000,
  });
  repAccounts.value = data.map((item: any) => ({
    label: (item.name == " " ? "No Name" : item.name) + " - (Uid)" + item.uid,
    value: item.id,
  }));
};

const resetForm = async () => {
  console.log("resetForm");
  if (!ruleFormRef.value) return;
  isLoading.value = true;
  ruleFormRef.value.resetFields();

  initFormData();
  if (formData.value.referCode !== "" && formData.value.referCode !== "none") {
    hasReferCode.value = true;
    try {
      const referredAccountInfo =
        await GlobalService.getReferralCodeAccountInfo(
          formData.value.referCode
        );

      await getReferralCodeSuppliment();
      roleDesignated.value =
        getAccountRoleDesignatedByReferCode(referredAccountInfo);

      formData.value.role = roleDesignated.value;
      emits("roleChanged", formData.value.role);

      if (formData.value.role === null) {
        ElNotification({
          title: "Warning",
          message: t("tip.referralCodeInvalid"),
          type: "warning",
        });
        // MsgPrompt.error(t("tip.referralCodeInvalid"));
        isLoading.value = false;
      }
      formData.value.salesName = referredAccountInfo.salesName;
      formData.value.salesCode = referredAccountInfo.salesAccountCode;
      formData.value.salesAccountId = referredAccountInfo.salesAccountId;
      if (formData.value.salesAccountId > 0) {
        const res = await AccountService.getAccountDetailById(
          formData.value.salesAccountId
        );
        formData.value.salesAccountUid = res.uid;
      }

      formData.value.accountName = referredAccountInfo.accountName;
      formData.value.group = referredAccountInfo.accountGroupName;
      formData.value.accountId = referredAccountInfo.accountId;
      if (formData.value.accountId > 0) {
        const res = await AccountService.getAccountDetailById(
          formData.value.accountId
        );
        formData.value.accountUid = res.uid;
      }
    } catch (e) {
      hasReferCode.value = false;
      // MsgPrompt.error(t("tip.referralCodeInvalid"));
      ElNotification({
        title: "Warning",
        message: t("tip.referralCodeInvalid"),
        type: "warning",
      });
    }
  } else {
    hasReferCode.value = false;
  }
  accountRoleSelections.value = await getAccountRoleSelectionsByKeys();
  await getRepAccounts();
  isLoading.value = false;
};

const accountRoleSelections = ref(Array<any>());
const queryAgentGroupAsync = generateAutoCompleteHandler(
  async (keywords: string) => {
    const res = await AccountService.getFullAccountGroupNames(
      AccountGroupTypes.Agent,
      keywords
    );
    return res.map((item) => ({ value: item, label: item }));
  }
);

const querySalesGroupAsync = generateAutoCompleteHandler(
  async (keywords: string) => {
    const res = await AccountService.getFullAccountGroupNames(
      AccountGroupTypes.Sales,
      keywords
    );
    return res.map((item) => ({ value: item, label: item }));
  }
);

onMounted(async () => {
  await resetForm();
  isLoading.value = false;
});

watch(
  () => formData.value.accountType,
  () => {
    if (Object.keys(referCodeAccountTypes.value).length === 0) {
      pcValue.value.pips = "-";
      pcValue.value.commission = "-";
      return;
    }

    const accountSchema = referCodeAccountTypes.value.find(
      (item: any) => item.accountType === formData.value.accountType
    );
    if (accountSchema) {
      pcValue.value.pips = accountSchema.pips;
      pcValue.value.commission = accountSchema.commission;
    } else {
      pcValue.value.pips = "-";
      pcValue.value.commission = "-";
    }
  }
);
</script>

<style scoped></style>
