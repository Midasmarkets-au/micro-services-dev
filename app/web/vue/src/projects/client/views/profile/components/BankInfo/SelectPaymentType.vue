<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('title.addPaymentAccount')"
    width="800"
    align-center
    :before-close="reset"
  >
    <el-steps style="max-width: 700px" :active="step" finish-status="success">
      <el-step title="Select Account Type" />
      <el-step title="Add Detail" />
      <el-step title="Confirm" />
    </el-steps>

    <div v-if="step == 0" class="mb-6 mt-14">
      <el-radio-group v-model="type">
        <el-radio
          v-for="item in bankInfos"
          :key="item.value"
          :label="item.value"
          size="large"
          border
        >
          {{ item.label }}
        </el-radio>
      </el-radio-group>
    </div>
    <div v-else-if="step == 1">
      <h3 class="mt-4 mb-4">
        {{ typeTitle }}
      </h3>
      <keep-alive>
        <component
          :is="formCollection[selectedForm]"
          :ref="setComponentRef"
          @submit="submit"
        ></component>
      </keep-alive>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <div v-if="step == 0">
          <el-button @click="dialogRef = false">Cancel</el-button>

          <el-button type="primary" @click="step++">Next</el-button>
        </div>
        <div v-else-if="step == 1">
          <el-button :disabled="isLoading" @click="step--">Previous</el-button>
          <el-button :loading="isLoading" type="primary" @click="validDate">
            Confirm
          </el-button>
        </div>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, computed } from "vue";
import { BankInfoOptions, BankInfoTypes } from "@/core/types/BankInfo";
import LocalBankForm from "../form/LocalBankForm.vue";
import WireForm from "../form/WireForm.vue";
import UsdtForm from "../form/UsdtForm.vue";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const isLoading = ref(false);
const dialogRef = ref(false);
const step = ref(0);
const type = ref(BankInfoTypes.Wire);
const bankInfos = ref(BankInfoOptions.value);
const selectedForm = computed(() => {
  switch (type.value) {
    case BankInfoTypes.Wire:
      return "WireForm";
    case BankInfoTypes.Other:
      return "LocalBankForm";
    case BankInfoTypes.USDT:
      return "UsdtForm";
    default:
      return "WireForm";
  }
});
const dynamicRef = ref(null);
const formCollection = {
  WireForm,
  LocalBankForm,
  UsdtForm,
};
const setComponentRef = (instance) => {
  dynamicRef.value = instance;
};
const typeTitle = computed(() => {
  return BankInfoOptions.value.find((item) => item.value === type.value)?.label;
});

const show = (hasUSDT?: boolean) => {
  checkUSDT(hasUSDT);
  dialogRef.value = true;
};

const checkUSDT = (hasUSDT: boolean) => {
  if (hasUSDT) {
    bankInfos.value = BankInfoOptions.value.filter(
      (item) => item.value !== BankInfoTypes.USDT
    );
  } else {
    bankInfos.value = BankInfoOptions.value;
  }
};

const emits = defineEmits(["submit"]);

const validDate = () => {
  dynamicRef.value?.returnFormData();
};

const reset = () => {
  step.value = 0;
  type.value = BankInfoTypes.Wire;
  dialogRef.value = false;
};

const submit = async (_form) => {
  isLoading.value = true;
  _form.type = type.value;
  try {
    await GlobalService.createUserPaymentInfo({
      name: _form.name,
      info: _form,
    }).then(() => {
      MsgPrompt.success(t("status.success"));
      emits("submit");
    });
  } catch (e) {
    console.log(e);
    MsgPrompt.error(t("status.error"));
  }
  step.value += 2;
  reset();
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
