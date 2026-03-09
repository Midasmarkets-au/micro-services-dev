<template>
  <el-dialog
    v-model="dialogVisible"
    title="Report Preview"
    width="80%"
    class="min-h-300px"
    ref="dialogRef"
    align-center
    draggable
  >
    <div v-if="isLoading">
      <LoadingRing />
    </div>
    <iframe v-else :srcdoc="htmlCode" width="100%" height="800"></iframe>
    <template #footer>
      <span class="dialog-footer">
        <el-button type="primary" @click="dialogVisible = false">
          {{ $t("action.close") }}
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref } from "vue";
import ReportService from "../../services/ReportService";

const dialogVisible = ref(false);
const dialogRef = ref(null);
const data = ref(Array<any>());
const htmlCode = ref(null);
const isLoading = ref(false);
const show = (item: any) => {
  dialogVisible.value = true;
  data.value = item;
  fetchData();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await ReportService.queryAccountReportPreviewById(
      data.value.id
    );
    htmlCode.value = res;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};
defineExpose({ show });
</script>
