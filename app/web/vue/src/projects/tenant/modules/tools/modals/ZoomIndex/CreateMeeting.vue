<template>
  <SimpleForm
    ref="modalRef"
    title="Create Zoom Meeting"
    :is-loading="isLoading"
    :submit="submit"
    style="width: 500px"
  >
    <div>
      <el-form
        ref="ruleFormRef"
        :model="formData"
        :rules="rules"
        class="demo-ruleForm"
        label-position="top"
      >
        <el-form-item label="Topic">
          <el-input v-model="formData.topic" />
        </el-form-item>
        <el-form-item label="Start Time">
          <el-date-picker
            v-model="formData.start_time"
            type="datetime"
            placeholder="Select date and time"
          />
        </el-form-item>
        <el-form-item label="TimeZone">
          <el-select v-model="formData.timezone" filterable>
            <el-option
              v-for="item in ZoomTimeZone"
              :key="item"
              :label="item"
              :value="item"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Recording">
          <el-select v-model="formData.auto_recording">
            <el-option
              v-for="item in autoRecordingOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </el-form-item>
      </el-form>
    </div>
  </SimpleForm>
</template>

<script lang="ts" setup>
import { ref, reactive } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import type { FormInstance } from "element-plus";
import { ZoomTimeZone } from "@/core/types/ZoomTimeZone";
import ToolServices from "../services/ToolServices";
import axios from "axios";
const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();

const ruleFormRef = ref<FormInstance>();
const rules = reactive<any>({});

const formData = ref<any>({
  audio: "both",
  meeting_authentication: false,
  join_before_host: true,
  host_video: false,
  participant_video: false,
  mute_upon_entry: true,
  waiting_room: false,
  timezone: "Australia/Sydney",
  auto_recording: "cloud",
  type: 2,
});

const autoRecordingOptions = [
  { label: "Local", value: "local" },
  { label: "Cloud", value: "cloud" },
  { label: "None", value: "none" },
];

const createMeeting = async () => {
  isLoading.value = true;

  isLoading.value = false;
};

const show = async () => {
  isLoading.value = true;
  modalRef.value?.show();
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};
const submit = async () => {
  isLoading.value = true;
  await createMeeting();
  isLoading.value = false;
};
defineExpose({
  show,
  hide,
});
</script>
