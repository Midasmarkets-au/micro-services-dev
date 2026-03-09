<template>
  <el-dialog
    v-model="dialogRef"
    width="761"
    align-center
    class="rounded-3"
    style="max-height: 85vh; overflow: auto"
    :show-close="false"
    :close-on-click-modal="exitPermission"
    :close-on-press-escape="exitPermission"
  >
    <template #header>
      <div
        :class="isMobile ? 'size-lg' : 'size-lg'"
        class="flex justify-between"
      >
        <div>{{ currentData.title }}</div>
        <div
          class="w-6 h-6 btn btn-light btn-icon btn-sm"
          @click="dialogRef = false"
        >
          <SvgIcon name="close" path="arrows" class="w-5 h-5" />
        </div>
      </div>
    </template>
    <div
      v-if="isLoading"
      style="height: 50vh"
      class="d-flex justify-content-center align-items-center"
    >
      <scale-loader :color="'#ffc730'"></scale-loader>
    </div>
    <div
      v-else
      class="overflow-scroll"
      style="max-height: 60vh; min-height: 40vh"
    >
      <div v-if="!isMobile">
        <div v-if="imageUrl != ''">
          <img :src="imageUrl" alt="Event Image" class="w-100 rounded-2" />
        </div>
      </div>
      <div v-else>
        <div v-if="mobileImageUrl != ''">
          <img
            :src="mobileImageUrl"
            alt="Event Image"
            class="w-100 rounded-2"
          />
        </div>
      </div>
      <div
        class="w-100 mt-6"
        v-if="currentData.key !== 'post'"
        v-html="currentData.description"
      ></div>
    </div>
    <template #footer>
      <div class="d-flex justify-content-end border-top">
        <el-checkbox
          :label="$t('fields.dontShowAgain')"
          size="large"
          v-model="isChecked"
          style="color: #909399"
          :disabled="isSubmitting"
          @change="noShow()"
        />
      </div>
      <div class="d-flex justify-content-center align-items-center gap-5">
        <el-button
          @click="fetchContent(currentIndex - 1)"
          :disabled="isLoading || currentIndex == 0"
          type="info"
          >{{ $t("fields.previous") }}</el-button
        >
        <div>{{ currentIndex + 1 }}/{{ dataLength }}</div>
        <el-button
          v-if="currentIndex == dataLength - 1"
          type="warning"
          @click="dialogRef = false"
          :disabled="isLoading"
        >
          {{ $t("action.close") }}
        </el-button>
        <el-button
          v-else
          type="warning"
          @click="fetchContent(currentIndex + 1)"
          :disabled="isLoading || currentIndex == dataLength - 1"
        >
          {{ $t("fields.next") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, onMounted, computed } from "vue";
import ClientGlobalService from "../../services/ClientGlobalService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useStore } from "@/store";
import { ElMessage } from "element-plus";
import i18n from "@/core/plugins/i18n";
import { Actions } from "@/store/enums/StoreEnums";

const t = i18n.global.t;
const store = useStore();
const config = ref<any>(store.state.AuthModule.user.configurations);
const isLoading = ref(false);
const isSubmitting = ref(false);
const dialogRef = ref(false);

const currentIndex = ref(0);
const dataLength = computed(() => data.value.length);
const data = ref<any>([]);
const currentKey = ref<string>("");
const excludedKeys = ref<string[]>(["EventShop"]);
const currentData = ref<any>({});
const isChecked = ref(false);
const checkedList = ref<any>([]);
const imageUrl = ref<string>("");
const mobileImageUrl = ref<string>("");
const exitPermission = ref(false);
const criteria = ref<any>({
  page: 1,
  size: 20,
  status: 1,
  sortField: "createdOn",
  sortFlag: false,
});

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await ClientGlobalService.getEventList(criteria.value);
    data.value = res.data;
    data.value = data.value.filter(
      (item: any) => !excludedKeys.value.includes(item.key)
    );
  } catch (e) {
    console.error(e);
  }
  isLoading.value = false;
};

const fetchContent = async (index: number) => {
  isLoading.value = true;

  try {
    currentIndex.value = index;
    if (currentIndex.value == dataLength.value - 1) {
      exitPermission.value = true;
    } else {
      exitPermission.value = false;
    }
    currentKey.value = data.value[index].key;
    if (checkedList.value.includes(currentKey.value)) {
      isChecked.value = true;
    } else {
      isChecked.value = false;
    }
    const res = await ClientGlobalService.getEventByKey(currentKey.value);
    currentData.value = res;

    var desktopImg = "";
    if (currentData.value.images["desktop"]) {
      desktopImg = await ClientGlobalService.getImagesWithGuid(
        currentData.value.images.desktop
      );
    }
    imageUrl.value = desktopImg;

    var mobileImg = "";
    if (currentData.value.images["mobile"]) {
      mobileImg = await ClientGlobalService.getImagesWithGuid(
        currentData.value.images.mobile
      );
    }
    mobileImageUrl.value = mobileImg;
  } catch (e) {
    console.error(e);
  }
  isLoading.value = false;
};

const noShow = async () => {
  if (!isChecked.value) {
    isChecked.value = true;
    return;
  }
  isSubmitting.value = true;
  try {
    await ClientGlobalService.InsertNoShowEventId(currentData.value.key);
    checkedList.value.push(currentData.value.key);
    const me = await ClientGlobalService.getMe();
    store.dispatch(Actions.UPDATE_USER, me);
    ElMessage.success(t("tip.updateSuccess"));
  } catch (e) {
    console.error(e);
    ElMessage.error(t("tip.updateFailed"));
  } finally {
    isSubmitting.value = false;
  }
};

const noShowCheck = () => {
  var hasNoShow = config.value.find(
    (item: any) => item.key == "UserLastCheckedEventId"
  );
  if (!hasNoShow) {
    return;
  } else {
    criteria.value.idLargerThan = hasNoShow.value["value"];
  }
};

const showData = async () => {
  noShowCheck();
  // await ClientGlobalService.InsertNoShowEventId(321);
  await fecthData();
  if (data.value.length > 0) {
    dialogRef.value = true;
    currentKey.value = data.value[0].key;
    await fetchContent(0);
  }
};

defineExpose({
  dialogRef,
  showData,
});

// onMounted(async () => {
//   noShowCheck();
//   // await ClientGlobalService.InsertNoShowEventId(321);
//   await fecthData();
//   if (data.value.length > 0) {
//     dialogRef.value = true;
//     currentKey.value = data.value[0].key;
//     await fetchContent(0);
//   }
// });
</script>
<style scoped>
.el-dialog:deep .el-dialog__header {
  border-bottom: 1px solid #ecede4 !important;
  padding-bottom: 0px !important;
}
.border-top {
  border-top: 1px solid #d4d4d2 !important;
}
.border-bottom {
  border-bottom: 1px solid #d4d4d2 !important;
}
.size-llg {
  font-size: 28px;
}
.size-lg {
  font-size: 20px;
}
</style>
