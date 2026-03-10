<template>
  {{ moment(props.datetime).fromNow() }}
</template>
<script lang="ts" setup>
import moment from "moment";

const props = defineProps<{
  datetime: string;
}>();
</script>
