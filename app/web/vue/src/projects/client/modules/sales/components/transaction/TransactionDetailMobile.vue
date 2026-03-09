<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.transferDetail')"
    width="500"
    align-center
  >
    <div>
      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.name") }}</div>
        <div>
          <span>
            {{
              item.user.nativeName ||
              item.user.displayName ||
              `${item.user.firstName} ${item.user.lastNameName}`
            }}
          </span>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.email") }}</div>
        <div>
          {{ item.user.email }}
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.currency") }}</div>
        <div>
          {{ $t(`type.currency.${item.currencyId}`) }}
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.amount") }}</div>
        <div>
          <BalanceShow :balance="item.amount" :currencyId="item.currencyId" />
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.status") }}</div>
        <div>
          <span
            class="badge"
            :class="`badge-${
              {
                [TransactionStateType.TransferCreated]: 'primary',
                [TransactionStateType.TransferCompleted]: 'success',
                [TransactionStateType.TransferRejected]: 'danger',
                [TransactionStateType.TransferApproved]: 'warning',
              }[item.stateId] ?? 'info'
            }`"
            >{{
              $t(`type.transactionState.${item.stateId}`).replace(
                /^deposit\s+/i,
                ""
              )
            }}</span
          >
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.createdOn") }}</div>
        <div>
          <TimeShow
            :date-iso-string="item.createdOn"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.sourceAccount") }}</div>
        <div>
          {{ item.targetAccount.accountNumber }}({{
            $t(`type.currency.${item.targetAccount.currencyId}`)
          }})
          <TinyCopyBox
            :val="item.targetAccount.accountNumber.toString()"
            class="ms-1"
          ></TinyCopyBox>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.sourceAccountGroup") }}</div>
        <div>
          {{ item.targetAccount.group || "***" }}
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.targetAccount") }}</div>

        <div>
          {{ item.sourceAccount.accountNumber }}({{
            $t(`type.currency.${item.sourceAccount.currencyId}`)
          }})
          <TinyCopyBox
            :val="item.sourceAccount.accountNumber.toString()"
            class="ms-1"
          ></TinyCopyBox>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.targetAccountGroup") }}</div>
        <div>
          {{ item.sourceAccount.group || "***" }}
        </div>
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import { TransactionStateType } from "@/core/types/StateInfos";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
const dialogRef = ref(false);
const item = ref<any>([]);
const show = (_item: any) => {
  dialogRef.value = true;
  item.value = _item;
};

defineExpose({
  show,
});
</script>
