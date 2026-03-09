<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.autoAssignSetting')"
    width="500"
    align-center
  >
    <div v-loading="isLoading">
      <div class="d-flex gap-5">
        <el-input
          v-model="uId"
          placeholder="Auto Assign Account UID"
          class="mb-3"
        >
          <template #prepend>Account UID</template>
        </el-input>

        <el-button type="success" @click="changeLeadAutoAssignUid">
          {{ $t("action.save") }}
        </el-button>
      </div>
      <div class="mt-5">
        <el-switch
          size="large"
          v-model="autoAssignSwitch"
          active-text="Enabled"
          inactive-text="Disabled"
          active-color="#13ce66"
          inactive-color="#ff4949"
          @change="switchAutoAssign"
        >
          <template #open>{{ $t("status.enabled") }}</template>
          <template #close>{{ $t("status.disabled") }}</template>
        </el-switch>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import UserService from "../../services/UserService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { ElLoading } from "element-plus";
import notification from "@/core/plugins/notification";
const dialogRef = ref(false);
const autoAssignSwitch = ref(false);
const uId = ref<any>(null);
const isLoading = ref(false);
const show = async () => {
  dialogRef.value = true;
  await fetchData();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await UserService.getLeadsAutoAssignSetting();
    autoAssignSwitch.value = res.enabled;
    uId.value = res.autoAssignAccountUid;
  } catch (e) {
    console.log(e);
    MsgPrompt.error(e);
  }
  isLoading.value = false;
};

const switchAutoAssign = async () => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });

  try {
    if (autoAssignSwitch.value == true)
      await UserService.enableLeadAutoAssign();
    else await UserService.disableLeadAutoAssign();
    notification.success();
  } catch (e) {
    console.log(e);
    MsgPrompt.error(e);
  }

  loading.close();
};

const changeLeadAutoAssignUid = async () => {
  const loading = ElLoading.service({
    lock: true,
    text: "Loading",
    background: "rgba(0, 0, 0, 0.7)",
  });

  try {
    await UserService.changeLeadAutoAssignUid({
      autoAssignAccountUid: uId.value,
    });
    notification.success();
  } catch (e) {
    console.log(e);
    MsgPrompt.error(e);
  }

  loading.close();
};

defineExpose({ show });
</script>
