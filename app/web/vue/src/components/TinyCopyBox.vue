<template>
  <i
    class="fa-regular fa-paste text-gray cursor-pointer position-relative"
    @click="copy"
  >
    <span
      v-if="showTip"
      class="tip fs-8 badge badge-light"
      :class="{ 'show-tip-animation': showTip }"
      >{{ $t("fields.copied") }}
    </span>
  </i>
</template>

<script setup lang="ts">
import { ref } from "vue";
import Clipboard from "clipboard";
const showTip = ref(false);
const props = defineProps({
  val: String,
});

const copy = () => {
  Clipboard.copy(props.val as string);
  showTip.value = true;
  setTimeout(() => {
    showTip.value = false;
  }, 1000);
};
</script>

<style scoped>
.tip {
  opacity: 0;
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  transition: all 1s ease-in-out;
  pointer-events: none; /* This prevents the tip from being clickable */
}

.show-tip-animation {
  animation: tip-show 1s forwards;
}

@keyframes tip-show {
  0% {
    opacity: 0;
    bottom: 100%;
  }
  40%,
  60% {
    opacity: 1;
    bottom: 160%;
  }
  100% {
    opacity: 0;
    bottom: 160%;
  }
}
</style>
