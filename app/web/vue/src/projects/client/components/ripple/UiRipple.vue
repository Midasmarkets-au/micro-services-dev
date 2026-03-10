<template>
  <div class="absolute inset-0">
    <RippleCircle
      v-for="index in numberOfCircles"
      :key="index"
      :opacity="baseCircleOpacity - index * circleOpacityDowngradeRatio"
      :size="baseCircleSize + index * spaceBetweenCircle"
      :animation-delay="index * waveSpeed"
      :border-style="index === numberOfCircles - 1 ? 'dashed' : 'solid'"
      :class="circleClass"
      :borderWidth="borderWidth"
    >
    </RippleCircle>
  </div>
</template>

<script setup lang="ts">
import RippleCircle from "./RippleCircle.vue";
interface Props {
  baseCircleSize?: number;
  baseCircleOpacity?: number;
  spaceBetweenCircle?: number;
  circleOpacityDowngradeRatio?: number;
  circleClass?: string;
  waveSpeed?: number;
  numberOfCircles?: number;
  borderWidth?: number;
}

withDefaults(defineProps<Props>(), {
  baseCircleSize: 400,
  baseCircleOpacity: 0.9,
  circleOpacityDowngradeRatio: 0.21,
  waveSpeed: 120,
  spaceBetweenCircle: 500,
  numberOfCircles: 7,
  borderWidth: 3,
});
</script>
