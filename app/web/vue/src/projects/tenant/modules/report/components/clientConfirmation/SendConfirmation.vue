<template>
  <el-dialog
    v-model="dialogVisible"
    title="Send Report"
    width="40%"
    class="min-h-200px"
    ref="dialogRef"
    align-center
    draggable
  >
    <div>
      <el-input
        class="mb-2 w-300px"
        v-model="email"
        placeholder="Email"
      ></el-input>
      <div>
        <el-text type="warning" class="mt-4 mx-1" size="large"
          >Leave email field blank will send the report to the user.</el-text
        >
      </div>
    </div>
    <template #footer>
      <span class="dialog-footer">
        <el-button @click="dialogVisible = false"> Close </el-button>
        <el-button type="primary" @click="send"> Send </el-button>
      </span>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref } from "vue";
import ReportService from "../../services/ReportService";
import { ElNotification } from "element-plus";

const dialogVisible = ref(false);
const dialogRef = ref(null);
const data = ref(Array<any>());
const email = ref(null);
const isLoading = ref(false);

const show = (item: any) => {
  dialogVisible.value = true;
  data.value = item;
};
const send = async (item: any) => {
  isLoading.value = true;

  try {
    await ReportService.sendAccountReport(data.value.id, {
      email: email.value,
    }).then((res) => {
      ElNotification({
        title: "Success",
        message: "Send successfully",
        type: "success",
      });
      dialogVisible.value = false;
    });
  } catch (error) {
    ElNotification({
      title: "Error",
      message: "Send failed",
      type: "error",
    });
  }
  isLoading.value = false;
};

defineExpose({ show });
</script>
