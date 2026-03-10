<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <h3>{{ eventDetail.title }}</h3>
        <span> - </span>
        <h3>{{ $t("title.termsAndConditions") }}</h3>
      </div>
      <div class="card-toolbar">
        <el-button
          type="warning"
          color="#ffce00"
          size="large"
          :icon="Back"
          circle
          @click="back"
        />
      </div>
    </div>
    <div class="card-body">
      <div class="w-100 notice-content" v-html="eventDetail.term"></div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import ShopService from "../../services/ShopService";
import { Back } from "@element-plus/icons-vue";
import { useRouter } from "vue-router";
const isLoading = ref(true);
const eventDetail = ref(<any>[]);
const router = useRouter();
async function fetchData() {
  isLoading.value = true;
  try {
    const res = await ShopService.queryEventByKey("EventShop");
    eventDetail.value = res;
  } catch (error) {
    console.error(error);
  }
  isLoading.value = false;
}
const back = () => {
  router.push("/eventshop");
};
onMounted(() => {
  fetchData();
});
</script>

<style lang="scss"></style>
