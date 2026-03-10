<template>
  <el-drawer
    v-model="dialogRef"
    :title="$t('fields.recordDetail')"
    direction="rtl"
    size="60%"
  >
    <div
      v-if="isLoading"
      class="d-flex align-items-center justify-content-center h-100"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
    <div v-else>
      <el-descriptions :column="3" border class="mb-3">
        <el-descriptions-item label="ID">{{ data?.id }}</el-descriptions-item>
        <el-descriptions-item label="Party ID">{{
          data?.receiverPartyId
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.receiver')">{{
          data?.receiver
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.receiverName')">{{
          data?.receiverName
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.method')">{{
          data?.method
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.eventId')">{{
          data?.eventId
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.event')">{{
          data?.event
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.status')">{{
          data?.status
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.note')">{{
          data?.note
        }}</el-descriptions-item>
        <el-descriptions-item :label="$t('fields.createdOn')">
          <TimeShow type="GMToneLiner" :date-iso-string="data.createdOn"
        /></el-descriptions-item>
      </el-descriptions>
      <div v-html="data?.content"></div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button
          type="danger"
          plain
          @click="dialogRef = false"
          :disabled="isLoading"
          >{{ $t("action.close") }}</el-button
        >
      </div>
    </template>
  </el-drawer>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import ReportService from "../../services/ReportService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
const isLoading = ref(false);
const dialogRef = ref(false);
const data = ref<any>({});
const id = ref(0);
const show = async (_item: any) => {
  dialogRef.value = true;
  id.value = _item.id;
  await fetchData();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await ReportService.queryMessageRecordById(id.value);
    data.value = res;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
