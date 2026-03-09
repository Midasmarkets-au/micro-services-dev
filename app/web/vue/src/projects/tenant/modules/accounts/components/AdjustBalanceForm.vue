<template>
  <SimpleForm
    ref="modalRef"
    :title="$t('title.adjustBalance')"
    :is-loading="isLoading"
    :submit="submit"
    disable-submit
  >
    <div class="d-flex align-items-center rounded p-5 mb-1 bg-light-primary">
      <span class="svg-icon svg-icon-warning me-5">
        <span class="svg-icon svg-icon-1">
          <inline-svg src="/images/icons/finance/fin008.svg" />
        </span>
      </span>

      <div class="flex-grow-1 me-2">
        <a href="#" class="fw-bold text-gray-800 text-hover-primary fs-2">{{
          $t("title.currentBalance")
        }}</a>
      </div>

      <span class="fw-bold py-1 text-primary fs-2">${{ currentBalance }}</span>
    </div>

    <div class="mt-5">
      <label class="required fs-6 fw-semobold mb-2">
        {{ $t("title.adjustmentAmount") }}
      </label>
      <el-form-item>
        <el-input
          v-model="amount"
          :placeholder="$t('tip.pleaseInput')"
          @blur="checkIfValid"
        />
      </el-form-item>
    </div>
  </SimpleForm>
</template>

<script setup lang="ts">
import { ref } from "vue";
import SimpleForm from "@/components/SimpleForm.vue";

const isLoading = ref(true);
const modalRef = ref<InstanceType<typeof SimpleForm>>();
const accountId = ref(0);
const userDeposit = ref(0);
const amount = ref();
const validAmount = ref(true);
const currentBalance = ref(0);

const show = async (_accountId: number, _currentBalance: number) => {
  modalRef.value?.show();
  isLoading.value = true;
  currentBalance.value = _currentBalance;
  amount.value = null;
  validAmount.value = true;
  accountId.value = _accountId;
  isLoading.value = false;
};

const hide = () => {
  modalRef.value?.hide();
};

const submit = () => {
  if (!validAmount.value) {
    alert("Deposit is not enough");
    return;
  }
  hide();
};

const checkIfValid = () =>
  (validAmount.value = amount.value <= userDeposit.value);

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
