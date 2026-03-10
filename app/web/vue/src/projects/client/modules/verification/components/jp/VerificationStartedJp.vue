<template>
  <div class="card">
    <div class="card-body">
      <div class="pb-3">
        <h2 class="fw-bold d-flex align-items-center text-dark">
          {{ $t("fields.accountInformation") }}
        </h2>
      </div>
      <hr />
      <el-form-item :label="$t('fields.accountType')" prop="accountType">
        <el-cascader
          :options="accountTypesSelection"
          v-model="formData.accountType"
          @change="handleChange"
          :disabled="isSubmitting"
          size="large"
          class="w-100"
        />
      </el-form-item>
      <el-form-item :label="$t('fields.leverage')" prop="leverage">
        <el-select
          v-model="formData.leverage"
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
</template>
<script setup lang="ts">
import { ref, inject, onMounted } from "vue";
import { ConfigLeverageSelections } from "@/core/types/LeverageTypes";
import { CurrencyTypes } from "@/core/types/CurrencyTypes";
import { ServiceTypes } from "@/core/types/ServiceTypes";
import { accountTypesSelection } from "@/core/types/jp/verificationStarted";

const items = inject<any>("items");
const formData = inject<any>("formData");
const isSubmitting = inject<any>("isSubmitting");
const item = ref<any>(items?.value?.data?.started || {});

const handleChange = (value) => {
  formData.value.accountType = value[1];
};

onMounted(() => {
  if (Object.keys(item.value).length === 0) {
    formData.value.currency = CurrencyTypes.JPY;
    formData.value.serviceId = ServiceTypes.MetaTrader5;
  } else {
    formData.value = item.value;
  }
});
</script>
<style scoped>
:deep .el-form-item__label {
  color: #181c32;
  font-weight: 600;
  font-size: 1.075rem;
}
</style>
