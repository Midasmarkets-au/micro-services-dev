<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('action.openTradeAccount')"
    width="800"
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
        <div class="col-6">
          <el-form-item :label="$t('fields.referCode')" prop="referCode">
            <el-input
              v-model="formData.referCode"
              :disabled="isLoading"
            ></el-input>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="$t('fields.accountType')" prop="accountType">
            <el-select v-model="formData.accountType" :disabled="isLoading">
              <el-option
                v-for="item in ConfigAllAccountTypeSelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-6">
          <el-form-item :label="$t('fields.currency')" prop="currencyId">
            <el-select v-model="formData.currencyId" :disabled="isLoading">
              <el-option
                v-for="item in ConfigCurrencySelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
        <div class="col-6">
          <el-form-item :label="$t('fields.leverage')" prop="leverage">
            <el-select v-model="formData.leverage" :disabled="isLoading">
              <el-option
                v-for="item in backendConfigLeverageSelections"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              />
            </el-select>
          </el-form-item>
        </div>
      </div>
      <div class="row">
        <div class="col-6">
          <el-form-item :label="$t('title.service')" prop="serviceId">
            <el-select v-model="formData.serviceId" :disabled="isLoading">
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
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="submit" :loading="isLoading">
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, reactive } from "vue";
import type { FormInstance } from "element-plus";
import UserService from "../../services/UserService";
import {
  ConfigAllAccountTypeSelections,
  AccountTypes,
} from "@/core/types/AccountInfos";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { backendConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";
import MsgPrompt from "@/core/plugins/MsgPrompt";
const isLoading = ref(false);
const dialogRef = ref(false);
const partyId = ref(0);
const formData = ref<any>({
  referCode: "",
  accountType: AccountTypes.Standard,
  currencyId: ConfigCurrencySelections.value.some(
    (currency) => currency.value === 840
  )
    ? 840
    : ConfigCurrencySelections.value[0].value,
  leverage: backendConfigLeverageSelections[0].value,
  serviceId: ConfigRealServiceSelections.value[0].id,
});
const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({
  referCode: [
    { required: true, message: "Please input refer code", trigger: "blur" },
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
    await UserService.openTradeAccount(partyId.value, formData.value).then(
      () => {
        MsgPrompt.success("Application submitted successfully").then(() => {
          dialogRef.value = false;
          emits("application-submitted");
        });
      }
    );
  } catch (error) {
    MsgPrompt.error("Failed to submit application");
    console.error(error);
  }
  isLoading.value = false;
};
const show = (_partyId) => {
  partyId.value = _partyId;
  dialogRef.value = true;
};
const hide = () => {
  dialogRef.value = false;
  formData.value = {
    referCode: "",
    accountType: AccountTypes.Standard,
    currencyId: ConfigCurrencySelections.value.some(
      (currency) => currency.value === 840
    )
      ? 840
      : ConfigCurrencySelections.value[0].value,
    leverage: backendConfigLeverageSelections[0].value,
    serviceId: ConfigRealServiceSelections.value[0].id,
  };
};

defineExpose({
  show,
});
</script>
