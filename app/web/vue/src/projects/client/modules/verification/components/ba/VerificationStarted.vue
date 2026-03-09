<template>
  <div class="w-100 card">
    <div class="card-header">
      <div class="card-title">{{ $t("fields.accountInformation") }}</div>
    </div>
    <div class="card-body">
      <div class="mb-10">
        <el-form-item
          :label="$t('fields.currency')"
          prop="currency"
          class="mb-10"
        >
          <el-select
            v-model="formData.currency"
            placeholder="Select"
            name="currency"
            size="large"
            :disabled="isSubmitting"
          >
            <el-option
              v-for="(item, index) in ConfigCurrencySelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </el-form-item>

        <el-form-item
          :label="$t('fields.accountType')"
          prop="accountType"
          class="mb-10"
        >
          <el-select
            v-model="formData.accountType"
            placeholder="Select"
            name="accountType"
            size="large"
            :disabled="isSubmitting"
          >
            <el-option
              v-for="(item, index) in ConfigAccountTypeSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </el-form-item>

        <el-form-item
          :label="$t('fields.platform')"
          prop="serviceId"
          class="mb-10"
        >
          <el-select
            v-model="formData.serviceId"
            placeholder="Select"
            name="serviceId"
            size="large"
            :disabled="isSubmitting"
          >
            <el-option
              v-for="(item, index) in ConfigRealServiceSelections"
              :key="index"
              :value="item.id"
              :label="item.label"
            />
          </el-select>
        </el-form-item>

        <el-form-item
          :label="$t('fields.leverage')"
          prop="leverage"
          class="mb-10"
          v-show="ConfigLeverageSelections.length > 1"
        >
          <el-select
            v-model="formData.leverage"
            placeholder="Select"
            name="leverage"
            size="large"
            :disabled="isSubmitting"
          >
            <el-option
              v-for="(item, index) in ConfigLeverageSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
        </el-form-item>
      </div>
    </div>
  </div>

  <VerificationQuestions
    v-model:questions="formData.questions"
    hide-questions
  />
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { ConfigCurrencySelections } from "@/core/types/CurrencyTypes";
import { ConfigAccountTypeSelections } from "@/core/types/AccountInfos";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";
import { useVerificationForm } from "../../composables/useVerificationForm";
import {
  getDefaultCurrency,
  getDefaultServiceId,
  checkHasMt5,
  createDefaultQuestions,
  initializeFormData,
} from "../../utils/verification-utils";
import VerificationQuestions from "../shared/VerificationQuestions.vue";

const { items, formData, isSubmitting } = useVerificationForm();
const item = ref<any>(items?.value?.data?.started || {});

const hasMt5 = ref(checkHasMt5(ConfigRealServiceSelections.value));

const setLeverage = () => {
  return ConfigLeverageSelections?.value[0]?.value || 30;
};

const setAccountType = () => {
  return ConfigAccountTypeSelections?.value[0]?.value;
};

onMounted(() => {
  initializeFormData(item.value, formData.value, {
    currency: getDefaultCurrency(36, ConfigCurrencySelections.value), // AUD
    accountType: setAccountType(),
    leverage: setLeverage(),
    serviceId: getDefaultServiceId(
      hasMt5.value,
      ConfigRealServiceSelections.value
    ),
    questions: createDefaultQuestions(true),
  });
});
</script>

<style scoped lang="scss">
::v-deep .el-form-item__label {
  color: #3a3e44;
  font-size: 1rem;
}

::v-deep .el-radio__input {
  .el-radio__inner {
    border: 1px solid #ccd3e0;
  }
}
</style>
