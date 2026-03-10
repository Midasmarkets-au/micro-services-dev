<template>
  <span
    class="position-relative cursor-pointer d-flex align-items-center"
    @click="copy"
    :class="{ 'pe-3': hasIcon }"
  >
    <span
      v-if="props.showText"
      class="d-flex"
      style="white-space: normal; text-overflow: ellipsis; overflow: hidden"
      :style="{ width: props.textWidth }"
    >
      {{ props.val }}
    </span>
    <inline-svg
      class="ml-1"
      v-if="hasIcon"
      src="/images/icons/general/gen054.svg"
    ></inline-svg>
    <span
      class="tip fs-8 badge badge-light"
      :class="{ 'show-tip-animation': showTip }"
      >{{ $t("tip.copiedToClipboard") }}
    </span>
  </span>
</template>
<script setup lang="ts">
import { ref } from "vue";
import Clipboard from "clipboard";

const props = defineProps({
  val: String,
  textWidth: { type: String, required: false },
  hasIcon: { type: Boolean, required: false },
  showText: { type: Boolean, required: false, default: true },
});

const showTip = ref(false);

const copy = () => {
  Clipboard.copy(props.val as string);
  showTip.value = true;
  setTimeout(() => {
    showTip.value = false;
  }, 1000);
};

defineExpose({
  copy,
});
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
