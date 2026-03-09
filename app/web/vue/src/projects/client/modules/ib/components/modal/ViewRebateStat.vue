<template>
  <el-dialog
    id="dialog-height"
    v-model="dialogRef"
    :width="isMobile ? '100%' : '900'"
    class="rounded-3"
    align-center
  >
    <template #header="{ titleId }">
      <div :class="isMobile ? '' : 'my-header'">
        <span :id="titleId">{{ title }}</span>

        <div
          :class="
            isMobile
              ? 'mt-2'
              : 'd-flex justify-content-between align-items-center gap-3'
          "
        >
          <el-date-picker
            class="w-250px"
            v-model="period"
            type="daterange"
            size="large"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            value-format="YYYY-MM-DD"
          />
          <div :class="isMobile ? 'mt-2' : ''">
            <el-button size="large" class="ms-4" @click="confirmSearch">{{
              $t("action.search")
            }}</el-button>
            <el-button
              size="large"
              class="ms-4"
              @click="clearSearchFilterCriteria"
              >{{ $t("action.clear") }}</el-button
            >
          </div>
        </div>
      </div>
    </template>
    <div style="max-height: 80vh; overflow: auto">
      <div class="mb-4" v-if="stats">
        <el-tag
          class="me-3"
          :class="isMobile ? 'mt-2' : ''"
          effect="dark"
          v-for="(value, key) in stats.depositAmounts"
          :key="key"
        >
          <span class="me-1"
            >{{ $t("title.deposit") }}: {{ $t(`type.currency.${key}`) }}</span
          >
          <BalanceShow :currency-id="Number(key)" :balance="value" />
        </el-tag>
        <el-tag
          class="me-3"
          :class="isMobile ? 'mt-2' : ''"
          type="warning"
          effect="dark"
          v-for="(value, key) in stats.withdrawalAmounts"
          :key="key"
        >
          <span class="me-1"
            >{{ $t("title.withdraw") }}: {{ $t(`type.currency.${key}`) }}</span
          >
          <BalanceShow :currency-id="Number(key)" :balance="value" />
        </el-tag>
        <el-tag
          class="me-3"
          :class="isMobile ? 'mt-2' : ''"
          type="success"
          effect="dark"
          v-for="(value, key) in stats.rebateAmounts"
          :key="key"
        >
          <span class="me-1"
            >{{ $t("title.rebate") }}: {{ $t(`type.currency.${key}`) }}</span
          >
          <BalanceShow :currency-id="Number(key)" :balance="value" />
        </el-tag>
      </div>

      <table
        class="table align-middle table-row-bordered gy-5"
        :class="isMobile ? 'fs-8' : ''"
      >
        <thead>
          <tr class="text-uppercase">
            <th>{{ $t("fields.symbol") }}</th>
            <th>{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.lot") }}</th>
            <th>{{ $t("fields.amount") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>

        <tbody
          v-else-if="
            !isLoading &&
            (data['rebateList'] == undefined ||
              data['rebateList']?.length === 0)
          "
        >
          <NoDataBox />
        </tbody>

        <tbody v-else>
          <tr v-for="(item, index) in data['rebateList']" :key="index">
            <td>{{ index }}</td>
            <td
              v-for="(amountValue, amountKey) in item.amounts"
              :key="amountKey"
            >
              <span class="me-1">{{ $t(`type.currency.${amountKey}`) }}</span>
            </td>
            <td>{{ item.volume / 100 }}</td>

            <td
              v-for="(amountValue, amountKey) in item.amounts"
              :key="amountKey"
            >
              <BalanceShow
                :currency-id="Number(amountKey)"
                :balance="amountValue"
                :class="amountValue > 0 ? '' : 'text-danger'"
              />
            </td>
          </tr>
          <tr
            class="text-success fw-bold text-uppercase"
            v-if="data['totalRebate']"
          >
            <td>{{ $t("title.total") }}</td>
            <td>{{ $t(`type.currency.${data["totalRebate"].currency}`) }}</td>
            <td>{{ data["totalRebate"].volume / 100 }}</td>
            <td>
              <BalanceShow
                :currency-id="Number(data['totalRebate'].currency)"
                :balance="data['totalRebate'].amounts"
                :class="data['totalRebate'].amounts > 0 ? '' : 'text-danger'"
              />
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <!-- <template #footer>
        <div class="dialog-footer">
          <el-button @click="dialogRef = false">{{
            $t("action.close")
          }}</el-button>
        </div>
      </template> -->
  </el-dialog>
</template>
<script setup lang="ts">
import { ref } from "vue";
import IbService from "../../services/IbService";
import { isMobile } from "@/core/config/WindowConfig";
import { convertTradeTime } from "@/core/helpers/helpers";
const dialogRef = ref(false);
const isLoading = ref(false);
const title = ref("");
const data = ref(<any>[]);
const stats = ref(<any>[]);
const period = ref([] as any);
const propsData = ref(<any>[]);

const criteria = ref<any>({
  uid: 0,
});
const rebateCriteria = ref<any>({
  uid: 0,
});
const show = async (_data: any) => {
  dialogRef.value = true;
  title.value = _data.user.displayName;
  await reset();
  criteria.value.uid = _data.uid;
  rebateCriteria.value.uid = _data.uid;

  propsData.value = _data;
  fecthData(_data);
};

const reset = async () => {
  period.value = [];
  data.value = [];
  stats.value = [];
  criteria.value.from = null;
  criteria.value.to = null;
  rebateCriteria.value.from = null;
  rebateCriteria.value.to = null;
  propsData.value = [];
};

const fecthData = async (_data: any) => {
  isLoading.value = true;
  try {
    data.value = [];
    stats.value = [];
    stats.value = await IbService.getIbStat(criteria.value);
    processSymbolData();
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const processSymbolData = async () => {
  const res = await IbService.getIbRebateStat(rebateCriteria.value);
  if (Object.keys(res).length == 0) return;
  var currency = res[Object.keys(res)[0]].amounts;
  currency = Object.keys(currency)[0];

  const total = {
    volume: Object.values(res).reduce(
      (acc: number, cur: any) => acc + cur.volume,
      0
    ),
    amounts: Object.values(res).reduce(
      (acc: any, cur: any) => acc + cur.amounts[currency],
      0
    ),
    currency: currency,
  };
  data.value["rebateList"] = res;
  data.value["totalRebate"] = total;
};

const clearSearchFilterCriteria = () => {
  criteria.value.from = null;
  criteria.value.to = null;
  rebateCriteria.value.from = null;
  rebateCriteria.value.to = null;
  period.value = [];
  fecthData(propsData.value);
};
const confirmSearch = () => {
  var convertedTime = convertTradeTime(period.value[0], period.value[1]);
  criteria.value.from = convertedTime[0];
  criteria.value.to = convertedTime[1];
  rebateCriteria.value.from = convertedTime[0];
  rebateCriteria.value.to = convertedTime[1];
  fecthData(propsData.value);
};

defineExpose({
  show,
});
</script>
<style>
:deep #dialog-height {
  max-height: 80vh !important;
}

.my-header {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 16px;
  align-items: center;
  line-height: 40px;
}
</style>
