<template>
  <div class="d-flex">
    <div>{{ props.id }}}</div>
    <div>{{ props.partyId }}}</div>
    <div>{{ props.createdOn }}}</div>
    <div>{{ props.updatedOn }}}</div>
    <div>{{ props.note }}}</div>
  </div>
</template>

<script lang="ts" setup>
const props = defineProps({
  id: { type: Number, required: true },
  partyId: { type: Number, required: true },
  createdOn: { type: String, required: true },
  updatedOn: { type: String, required: true },
  note: { type: String, required: true },
});
</script>
