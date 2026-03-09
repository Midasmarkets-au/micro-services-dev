<template>
  <div class="d-flex gap-4" style="overflow-x: auto">
    <!-- Demo Account Prefixes Card -->
    <div class="card" style="flex: 1; min-width: 400px">
      <div class="card-header">
        <h3 class="card-title">{{ $t("title.demoAccountPrefixes") }}</h3>
      </div>
      <div class="card-body">
        <el-alert
          v-if="demoError"
          :title="demoError"
          type="error"
          show-icon
          :closable="false"
          class="mb-4"
        />

        <div v-if="demoLoading" class="text-center py-5">
          <el-icon class="is-loading" :size="40">
            <Loading />
          </el-icon>
        </div>

        <div v-else>
          <div class="d-flex flex-column gap-3">
            <div v-for="(value, currency) in demoData" :key="currency">
              <div class="d-flex align-items-center gap-3">
                <span class="fs-5 fw-bold" style="min-width: 80px">{{
                  currency
                }}</span>
                <el-input
                  v-model="demoInput1[currency]"
                  :placeholder="$t('fields.prefix')"
                  style="max-width: 300px"
                  @input="validateDemoInput(currency)"
                />
              </div>
              <div
                v-if="demoErrors[currency]"
                class="text-danger fs-7 mt-1"
                style="margin-left: 92px"
              >
                {{ demoErrors[currency] }}
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-end mt-4">
            <el-button
              type="primary"
              :loading="demoUpdating"
              :disabled="hasErrors('demo')"
              @click="updatePrefixes('demo')"
            >
              {{ $t("action.update") }}
            </el-button>
          </div>
        </div>
      </div>
    </div>

    <!-- Real Account Prefixes Card -->
    <div class="card" style="flex: 1; min-width: 400px">
      <div class="card-header">
        <h3 class="card-title">{{ $t("title.realAccountPrefixes") }}</h3>
      </div>
      <div class="card-body">
        <el-alert
          v-if="realError"
          :title="realError"
          type="error"
          show-icon
          :closable="false"
          class="mb-4"
        />

        <div v-if="realLoading" class="text-center py-5">
          <el-icon class="is-loading" :size="40">
            <Loading />
          </el-icon>
        </div>

        <div v-else>
          <div class="d-flex flex-column gap-3">
            <div v-for="(value, currency) in realData" :key="currency">
              <div class="d-flex align-items-center gap-3">
                <span class="fs-5 fw-bold" style="min-width: 80px">{{
                  currency
                }}</span>
                <el-input
                  v-model="realInput1[currency]"
                  :placeholder="$t('fields.prefix')"
                  class="flex-1"
                  @input="validateInput1(currency, realInput1, 'real')"
                />
                <span>-</span>
                <el-input
                  v-model="realInput2[currency]"
                  :placeholder="$t('fields.suffix')"
                  class="flex-1"
                  @input="validateInput2(currency, realInput2, 'real')"
                />
              </div>
              <div
                v-if="realErrors[currency]"
                class="text-danger fs-7 mt-1"
                style="margin-left: 92px"
              >
                {{ realErrors[currency] }}
              </div>
            </div>
          </div>

          <div class="d-flex justify-content-end mt-4">
            <el-button
              type="primary"
              :loading="realUpdating"
              :disabled="hasErrors('real')"
              @click="updatePrefixes('real')"
            >
              {{ $t("action.update") }}
            </el-button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from "vue";
import { Loading } from "@element-plus/icons-vue";
import AccountService from "../services/AccountService";
import { ElMessage } from "element-plus";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

// Service IDs
const DEMO_SERVICE_ID = 31;
const REAL_SERVICE_ID = 30;

// Demo account data
const demoData = ref<Record<string, number>>({});
const demoInput1 = reactive<Record<string, string>>({});
const demoInput2 = reactive<Record<string, string>>({});
const demoErrors = reactive<Record<string, string>>({});
const demoLoading = ref(false);
const demoUpdating = ref(false);
const demoError = ref("");

// Real account data
const realData = ref<Record<string, number>>({});
const realInput1 = reactive<Record<string, string>>({});
const realInput2 = reactive<Record<string, string>>({});
const realErrors = reactive<Record<string, string>>({});
const realLoading = ref(false);
const realUpdating = ref(false);
const realError = ref("");

// Split number into two parts
const splitNumber = (value: number): [string, string] => {
  const str = value.toString();
  let firstNonZero = 0;

  // Find first non-zero position
  for (let i = 0; i < str.length; i++) {
    if (str[i] !== "0") {
      firstNonZero = i;
      break;
    }
  }

  // Find last non-zero position
  let lastNonZero = str.length - 1;
  for (let i = str.length - 1; i >= 0; i--) {
    if (str[i] !== "0") {
      lastNonZero = i;
      break;
    }
  }

  const input1 = str.substring(0, lastNonZero + 1);
  const input2 = str.substring(lastNonZero + 1);

  return [input1, input2.padEnd(6, "0")]; // Default to 6 zeros if needed
};

// Validate Demo Input: must be numeric only
const validateDemoInput = (currency: string) => {
  const value = demoInput1[currency];

  if (!value) {
    demoErrors[currency] = t("error.input1Required");
    return false;
  }

  // Must be numeric
  if (!/^\d+$/.test(value)) {
    demoErrors[currency] = t("error.input1MustBeNumeric");
    return false;
  }

  delete demoErrors[currency];
  return true;
};

// Validate Input1: cannot start with 0, cannot have all zeros, must be numeric
const validateInput1 = (
  currency: string,
  inputObj: Record<string, string>,
  type: "demo" | "real"
) => {
  const value = inputObj[currency];
  const errors = type === "demo" ? demoErrors : realErrors;

  if (!value) {
    errors[currency] = t("error.input1Required");
    return false;
  }

  // Must be numeric
  if (!/^\d+$/.test(value)) {
    errors[currency] = t("error.input1MustBeNumeric");
    return false;
  }

  // Cannot start with 0
  if (value[0] === "0") {
    errors[currency] = t("error.input1CannotStartWithZero");
    return false;
  }

  // Cannot have all digits be 0
  if (/^0+$/.test(value)) {
    errors[currency] = t("error.input1CannotBeAllZeros");
    return false;
  }

  // Check if any digit is 0 (100 is invalid)
  if (value.includes("0")) {
    errors[currency] = t("error.input1CannotContainZero");
    return false;
  }

  delete errors[currency];
  return true;
};

// Validate Input2: length must be between 4-7
const validateInput2 = (
  currency: string,
  inputObj: Record<string, string>,
  type: "demo" | "real"
) => {
  const value = inputObj[currency];
  const errors = type === "demo" ? demoErrors : realErrors;

  if (!value) {
    errors[currency] = t("error.input2Required");
    return false;
  }

  if (value.length < 4 || value.length > 7) {
    errors[currency] = t("error.input2LengthInvalid");
    return false;
  }

  delete errors[currency];
  return true;
};

// Check if there are any errors
const hasErrors = (type: "demo" | "real"): boolean => {
  const errors = type === "demo" ? demoErrors : realErrors;
  const data = type === "demo" ? demoData.value : realData.value;

  // Check if there are any error messages
  if (Object.keys(errors).length > 0) {
    return true;
  }

  // Validate all currencies
  for (const currency in data) {
    if (type === "demo") {
      if (!validateDemoInput(currency)) return true;
    } else {
      const input1 = realInput1;
      const input2 = realInput2;
      if (!validateInput1(currency, input1, type)) return true;
      if (!validateInput2(currency, input2, type)) return true;
    }
  }

  return false;
};

// Load demo account prefixes
const loadDemoPrefixes = async () => {
  demoLoading.value = true;
  demoError.value = "";

  try {
    const response = await AccountService.getAccountPrefixes(DEMO_SERVICE_ID);
    demoData.value = response.accountPrefixes;

    // For demo, use the complete value without splitting
    for (const [currency, value] of Object.entries(demoData.value)) {
      demoInput1[currency] = value.toString();
    }
  } catch (error: any) {
    demoError.value = error.message || t("error.loadFailed");
  } finally {
    demoLoading.value = false;
  }
};

// Load real account prefixes
const loadRealPrefixes = async () => {
  realLoading.value = true;
  realError.value = "";

  try {
    const response = await AccountService.getAccountPrefixes(REAL_SERVICE_ID);
    realData.value = response.accountPrefixes;

    // Split each value into input1 and input2
    for (const [currency, value] of Object.entries(realData.value)) {
      const [input1, input2] = splitNumber(value);
      realInput1[currency] = input1;
      realInput2[currency] = input2;
    }
  } catch (error: any) {
    realError.value = error.message || t("error.loadFailed");
  } finally {
    realLoading.value = false;
  }
};

// Update prefixes
const updatePrefixes = async (type: "demo" | "real") => {
  const serviceId = type === "demo" ? DEMO_SERVICE_ID : REAL_SERVICE_ID;
  const data = type === "demo" ? demoData.value : realData.value;

  if (type === "demo") {
    demoUpdating.value = true;
  } else {
    realUpdating.value = true;
  }

  try {
    const payload: Record<string, number> = {};

    if (type === "demo") {
      // For demo, use input1 directly
      for (const currency in data) {
        payload[currency] = parseInt(demoInput1[currency], 10);
      }
    } else {
      // For real, merge input1 and input2
      for (const currency in data) {
        const combined = realInput1[currency] + realInput2[currency];
        payload[currency] = parseInt(combined, 10);
      }
    }

    await AccountService.updateAccountPrefixes(serviceId, payload);

    ElMessage.success(t("message.updateSuccess"));

    // Reload data
    if (type === "demo") {
      await loadDemoPrefixes();
    } else {
      await loadRealPrefixes();
    }
  } catch (error: any) {
    ElMessage.error(error.message || t("error.updateFailed"));
  } finally {
    if (type === "demo") {
      demoUpdating.value = false;
    } else {
      realUpdating.value = false;
    }
  }
};

onMounted(async () => {
  await Promise.all([loadDemoPrefixes(), loadRealPrefixes()]);
});
</script>

<style scoped>
.border {
  border: 1px solid #e4e6ef;
}

.rounded {
  border-radius: 0.475rem;
}
</style>
