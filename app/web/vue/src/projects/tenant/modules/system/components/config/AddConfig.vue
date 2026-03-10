<template>
  <el-dialog
    v-model="dialogRef"
    title="Add Account Config"
    width="900"
    align-center
  >
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">Cancel</el-button>
        <el-button type="primary" @click="dialogRef = false">
          Confirm
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, inject } from "vue";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";

const dialogRef = ref(false);

const show = () => {
  dialogRef.value = true;
};

defineExpose({ show });
</script>
