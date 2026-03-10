<template>
  <div class="absolute inset-0">
    <RippleLinear
      v-for="index in numberOfCircles"
      :key="index"
      :opacity="baseCircleOpacity - index * circleOpacityDowngradeRatio"
      :size="baseCircleSize + index * spaceBetweenCircle"
      :animation-delay="index * waveSpeed"
      :border-style="index === numberOfCircles - 1 ? 'dashed' : 'solid'"
      :class="circleClass"
      :borderWidth="borderWidth"
    >
    </RippleLinear>
  </div>
</template>

<script setup lang="ts">
import RippleLinear from "./RippleLinear.vue";
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
  baseCircleSize: 200,
  baseCircleOpacity: 1,
  circleOpacityDowngradeRatio: 0.15,
  waveSpeed: 500,
  spaceBetweenCircle: 300,
  numberOfCircles: 6,
  borderWidth: 2,
});
</script>
