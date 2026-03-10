<template>
  <!--begin::Role Show-->
  <el-drawer
    v-model="drawer"
    :title="title"
    :size="width"
    :width="width"
    :append-to-body="true"
    :direction="direction"
    :before-close="hide"
    destroy-on-close
  >
    <slot></slot>
  </el-drawer>
</template>

<script setup lang="ts">
import { ref, nextTick } from "vue";

const props = defineProps({
  title: { type: String, required: true },
  width: String,
  defaultTab: String,
});

const drawer = ref(false);
const direction = ref("rtl");
const show = () => {
  // console.log("has: ", DrawerComponent.hasInstace(props.elId));
  drawer.value = true;
};

const emits = defineEmits(["close"]);

const hide = async () => {
  emits("close");
  await nextTick();
  drawer.value = false;
};
defineExpose({
  hide,
  show,
});
</script>
