<template>
  <div>
    <CenterMenu activeMenuItem="address" />
  </div>
  <div class="card round-bl-br mb-2" v-if="$cans(['EventShop'])">
    <div class="card-header">
      <div class="card-title">{{ $t("title.address") }}</div>
      <div class="card-toolbar">
        <!-- <el-button @click="addAddress()" :icon="Plus">
          {{ $t("title.addNewAddress") }}
        </el-button> -->
        <button
          @click="addAddress()"
          class="btn btn-sm btn-light-primary btn-bordered"
        >
          <div class="d-flex align-items-center">
            <i class="fa-regular fa-plus"></i>
            <span>{{ $t("title.addNewAddress") }}</span>
          </div>
        </button>
      </div>
    </div>
  </div>
  <div class="card round-tl-tr">
    <div class="card-body">
      <div v-if="isLoading">
        <LoadingRing />
      </div>
      <div
        v-else-if="!isLoading && data.length == 0"
        class="d-flex justify-content-center"
      >
        <NoDataBox />
      </div>
      <div class="row" v-else>
        <div v-for="item in data" :key="item.id" class="col-12 col-md-3">
          <el-card class="box-card mb-5 rounded-3">
            <div class="address-item-header">
              <div class="d-flex align-items-center">
                <el-icon size="20" class="me-2"><Location /></el-icon>
                <div class="address-item-title">{{ item.name }}</div>
              </div>
              <div>
                <el-button :icon="Edit" circle @click="editAddress(item)" />
              </div>
            </div>
            <el-descriptions :column="1" class="my-5">
              <el-descriptions-item :label="$t('fields.address')">{{
                item.content?.address
              }}</el-descriptions-item>
              <el-descriptions-item :label="$t('fields.city')">{{
                item.content?.city
              }}</el-descriptions-item>
              <el-descriptions-item :label="$t('fields.state')">{{
                item.content?.state
              }}</el-descriptions-item>
              <el-descriptions-item :label="$t('fields.code')">{{
                item.content?.postalCode
              }}</el-descriptions-item>
              <el-descriptions-item :label="$t('fields.country')">{{
                regionCodes[item.country]?.name
              }}</el-descriptions-item>
              <el-descriptions-item :label="$t('fields.phone')"
                >{{ item.ccc }} {{ item.phone }}</el-descriptions-item
              >
            </el-descriptions>
          </el-card>
        </div>
      </div>
    </div>
  </div>
  <AddAddress ref="modalRef" @submit="fetchData" />
</template>
<script setup lang="ts">
import { Plus } from "@element-plus/icons-vue";
import { ref, onMounted, inject } from "vue";
import AddAddress from "./components/AddAddress.vue";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import { Location, Edit } from "@element-plus/icons-vue";
import ClientGlobalInjectionKeys from "@/core/types/ClientGlobalInjectionKeys";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { getRegionCodes } from "@/core/data/phonesData";
import CenterMenu from "./components/CenterMenu.vue";
const openConfirmBoxModel = inject(
  ClientGlobalInjectionKeys.OPEN_CONFIRM_MODAL
);
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof AddAddress>>();
const selectedId = ref(-1);
const data = ref(<any>[]);
const regionCodes = ref(getRegionCodes());
const openConfirmBoxPanel = (_id: number) => {
  selectedId.value = _id;
  openConfirmBoxModel?.(deleteAddress);
};

const deleteAddress = async () => {
  isLoading.value = true;
  try {
    await ClientGlobalService.deleteAddress(selectedId.value);
    MsgPrompt.success("Delete successful");
    fetchData();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
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

const editAddress = (item: any) => {
  modalRef.value.show(item);
};

const addAddress = () => {
  modalRef.value.show();
};
onMounted(() => {
  fetchData();
});
</script>
<style scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
.address-item-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 10px;
  border-bottom: 1px solid #ebeef5;
}
.address-item-title {
  font-size: 16px;
  font-weight: 500;
}
.address-item-content {
  padding: 10px 0;
}
.address-item-text {
  font-size: 14px;
  color: #606266;
  font-weight: 600;
}
.item-title {
  color: #6f6f76;
  font-size: 14px;
  font-weight: 600;
}
:deep
  .el-descriptions__body
  .el-descriptions__table:not(.is-bordered)
  .el-descriptions__cell {
  padding-bottom: 0;
}
</style>
