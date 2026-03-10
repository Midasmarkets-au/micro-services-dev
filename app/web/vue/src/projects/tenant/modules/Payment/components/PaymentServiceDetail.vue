<template>
  <SiderDetail
    :title="detailTitle"
    width="90%"
    :discard="close"
    direction="ltr"
    ref="paymentDetailRef"
  >
    <el-tabs type="border-card" v-model="activeTab">
      <el-tab-pane name="information">
        <template #label>
          <span class="custom-tabs-label">
            <span class="fs-4">{{ $t("action.detail") }}</span>
          </span>
        </template>
        <PaymentServiceInfo
          :key="paymentId"
          :payment-service-id="paymentId"
          :close-function="close"
          :sorted-services="sortedServices"
          @update="emits('update')"
      /></el-tab-pane>

      <el-tab-pane name="instruction">
        <template #label>
          <span class="custom-tabs-label">
            <span class="fs-4">{{ $t("title.instruction") }}</span>
          </span>
        </template>
        <PaymentServiceInstruction
          :key="paymentId"
          :payment-service-id="paymentId"
          :close-function="close"
      /></el-tab-pane>

      <el-tab-pane name="policy">
        <template #label>
          <span class="custom-tabs-label">
            <span class="fs-4">{{ $t("title.policy") }}</span>
          </span>
        </template>
        <PaymentServicePolicy
          :key="paymentId"
          :payment-service-id="paymentId"
          :close-function="close"
      /></el-tab-pane>
    </el-tabs>
  </SiderDetail>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SiderDetail from "@/components/SiderDetail2.vue";
import PaymentService from "../services/PaymentService";
import PaymentServiceInstruction from "../modal/PaymentServiceInstruction.vue";
import PaymentServicePolicy from "../modal/PaymentServicePolicy.vue";
import PaymentServiceInfo from "../modal/PaymentServiceInfo.vue";
import { provide } from "vue";

const paymentDetailRef = ref(null);
const detail = ref(Array<any>());
const paymentId = ref();
const isLoading = ref(true);
const activeTab = ref("information");
const detailTitle = ref("");
const sortedServices = ref(Array<any>());

const show = (data, _sortedServices) => {
  detail.value = data;
  detailTitle.value = data.name;
  paymentId.value = data.id;
  sortedServices.value = _sortedServices;

  paymentDetailRef.value?.show();
};

const emits = defineEmits<{
  (e: "update"): void;
}>();

const close = () => {
  paymentDetailRef.value?.hide();
};

provide("paymentDetailRef", paymentDetailRef);
defineExpose({
  show,
  close,
});
</script>
