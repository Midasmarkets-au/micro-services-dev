<template>
  <DropDownBox :title="title" :show="show">
    <div class="row">
      <template v-for="(value, key) in details" :key="key">
        <DetailInput
          :label="$t(`label.${key}`)"
          :value="value"
          :disabled="true"
          :size="2"
        />
      </template>
    </div>
    <div>{{ details }}</div>
  </DropDownBox>
</template>

<script setup lang="ts">
import DetailInput from "./DetailInput.vue";
import DropDownBox from "@/components/DropDownBox.vue";
withDefaults(
  defineProps<{
    details: any;
    title: string | number;
    show?: boolean;
  }>(),
  {
    show: false,
  }
);
</script>

<style scoped></style>
