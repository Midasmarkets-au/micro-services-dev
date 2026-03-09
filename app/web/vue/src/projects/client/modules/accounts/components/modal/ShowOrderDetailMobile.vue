<template>
  <el-dialog v-model="dialogRef" :title="trade.symbol" width="500" align-center>
    <div class="d-flex align-items-center justify-content-between">
      <div class="d-flex align-items-center gap-2">
        <p>{{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}</p>
        <p>{{ trade.volume }}</p>
        <p>{{ $t("fields.volume") }}</p>
      </div>
      <h5
        :class="{
          'text-danger': trade.profit < 0,
          'text-success': trade.profit > 0,
        }"
      >
        {{ trade.profit }}
      </h5>
    </div>

    <div class="d-flex align-items-center gap-2 mb-2">
      <div>{{ handleTradeFormatted(trade.openPrice, trade.digits) }}</div>
      <el-icon><CaretRight /></el-icon>
      <div
        v-if="trade.closeAt != null"
        :class="{
          'text-danger': trade.profit < 0,
          'text-success': trade.profit > 0,
        }"
      >
        {{ handleTradeFormatted(trade.closePrice, trade.digits) }}
      </div>
      <div
        v-else
        :class="{
          'text-danger': trade.profit < 0,
          'text-success': trade.profit > 0,
        }"
      >
        {{ handleTradeFormatted(trade.currentPrice, trade.digits) }}
      </div>
    </div>
    <el-card>
      <div class="d-flex justify-content-between">
        <div>
          {{ $t("fields.ticket") }}
        </div>
        <div>
          <TinyCopyBox :val="trade.ticket.toString()"></TinyCopyBox>
          {{ trade.ticket }}
        </div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.openTime") }}
        </div>
        <TimeShow
          :date-iso-string="trade.openAt"
          style="font-size: 12px; color: rgb(113, 113, 113)"
        />
      </div>
      <div
        class="d-flex justify-content-between mt-1"
        v-if="trade.closeAt != null"
      >
        <div>
          {{ $t("fields.closeTime") }}
        </div>
        <TimeShow
          :date-iso-string="trade.closeAt"
          style="font-size: 12px; color: rgb(113, 113, 113)"
        />
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.s/l") }}
        </div>
        <div>{{ trade.sl }}</div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.tp") }}
        </div>
        <div>{{ trade.tp }}</div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.commission") }}
        </div>
        <div>{{ trade.commission }}</div>
      </div>
      <div class="d-flex justify-content-between mt-1">
        <div>
          {{ $t("fields.swaps") }}
        </div>
        <div>{{ trade.swaps }}</div>
      </div>
    </el-card>
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
import {
  handleTradeBuySellDisplay,
  handleTradeFormatted,
} from "@/core/helpers/helpers";
import { CaretRight } from "@element-plus/icons-vue";
import TinyCopyBox from "@/components/TinyCopyBox.vue";
const dialogRef = ref(false);
const trade = ref<any>([]);

const show = (_trade) => {
  dialogRef.value = true;
  trade.value = _trade;
};

defineExpose({
  show,
});
</script>
