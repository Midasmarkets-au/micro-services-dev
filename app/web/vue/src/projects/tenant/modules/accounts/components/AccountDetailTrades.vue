<template>
  <AccountTradeStat :accountId="props.accountId" />
  <div class="card mt-4">
    <div class="card-header">
      <div class="card-title">{{ $t("title.tradeHistory") }}</div>
      <div class="card-toolbar">
        <!-- <el-button
          type="primary"
          v-if="$can('SuperAdmin')"
          @click="showReport()"
        >
          {{ $t("title.activityReport") }}
        </el-button> -->
      </div>
    </div>
    <div class="card-body py-4">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="text-start w-70px">{{ $t("fields.ticket") }}</th>
            <th class="w-70px text-end">{{ $t("fields.tp") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.symbol") }}</th>
            <th class="min-w-40px text-end">{{ $t("fields.buy/Sell") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.openPrice") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.closePrice") }}</th>
            <th class="min-w-40px text-end">{{ $t("fields.volume") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.profit") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.openTime") }}</th>
            <th class="min-w-60px text-end">{{ $t("fields.closeTime") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && trades.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr
            class="text-center"
            v-for="(transaction, index) in trades"
            :key="index"
          >
            <td class="text-end">{{ transaction.ticket }}</td>
            <td class="text-end">{{ transaction.tp }}</td>
            <td class="text-end">{{ transaction.symbol }}</td>
            <td class="text-end">{{ transaction.buySell }}</td>
            <td class="text-end">{{ transaction.openPrice }}</td>
            <td class="text-end">{{ transaction.closePrice }}</td>
            <td class="text-end">{{ transaction.volume }}</td>
            <td
              class="text-end"
              :class="{
                'text-success': transaction.profit >= 0,
                'text-danger': transaction.profit < 0,
              }"
            >
              {{ transaction.profit }}
            </td>
            <td class="text-end">
              <TimeShow :date-iso-string="transaction.openTime" />
            </td>
            <td class="text-end">
              <TimeShow
                :date-iso-string="transaction.closeTime"
                v-if="transaction.closeTime != null"
              />
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
    <ActivityReport ref="activityReportRef" />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import TableFooter from "@/components/TableFooter.vue";
import TimeShow from "@/components/TimeShow.vue";
import AccountService from "../services/AccountService";
import i18n from "@/core/plugins/i18n";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import ActivityReport from "./form/ActivityReportForm.vue";
import AccountTradeStat from "./accountTradeStat/AccountTradeStat.vue";
import { handleTradeBuySellDisplay } from "@/core/helpers/helpers";

const { t } = i18n.global;

const activityReportRef = ref<any>(null);
const props = defineProps<{
  accountId: any;
  serviceId: any;
}>();

const isLoading = ref(true);

const trades = ref<any>([]);

const criteria = ref({
  size: 10,
  accountId: props.accountId,
  serviceId: props.serviceId,
} as any);

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;
  criteria.value.page = selectedPage;
  try {
    const responseBody = await AccountService.queryTrades(criteria.value);
    criteria.value = responseBody.criteria;
    trades.value = responseBody.data.map((item: any) => ({
      ticket: item.ticket,
      symbol: item.symbol,
      tp: item.tp,
      volume: item.volume,
      profit: item.profit,
      openTime: item.openAt,
      closeTime: item.closeAt,
      openPrice: item.openPrice,
      closePrice: item.closePrice,
      buySell: t(`type.cmd.${handleTradeBuySellDisplay(item)}`),
    }));
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const showReport = () => {
  activityReportRef.value.show(props.accountId);
};

onMounted(() => {
  fetchData(1);
});

const pageChange = (page: number) => {
  fetchData(page);
};
</script>
