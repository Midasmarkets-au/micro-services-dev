<template>
  <el-dialog
    v-model="dialogRef"
    :title="title"
    align-center
    class="rounded-3 max-w-800px"
    :class="[isMobile ? 'w-100' : '']"
    :before-close="close"
  >
    <div v-if="!isMobile">
      <div v-for="(item, index) in data" :key="index">
        <div
          class="item-border mb-4 position-relative"
          :class="
            selectedAddress.hashId === item.hashId ? 'selected-border' : ''
          "
          @click="chooseAddress(item)"
        >
          <div class="d-flex gap-4 mb-1">
            <div class="text-black">{{ item.name }}</div>
            <div class="text-gray-500">{{ item.ccc }} {{ item.phone }}</div>
            <div v-if="selectedAddress.hashId === item.hashId">
              <el-tag type="danger" effect="dark" size="small">{{
                $t("tip.selected")
              }}</el-tag>
            </div>
          </div>
          <div v-if="item.content.socialMediaType">
            <div class="text-gray-500">
              {{ $t("fields." + item.content?.socialMediaType) }}:
              {{ item.content?.socialMediaAccount }}
            </div>
          </div>
          <div class="d-flex gap-4 text-gray-500">
            <div>{{ item.content?.address }}</div>
            <div>{{ item.content?.city }}</div>
            <div>{{ item.content?.state }}</div>
            <div>{{ item.content?.postalCode }}</div>
            <div>{{ regionCodes[item.country]?.name }}</div>
          </div>
          <div class="edit-icon cursor-pointer" @click="editAddress(item)">
            <el-icon size="16"><EditPen color="#000" /></el-icon>
          </div>
        </div>
      </div>
    </div>
    <div v-else>
      <div v-for="(item, index) in data" :key="index">
        <div
          class="item-border mb-4 position-relative"
          :class="
            selectedAddress.hashId === item.hashId ? 'selected-border' : ''
          "
          @click="chooseAddress(item)"
        >
          <div
            class="d-flex gap-4 mb-2 align-items-center justify-content-between"
          >
            <div class="text-black">{{ item.name }}</div>

            <div v-if="selectedAddress.hashId === item.hashId">
              <el-tag type="danger" effect="dark" size="small">{{
                $t("tip.selected")
              }}</el-tag>
            </div>
            <div class="cursor-pointer" @click="editAddress(item)">
              <el-icon size="20" class="mt-2 ms-4"
                ><EditPen color="#000"
              /></el-icon>
            </div>
          </div>
          <div class="row row-cols-2 text-gray-500">
            <div class="text-gray-500">{{ item.ccc }} {{ item.phone }}</div>
            <div>{{ item.content?.address }}</div>
            <div>{{ item.content?.city }}</div>
            <div>{{ item.content?.state }}</div>
            <div>{{ item.content?.postalCode }}</div>
            <div>{{ regionCodes[item.country]?.name }}</div>
          </div>
        </div>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button @click="addAddress()">
          {{ $t("title.addAddress") }}
        </el-button>
      </div>
    </template>
    <AddAddress ref="addAddressRef" @submit="fetchData" />
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, reactive } from "vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
import AddAddress from "@/projects/client/views/profile/components/AddAddress.vue";
import i18n from "@/core/plugins/i18n";
import { EditPen } from "@element-plus/icons-vue";
import { getRegionCodes } from "@/core/data/phonesData";
const regionCodes = ref(getRegionCodes());
const { t } = i18n.global;
const isLoading = ref(false);
const dialogRef = ref(false);
const title = ref(t("title.address"));
const addAddressRef = ref<InstanceType<typeof AddAddress>>();
const data = ref(<any>[]);
const emit = defineEmits<{
  (e: "submit"): void;
  (e: "chooseAddress", data: any): void;
}>();
const selectedAddress = ref({});
const show = (data?: any) => {
  dialogRef.value = true;
  selectedAddress.value = data;
  fetchData();
};
const addAddress = () => {
  addAddressRef.value.show();
};
const editAddress = (item: any) => {
  event.stopPropagation();
  addAddressRef.value.show(item);
};
const chooseAddress = (item: any) => {
  emit("chooseAddress", item);
  close();
};
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await ClientGlobalService.queryAddressList();
    data.value = res.data;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};
const close = () => {
  dialogRef.value = false;
};

defineExpose({
  show,
});
</script>
<style lang="scss">
.item-border {
  border: 1px solid #e4e6ef;
  padding: 10px;
  border-radius: 5px;
  cursor: pointer;
}
.item-border:hover {
  border: 1px solid #ffce00;
}

.edit-icon {
  position: absolute;
  top: 50%;
  right: 30px;
  transform: translateY(-50%);
}
@media (max-width: 768px) {
  .edit-icon {
    top: 30%;
    right: 30px;
    transform: translateY(-30%);
  }
}
</style>
