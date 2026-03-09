<template>
  <el-dialog v-model="dialogRef" title="Account Info" width="1000" align-center>
    <span>Open the dialog from the center from the screen</span>
    <template #footer>
      <div class="dialog-footer">
        <el-button type="primary" @click="dialogRef = false">
          {{ $t("action.close") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";

const dialogRef = ref(false);

const show = () => {
  dialogRef.value = true;
};

defineExpose({
  show,
});
</script>
