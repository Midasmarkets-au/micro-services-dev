<template>
  <el-dialog v-model="showDialog" :title="title" width="1200" align-center>
    <el-descriptions class="margin-top" :column="3" border>
      <el-descriptions-item :label="$t('fields.orderNumber')">{{
        data.id
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

    <el-descriptions
      class="mt-5"
      border
      :title="$t('title.userDetails')"
      direction="vertical"
    >
      <el-descriptions-item :label="$t('fields.name')">
        <div>{{ data.eventPartyNativeName }}</div>
      </el-descriptions-item>
      <el-descriptions-item :label="$t('fields.email')">
        <div>{{ data.eventPartyEmail }}</div>
      </el-descriptions-item>
    </el-descriptions>

    <el-descriptions
      class="mt-5"
      :title="$t('fields.shippingAddress')"
      :column="2"
      border
    >
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
        <div>{{ data.address?.country }}</div>
      </el-descriptions-item>
      <el-descriptions-item
        :label="$t('fields.secondaryContact')"
        v-if="data.content?.socialMediaType"
      >
        <div>
          {{ $t("fields." + data.content?.socialMediaType) }}:
          {{ data.content?.socialMediaAccount }}
        </div>
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
import { EventShopOrderStatusOptions } from "@/core/types/shop/ShopPointsTypes";
import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;
const showDialog = ref(false);
const isLoading = ref(false);
const title = ref("");
const data = ref(<any>[]);
const show = (item: any) => {
  showDialog.value = true;
  title.value = t("fields.orderDetail") + " " + item.id;
  fecthData(item);
};

const fecthData = async (item: any) => {
  isLoading.value = true;
  try {
    const res = await EventsServices.queryOrderDetail(item.id);
    data.value = res;
  } catch (e) {
    console.log(e);
  }
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
