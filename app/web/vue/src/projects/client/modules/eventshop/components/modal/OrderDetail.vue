<template>
  <el-dialog
    v-model="dialogRef"
    :title="title"
    width="1200"
    align-center
    class="rounded-3"
    :before-close="reset"
  >
    <el-descriptions class="margin-top" :column="isMobile ? 1 : 3" border>
      <el-descriptions-item :label="$t('fields.orderNumber')">{{
        data.hashId
      }}</el-descriptions-item>

      <el-descriptions-item :label="$t('fields.itemName')">{{
        data.eventShopItemName
      }}</el-descriptions-item>

      <el-descriptions-item :label="$t('fields.status')">{{
        getLabel(data.status)
      }}</el-descriptions-item>

      <el-descriptions-item :label="$t('fields.quantity')">{{
        data.quantity
      }}</el-descriptions-item>

      <el-descriptions-item :label="$t('fields.points')"
        ><ShopPoints :points="data.totalPoint"
      /></el-descriptions-item>

      <el-descriptions-item :label="$t('fields.date')">
        <TimeShow :date-iso-string="data.createdOn" type="customCSS"
      /></el-descriptions-item>
    </el-descriptions>

    <div class="mt-5 mb-4 d-flex align-items-center gap-3">
      <label>{{ $t("fields.shippingAddress") }}</label>
      <el-icon
        v-if="data.status == EventShopOrderStatusTypes.Pending"
        :size="20"
        class="me-3 cursor-pointer"
        :disabled="isLoading"
        @click="showAddressList(data.address)"
        ><Edit
      /></el-icon>
    </div>

    <el-descriptions :column="isMobile ? 1 : 2" border>
      <el-descriptions-item :label="$t('fields.name')">
        <div>{{ data.address?.name }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.phone')">
        <div>{{ data.address?.ccc }} {{ data.address?.phone }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.address')">
        <div>{{ data.address?.content.address }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.city')">
        <div>{{ data.address?.content.city }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.state')">
        <div>{{ data.address?.content.state }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.country')">
        <div>{{ regionCodes[data.address?.country]?.name }}</div>
      </el-descriptions-item>
    </el-descriptions>

    <el-descriptions
      class="mt-5"
      border
      direction="vertical"
      v-if="data.comment != ''"
    >
      <el-descriptions-item :label="$t('fields.comment')">
        <div>{{ data.comment }}</div>
      </el-descriptions-item>
    </el-descriptions>
    <el-descriptions
      class="my-5"
      border
      :column="2"
      :title="$t('fields.trackingInformation')"
    >
      <el-descriptions-item
        v-if="data.shipping?.trackingNumber"
        :label="$t('fields.trackingNumber')"
      >
        <div>{{ data.shipping?.trackingNumber }}</div>
      </el-descriptions-item>

      <el-descriptions-item v-else :label="$t('fields.trackingNumber')">
        <div>{{ $t("fields.waitingForShippment") }}</div>
      </el-descriptions-item>
    </el-descriptions>

    <template #footer>
      <div class="dialog-footer d-flex" style="justify-content: right">
        <el-button
          type="primary"
          v-if="canUpdateAddress"
          @click="updateAddress"
          :loading="isLoading"
        >
          {{ $t("action.updateAddress") }}
        </el-button>
        <el-popconfirm
          :title="$t('tip.confirmPrompt')"
          width="250px"
          @confirm="confirmDeliver"
          ><template #reference>
            <el-button
              v-if="data.status == EventShopOrderStatusTypes.Shipped"
              color="#ffce00"
              :loading="isLoading"
              >{{ $t("action.confirmDelivered") }}</el-button
            >
          </template>
        </el-popconfirm>
        <el-button
          class="btn btn-light btn-sm btn-radius btn-bordered d-flex justify-content-end"
          @click="reset()"
          :disabled="isLoading"
          >{{ $t("action.close") }}</el-button
        >
      </div>
    </template>
    <AddressList ref="addressListRef" @choose-address="selectAddress" />
  </el-dialog>
</template>

<script setup lang="ts">
import { ref } from "vue";
import ShopService from "../../services/ShopService";
import {
  EventShopOrderStatusOptions,
  EventShopOrderStatusTypes,
} from "@/core/types/shop/ShopPointsTypes";
import i18n from "@/core/plugins/i18n";
import { getRegionCodes } from "@/core/data/phonesData";
import { isMobile } from "@/core/config/WindowConfig";
import { Edit } from "@element-plus/icons-vue";
import AddressList from "./AddressList.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const regionCodes = ref(getRegionCodes());
const { t } = i18n.global;
const dialogRef = ref(false);
const isLoading = ref(false);
const title = ref("");
const data = ref(<any>[]);
const addressListRef = ref<InstanceType<typeof AddressList>>();
const canUpdateAddress = ref(false);

const emits = defineEmits(["fetch-data"]);

const showAddressList = (data: any) => {
  addressListRef.value.show(data);
};

const selectAddress = (item: any) => {
  data.value.address = item;
  canUpdateAddress.value = true;
};

const confirmDeliver = async () => {
  isLoading.value = true;
  try {
    await ShopService.confirmDelivery(data.value.hashId).then(() => {
      MsgPrompt.success(t("status.success"));
      emits("fetch-data");
    });
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  } finally {
    dialogRef.value = false;
    isLoading.value = false;
  }
};

const updateAddress = async () => {
  isLoading.value = true;
  try {
    await ShopService.updateOrderAddress(
      data.value.hashId,
      data.value.address.hashId
    ).then(() => {
      MsgPrompt.success(t("status.success"));
    });
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  } finally {
    reset();
  }
};

const show = (_data: any) => {
  dialogRef.value = true;
  title.value = t("fields.orderDetail") + " " + _data.hashId;
  fetchData(_data.hashId);
};

const fetchData = async (hashId: string) => {
  isLoading.value = true;
  try {
    const res = await ShopService.getOrderDetail(hashId);
    data.value = res;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  data.value = {};
  canUpdateAddress.value = false;
  dialogRef.value = false;
  isLoading.value = false;
};

const getLabel = (value: number) => {
  const option = EventShopOrderStatusOptions.value.find(
    (option) => option.value === value
  );
  return option ? option.label : "";
};
defineExpose({
  show,
});
</script>

<style lang="scss" scoped>
label {
  color: #303133;
  font-size: 16px;
  font-weight: 700;
}
</style>
