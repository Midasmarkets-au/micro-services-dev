<template>
  <inline-svg
    v-if="props.imgType == 'svg'"
    :src="svgPath"
    :class="mergedClass"
    v-bind="$attrs"
  />
  <img
    v-if="props.imgType == 'png'"
    :src="svgPath"
    v-bind="$attrs"
    :class="mergedClass"
  />
</template>

<script setup>
import { computed } from "vue";
// 正确方式：只调用一次 defineProps，并结构出值
const defaultClass = "cursor-pointer";
const mergedClass = computed(() => {
  return [defaultClass, props.customClass].filter(Boolean).join(" ");
});
const props = defineProps({
  name: {
    type: String,
    required: true,
  },
  customClass: {
    type: String,
    default: "",
  },
  path: {
    type: String,
    required: true,
    default: "login",
  },
  imgType: {
    type: String,
    required: true,
    default: "svg",
  },
});
// 拼接路径（假设 SVG 存放在 public/images/icons/login/ 下）
const svgPath = computed(
  () => `/images/icons/${props.path}/${props.name}.${props.imgType}`
);
</script>
