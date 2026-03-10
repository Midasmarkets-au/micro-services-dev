<template>
  <el-dialog
    title="Change Fund Type"
    v-model="dialogVisible"
    width="30%"
    align-center
  >
    <el-select v-model="fundType">
      <el-option
        v-for="item in ConfigFundTypeSelections"
        :key="item.value"
        :label="item.label"
        :value="item.value"
      />
    </el-select>
    <template #footer>
      <el-button @click="dialogVisible = false">{{
        $t("action.cancel")
      }}</el-button>
      <el-button type="primary" @click="handleSubmit">{{
        $t("action.save")
      }}</el-button>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref } from "vue";
import { ConfigFundTypeSelections } from "@/core/types/FundTypes";
import PaymentService from "../services/PaymentService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";
const { t } = i18n.global;
const isLoading = ref(false);
const dialogVisible = ref(false);
const fundType = ref<any>(null);
const walletId = ref(0);
const show = (data: any) => {
  dialogVisible.value = true;
  fundType.value = data.fundType;
  walletId.value = data.id;
};
const emits = defineEmits<{
  (e: "eventSubmit"): void;
}>();
const handleSubmit = async () => {
  isLoading.value = true;
  try {
    await PaymentService.changeWalletFundType(walletId.value, fundType.value);
    MsgPrompt.success(t("status.success"));
    emits("eventSubmit");
  } catch (e) {
    MsgPrompt.error(t("status.error"));
    console.log(e);
  }
  isLoading.value = false;
  dialogVisible.value = false;
};

defineExpose({
  show,
});
</script>
