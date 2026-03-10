<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.EditContract')"
    width="900"
    align-center
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-width="70px"
      class="demo-ruleForm"
      label-position="top"
    >
      <div class="row row-cols-2 g-5">
        <el-form-item :label="$t('fields.symbol')" prop="symbol">
          <el-input v-model="formData.symbol" disabled />
        </el-form-item>
        <el-form-item :label="$t('fields.category')" prop="category">
          <el-input v-model="formData.category" disabled />
        </el-form-item>
      </div>
      <div class="row row-cols-4 g-5">
        <el-form-item
          :label="$t('fields.tradeStartTime')"
          prop="trading_start_time"
        >
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.trading_start_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item
          :label="$t('fields.tradeEndTime')"
          prop="trading_end_time"
        >
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.trading_end_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item
          :label="$t('fields.tradeStartWeekDay')"
          prop="trading_start_weekday"
        >
          <el-select
            v-model="formData.trading_start_weekday"
            :disabled="isLoading"
          >
            <el-option
              v-for="(item, index) in weekdayOptions"
              :key="index"
              :label="item.label"
              :value="item.value"
            >
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item
          :label="$t('fields.tradeEndWeekDay')"
          prop="trading_end_weekday"
        >
          <el-select
            v-model="formData.trading_end_weekday"
            :disabled="isLoading"
          >
            <el-option
              v-for="(item, index) in weekdayOptions"
              :key="index"
              :label="item.label"
              :value="item.value"
            >
            </el-option>
          </el-select>
        </el-form-item>
      </div>
      <div class="row row-cols-4 g-5">
        <el-form-item :label="$t('fields.breakStartTime')">
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.break_start_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.breakEndTime')">
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.break_end_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.breakStartTime') + '- 2'">
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.more_break_start_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.breakEndTime') + '- 2'">
          <el-time-picker
            format="HH:mm"
            value-format="HH:mm"
            v-model="formData.more_break_end_time"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.contractSize')" prop="contract_size">
          <el-input-number
            v-model="formData.contract_size"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
        <el-form-item :label="$t('fields.commission')">
          <el-input-number
            v-model="formData.commission"
            :disabled="isLoading"
            class="w-200px"
          />
        </el-form-item>
      </div>
      <h6>{{ $t("fields.marginRequirement") }}</h6>
      <div class="row row-cols-4 g-5" v-if="site == 'bvi'">
        <el-form-item
          v-for="(item, index) in bviMargins"
          :key="index"
          :label="item.label"
        >
          <el-input
            v-model="formData.margin_requirements[item.value]"
            class="w-200px"
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
      <div v-else class="row row-cols-2 g-5">
        <el-form-item v-for="(margin, key) in auMargins" :key="key">
          <el-input
            class="w-100px"
            v-model="margin.label"
            :disabled="isLoading"
          />
          <el-input
            class="w-100px ms-2"
            v-model="margin.value"
            :disabled="isLoading"
          />
        </el-form-item>
      </div>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button
          type="warning"
          @click="submitForm"
          :loading="isLoading"
          plain
        >
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, reactive } from "vue";
import { weekdayOptions } from "@/core/types/documents/ContracSpecsTypes";
import { bviMargins } from "@/core/types/documents/ContracSpecsTypes";
import type { FormInstance } from "element-plus";
import DocsServices from "../../services/DocsServices";
import { ElNotification } from "element-plus";
import { useStore } from "@/store";
const store = useStore();
const user = store.state.AuthModule.user;
const dialogRef = ref(false);
const id = ref(0);
const site = ref("bvi");
const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();
const auMargins = ref<any>([]);
const formData = ref<any>({});
const show = (item: any) => {
  formData.value = item;
  id.value = item.id;
  site.value = item.site;
  if (site.value == "ba") {
    processAUMargins(item.margin_requirements);
  }
  dialogRef.value = true;
};

const processAUMargins = (margins: any) => {
  for (const key in margins) {
    if (Object.prototype.hasOwnProperty.call(margins, key)) {
      auMargins.value.push({
        label: key,
        value: margins[key],
      });
    }
  }
};

const rules = reactive<any>({
  trading_start_time: [
    {
      required: true,
      message: "Please input Trading Start Time",
      trigger: "blur",
    },
  ],
  trading_end_time: [
    {
      required: true,
      message: "Please input Trading End Time",
      trigger: "blur",
    },
  ],
  trading_start_weekday: [
    {
      required: true,
      message: "Please input Trading Start Weekday",
      trigger: "blur",
    },
  ],
  trading_end_weekday: [
    {
      required: true,
      message: "Please input Trading End Weekday",
      trigger: "blur",
    },
  ],
  contract_size: [
    { required: true, message: "Please input Contract Size", trigger: "blur" },
  ],
});

const getOperatorData = async () => {
  const operatorData = formData.value.operator_info ?? [];

  const data = {
    name: user.nativeName,
    email: user.email,
    updatedAt: new Date().toISOString(),
  };

  // Add the new data to the beginning of the array
  operatorData.unshift(data);

  // Limit the array to the latest 5 entries
  if (operatorData.length > 5) {
    operatorData.pop();
  }

  // Update the formData with the modified operator_info array
  formData.value.operator_info = operatorData;
};

const submitForm = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  isLoading.value = true;
  await ruleFormRef.value.validate(async (valid) => {
    isValid = valid;
  });
  if (!isValid) {
    isLoading.value = false;
    return;
  }
  try {
    if (site.value == "ba") {
      formData.value.margin_requirements = {};
      auMargins.value.forEach((margin: any) => {
        formData.value.margin_requirements[margin.label] = margin.value;
      });
    }
    await getOperatorData();
    await DocsServices.updateContractSpecs(id.value, formData.value);
    ElNotification({
      title: "Success",
      message: "Contract Spec updated successfully",
      type: "success",
    });
    dialogRef.value = false;
    emit("eventSubmit");
  } catch (error) {
    console.error(error);
    ElNotification({
      title: "Error",
      message: "Update Contract Spec failed",
      type: "error",
    });
  }
  isLoading.value = false;
};

const emit = defineEmits(["eventSubmit"]);

defineExpose({
  show,
});
</script>
