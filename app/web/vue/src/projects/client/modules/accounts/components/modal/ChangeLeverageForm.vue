<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.changeLeverage')"
    :is-loading="isSubmitting"
    :submit="submit"
    :disable-submit="newLeverage === leverage"
  >
    <h2 class="text-gray-600 fw-semobold fs-2 p-0 mb-6">
      {{
        $t("tip.currentLeverageFor") +
        accountNumber +
        $t("wholesale.yes") +
        leverage +
        ":1"
      }}
    </h2>

    <label class="required fs-6 fw-semobold mb-2">
      {{ $t("tip.newLeverage") }}
    </label>
    <el-form-item>
      <el-select
        v-model="newLeverage"
        name="leverage"
        type="text"
        placeholder="Select Leverage"
      >
        <el-option
          v-for="({ label, value }, index) in leverageSelection"
          :label="label"
          :key="index"
          :value="value"
        />
      </el-select>
    </el-form-item>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import AccountService from "../../services/AccountService";
import SimpleForm from "@/components/SimpleForm.vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { useI18n } from "vue-i18n";

const emits = defineEmits<{
  (e: "submit"): void;
}>();
const accountNumber = ref(0);
const uid = ref(0);
const leverage = ref<number>(-1);
const newLeverage = ref<number>(-1);
const isSubmitting = ref(false);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const leverageSelection = ref<any>([]);
const { t } = useI18n();

const show = async (_tradeAccount: any) => {
  accountNumber.value = _tradeAccount.accountNumber;
  leverage.value = _tradeAccount.leverage;
  newLeverage.value = _tradeAccount.leverage;
  uid.value = _tradeAccount.uid;
  await getLeverage(uid.value);
  modalRef.value?.show();
};

const hide = () => {
  modalRef.value?.hide();
};

const getLeverage = async (uid: number) => {
  isSubmitting.value = true;
  try {
    const res = await AccountService.getConfig(uid);
    leverageSelection.value = res.leverageAvailable.map((item: any) => {
      return {
        label: item + ":1",
        value: item,
      };
    });
  } catch (error) {
    MsgPrompt.error(error);
  }
  isSubmitting.value = false;
};

const submit = async () => {
  try {
    isSubmitting.value = true;
    await AccountService.submitLeverageChangeRequest(
      uid.value,
      accountNumber.value,
      newLeverage.value
    );
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      emits("submit");
      modalRef.value?.hide();
    });
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isSubmitting.value = false;
  }
};
defineExpose({
  show,
  hide,
});
</script>

<style lang="scss">
.el-select {
  width: 100%;
}

.el-date-editor.el-input,
.el-date-editor.el-input__inner {
  width: 100%;
}
</style>
