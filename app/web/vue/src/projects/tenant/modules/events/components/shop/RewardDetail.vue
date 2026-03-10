<template>
  <el-dialog
    v-model="showDialog"
    title="Reward Detail"
    width="800"
    align-center
  >
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="showDialog = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import EventsServices from "../../services/EventsServices";
import { EventShopOrderStatusTypes } from "@/core/types/shop/ShopPointsTypes";
const showDialog = ref(false);
const isLoading = ref(false);
const data = ref(<any>[]);
const show = (item: any) => {
  showDialog.value = true;
  fecthData(item);
};

const fecthData = async (item: any) => {
  isLoading.value = true;
  try {
    const res = await EventsServices.queryShopRewardById(item.id);
    data.value = res;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
<style scoped>
.title {
  font-size: 18px;
  font-weight: bold;
  margin-bottom: 10px;
  color: #50cd89;
}

.item-title {
  font-size: 15px;
  font-weight: 500;
  color: rgb(63, 61, 61);
}

.item-content {
  font-size: 15px !important;
  color: #606266 !important;
}
</style>
