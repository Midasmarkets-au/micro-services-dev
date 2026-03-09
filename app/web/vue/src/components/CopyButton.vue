<template>
  <div class="position-relative">
    <button class="btn btn-light btn-primary btn-sm" @click="copy">
      {{ $t("action.copy") }}
    </button>
    <span
      class="tip fs-8 badge badge-light"
      :class="{ 'show-tip-animation': showTip }"
      >{{ $t("tip.copiedToClipboard") }}
    </span>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";

const props = defineProps({
  content: String,
});

const showTip = ref(false);

const copy = (event) => {
  event.preventDefault();
  navigator.clipboard.writeText(props.content);

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
