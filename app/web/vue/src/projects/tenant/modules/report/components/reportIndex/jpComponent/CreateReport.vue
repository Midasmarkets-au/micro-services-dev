<template>
  <el-dialog
    v-model="dialogRef"
    title="Create Report Types"
    width="500"
    align-center
    :before-close="close"
  >
    <el-form
      ref="ruleFormRef"
      :model="query"
      :rules="rules"
      label-position="top"
    >
      <el-form-item label="Group or Account Numbers" v-if="showSelectByGroup">
        <el-radio-group
          v-model="selectByGroup"
          :disabled="isLoading"
          @change="changeSelection()"
        >
          <el-radio :label="true">Group</el-radio>
          <el-radio :label="false">AccountNumbers</el-radio>
        </el-radio-group>
      </el-form-item>
      <el-form-item
        v-for="(item, index) in query"
        :key="index"
        :label="index"
        :prop="index"
      >
        <el-date-picker
          v-if="queryDate.includes(index)"
          v-model="query[index]"
          :type="index === 'month' ? 'month' : 'date'"
          :format="index === 'month' ? 'YYYY-MM' : 'YYYY-MM-DD'"
          :value-format="index === 'month' ? 'YYYY-MM' : 'YYYY-MM-DD'"
          :disabled="isLoading"
        />

        <el-select
          v-else-if="index === 'group' && groups.length !== 0"
          v-model="query[index]"
          filterable
          :disabled="isLoading"
        >
          <el-option v-for="group in groups" :key="group" :value="group">
            {{ group }}
          </el-option>
        </el-select>

        <el-select
          v-else-if="index === 'type'"
          v-model="query[index]"
          :disabled="isLoading"
        >
          <el-option
            v-for="type in productTypes"
            :key="type.value"
            :value="type.value"
            :label="type.label"
          >
          </el-option>
        </el-select>

        <el-input
          v-else
          v-model="query[index]"
          @input="query[index] = query[index].toUpperCase()"
          :disabled="isLoading"
        ></el-input>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" :loading="isLoading" @click="submit">
          {{ $t("action.submit") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import type { FormInstance } from "element-plus";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";
import notification from "@/core/plugins/notification";

const isLoading = ref(false);
const dialogRef = ref(false);
const ruleFormRef = ref<FormInstance>();
const query = ref<any>({});
const formData = ref<any>({});
const queryDate = ref<any>(["from", "to", "date", "month"]);
const groups = ref<any>([]);

const showSelectByGroup = ref(false);
const selectByGroup = ref(true);

const productTypes = ref<any>([
  { label: "FX STD", value: 17 },
  { label: "FX Raw", value: 20 },
  { label: "IND STD", value: 16 },
  { label: "IND Raw", value: 19 },
  { label: "CFD STD", value: 15 },
  { label: "CFD Raw", value: 18 },
]);

const rules = {
  accountNumber: [
    { required: true, message: "Please input account number", trigger: "blur" },
  ],
  uid: [{ required: true, message: "Please input uid", trigger: "blur" }],
  group: [
    { required: true, message: "Please select a group", trigger: "blur" },
  ],
  date: [{ required: true, message: "Please select a date", trigger: "blur" }],
  from: [
    { required: true, message: "Please select a from date", trigger: "blur" },
  ],
  to: [{ required: true, message: "Please select a to date", trigger: "blur" }],
  type: [{ required: true, message: "Please select a type", trigger: "blur" }],
};

const showSelectByGroupArray = ["ClientStatement", "TaxationReport"];

const show = async (data: any, _group) => {
  dialogRef.value = true;
  formData.value = data;
  groups.value = _group;
  if (formData.value.query && typeof formData.value.query === "string") {
    query.value = JSON.parse(formData.value.query);
  } else {
    query.value = formData.value.query || {};
  }
  if (query.value.type === null) {
    query.value.type = productTypes.value[0].value;
  }
  if (showSelectByGroupArray.includes(formData.value.key)) {
    showSelectByGroup.value = true;
    changeSelection();
  }
};
const close = () => {
  formData.value = {};
  query.value = {};
  dialogRef.value = false;
  showSelectByGroup.value = false;
  selectByGroup.value = true;
};

const changeSelection = () => {
  if (selectByGroup.value == true) {
    delete query.value.accountNumbers;
    query.value.group = null;
  } else {
    delete query.value.group;
    query.value.accountNumbers = null;
  }
};

const submit = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;
  formData.value.query = query.value;
  isLoading.value = true;
  try {
    formData.value.query = query.value;
    await ReportService.postReportRequest(formData.value);
    emits("fetchData");
    close();
  } catch (error) {
    console.error(error);
    notification.danger();
  }
  isLoading.value = false;
};

const emits = defineEmits<{
  (e: "fetchData"): void;
}>();

defineExpose({
  show,
});
</script>
