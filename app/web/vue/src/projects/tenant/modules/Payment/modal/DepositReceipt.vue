<template>
  <div>
    <SimpleForm
      ref="modalForm"
      :title="$t('title.depositReceipt')"
      :is-loading="isLoading"
      disable-footer
    >
      <div class="w-100 d-flex justify-content-center align-items-center">
        <AuthImage :image-guid="currentGuid" size="600px" side="tenant" />
      </div>
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";
import PaymentService from "../services/PaymentService";
import { PaymentInfo, PaymentStatusTypes } from "@/core/types/PaymentTypes";
import TimeShow from "@/components/TimeShow.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import AuthImage from "@/components/AuthImage.vue";

const isLoading = ref(true);
const modalForm = ref<InstanceType<typeof SimpleForm>>();
const currentGuid = ref("-1");

defineExpose({
  async show(guid: Array) {
    modalForm.value?.show();
    currentGuid.value = guid[guid.length - 1];
  },
  hide() {
    modalForm.value?.hide();
  },
});
</script>

<style scoped></style>
