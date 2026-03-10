<template>
  <el-dialog
    v-model="dialogRef"
    :title="t('action.addTrackingNumber')"
    width="500"
    align-center
    :before-close="hide"
  >
    <el-input
      v-model="trackingNumber"
      :placeholder="t('action.addTrackingNumber')"
      :disabled="isLoading"
    ></el-input>
    <label v-if="showError" class="text-danger my-1">{{
      $t("title.fillRequiredInfo")
    }}</label>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" :loading="isLoading" @click="submit">
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import EventsServices from "../../services/EventsServices";
import { ElNotification } from "element-plus";
import i18n from "@/core/plugins/i18n";

const t = i18n.global.t;
const isLoading = ref(false);
const dialogRef = ref(false);
const trackingNumber = ref("");
const data = ref<any>({});
const showError = ref(false);
const checkTrackingNumber = () => {
  showError.value = trackingNumber.value.trim().length === 0;
};
const show = (_data: any) => {
  dialogRef.value = true;
  data.value = _data;
};
const emits = defineEmits(["fetch-data"]);
const submit = async () => {
  isLoading.value = true;
  try {
    checkTrackingNumber();
    if (showError.value) {
      isLoading.value = false;
      return;
    }
    await EventsServices.addTrackingNumber(data.value.id, {
      trackingNumber: trackingNumber.value,
    });
    await EventsServices.shipOrder(data.value.id);
    ElNotification({
      title: t("status.success"),
      message: t("status.success"),
      type: "success",
    });
    emits("fetch-data");
    hide();
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const hide = () => {
  dialogRef.value = false;
  trackingNumber.value = "";
  showError.value = false;
};

defineExpose({
  show,
});
</script>
