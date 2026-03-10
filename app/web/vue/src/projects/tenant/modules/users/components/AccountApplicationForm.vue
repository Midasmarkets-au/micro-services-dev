<template>
  <div class="">
    <!-- <div
      class="p-4 d-flex justify-content-between align-items-center bg-opacity-10 bg-black rounded"
    >
      <h3 class="m-0 fw-semibold fs-5">
        {{ $t("title.pendingApplications") }}
      </h3>
    </div> -->

    <!-- <div class="border border-secondary border-top-0 p-5 px-20 rounded"> -->
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
        <!--        <el-form-item :label="$t('fields.status')" prop="status">-->
        <!--          <el-radio-group-->
        <!--            v-model="status"-->
        <!--            :disabled="-->
        <!--              pendingApplication.status !==-->
        <!--              ApplicationStatusType.AwaitingApproval-->
        <!--            "-->
        <!--          >-->
        <!--            <el-radio-->
        <!--              v-for="item in accountStatusList"-->
        <!--              :key="item.value"-->
        <!--              :value="item.value"-->
        <!--              :label="item.label"-->
        <!--            />-->
        <!--          </el-radio-group>-->
        <!--        </el-form-item>-->

        <el-form-item :label="$t('fields.role')" prop="role">
          <el-select
            v-model="formData.role"
            @change="emits('roleChanged', formData.role)"
            :disabled="
              hasReferCode ||
              pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
            "
          >
            <el-option
              v-for="item in accountRoleList"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="$t('fields.category')" prop="status">
          <el-radio-group
            v-model="category"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
          >
            <el-radio
              v-for="item in accountCategoryList"
              :key="item.value"
              :value="item.value"
              :label="item.label"
            />
          </el-radio-group>
        </el-form-item>

        <!-- <el-form-item label="Language" prop="language">
          <el-radio-group
            v-model="language"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
          >
            <el-radio label="English" />
            <el-radio label="Traditional Chinese" />
            <el-radio label="Simplified Chinese" />
            <el-radio label="Vietnamese" />
          </el-radio-group>
        </el-form-item> -->

        <el-form-item :label="$t('fields.openAt')" prop="fundType">
          <el-radio-group
            v-model="formData.fundType"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
          >
            <el-radio
              v-for="item in ConfigFundTypeSelections"
              :key="item.value"
              :label="item.value"
            >
              {{ item.label }}
            </el-radio>
          </el-radio-group>
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
            />
          </el-select>
        </el-form-item>

        <el-form-item :label="$t('fields.referralCode')" prop="referCode">
          <el-input
            v-model="formData.referCode"
            :disabled="
              hasReferCode ||
              pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
            "
          />
        </el-form-item>

        <!-- 
        <el-form-item
          v-if="formData.role === AccountRole.Agent"
          :label="$t('fields.ibReferral')"
          prop="ibReferral"
        >
          <el-input
            v-model="formData.ibReferral"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
          />
        </el-form-item> -->

        <template v-if="formData.role !== AccountRoleTypes.Unknown">
          <el-form-item :label="$t('fields.salesCode')" prop="salesCode">
            <el-input
              v-model="formData.salesCode"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
            />
          </el-form-item>

          <el-form-item
            v-if="
              formData.role === AccountRoleTypes.IB ||
              formData.role === AccountRoleTypes.Client
            "
            :label="$t('fields.salesInfo')"
            prop="salesInfo"
          >
            <el-input v-model="salesInfo" disabled />
          </el-form-item>

          <el-form-item :label="$t('fields.group')" prop="group">
            <el-input
              v-model="formData.group"
              :disabled="
                pendingApplication.status !==
                ApplicationStatusType.AwaitingApproval
              "
            />
          </el-form-item>
        </template>

        <el-form-item>
          <el-button
            type="primary"
            @click="submitForm(ruleFormRef)"
            :loading="isLoading"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
            >{{ $t("action.submit") }}
          </el-button>
          <el-button
            @click="resetForm(ruleFormRef)"
            :disabled="
              pendingApplication.status !==
              ApplicationStatusType.AwaitingApproval
            "
            >{{ $t("action.reset") }}</el-button
          >
          <!--          <div class="btn btn-primary">{{ formData }}</div>-->
          <!--          <div class="btn btn-primary">{{ ConfigFundTypeSelections }}</div>-->
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted, watch } from "vue";
import {
  AccountRoleTypes,
  ConfigAllAccountTypeSelections,
  getAccountRoleSelectionsByKeys,
} from "@/core/types/AccountInfos";
import i18n from "@/core/plugins/i18n";
import { AccountStatusTypes, AccountCategory } from "@/core/types/AccountInfos";

import { AccountInfoCriteria } from "../types/Criterias";
import UserService from "../services/UserService";
import type { FormInstance, FormRules } from "element-plus";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";
import GlobalService from "@/projects/tenant/services/TenantGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import {
  ConfigDefaultFundType,
  ConfigFundTypeSelections,
} from "@/core/types/FundTypes";

const props = defineProps<{
  pendingApplication: any;
}>();

const emits = defineEmits<{
  (event: "application-submitted", createdAccount: object): void;
  (event: "roleChanged", role: AccountRoleTypes): void;
}>();

const { t } = i18n.global;
const accountCriteria = reactive<AccountInfoCriteria>({
  size: 1,
} as AccountInfoCriteria);

const ruleFormRef = ref<FormInstance>();

const status = ref(0);
const category = ref(0);
const salesInfo = ref("");
const salesAccountId = ref(-1);
const hasReferCode = ref(false);
const isLoading = ref(true);

const initFormData = {
  referCode: "",
  accountType: props.pendingApplication.accountType,
  role: AccountRoleTypes.Client,
  salesCode: "",
  group: "",
  fundType: 1,
};

const formData = ref<any>(initFormData);
watch(
  () => props.pendingApplication,
  () => {
    resetForm();
  }
);

const checkSalesCode = async (rule: any, value: any, callback: any) => {
  /**
   * if the role is sales, validate the sales code
   */
  isLoading.value = true;
  if (value === "" || value === undefined || value === null) {
    console.log('new Error("Please input sales code")');
    salesInfo.value = "";
    salesAccountId.value = -1;
    callback(new Error("Please input sales code"));
    isLoading.value = false;
    return;
  }
  clearAccountCriteria();
  accountCriteria.role = AccountRoleTypes.Sales;
  accountCriteria.code = formData.value.salesCode;

  try {
    const data = await UserService.queryUserAccounts(accountCriteria);
    const salesExisted = data.data.length !== 0;

    // for role is sales
    if (formData.value.role === AccountRoleTypes.Sales && !salesExisted) {
      // callback(new Error(t("fields.salesCodeValid")));
      isLoading.value = false;
      return;
    }

    if (formData.value.role === AccountRoleTypes.Sales && salesExisted) {
      callback(new Error(t("tip.salesCodeInvalid")));
      isLoading.value = false;
      return;
    }

    // for role is ib or client
    if (!salesExisted) {
      // ib or client, sales code not found
      callback(new Error(t("tip.salesInfoNotFound")));
      salesInfo.value = "";
      salesAccountId.value = -1;
      isLoading.value = false;
      return;
    }

    salesInfo.value = data.data[0].name;
    salesAccountId.value = data.data[0].id;
  } catch (err) {
    callback(err);
  } finally {
    isLoading.value = false;
  }
};

const checkGroupCode = async (rule: any, value: any, callback: any) => {
  isLoading.value = true;
  if (value === "") {
    isLoading.value = false;
    callback(new Error("Please input group code"));
  }
  if (formData.value.group.length < 3) {
    callback(new Error(t("tip.groupLengthInvalid")));
    isLoading.value = false;
    return;
  }

  if (formData.value.role === AccountRoleTypes.IB) {
    isLoading.value = false;
    return;
  }

  clearAccountCriteria();
  accountCriteria.group = formData.value.group;

  try {
    const data = await UserService.queryUserAccounts(accountCriteria);
    const groupExisted = data.data.length !== 0;

    // Sales should have a unique group code
    if (formData.value.role === AccountRoleTypes.Sales && !groupExisted) {
      isLoading.value = false;
      return;
    }

    if (formData.value.role === AccountRoleTypes.Sales && groupExisted) {
      isLoading.value = false;
      callback(new Error(t("tip.salesGroupInvalid")));
      return;
    }

    if (!groupExisted) {
      callback(new Error(t("tip.salesGroupNotFound")));
      isLoading.value = false;
      return;
    }
    // for sales group found
  } catch (err) {
    callback(err);
  } finally {
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
  salesCode: [{ asyncValidator: checkSalesCode, trigger: "blur" }],
  group: [{ asyncValidator: checkGroupCode, trigger: "blur" }],
});

// ibReferral: [{ asyncValidator: checkIbReferral, trigger: "blur" }],

const submitForm = (formEl: FormInstance | undefined) => {
  if (!formEl) return;

  if (formData.value.role === AccountRoleTypes.Broker)
    formData.value.role = AccountRoleTypes.IB;

  formEl.validate(async (valid) => {
    if (valid) {
      isLoading.value = true;
      const createdAccount = await UserService.approveApplication(
        props.pendingApplication.id,
        formData.value
      );

      MsgPrompt.success(t("tip.bcrAccountApproveSuccess")).then(() => {
        emits("application-submitted", createdAccount);
        isLoading.value = false;
      });
    } else {
      MsgPrompt.error("Error Submit");
      isLoading.value = false;
      return false;
    }
  });
};

const resetForm = async () => {
  if (!ruleFormRef.value) return;
  isLoading.value = true;
  ruleFormRef.value.resetFields();
  formData.value = {
    ...props.pendingApplication.supplement,
  };

  // for the step after the user has been verified, get the referral code supplement
  // if the referral code is not empty
  if (formData.value.referCode !== "" && formData.value.referCode !== "none") {
    hasReferCode.value = true;
    // get referral info
    const referralCodeSupplement =
      await GlobalService.getReferralCodeSupplement(formData.value.referCode);

    // 1. use the role designated by the referral code supplement; 2 for Agent-2, 3 for client
    formData.value.role =
      {
        1: AccountRoleTypes.Broker,
        2: AccountRoleTypes.IB,
        3: AccountRoleTypes.Client,
      }[referralCodeSupplement.serviceType] ?? null;

    // 2.restrict the account type to the referral code supplement
    // accountTypeList.value = getAccountTypeSelectionsByKeys();

    // 3. set the user's preferred language by
  } else {
    hasReferCode.value = false;
    // accountTypeList.value = getAccountTypeSelectionsByKeys();
  }
  accountRoleList.value = await getAccountRoleSelectionsByKeys();
  isLoading.value = false;
};

onMounted(() => {
  resetForm();
  isLoading.value = false;
});

const clearAccountCriteria = () => {
  Object.keys(accountCriteria).forEach((key) => {
    if (key === "size") return;
    accountCriteria[key] = null;
  });
};

const accountStatusList = ref([
  { label: t("status.enabled"), value: AccountStatusTypes.Activate },
  { label: t("status.paused"), value: AccountStatusTypes.Pause },
  { label: t("status.closed"), value: AccountStatusTypes.Inactivated },
]);

const accountCategoryList = ref([
  { label: t("fields.wire"), value: AccountCategory.Wire },
  // { label: t("fields.ips"), value: AccountCategory.Ips },
]);

// const accountOpenAtList = ref([
//   { label: t("fields.bvi"), value: AccountOpenAt.Bvi },
//   { label: t("fields.cn"), value: AccountOpenAt.Cn },
// ]);

const accountRoleList = ref(Array<any>());
</script>

<style scoped></style>
