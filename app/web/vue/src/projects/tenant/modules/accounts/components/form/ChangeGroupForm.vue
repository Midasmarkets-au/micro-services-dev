<template>
  <el-dialog
    v-model="showGroup"
    title="Change IB Group"
    width="500"
    align-center
  >
    <span class="bg-success text-white fs-4 rounded p-2 text-center">
      <span class="text-body-tertiary">Current Group: </span
      ><span class="fw-bold">{{ currentGroup }}</span>
    </span>
    <el-form
      class="mt-8"
      :rules="formRules"
      :model="formData"
      ref="ruleFormRef"
    >
      <el-form-item prop="groupName">
        <el-input
          v-model="formData.groupName"
          class="w-200px"
          placeholder="Please input new group name"
        />
      </el-form-item>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="showGroup = false" :disabled="isLoading">{{
          $t("action.cancel")
        }}</el-button>
        <el-button type="primary" @click="updateGroup()" :disabled="isLoading">
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, onMounted, reactive } from "vue";
import AccountService from "../../services/AccountService";
import { type FormRules, FormInstance } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const isLoading = ref(false);
const ruleFormRef = ref<FormInstance>();
const showGroup = ref(false);
const currentGroup = ref("");
const formData = ref({
  id: 0,
  groupName: "",
});

const formRules = reactive<FormRules>({
  groupName: [
    { required: true, message: "Please input new group name", trigger: "blur" },
  ],
});

const show = (_id: number, _group: string) => {
  showGroup.value = true;
  currentGroup.value = _group;
  formData.value.id = _id;
};

const updateGroup = async () => {
  if (!ruleFormRef.value) return;
  isLoading.value = true;
  let isValid = false;
  await ruleFormRef.value.validate(async (valid, fields) => {
    isValid = valid;
  });
  if (!isValid) {
    isLoading.value = false;
    return;
  } else {
    try {
      await AccountService.updateGroupName(formData.value.id, formData.value);
      MsgPrompt.success("success", "Update group successfully");
      showGroup.value = false;
      formData.value.groupName = "";
    } catch (e) {
      const apiErrorMessage = e.response?.data?.message; // Adjust this path as needed
      const errorMessage = apiErrorMessage || "Update group failed";
      MsgPrompt.error("error", errorMessage);
      formData.value.groupName = "";
      console.log(e);
    }
  }

  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
