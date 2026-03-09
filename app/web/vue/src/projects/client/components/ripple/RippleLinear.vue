<template>
  <div
    :class="cn('absolute shadow-xl', 'animate-ripple-circle', props.class)"
  />
</template>

<script setup lang="ts">
import { cn } from "@/lib/utils";

interface Props {
  size?: number;
  class?: string;
  opacity?: number;
  animationDelay?: number;
  borderStyle?: string;
  borderWidth?: number;
}

const props = withDefaults(defineProps<Props>(), {
  size: 210,
  opacity: 0.24,
});
</script>

<style scoped>
.animate-ripple-circle {
  animation: ripple-effect var(--duration, 4s) linear infinite;
  border-width: v-bind('props.borderWidth + "px"');
  top: 20%;
  left: 20%;
  width: v-bind('props.size + "px"');
  height: v-bind('props.size + "px"');
  animation-delay: v-bind('props.animationDelay + "ms"');
  opacity: v-bind("props.opacity");
  transform: translate(-50%, -50%) scale(1);
  border-style: v-bind("props.borderStyle");
}

@keyframes ripple-effect {
  0% {
    transform: translate(-50%, -50%) scale(0.6);
  }
  99% {
    transform: translate(-50%, -50%) scale(1.4);
  }
  100% {
    transform: translate(-50%, -50%) scale(0.6);
    opacity: 0;
  }
}
</style>
