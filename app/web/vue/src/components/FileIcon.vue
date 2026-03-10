<template>
  <i class="fa-regular fa-file-pdf" v-if="props.type == 'pdf'"></i>
  <i class="fa-solid fa-file-csv" v-else-if="props.type == 'csv'"></i>

  <i class="fa-regular fa-file-excel" v-else-if="props.type == 'excel'"></i>
  <i class="" v-else-if="props.type == 'json'">{}</i>
  <i class="fa-regular fa-file" v-else></i>
</template>
<script lang="ts" setup>
import { defineProps } from "vue";
const props = defineProps<{
  type: string;
}>();
</script>
