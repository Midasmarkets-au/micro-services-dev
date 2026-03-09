<template>
  <div class="position-relative">
    <button
      class="btn btn-light-primary btn-bordered btn-radius btn-xs fs"
      style="font-weight: normal"
      @click="copy"
    >
      {{ $t("action.copyReferalLink") }}
    </button>
    <span
      class="tip fs-8 badge badge-light"
      :class="{ 'show-tip-animation': showTip }"
      >{{ $t("tip.copiedToClipboard") }}
    </span>
  </div>
</template>
<script setup lang="ts">
import { ref, computed } from "vue";
import { useStore } from "@/store";
const store = useStore();
const siteId = computed(() => store.state.AuthModule.config.siteId);

const props = defineProps({
  code: String,
  language: String,
  siteId: Number,
  hasIcon: { type: Boolean, required: false },
  showText: { type: Boolean, required: false, default: true },
});

var baseUrl = process.env.VUE_APP_BASE_URL;
if (siteId.value == 3 || siteId.value == 1) {
  baseUrl = process.env.VUE_APP_BASE_CDN_URL;
}

const showTip = ref(false);

const copy = () => {
  let link =
    baseUrl +
    "/sign-up?code=" +
    props.code +
    "&siteId=" +
    props.siteId +
    "&lang=" +
    props.language;

  navigator.clipboard.writeText(link);

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
@media (max-width: 768px) {
  .fs {
    font-size: 0.65rem;
  }
}
</style>
