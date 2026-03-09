<template>
  <div class="my-2">
    <h1>{{ $t("tip.verifyDocuments") }}</h1>
  </div>

  <div class="d-flex flex-wrap">
    <UserImageDoc
      v-for="(item, index) in data"
      :key="index"
      class="me-3"
      :title="item.context"
      :media="item"
      :verification-details="verificationDetails"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import UserImageDoc from "./UserImageDoc.vue";

const props = defineProps<{
  verificationDetails: any;
  verifyOperation?: boolean;
  height?: string;
}>();

const isLoading = ref(false);
const data = ref<any>([]);

const fetchData = async () => {
  isLoading.value = true;
  data.value = props.verificationDetails.document;
  isLoading.value = false;
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped></style>
