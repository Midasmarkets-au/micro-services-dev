<template>
  <el-dialog
    v-model="dialogRef"
    class="video_dialog"
    :width="dialogWidth"
    align-center
    @closed="handleClosed"
  >
    <template #header></template>
    <div class="video-container">
      <iframe
        :src="source"
        allow="accelerometer; gyroscope;encrypted-media;"
        allowfullscreen="true"
      ></iframe>
    </div>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, computed } from "vue";
import { isMobile } from "@/core/config/WindowConfig";
const emit = defineEmits(["close"]);
const videoRef = ref<any>(null);
const source = ref("");
const dialogRef = ref(false);
const dialogWidth = computed(() => (isMobile.value ? "100%" : "50%"));
const show = (_source: string) => {
  source.value = _source;
  dialogRef.value = true;
};

const close = () => {
  source.value = "";
  dialogRef.value = false;
};

const handleClosed = () => {
  source.value = "";
  emit("close");
};
defineExpose({
  show,
  close,
});
</script>
<style scoped lang="scss">
@media (min-width: 949px) {
  .modal-width .el-dialog {
    min-width: 950px;
  }
}

@media (max-width: 950px) {
  .modal-width .el-dialog {
    min-width: 90%;
    min-height: 700px;
  }
}
@media (max-width: 768px) {
  .modal-width .el-dialog {
    min-width: 90% !important;
    min-height: 500px;
  }
}
.video-container {
  position: relative;
  padding-bottom: 56.25%;
}
.video-container {
  iframe {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
  }
}
</style>
