<template>
  <div
    v-if="showImageModal"
    class="showImageModalBackDrop"
    @click="hide"
    style="z-index: 999"
  >
    <img :src="src" alt="" style="height: 70vh" />
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";
import JwtService from "@/core/services/JwtService";

const src = ref("");
const showImageModal = ref(false);

const show = async (key?: string) => {
  showImageModal.value = true;

  if (key === "no-image") src.value = "/images/no-image.png";
  else if (key === null || key === "" || key === "no-avatar") {
    src.value = "/images/avatar.png";
  } else {
    src.value =
      window["api"] +
      "/api/v1/client/media/" +
      key +
      "?access_token=" +
      JwtService.getToken();
  }
};

const hide = () => {
  showImageModal.value = false;
};

defineExpose({
  show,
});
</script>

<style scoped>
.showImageModalBackDrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
