<template>
  <el-dialog
    v-model="dialogRef"
    :title="t('action.createTradeAccount') + ' - ' + title"
    width="750"
    align-center
    class="rounded"
    :before-close="hide"
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <div class="row">
        <div class="col-12">
          <el-form-item :label="t('fields.referCode')" prop="referCode">
            <el-input
              v-model="formData.referCode"
              clearable
              :disabled="isLoading"
            >
              <template #append>
                <el-button
                  :icon="Search"
                  @click="referCodeCheck()"
                  :disabled="isLoading || formData.referCode === ''"
                />
              </template>
            </el-input>
          </el-form-item>
        </div>
        <div class="col-12" v-show="isShow">
          <el-form-item :label="t('fields.role')">
            <el-input v-model="referCodeRole" disabled> </el-input>
          </el-form-item>
        </div>
      </div>
      <div class="row" v-show="isShow">
        <div class="col-6">
          <el-form-item :label="t('fields.accountType')">
            <el-select
              v-model="formData.accountType"
              :disabled="isLoading || referCodeStatus"
            >
              <el-option
                v-for="item in accountTypesOptions"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
            <!-- <el-input v-model="formData.accountType" disabled></el-input> -->
          </el-form-item>
        </div>
        <div class="col-6" v-show="isShow">
          <el-form-item :label="t('fields.currency')" prop="currencyId">
            <el-select
              v-model="formData.currencyId"
              :disabled="isLoading || referCodeStatus"
            >
              <el-option
                v-for="item in ConfigCurrencySelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
      </div>
      <div class="row" v-show="isShow">
        <div class="col-6">
          <el-form-item :label="t('fields.leverage')" prop="leverage">
            <el-select
              v-model="formData.leverage"
              :disabled="isLoading || referCodeStatus"
            >
              <el-option
                v-for="(item, index) in leverageOptions"
                :key="index"
                :label="item + ':1'"
                :value="item"
              />
            </el-select>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="t('fields.platform')" prop="serviceId">
            <el-select
              v-model="formData.serviceId"
              :disabled="isLoading || referCodeStatus"
            >
              <el-option
                v-for="item in ConfigRealServiceSelections"
                :key="item.id"
                :label="item.label"
                :value="item.id"
              />
            </el-select>
          </el-form-item>
        </div>
      </div>
    </el-form>
    <template #footer>
      <div class="dialog-footer" v-show="isShow">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button
          type="primary"
          @click="submit"
          :loading="isLoading"
          :disabled="referCodeStatus"
        >
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, reactive } from "vue";
import { ElNotification, type FormInstance } from "element-plus";
import SalesService from "../services/SalesService";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
import { Search } from "@element-plus/icons-vue";
import { useStore } from "@/store";
import { PublicSetting } from "@/core/types/ConfigTypes";
const store = useStore();
const t = i18n.global.t;
const isLoading = ref(false);
const referCodeStatus = ref(true);
const referCodeRole = ref("");
const dialogRef = ref(false);
const accountUid = ref(0);
const title = ref("");
const isShow = ref(false);
const projectConfig: PublicSetting = store.state.AuthModule.config;
const accountTypesOptions = ref(<any>[]);
projectConfig.accountTypeAvailable.forEach((value) => {
  accountTypesOptions.value.push({
    label: t("type.account." + value),
    value: value,
  });
});
const leverageOptions = ref(projectConfig.leverageAvailable);
const formData = ref<any>({
  referCode: "",
  accountType: null,
  currencyId: 840,
  leverage: leverageOptions.value[0],
  serviceId: ConfigRealServiceSelections.value[0].id,
});
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  referCode: [
    {
      required: true,
      message: t("error.INPUT_REQUIRE") + t("fields.referCode"),
      trigger: "blur",
    }, //"Please input refer code"
  ],
  accountType: [
    { required: true, message: "Please select account type", trigger: "blur" },
  ],
  currencyId: [
    { required: true, message: "Please select currency", trigger: "blur" },
  ],
  leverage: [
    { required: true, message: "Please select leverage", trigger: "blur" },
  ],
  serviceId: [
    { required: true, message: "Please select service", trigger: "blur" },
  ],
});

const emits = defineEmits<{
  (e: "application-submitted"): void;
}>();

const referCodeCheck = async () => {
  isLoading.value = true;
  try {
    const result = await SalesService.checkReferCode(formData.value.referCode);
    referCodeStatus.value = false;
    isShow.value = true;
    referCodeRole.value = t("type.accountRole." + result.serviceType);
    await getAccountOptions(result);
    ElNotification.success({
      title: "Success",
      message: "Refer code is valid",
    });
  } catch (error) {
    referCodeStatus.value = true;
    ElNotification.error({
      title: "Error",
      message: "Failed to check refer code or Refer code is invalid",
    });
    console.error(error);
  }
  isLoading.value = false;
};

const getAccountOptions = async (result: any) => {
  accountTypesOptions.value = [];
  switch (result.serviceType) {
    case AccountRoleTypes.IB:
      result.summary.schema.forEach((value: any) => {
        accountTypesOptions.value.push({
          label: t("type.account." + value.accountType),
          value: value.accountType,
        });
      });
      break;
    case AccountRoleTypes.Broker:
      result.summary.schema.forEach((value: any) => {
        accountTypesOptions.value.push({
          label: t("type.account." + value.accountType),
          value: value.accountType,
        });
      });
      break;
    case AccountRoleTypes.Client:
      result.summary.allowAccountTypes.forEach((value: any) => {
        accountTypesOptions.value.push({
          label: t("type.account." + value.accountType),
          value: value.accountType,
        });
      });
      break;
    default:
      projectConfig.accountTypeAvailable.forEach((value) => {
        accountTypesOptions.value.push({
          label: t("type.account." + value),
          value: value,
        });
      });
      break;
  }
  formData.value.accountType = accountTypesOptions.value[0].value;
};

const submit = async () => {
  if (ruleFormRef.value) {
    const valid = await ruleFormRef.value.validate();
    if (!valid) {
      return;
    }
  }
  isLoading.value = true;

  try {
    formData.value.platform = ConfigRealServiceSelections.value.find(
      (item) => item.id === formData.value.serviceId
    )?.platform;
    await SalesService.openTradeAccount(accountUid.value, formData.value).then(
      MsgPrompt.success("Application submitted successfully")
    );
    hide();
  } catch (error) {
    MsgPrompt.error(error);
    dialogRef.value = false;
    console.error(error);
  }
  isLoading.value = false;
};
const show = (_data) => {
  accountUid.value = _data.uid;
  title.value = getUserName(_data);

  dialogRef.value = true;
};

const hide = () => {
  dialogRef.value = false;
  referCodeStatus.value = true;
  referCodeRole.value = "";
  isShow.value = false;
  formData.value = {
    referCode: "",
    accountType: null,
    currencyId: 840,
    leverage: leverageOptions.value[0],
    serviceId: ConfigRealServiceSelections.value[0].id,
  };
};
const getUserName = (item: any) => {
  if (
    !item.user?.nativeName ||
    item.user?.nativeName === "" ||
    item.user?.nativeName === " "
  ) {
    if (
      !item.user?.displayName ||
      item.user?.displayName === "" ||
      item.user?.displayName === " "
    ) {
      if (
        !item.user?.firstName ||
        !item.user?.lastName ||
        item.user?.firstName === "" ||
        item.user?.lastName === "" ||
        item.user?.firstName === " " ||
        item.user?.lastName === " "
      ) {
        return "No Name";
      } else {
        return item.user?.firstName + " " + item.user?.lastName;
      }
    } else {
      return item.user?.displayName;
    }
  } else {
    return item.user?.nativeName;
  }
};
defineExpose({
  show,
});
</script>
