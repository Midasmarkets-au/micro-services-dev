<template>
  <div v-if="rebateFilterRef?.isLoading" class="d-flex justify-content-center">
    <LoadingRing />
  </div>
  <div
    v-else-if="!rebateFilterRef?.isLoading && rebateFilterRef?.data.length == 0"
    class="d-flex justify-content-center"
  >
    <NoDataBox />
  </div>

  <div v-else>
    <el-card class="mt-3">
      <div>
        <h6 class="fw-semibold">
          {{
            $t("action.showing") +
            " " +
            (rebateFilterRef?.criteria.total ?? "0") +
            " " +
            $t("title.results")
          }}
        </h6>
      </div>
      <div class="d-flex gap-4">
        <div>{{ $t("title.subTotal") }}</div>
        <div>
          {{ $t("fields.volume") }} -
          {{ rebateFilterRef?.criteria.pageTotalVolume / 100 ?? 0 }}
        </div>
        <div>
          <BalanceShow
            :balance="rebateFilterRef?.criteria.pageTotalAmount ?? 0"
            :currency-id="agentAccount.currencyId"
          />
        </div>
      </div>
      <div class="d-flex gap-4">
        <div>{{ $t("title.total") }}</div>
        <div>
          {{ $t("fields.volume") }} -
          {{ rebateFilterRef?.criteria.totalVolume / 100 ?? 0 }}
        </div>
        <div>
          <BalanceShow
            :balance="rebateFilterRef?.criteria.totalAmount ?? 0"
            :currency-id="agentAccount.currencyId"
          />
        </div>
      </div>
    </el-card>
    <el-card
      v-for="(item, index) in rebateFilterRef?.getData()"
      :key="index"
      class="mt-3"
      @click="showDetail(item)"
    >
      <div>
        <div class="d-flex justify-content-between align-items-center">
          <div>
            {{ item.trade?.accountName }} - {{ item.trade?.accountNumber }}
          </div>
          <div>{{ item.trade?.symbol }}</div>
        </div>
        <div class="d-flex justify-content-between align-items-center">
          <div>
            {{ $t("fields.volume") }} - {{ item.trade?.volume / 100 }} /
            <BalanceShow
              :balance="item.amount"
              :currency-id="item.currencyId"
            />
          </div>
          <div>
            <TimeShow :date-iso-string="item.trade?.closeAt" type="exactTime" />
          </div>
        </div>
      </div>
    </el-card>
  </div>

  <RebateDetailMobile ref="rebateDetailRef" />
</template>
<script setup lang="ts">
import { ref, inject } from "vue";
import RebateDetailMobile from "./RebateDetailMobile.vue";
const rebateDetailRef = ref<any>(null);
const agentAccount = inject<any>("agentAccount");
const rebateFilterRef = inject<any>("rebateFilterRef");

const showDetail = (item: any) => {
  rebateDetailRef.value.show(item);
};
</script>
