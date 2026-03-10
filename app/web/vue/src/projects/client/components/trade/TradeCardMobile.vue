<template>
  <div>
    <div v-if="tradeFilterRef?.isLoading" class="d-flex justify-content-center">
      <LoadingRing />
    </div>
    <div
      class="d-flex justify-content-center card mt-2"
      v-else-if="
        !tradeFilterRef?.isLoading && tradeFilterRef?.data.length === 0
      "
    >
      <div class="d-flex justify-content-center">
        <NoDataBox />
      </div>
    </div>
    <div v-else class="card mt-2">
      <el-card
        class="mt-3 mx-2"
        v-for="(trade, index) in tradeFilterRef?.getData()"
        :key="index"
        @click="showOrderDetail(trade)"
      >
        <div class="d-flex justify-content-between">
          <h5>{{ trade.symbol }}</h5>
          <h5
            :class="{
              'text-danger': trade.profit < 0,
              'text-success': trade.profit > 0,
            }"
          >
            {{ trade.profit }}
          </h5>
        </div>
        <div class="d-flex justify-content-between">
          <TimeShow
            v-if="tradeFilterRef?.criteria.isClosed"
            :date-iso-string="trade.closeAt"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
          <TimeShow
            v-else
            :date-iso-string="trade.openAt"
            style="font-size: 12px; color: rgb(113, 113, 113)"
          />
          <div class="d-flex align-items-center gap-2">
            <div v-if="!isCustomer">{{ trade.accountNumber }}</div>
            <el-icon v-if="!isCustomer"><Right /></el-icon>
            <div>{{ trade.ticket }}</div>
          </div>
        </div>
        <div class="d-flex justify-content-between">
          <div class="d-flex align-items-center gap-2 mt-2">
            <div>{{ $t(`type.cmd.${handleTradeBuySellDisplay(trade)}`) }}</div>
            <div>{{ trade.volume }}</div>
            <div>{{ $t("fields.volume") }}</div>
          </div>
          <div class="d-flex align-items-center gap-2">
            <div>{{ handleTradeFormatted(trade.openPrice, trade.digits) }}</div>
            <el-icon><CaretRight /></el-icon>
            <div
              v-if="tradeFilterRef?.criteria.isClosed"
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
        </div>
      </el-card>
      <el-card class="mt-4 mx-2 mb-2">
        <div class="d-flex justify-content-between">
          <div>{{ $t("fields.totalVolumes") }}</div>
          <div>{{ tradeFilterRef?.criteria.totalVolume ?? 0 }}</div>
        </div>

        <div class="d-flex justify-content-between">
          <div>{{ $t("fields.totalProfits") }}</div>
          <div
            :class="{
              'text-danger': tradeFilterRef?.criteria.totalProfit < 0,
              'text-success': tradeFilterRef?.criteria.totalProfit > 0,
            }"
          >
            {{ tradeFilterRef?.criteria.totalProfit ?? 0 }}
          </div>
        </div>
        <div class="d-flex justify-content-between">
          <div>{{ $t("fields.totalCommission") }}</div>
          <div>{{ tradeFilterRef?.criteria.totalCommission ?? 0 }}</div>
        </div>
        <div class="d-flex justify-content-between">
          <div>{{ $t("fields.totalSwaps") }}</div>
          <div>{{ tradeFilterRef?.criteria.totalSwap ?? 0 }}</div>
        </div>
      </el-card>
    </div>
    <TableFooter
      @page-change="tradeFilterRef?.fetchData"
      :criteria="tradeFilterRef?.criteria"
    />
  </div>
  <ShowOrderDetailMobile ref="showOrderRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import {
  handleTradeBuySellDisplay,
  handleTradeFormatted,
} from "@/core/helpers/helpers";
import { CaretRight, Right } from "@element-plus/icons-vue";
import ShowOrderDetailMobile from "./ShowOrderDetailMobile.vue";

const tradeFilterRef = inject<any>("tradeFilterRef");
const isCustomer = inject<any>("isCustomer");
const showOrderRef = ref<any>(null);
const showOrderDetail = (trade: any) => {
  showOrderRef.value.show(trade);
};
</script>
