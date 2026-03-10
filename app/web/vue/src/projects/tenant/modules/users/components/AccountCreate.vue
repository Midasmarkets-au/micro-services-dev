<template>
  <div>
    <div v-if="!isLoading">
      <div class="row">
        <div class="col-6">
          <el-form
            label-width="100px"
            ref="elFormRef"
            :model="formData"
            :rules="rules"
          >
            <el-form-item :label="$t('fields.platform')" prop="platform">
              <el-select
                v-model="formData.platform"
                :disabled="
                  applicationDetails.status !== ApplicationStatusType.Approved
                "
                name="platform"
                type="text"
                :placeholder="$t('action.select')"
              >
                <el-option
                  v-for="(
                    { label, platform }, index
                  ) in ConfigAllPlatformSelections"
                  :label="label"
                  :key="index"
                  :value="platform"
                />
              </el-select>
            </el-form-item>

            <el-form-item
              v-if="typeof formData.platform === 'number'"
              :label="$t('fields.server')"
              prop="serviceId"
            >
              <el-select
                v-model="formData.serviceId"
                :disabled="
                  applicationDetails.status !== ApplicationStatusType.Approved
                "
                name="serviceId"
                type="text"
                :placeholder="$t('action.select')"
              >
                <el-option
                  v-for="({ label, value }, index) in serverSelections"
                  :key="index"
                  :label="label"
                  :value="value"
                />
              </el-select>
            </el-form-item>

            <el-form-item
              v-if="typeof formData.serviceId === 'number'"
              :label="$t('fields.group')"
              prop="group"
            >
              <el-select
                v-model="formData.group"
                filterable
                :disabled="
                  applicationDetails.status !== ApplicationStatusType.Approved
                "
                name="group"
                type="text"
                :placeholder="$t('action.select')"
              >
                <el-option
                  v-for="(_group, index) in groupSelections"
                  :key="index"
                  :value="_group"
                />
              </el-select>
            </el-form-item>

            <el-form-item :label="$t('fields.currency')" prop="currencyId">
              <el-select
                v-model="formData.currencyId"
                :disabled="
                  applicationDetails.status !== ApplicationStatusType.Approved
                "
                name="currencyId"
                type="text"
                :placeholder="$t('action.select')"
              >
                <el-option
                  v-for="(
                    { value, label }, index
                  ) in filteredCurrencySelections"
                  :key="index"
                  :label="label"
                  :value="value"
                >
                </el-option>
              </el-select>
            </el-form-item>

            <el-form-item :label="$t('fields.leverage')" prop="leverage">
              <div class="d-flex align-items-center">
                <el-input
                  :disabled="
                    applicationDetails.status !== ApplicationStatusType.Approved
                  "
                  v-model="formData.leverage"
                  name="leverage"
                  style="width: 50px"
                />
                <div class="w-50px fs-3 fw-bold text-black-50 ms-3">: 1</div>
              </div>
            </el-form-item>

            <el-form-item :label="$t('fields.accountTag')" prop="pcTag">
              <el-radio-group v-model="formData.pcTag" size="large">
                <el-radio-button label="AddPips" value="AddPips" />
                <el-radio-button label="AddCommission" value="AddCommission" />
                <el-radio-button label="None" value="None" />
              </el-radio-group>
              <div style="color: #900000">
                {{ $t("tip.accountPcTag") }}
              </div>
            </el-form-item>

            <el-form-item>
              <el-button type="primary" @click="submit" :loading="isSaving">{{
                $t("action.submit")
              }}</el-button>
              <el-button>{{ $t("action.review") }}</el-button>
            </el-form-item>
          </el-form>
        </div>
        <div class="col-6">
          <el-form label-width="150px">
            <el-form-item :label="$t('fields.platform')" prop="platform">
              <div class="d-flex align-items-center">
                <el-input
                  :value="
                    originalFormData.platform
                      ? $t('type.platform.' + originalFormData.platform)
                      : 'None'
                  "
                  name="platform"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
            <el-form-item :label="$t('fields.server')" prop="server">
              <div class="d-flex align-items-center">
                <el-input
                  :value="
                    services.find(
                      (item) => item.id === originalFormData.serviceId
                    )?.name +
                      ' ( ' +
                      services.find(
                        (item) => item.id === originalFormData.serviceId
                      )?.description +
                      ' )' ?? 'None'
                  "
                  name="server"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
            <el-form-item :label="$t('fields.currency')" prop="currency">
              <div class="d-flex align-items-center">
                <el-input
                  :value="
                    ConfigCurrencySelections.find(
                      (item) => item.value === originalFormData.currencyId
                    )?.label ?? 'None'
                  "
                  name="currency"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
            <el-form-item :label="$t('fields.leverage')" prop="leverage">
              <div class="d-flex align-items-center">
                <el-input
                  :value="formData.leverage + ' : 1'"
                  name="leverage"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>

            <el-form-item :label="$t('fields.accountType')" prop="accountType">
              <div class="d-flex align-items-center">
                <el-input
                  :value="
                    $t(
                      'type.account.' +
                        applicationDetails.supplement.accountType
                    )
                  "
                  name="accountType"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
            <el-form-item :label="'Rate Option'" prop="rateOption">
              <div class="d-flex align-items-center">
                <el-input
                  :value="pipCommissionDecision.optionName"
                  name="rateOption"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
            <el-form-item
              :label="$t('fields.pipCommission')"
              prop="pipCommission"
            >
              <div class="d-flex align-items-center">
                <el-input
                  :value="
                    pipCommissionDecision.commission == 0
                      ? pipCommissionDecision.pips == 0
                        ? 'None'
                        : 'Pips'
                      : 'Commission'
                  "
                  name="pipCommission"
                  style="width: 100%"
                  disabled
                />
                <span class="ms-2 me-2">=></span>
                <el-input
                  :value="
                    pipCommissionDecision.commission == 0
                      ? pipCommissionDecision.pips == 0
                        ? 'None'
                        : pipCommissionDecision.pips
                      : pipCommissionDecision.commission
                  "
                  name="value"
                  style="width: 100%"
                  disabled
                />
              </div>
            </el-form-item>
          </el-form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, inject, onMounted, reactive, ref, watch } from "vue";
import {
  ConfigCurrencySelections,
  CurrencyTypes,
} from "@/core/types/CurrencyTypes";
import UserService from "../services/UserService";
import RebateService from "@/projects/tenant/modules/rebate/services/RebateService";

import {
  ConfigAllServiceSelections,
  ConfigAllPlatformSelections,
} from "@/core/types/ServiceTypes";
import type { FormInstance, FormRules } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import GlobalService from "@/projects/tenant/services/TenantGlobalService";
import { defineEmits, defineExpose } from "vue";
import i18n from "@/core/plugins/i18n";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import { ApplicationStatusType } from "@/core/types/ApplicationInfos";
import { ElNotification } from "element-plus";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
const { t } = i18n.global;
const isLoading = ref(false);
const isSaving = ref(false);

const props = defineProps<{
  accountDetails?: any;
}>();

const accountDetails = inject(
  AccountInjectionKeys.ACCOUNT_DETAILS,
  ref<any>({})
);

const applicationDetails = inject(
  AccountInjectionKeys.APPLICATION_DETAILS,
  ref<any>({})
);

const pendingApplication = inject<any>(
  AccountInjectionKeys.APPLICATION_DETAILS
);

const services = inject(AccountInjectionKeys.PLATFORM_SERVICES, ref<any>({}));

// 过滤 USC 货币（如果 hasUSCAccout == true）
const filteredCurrencySelections = computed(() => {
  if (applicationDetails.value?.user?.hasUSCAccount === true) {
    return ConfigCurrencySelections.value?.filter(
      (currency) => currency.value !== CurrencyTypes.USC
    );
  }
  return ConfigCurrencySelections.value;
});

const elFormRef = ref<FormInstance>();

const rules = reactive<FormRules>({
  platform: [
    { required: true, message: "Please select Platform", trigger: "blur" },
  ],
  serviceId: [
    { required: true, message: "Please select Server", trigger: "blur" },
  ],
  group: [{ required: true, message: "Please select Group", trigger: "blur" }],
  currencyId: [
    { required: true, message: "Please select Currency", trigger: "blur" },
  ],
  leverage: [
    { required: true, message: "Please select Leverage", trigger: "blur" },
  ],
  pcTag: [{ required: true, message: "Please select Tag", trigger: "blur" }],
});

const serverSelections = ref(Array<any>());
const groupSelections = ref(Array<any>());
const formData = ref({} as any);
const originalFormData = ref({} as any);
const pipCommissionDecision = ref({} as any);
const initialFlag = ref(true);

watch(
  () => formData.value.platform,
  () => {
    serverSelections.value = services.value.reduce((acc: any, service: any) => {
      if (service.platform !== formData.value.platform) return acc;
      acc.push({
        label: service.name + " (" + service.description + ")",
        value: service.id,
      });
      return acc;
    }, []);

    if (!initialFlag.value) formData.value.serviceId = null;
    initialFlag.value = false;
  }
);

watch(
  () => formData.value.serviceId,
  () => {
    formData.value.group = null;
    groupSelections.value = services.value.find(
      (service: any) => service.id === formData.value.serviceId
    )?.groups;
  }
);

const emits = defineEmits<{
  (event: "accountSubmitted", createdAccount: object): void;
  (event: "closeTest"): void;
}>();

const submit = async () => {
  if (!elFormRef.value) return;
  await elFormRef.value.validate(async (valid, fields) => {
    if (valid) {
      isSaving.value = true;
      try {
        const createdAccount = await UserService.createTradeAccount(
          formData.value
        ).then(async (res) => {
          await updateTag(res.id);
          return res;
        });

        console.log("createdAccount", createdAccount);

        await UserService.completeApplication(applicationDetails.value.id, {});
        isSaving.value = false;
        ElNotification({
          title: t("tip.accountCreateSuccess"),
          type: "success",
        });

        emits("accountSubmitted", createdAccount);

        // MsgPrompt.success(t("tip.accountCreateSuccess")).then(() =>
        //   emits("accountSubmitted", createdAccount)
        // );
      } catch (e) {
        console.log(e);
        ElNotification.error({
          title: "Error",
          message: e.response.data.message
            ? e.response.data.message
            : "Error Submit",
          type: "error",
        });
        // MsgPrompt.error(error);
      } finally {
        isSaving.value = false;
      }
    } else {
      ElNotification.error({
        title: "Error",
        message: "error submit!" + fields,
        type: "error",
      });
      // MsgPrompt.error("error submit!" + fields);
    }
  });
};

const resetForm = () => {
  formData.value.platform = 0;
  formData.value.serviceId = 0;
  formData.value.group = "";
  formData.value.pcTag = "None";
};

const getAccountAndApplicationInfo = async () => {
  isLoading.value = true;
  formData.value = {
    pcTag: "None",
    password: "Pass!1234", // only for development
    accountId:
      accountDetails.value?.id ?? applicationDetails.value?.referenceId,
    ...applicationDetails.value.supplement,
    currencyId:
      applicationDetails.value.supplement.currencyId === -1
        ? null
        : applicationDetails.value.supplement.currencyId,
  };

  originalFormData.value = { ...formData.value };
  formData.value.serviceId = originalFormData.value.serviceId;

  // try {
  //   const getClientRebateRule = await RebateService.getRebateClientRule({
  //     clientAccountId: accountDetails.value.id,
  //   });
  //   console.log("getClientRebateRule", getClientRebateRule);

  //   pipCommissionDecision.value = getClientRebateRule.data[0].schema.find(
  //     (obj) =>
  //       obj.accountType === applicationDetails.value.supplement.accountType
  //   );

  //   if (pipCommissionDecision.value == undefined) {
  //     pipCommissionDecision.value = {
  //       commission: 0,
  //       pips: 0,
  //     };
  //   }
  // } catch (e) {
  //   try {
  //     const referralCodeInfo = await GlobalService.getReferralCodeSupplement(
  //       pendingApplication.value.supplement.referCode
  //     );
  //     pipCommissionDecision.value =
  //       referralCodeInfo.summary.allowAccountTypes.find(
  //         (obj) =>
  //           obj.accountType === applicationDetails.value.supplement.accountType
  //       );
  //     console.log("referralCodeInfo", referralCodeInfo);
  //   } catch (e) {
  //     console.log("e", e);
  //     pipCommissionDecision.value = {
  //       commission: 0,
  //       pips: 0,
  //     };
  //   }
  //   // MsgPrompt.error("Error getting rebate rule");
  // }

  const referralCode =
    props.accountDetails?.referCode ??
    pendingApplication.value.supplement.referCode;

  try {
    const referralCodeInfo = await GlobalService.getReferralCodeSupplement(
      referralCode
    );
    const allowAccountTypes =
      referralCodeInfo.summary?.allowAccountTypes ??
      referralCodeInfo.summary?.Schema ??
      referralCodeInfo.summary?.schema;

    pipCommissionDecision.value = allowAccountTypes.find(
      (obj) =>
        obj.accountType === applicationDetails.value.supplement.accountType
    );

    if (pipCommissionDecision.value == undefined) {
      console.log(
        "application account type",
        applicationDetails.value.supplement.accountType
      );
      console.log("referralCode AllowAccountTypes", allowAccountTypes);
      pipCommissionDecision.value = {
        commission: 0,
        pips: 0,
      };
    }
  } catch (e) {
    console.log(e);
    pipCommissionDecision.value = {
      commission: 0,
      pips: 0,
    };
  }

  isLoading.value = false;
};

const updateTag = async (_id: any) => {
  if (formData.value.pcTag == "None") {
    return;
  }

  const finalData = {
    id: _id,
    tagNames: [formData.value.pcTag],
  };

  await AccountService.updateAccountTagsById(
    accountDetails.value.id,
    finalData
  );
};

onMounted(async () => {
  await getAccountAndApplicationInfo();
});

defineExpose({
  resetForm,
  show: getAccountAndApplicationInfo,
  close,
});
</script>

<style scoped></style>
