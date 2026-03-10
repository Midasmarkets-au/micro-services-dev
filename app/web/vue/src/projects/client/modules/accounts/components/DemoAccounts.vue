<template>
  <template v-for="(item, index) in items" :key="index">
    <div
      :class="{ 'mb-8': items.length - 1 !== index }"
      class="d-flex align-items-center"
    >
      <div class="flex-grow-1">
        <span class="text-gray-800 text-hover-primary fw-bold fs-6">{{
          item.accountNumber
        }}</span>

        <span class="text-muted fw-semobold d-block">{{ item.createdOn }}</span>
      </div>
      <span class="badge fs-8 fw-bold" :class="`badge-light`">{{
        $t("fields.platforms." + item.serviceId)
      }}</span>
    </div>
    <!--end:Item-->
  </template>
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";

const items = ref<any>();

onMounted(async () => {
  // console.log("tbd");
});
</script>
