<template>
  <el-dialog
    v-model="dialogRef"
    :title="getUserName(item)"
    width="500"
    align-center
  >
    <div class="d-flex align-items-center justify-content-between mb-3">
      <div>{{ item?.user?.email }}</div>
      <div>
        <el-button
          class="small"
          type="warning"
          :loading="isLoading"
          @click="sendCode"
        >
          {{ $t("action.sendCode") }}</el-button
        >
      </div>
    </div>
    <div v-if="ableToCheck" class="mb-3">
      <div
        class="d-flex justify-content-center p-1 mb-2"
        style="
          border: 1px solid #e0e0e0;
          min-width: 350px;
          border-radius: 100px;
          background-color: cornsilk;
          color: #900000;
        "
      >
        {{ showMessage }}
      </div>
      <div class="d-flex align-items-center justify-content-end">
        <el-input
          clearable
          class="w-150px"
          :placeholder="$t('fields.oneTimeCode')"
          v-model="verificationCode"
          :disabled="isLoading"
        />
        <el-button
          type="primary"
          class="ms-4"
          @click="submit"
          :loading="isLoading"
          :disabled="isSubmitting"
        >
          {{ $t("action.confirm") }}
        </el-button>
      </div>
    </div>
    <el-card>
      <div
        class="d-flex justify-content-between mt-1"
        v-if="
          item.role == AccountRoleTypes.IB ||
          item.role == AccountRoleTypes.Sales
        "
      >
        <div>
          {{ $t("fields.accountUid") }}
        </div>
        <div>
          {{ item.uid }}
          <TinyCopyBox :val="item.uid.toString()" class="ms-1"></TinyCopyBox>
        </div>
      </div>
      <div v-else class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.accountNo") }}
        </div>
        <div>
          {{ item.accountNumber }}
          <TinyCopyBox
            :val="item.accountNumber.toString()"
            class="ms-1"
          ></TinyCopyBox>
        </div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.type") }}
        </div>
        <div>{{ getAccountType(item) }}</div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>{{ $t("fields.role") }}</div>
        <div>
          <span :class="getRoleType(item).class">
            {{ getRoleType(item).label }}
          </span>
        </div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>{{ $t("fields.group") }}</div>
        <div>{{ item.group }}</div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>{{ $t("fields.code") }}</div>
        <div>{{ item.code }}</div>
      </div>
      <div
        class="d-flex justify-content-between mt-1"
        v-if="item.role != AccountRoleTypes.IB"
      >
        <div>
          {{ $t("fields.balance") }}
        </div>
        <div>
          <BalanceShow
            :balance="item?.tradeAccount?.balanceInCents"
            :currency-id="item?.tradeAccount?.currencyId"
          />
        </div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>{{ $t("fields.createdOn") }}</div>
        <div>
          <TimeShow
            :date-iso-string="item.createdOn"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
        </div>
      </div>
    </el-card>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false" :disabled="isLoading">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref, inject } from "vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import SalesService from "../../services/SalesService";
import i18n from "@/core/plugins/i18n";
import TinyCopyBox from "@/components/TinyCopyBox.vue";

const isLoading = ref<any>(false);
const isSubmitting = ref(false);
const t = i18n.global.t;
const dialogRef = ref(false);
const getUserName = inject<any>("getUserName");
const ableToCheck = ref(false);
const verificationCode = ref("");
const showMessage = ref("t('tip.needVerificationCode')");
const item = ref<any>(null);
const show = (_item) => {
  dialogRef.value = true;
  if (_item.uid !== item?.value?.uid) {
    ableToCheck.value = false;
    showMessage.value = t("tip.needVerificationCode");
    isLoading.value = false;
    isSubmitting.value = false;
    verificationCode.value = "";
  }
  item.value = _item;
};

const submit = async () => {
  isSubmitting.value = true;

  if (!verificationCode.value) {
    showMessage.value = t("tip.verificationCodeRequired");
    isSubmitting.value = false;
    return;
  }

  try {
    const res = await SalesService.getEmailByCode(
      item.value.uid,
      verificationCode.value
    );
    item.value.user.email = res;
    showMessage.value = t("status.success");
  } catch (e) {
    showMessage.value = t("error." + e.response.data);
  }

  verificationCode.value = "";
  isSubmitting.value = false;
};

const sendCode = async () => {
  isLoading.value = true;

  try {
    const res = await SalesService.getViewEmailCode(item.value.uid);
    ableToCheck.value = true;
    showMessage.value = t("tip.codeSentToEmail");
  } catch (e) {
    if (e.response.data == "CODE_ALREADY_SENT") {
      ableToCheck.value = true;
    } else {
      ableToCheck.value = false;
    }
    showMessage.value = t("error." + e.response.data);
  }

  verificationCode.value = "";
  isLoading.value = false;
};

const getRoleType = (item: any) => {
  switch (item.role) {
    case AccountRoleTypes.Client:
      return { class: "badge badge-primary", label: t("fields.client") };
    case AccountRoleTypes.IB:
      return { class: "badge badge-success", label: t("fields.ib") };
    case AccountRoleTypes.Sales:
      return { class: "badge badge-danger", label: t("fields.sales") };
  }
};

const getAccountType = (item: any) => {
  return t(`type.account.${item.type}`);
};

defineExpose({
  show,
});
</script>
