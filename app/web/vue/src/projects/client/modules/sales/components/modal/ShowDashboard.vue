<template>
  <el-dialog v-model="dialogRef" :title="title" width="1200" align-center>
    <SalesDashboard :childUid="childUid" v-if="dialogRef" />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import SalesDashboard from "../SalesDashboard.vue";

const dialogRef = ref(false);
const childUid = ref(0);
const title = ref("");
const show = (item: any) => {
  dialogRef.value = true;
  title.value = getUserName(item);
  childUid.value = item.uid;
};

const getUserName = (item: any) => {
  if (
    !item.user?.nativeName ||
    item.user?.nativeName === "" ||
    item.user?.nativeName === " "
  ) {
    if (
      !item.user?.displayName ||
      item.user?.displayName === "" ||
      item.user?.displayName === " "
    ) {
      if (
        !item.user?.firstName ||
        !item.user?.lastName ||
        item.user?.firstName === "" ||
        item.user?.lastName === "" ||
        item.user?.firstName === " " ||
        item.user?.lastName === " "
      ) {
        return "No Name";
      } else {
        return item.user?.firstName + " " + item.user?.lastName;
      }
    } else {
      return item.user?.displayName;
    }
  } else {
    return item.user?.nativeName;
  }
};
defineExpose({
  show,
});
</script>
