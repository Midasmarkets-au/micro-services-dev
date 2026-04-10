<template>
  <el-dialog
    v-model="dialogRef"
    title="Auto Transfer Setting"
    width="500"
    align-center
  >
    <el-form>
      <el-form-item label="Enabled">
        <div class="mb-5">
          <el-switch v-model="formData.enabled" :disalbed="isLoading" />
        </div>
      </el-form-item>
      <el-form-item label="Amount">
        <div>
          <el-input-number
            v-model="formData.amount"
            :disalbed="isLoading"
          ></el-input-number>
          <span>&nbsp; USD</span>
        </div>
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disalbed="isLoading"
          >Cancel</el-button
        >
        <el-button type="primary" @click="submit" :loading="isLoading">
          Submit
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import PaymentService from "../../services/PaymentService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const dialogRef = ref(false);
const isLoading = ref(false);
const formData = ref({
  enabled: false,
  amount: 0,
});

const show = () => {
  dialogRef.value = true;
  fetchData();
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const response = await PaymentService.transferAutoComplete();
    formData.value.enabled = response.enabled;
    formData.value.amount = response.amount / 100;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

const submit = async () => {
  isLoading.value = true;
  try {
    await PaymentService.updateTransferAutoComplete({
      enabled: formData.value.enabled,
      amount: formData.value.amount * 100,
    });
    MsgPrompt.success("Auto Transfer Setting updated successfully").then(() => {
      dialogRef.value = false;
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
