<template>
  <el-dialog
    v-model="dialogRef"
    title="Create Report Types"
    width="500"
    align-center
  >
    <el-form
      ref="ruleFormRef"
      :model="formData"
      :rules="rules"
      label-position="top"
    >
      <el-form-item>
        <el-input
          v-model="formData.title"
          placeholder="Title"
          :disabled="isLoading"
          clearable
        ></el-input>
      </el-form-item>
      <el-form-item>
        <el-input
          v-model="formData.key"
          placeholder="Key"
          clearable
          :disabled="isLoading"
        ></el-input>
      </el-form-item>
      <el-form-item>
        <el-select
          v-model="formData.type"
          placeholder="Type"
          :disabled="isLoading"
        >
          <el-option value="pdf" label="PDF"></el-option>
          <el-option value="csv" label="CSV"></el-option>
          <el-option value="excel" label="Excel"></el-option>
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-checkbox-group v-model="query" :disabled="isLoading">
          <el-checkbox label="month">Month</el-checkbox>
          <el-checkbox label="year">Year</el-checkbox>
          <el-checkbox label="date">Date</el-checkbox>
          <el-checkbox label="from">From</el-checkbox>
          <el-checkbox label="to">To</el-checkbox>
          <el-checkbox label="group">Group</el-checkbox>
          <el-checkbox label="uid">Uid</el-checkbox>
          <el-checkbox label="accountNumber">Account Number</el-checkbox>
        </el-checkbox-group>
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading"
          >Cancel</el-button
        >
        <el-button type="primary" @click="submitForm" :loading="isLoading">
          Confirm
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import type { FormInstance } from "element-plus";
import ReportService from "@/projects/tenant/modules/report/services/ReportService";

const isLoading = ref(false);
const dialogRef = ref(false);
const ruleFormRef = ref<FormInstance>();
const query = ref<Array<any>>([]);
const formData = ref<any>({
  title: "",
  key: "",
  type: "",
  query: {
    date: null,
    group: null,
  },
  status: 0,
});

const rules = {
  title: [{ required: true, message: "Please input title", trigger: "blur" }],
  key: [{ required: true, message: "Please input key", trigger: "blur" }],
};
const show = () => {
  dialogRef.value = true;
};

const handleQuery = async () => {
  if (!query.value.includes("date")) {
    delete formData.value.query.date;
  } else {
    formData.value.query.date = null;
  }
  if (!query.value.includes("group")) {
    delete formData.value.query.group;
  } else {
    formData.value.query.group = null;
  }
  if (!query.value.includes("from")) {
    delete formData.value.query.from;
  } else {
    formData.value.query.from = null;
  }
  if (!query.value.includes("to")) {
    delete formData.value.query.to;
  } else {
    formData.value.query.to = null;
  }
  if (!query.value.includes("uid")) {
    delete formData.value.query.uid;
  } else {
    formData.value.query.uid = null;
  }
  if (!query.value.includes("accountNumber")) {
    delete formData.value.query.accountNumber;
  } else {
    formData.value.query.accountNumber = null;
  }
  if (!query.value.includes("month")) {
    delete formData.value.query.month;
  } else {
    formData.value.query.month = null;
  }
  if (!query.value.includes("year")) {
    delete formData.value.query.year;
  } else {
    formData.value.query.year = null;
  }
};

const submitForm = async () => {
  if (!ruleFormRef.value) return;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) return;

  try {
    isLoading.value = true;
    await handleQuery();
    await ReportService.createReportType(formData.value);
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
});
</script>
