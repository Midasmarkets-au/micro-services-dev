<template>
  <div class="card h-100">
    <div class="card-header card-header-bottom">
      <div class="card-title-noicon flex-column">
        <span class="mb-1 text-gray" style="font-weight: 400; font-size: 12px">
          {{ $t("title.top5ProductsToday") }}
        </span>
        <div class="fs-2 ml-0">{{ $t("title.lotsOverview") }}</div>
      </div>
      <div class="card-toolbar">
        <router-link to="/ib" class="view-more"
          >{{ $t("action.viewMore")
          }}<inline-svg
            src="/images/icons/general/show_more.svg"
            class="ml-2"
          ></inline-svg
        ></router-link>
      </div>
    </div>
    <div class="h-100 overflow-hidden">
      <table
        v-if="isLoading || lots.length === 0"
        class="table align-middle fs-6 gy-5 h-100"
      >
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && lots.length === 0">
          <NoDataBox />
        </tbody>
      </table>

      <div
        v-else
        class="d-flex flex-column mt-2"
        style="border-bottom: 1px #f2f4f7 solid"
      >
        <div
          class="d-flex justify-content-between align-items-center px-10 py-7"
          :style="{
            backgroundColor: index % 2 === 0 ? '#FAFBFD' : '#fff',
            flex: 1,
          }"
          v-for="(item, index) in lots"
          :key="index"
        >
          <span class="text-start" style="color: #3a3e44">{{
            item.symbol
          }}</span>
          <span class="text-end">{{ item.volume / 100 }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import { useStore } from "@/store";
import IbReportService from "../services/IbReportService";
import MsgPrompt from "@/core/plugins/MsgPrompt";

const store = useStore();
const isLoading = ref(true);

const lots = ref(Array<any>());

onMounted(async () => {
  try {
    lots.value = await IbReportService.getTradeSymbolTodayVolume();
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
  // setTimeout(() => {
  //   lots.value = [
  //     { symbol: "EURUSD", volume: 24 },
  //     { symbol: "XAUUSD", volume: 32 },
  //     { symbol: "XAGUSD", volume: 19 },
  //     { symbol: "US500", volume: 20 },
  //     { symbol: "USDJPY", volume: 15 },
  //   ];
  //   isLoading.value = false;
  // }, 100);
});
</script>

<style scoped></style>
