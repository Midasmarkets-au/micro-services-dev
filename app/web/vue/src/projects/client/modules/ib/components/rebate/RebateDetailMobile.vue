<template>
  <el-dialog
    v-model="dialogRef"
    :title="$t('fields.rebate')"
    width="500"
    align-center
  >
    <div>
      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.name") }}</div>
        <div>
          <span>
            {{ item.trade?.accountName }}
          </span>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.accountNumber") }}</div>
        <div>
          <span>
            {{ item.trade?.accountNumber }}
          </span>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.symbol") }}</div>
        <div>
          {{ item.trade?.symbol }}
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.ticket") }}</div>
        <div>
          {{ item.trade?.ticket }}
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.volume") }}</div>
        <div>{{ item.trade?.volume / 100 }}</div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.amountIn") }}</div>
        <div>
          <BalanceShow :balance="item.amount" :currencyId="item.currencyId" />
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.status") }}</div>
        <div>
          <span> {{ $t(`type.transactionState.${item.stateId}`) }}</span>
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.createdOn") }}</div>
        <div>
          <TimeShow
            :date-iso-string="item.createdOn"
            type="exactTime"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
        </div>
      </div>

      <div class="d-flex justify-content-between align-items-center mb-1">
        <div class="text-gray-600">{{ $t("fields.closeTime") }}</div>
        <div>
          <TimeShow
            :date-iso-string="item.closeAt"
            type="exactTime"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
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
