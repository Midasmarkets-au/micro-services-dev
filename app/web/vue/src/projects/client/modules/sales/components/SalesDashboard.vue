<template>
  <div class="card" :class="{ 'p-5': !isMobile }">
    <div class="card-header">
      <div class="card-title">
        <div :class="isMobile && dayId == 9 ? '' : 'd-flex'">
          <el-select v-model="dayId" :disable="isLoading" class="w-150px me-3">
            <el-option
              v-for="shortcut in daysShortcuts"
              :key="shortcut.text"
              :label="shortcut.text"
              :value="shortcut.value"
            />
          </el-select>
          <div v-if="dayId !== 9 && dayId !== 8">
            <el-tag size="large" type="success"
              >{{ currentDay[0] }} - {{ currentDay[1] }}</el-tag
            >
          </div>
          <div
            class="d-flex gap-3"
            :class="{ 'mt-3': isMobile }"
            v-if="dayId == 9"
          >
            <el-date-picker
              v-model="currentDay[0]"
              type="date"
              value-format="YYYY-MM-DD"
              :disabled="isLoading"
            />
            <el-date-picker
              v-model="currentDay[1]"
              type="date"
              value-format="YYYY-MM-DD"
              :disabled="isLoading"
            />
            <el-button @click="fetchData" :loading="isLoading">
              {{ $t("action.search") }}
            </el-button>
          </div>
        </div>
      </div>
    </div>
    <div class="card-body mx-auto" v-if="isLoading">
      <LoadingRing />
    </div>
    <div class="card-body mx-auto" v-else-if="!isLoading && data.length === 0">
      <NoDataBox />
    </div>
    <div class="card-body" v-else>
      <div
        class="d-flex justify-content-between"
        :class="{ 'justify-content-center gap-2': isMobile }"
      >
        <div>
          <h2 class="text-center">{{ $t("fields.account") }}</h2>
          <div class="d-flex gap-4" :class="{ 'flex-wrap': isMobile }">
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.newAccounts") }}: </span>
                {{ data.newAccountCount }}
              </div>
            </el-card>
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.newAgents") }}: </span>
                {{ data.newAgentCount }}
              </div>
            </el-card>
          </div>
        </div>
        <div>
          <h2 class="text-center">{{ $t("fields.rebate") }}</h2>
          <div class="d-flex gap-4" :class="{ 'flex-wrap': isMobile }">
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.counts") }}: </span>
                {{ data.rebateCount }}
              </div>
            </el-card>
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.amount") }}: </span>
                <BalanceShow :balance="data.rebateAmount" />
              </div>
            </el-card>
          </div>
        </div>
      </div>
      <div
        class="d-flex justify-content-between mt-10"
        :class="{ 'justify-content-center gap-2': isMobile }"
      >
        <div>
          <h2 class="text-center">{{ $t("title.deposit") }}</h2>
          <div class="d-flex gap-4" :class="{ 'flex-wrap': isMobile }">
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.counts") }}: </span
                >{{ data.depositCount }}
              </div>
            </el-card>
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.amount") }}: </span>
                <BalanceShow :balance="data.depositAmount" />
              </div>
            </el-card>
          </div>
        </div>
        <div>
          <h2 class="text-center">{{ $t("title.withdrawal") }}</h2>
          <div class="d-flex gap-4" :class="{ 'flex-wrap': isMobile }">
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.counts") }}: </span>
                {{ data.withdrawCount }}
              </div>
            </el-card>
            <el-card>
              <div>
                <span class="fw-bold">{{ $t("fields.amount") }}: </span>
                <BalanceShow :balance="data.withdrawAmount" />
              </div>
            </el-card>
          </div>
        </div>
      </div>

      <div class="mt-10">
        <h2 class="text-center">{{ $t("title.trade") }}</h2>
        <div class="d-flex justify-content-center gap-4">
          <el-card>
            <div>
              <span class="fw-bold">{{ $t("fields.counts") }}: </span>
              {{ data.tradeCount }}
            </div>
          </el-card>
          <el-card>
            <div>
              <span class="fw-bold">{{ $t("fields.volume") }}: </span>
              {{ data.tradeVolume }}
            </div>
          </el-card>
          <el-card>
            <div>
              <span class="fw-bold">{{ $t("fields.profit") }}: </span>
              <BalanceShow :balance="data.tradeProfit" />
            </div>
          </el-card>
        </div>
        <el-card style="width: 100% !important" class="mt-8 mb-10">
          <table class="mx-auto table align-middle table-row-bordered fs-6">
            <thead>
              <tr class="gs-0">
                <th>{{ $t("fields.symbol") }}</th>
                <th>{{ $t("fields.counts") }}</th>
                <th>{{ $t("fields.volume") }}</th>
                <th>{{ $t("fields.profit") }}</th>
              </tr>
            </thead>
            <tbody v-if="!isLoading && data.tradeBySymbol.length === 0">
              <NoDataBox />
            </tbody>
            <tbody v-else>
              <tr v-for="(item, index) in data.tradeBySymbol" :key="index">
                <td>{{ item.symbol }}</td>
                <td>{{ item.totalTrade }}</td>
                <td>{{ item.totalVolume }}</td>
                <td>
                  <BalanceShow :balance="item.totalProfit" />
                </td>
              </tr>
            </tbody>
          </table>
        </el-card>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, watch, onBeforeMount } from "vue";
import NoDataBox from "@/components/NoDataBox.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import moment from "moment";
import salesService from "../services/SalesService";
import Decimal from "decimal.js";
import i18n from "@/core/plugins/i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { isMobile } from "@/core/config/WindowConfig";
const t = i18n.global.t;
const props = defineProps<{
  childUid: number;
}>();
const isLoading = ref(false);
const currentDay = ref<[string, string]>([
  moment().startOf("day").format("YYYY-MM-DD"),
  moment().endOf("day").format("YYYY-MM-DD"),
]);

const criteria = ref({
  from: currentDay.value[0],
  to: currentDay.value[1],
});

const data = ref<any>({});

const fetchData = async () => {
  isLoading.value = true;
  criteria.value.from = currentDay.value[0];
  criteria.value.to = currentDay.value[1];
  try {
    const res = await salesService.queryAccountStat(
      props.childUid,
      criteria.value
    );
    data.value = res;
    data.value = {
      ...data.value,
      tradeVolume: new Decimal(data.value.tradeVolume).div(100).toNumber(),
    };
    for (let i = 0; i < data.value.tradeBySymbol.length; i++) {
      const item = data.value.tradeBySymbol[i];
      data.value.tradeBySymbol[i].totalVolume = new Decimal(item.totalVolume)
        .div(100)
        .toNumber();
    }
  } catch (error) {
    console.log(error);
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};

onBeforeMount(() => {
  fetchData();
});

watch(currentDay, () => {
  fetchData();
});

const dayId = ref(0);
const daysShortcuts = ref([
  {
    text: t("fields.customTime"),
    value: 9,
  },
  {
    text: t("fields.today"),
    value: 0,
  },
  {
    text: t("fields.yesterday"),
    value: 1,
  },
  {
    text: t("fields.thisWeek"),
    value: 2,
  },
  {
    text: t("fields.lastWeek"),
    value: 3,
  },
  {
    text: t("fields.thisMonth"),
    value: 4,
  },
  {
    text: t("fields.lastMonth"),
    value: 5,
  },
  {
    text: t("fields.thisYear"),
    value: 6,
  },
  {
    text: t("fields.lastYear"),
    value: 7,
  },
  {
    text: t("fields.allTime"),
    value: 8,
  },
]);
watch(dayId, () => {
  switch (dayId.value) {
    case 0:
      currentDay.value = [
        moment().startOf("day").format("YYYY-MM-DD"),
        moment().endOf("day").format("YYYY-MM-DD"),
      ];
      break;
    case 1:
      currentDay.value = [
        moment().subtract(1, "days").startOf("day").format("YYYY-MM-DD"),
        moment().subtract(1, "days").endOf("day").format("YYYY-MM-DD"),
      ];
      break;
    case 2:
      currentDay.value = [
        moment().startOf("isoWeek").format("YYYY-MM-DD"),
        moment().endOf("isoWeek").format("YYYY-MM-DD"),
      ];
      break;
    case 3:
      currentDay.value = [
        moment().subtract(1, "weeks").startOf("isoWeek").format("YYYY-MM-DD"),
        moment().subtract(1, "weeks").endOf("isoWeek").format("YYYY-MM-DD"),
      ];
      break;
    case 4:
      currentDay.value = [
        moment().startOf("month").format("YYYY-MM-DD"),
        moment().endOf("month").format("YYYY-MM-DD"),
      ];
      break;
    case 5:
      currentDay.value = [
        moment().subtract(1, "months").startOf("month").format("YYYY-MM-DD"),
        moment().subtract(1, "months").endOf("month").format("YYYY-MM-DD"),
      ];
      break;
    case 6:
      currentDay.value = [
        moment().startOf("year").format("YYYY-MM-DD"),
        moment().endOf("year").format("YYYY-MM-DD"),
      ];
      break;
    case 7:
      currentDay.value = [
        moment().subtract(1, "years").startOf("year").format("YYYY-MM-DD"),
        moment().subtract(1, "years").endOf("year").format("YYYY-MM-DD"),
      ];
      break;
    case 8:
      currentDay.value = [null, null];
      break;
    case 9:
      break;
  }
});
</script>
<style scoped lang="scss">
.el-card {
  margin-top: 10px;
  width: 250px;
  text-align: center;
}
@media (max-width: 768px) {
  .el-card {
    width: 100% !important;
  }
  .card-body {
    padding: 0.5rem !important;
  }
}
h2 {
  color: #5cb85c;
}
</style>
