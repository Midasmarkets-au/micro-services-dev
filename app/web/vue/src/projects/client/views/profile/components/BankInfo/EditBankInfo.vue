<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('title.editPaymentAccount')"
    width="800"
    align-center
    :before-close="reset"
  >
    <div v-if="step == 0 && isUncategorized" class="mb-6 mt-14">
      <el-radio-group v-model="type">
        <el-radio
          v-for="item in BankInfoNoUSDT"
          :key="item.value"
          :label="item.value"
          size="large"
          border
        >
          {{ item.label }}
        </el-radio>
      </el-radio-group>
    </div>
    <div v-if="step == 1">
      <component
        :is="formCollection[selectedForm]"
        :ref="setComponentRef"
        :key="type"
        :data="data"
        @submit="submit"
      ></component>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <div v-if="step == 0">
          <el-button @click="dialogRef = false">Cancel</el-button>

          <el-button type="primary" @click="nextStep()">Next</el-button>
        </div>
        <div v-else-if="step == 1">
          <el-button
            :disabled="isLoading"
            @click="step--"
            v-if="isUncategorized"
            >Previous</el-button
          >
          <el-button :loading="isLoading" type="primary" @click="validDate">
            Confirm
          </el-button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, computed, nextTick } from "vue";
import LocalBankForm from "../form/LocalBankForm.vue";
import WireForm from "../form/WireForm.vue";
import UsdtForm from "../form/UsdtForm.vue";
import { BankInfoNoUSDT, BankInfoTypes } from "@/core/types/BankInfo";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import i18n from "@/core/plugins/i18n";

const { t } = i18n.global;
const dialogRef = ref(false);
const isLoading = ref(false);
const isUncategorized = ref(false);
const data = ref<any>({});
const type = ref<any>(0);
const step = ref(1);
const emits = defineEmits(["submit"]);

const dynamicRef = ref(null);
const formCollection = {
  WireForm,
  LocalBankForm,
  UsdtForm,
};

const selectedForm = computed(() => {
  switch (type.value) {
    case BankInfoTypes.Wire:
      return "WireForm";
    case BankInfoTypes.Other:
      return "LocalBankForm";
    case BankInfoTypes.USDT:
      return "UsdtForm";
    default:
      return "Uncategorized";
  }
});

const setComponentRef = (instance) => {
  dynamicRef.value = instance;
};

const nextStep = async () => {
  step.value++;
  await nextTick();
  dynamicRef.value.wireForm = data.value.info;
};

const validDate = () => {
  dynamicRef.value?.returnFormData();
};

const setData = (item: any) => {
  data.value = item;
};

const show = async (item: any) => {
  // reset type to zero to reset form data or get new form data
  type.value = 0;
  await nextTick();
  if (!item.info.type) {
    isUncategorized.value = true;
    type.value = 0;
    step.value = 0;
  } else {
    type.value = item.info.type;
  }
  setData(item);

  dialogRef.value = true;
};

const submit = async () => {
  isLoading.value = true;

  if (isUncategorized.value) {
    data.value.info.type = type.value;
  }
  try {
    await GlobalService.updateUserPaymentInfo(data.value.id, {
      name: data.value.info.name,
      info: data.value.info,
    });
    MsgPrompt.success(t("tip.formSubmitSuccess")).then(() => {
      emits("submit");
      reset();
    });
  } catch (error) {
    MsgPrompt.error(error);
  }

  isLoading.value = false;
};

const reset = async () => {
  dialogRef.value = false;
  await nextTick();
  step.value = 1;
  isUncategorized.value = false;
};

defineExpose({ show });
</script>
