<template>
  <div class="card h-100">
    <div class="card-header card-header-bottom h-lg-25 h-100">
      <div class="card-title-noicon flex-column">
        <span class="fs-4" style="color: #3a3e44">{{
          $t("title.rebate")
        }}</span>
        <h2 class="fw-semibold mt-1">
          <BalanceShow :balance="total" :currency-id="currencyId" />
          <span class="fs-7 ms-2 text-gray">
            <template v-if="tab === 'Hourly'">{{
              $t("tip.totalByNow")
            }}</template>
            <template v-else-if="tab === 'Daily'">{{
              $t("tip.totalByNow")
            }}</template>
            <template v-else-if="tab === 'Monthly'">{{
              $t("tip.totalByNow")
            }}</template>
          </span>
        </h2>
      </div>
      <div class="card-toolbar">
        <div class="nav account-tabs nav-pills nav-pills-custom">
          <button
            class="account-tab nav-item active"
            data-bs-toggle="pill"
            href="#rebate-chart-hourly"
            @click="changeTab('Hourly')"
          >
            {{ $t("status.hourly") }}
          </button>

          <button
            class="account-tab nav-item"
            data-bs-toggle="pill"
            href="#rebate-chart-daily"
            @click="changeTab('Daily')"
          >
            {{ $t("status.daily") }}
          </button>

          <button
            class="account-tab nav-item"
            data-bs-toggle="pill"
            href="#rebate-chart-monthly"
            @click="changeTab('Monthly')"
          >
            {{ $t("status.monthly") }}
          </button>
        </div>
      </div>
    </div>
    <!-- <div class="separator mx-auto" style="width: 95%"></div> -->
    <div class="px-lg-6 h-100">
      <table v-if="isLoading" class="table align-middle fs-6 gy-5 h-100">
        <tbody>
          <LoadingRing />
        </tbody>
      </table>
      <template v-else>
        <template v-if="tab === 'Hourly'">
          <apexchart
            ref="chartRef"
            type="area"
            :options="hourlyChart"
            :series="hourlySeries"
            :height="300"
          />
        </template>
        <template v-if="tab === 'Daily'">
          <apexchart
            ref="chartRef"
            type="area"
            :options="dailyChart"
            :series="dailySeries"
            :height="300"
          />
        </template>
        <template v-if="tab === 'Monthly'">
          <apexchart
            ref="chartRef"
            type="bar"
            :options="monthlyChart"
            :series="monthlySeries"
            :height="300"
          />
        </template>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import BalanceShow from "@/components/BalanceShow.vue";
import { ref, onMounted, onBeforeMount, computed } from "vue";
import { getCSSVariableValue } from "@/assets/ts/_utils";
import { ApexOptions } from "apexcharts";
import VueApexCharts from "vue3-apexcharts";
import { useI18n } from "vue-i18n";
import LoadingRing from "@/components/LoadingRing.vue";
import { useStore } from "@/store";
import IbReportService from "../services/IbReportService";
import filters from "@/core/helpers/filters";
import { TimeZoneService } from "@/core/plugins/TimerService";

const store = useStore();
const agentAccount = computed(() => store.state.AgentModule.agentAccount);
const timeZone = computed(() => store.state.AuthModule.user.timezone);

const currencyId = ref(840);
const isLoading = ref(true);
const currentTimeZoneOffsetInHours = TimeZoneService.getTimeZoneOffsetInHours();
const tab = ref("Hourly");
const { t } = useI18n();

const chartRef = ref<typeof VueApexCharts>();
const dailyChart = ref<ApexOptions>({} as ApexOptions);
const hourlyChart = ref<ApexOptions>({} as ApexOptions);
const monthlyChart = ref<ApexOptions>({} as ApexOptions);

const monthlySeries = ref([
  {
    name: t("title.rebate"),
    sum: 0,
    data: Array<any>(),
  },
]);

const dailySeries = ref([
  {
    name: t("title.rebate"),
    sum: 0,
    data: Array<any>(),
  },
]);

const hourlySeries = ref([
  {
    name: t("title.rebate"),
    sum: 0,
    data: Array<any>(),
  },
]);

const total = ref(0);

const changeTab = (tabName: string) => {
  if (isLoading.value || tabName === tab.value) return;
  isLoading.value = true;
  setTimeout(() => {
    tab.value = tabName;
    total.value =
      {
        Hourly: hourlySeries.value[0].sum,
        Daily: dailySeries.value[0].sum,
        Monthly: monthlySeries.value[0].sum,
      }[tabName] ?? 0;
    isLoading.value = false;
  }, 500);
};

onMounted(async () => {
  generateReportCharts();
});

const generateReportCharts = async () => {
  const hourly = await IbReportService.getRebateHourlySeries(
    currentTimeZoneOffsetInHours
  );
  const chartOptions = { yaxisMax: 0 };
  const result = Array.from({ length: new Date().getHours() }, () => 0);
  chartOptions.yaxisMax = 0;
  hourly.forEach(({ totalValue, hour }) => {
    hour = (hour + currentTimeZoneOffsetInHours + 24) % 24;
    result[hour] = totalValue;
    hourlySeries.value[0].sum += totalValue;
    chartOptions.yaxisMax = Math.max(chartOptions.yaxisMax, result[hour]);
  });
  hourlySeries.value[0].data = result;
  chartOptions.yaxisMax =
    chartOptions.yaxisMax === 0 ? 5 : chartOptions.yaxisMax;
  hourlyChart.value = hourlyChartOptions(chartOptions);
  total.value = hourlySeries.value[0].sum;

  const daily = await IbReportService.getRebateDailySeries(
    currentTimeZoneOffsetInHours
  );
  const chartOptionsDaily = { yaxisMax: 0, x_values: [] };
  const x_values = [];
  const result2 = Array.from({ length: new Date().getDate() }, () => 0);
  const new_result = [];
  daily.forEach(({ totalValue, day }) => {
    day--;
    result2[day] = totalValue;
    dailySeries.value[0].sum += totalValue;
    chartOptionsDaily.yaxisMax = Math.max(
      chartOptionsDaily.yaxisMax,
      result2[day]
    );

    x_values.push(getDay(day + 1));
    new_result.push(totalValue);
  });

  if (result2[result2.length - 1] === 0) {
    result2[result2.length - 1] = hourlySeries.value[0].sum;
  }
  // dailySeries.value[0].data = result2;
  dailySeries.value[0].data = new_result;
  chartOptionsDaily.yaxisMax =
    chartOptionsDaily.yaxisMax === 0 ? 5 : chartOptionsDaily.yaxisMax;

  chartOptionsDaily.x_values = x_values;

  dailyChart.value = dailyChartOptions(chartOptionsDaily);

  IbReportService.getRebateMonthlySeries(timeZone.value).then((monthly) => {
    const chartOptions = {
      yaxisMax: 0,
    };
    const result = Array.from({ length: 12 }, () => 0);
    monthly.forEach(({ totalValue, month }) => {
      month--;
      result[month] = totalValue;
      monthlySeries.value[0].sum += totalValue;
      chartOptions.yaxisMax = Math.max(chartOptions.yaxisMax, result[month]);
    });
    monthlySeries.value[0].data = result;
    chartOptions.yaxisMax =
      chartOptions.yaxisMax === 0 ? 5 : chartOptions.yaxisMax;
    monthlyChart.value = monthlyChartOptions(chartOptions);
  });
  isLoading.value = false;
};

const getDay = (val: any) => {
  if (val % 10 === 1) return val + "st";
  if (val % 10 === 2) return val + "nd";
  if (val % 10 === 3) return val + "rd";
  return val + "th";
};

onBeforeMount(() => {
  dailyChart.value = dailyChartOptions();
  hourlyChart.value = hourlyChartOptions();
  monthlyChart.value = monthlyChartOptions();
});

const monthlyChartOptions = (options?): ApexOptions => {
  const labelColor = getCSSVariableValue("--kt-gray-500");
  const borderColor = getCSSVariableValue("--kt-gray-200");
  const baseColor = "#0053AD";
  const secondaryColor = "#DBEDFF";

  return {
    chart: {
      fontFamily: "inherit",
      type: "bar",
      toolbar: {
        show: false,
      },
    },
    plotOptions: {
      bar: {
        borderRadius: 4,
        horizontal: false,
        columnWidth: "50%",
      },
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 2,
      colors: ["transparent"],
    },
    xaxis: {
      categories: Array.from({ length: 12 }, (_, i) => i + 1),
      min: 1,
      max: 12,
      axisBorder: {
        show: false,
      },
      axisTicks: {
        show: false,
      },
      labels: {
        style: {
          colors: labelColor,
          fontSize: "12px",
        },
        formatter: (value: string) => {
          return [
            "Jan",
            "Feb",
            "Mar",
            "Apr",
            "May",
            "Jun",
            "Jul",
            "Aug",
            "Sept",
            "Oct",
            "Nov",
            "Dec",
          ][parseInt(value) - 1];
        },
      },
    },
    yaxis: {
      min: options?.yaxisMin ?? 0,
      max: options?.yaxisMax ?? 10,
      tickAmount: 5,
      labels: {
        style: {
          colors: labelColor,
          fontSize: "12px",
        },
        formatter: (value: number) => {
          return filters.toCurrency(value);
        },
      },
    },
    fill: {
      opacity: 1,
    },
    states: {
      normal: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      hover: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      active: {
        allowMultipleDataPointsSelection: false,
        filter: {
          type: "none",
          value: 0,
        },
      },
    },
    tooltip: {
      style: {
        fontSize: "12px",
      },
      y: {
        formatter: function (val) {
          return (
            filters.toCurrency(val) +
            " " +
            t(`type.currency.${currencyId.value}`)
          );
        },
      },
    },
    colors: [baseColor, secondaryColor],
    grid: {
      borderColor: borderColor,
      strokeDashArray: 4,
      yaxis: {
        lines: {
          show: true,
        },
      },
    },
  };
};

const dailyChartOptions = (options?): ApexOptions => {
  const labelColor = getCSSVariableValue("--kt-gray-500");
  const borderColor = getCSSVariableValue("--kt-gray-200");
  const baseColor = "#0053AD";
  const secondaryColor = "#DBEDFF";
  return {
    chart: {
      fontFamily: "inherit",
      type: "area",
      toolbar: {
        show: false,
      },
      zoom: {
        enabled: false,
      },
    },
    plotOptions: {
      bar: {
        borderRadius: 4,
        horizontal: false,
        columnWidth: "50%",
      },
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 3,
    },
    xaxis: {
      categories: options?.x_values ?? [],
      min: 1,
      max: options?.x_values?.length ?? 10,
      axisBorder: {
        show: false,
      },
      axisTicks: {
        show: false,
      },
      labels: {
        style: {
          colors: labelColor,
          fontSize: "11px",
        },
      },
    },
    yaxis: {
      min: options?.yaxisMin ?? 0,
      max: options?.yaxisMax ?? 10,
      tickAmount: 5,
      labels: {
        style: {
          colors: labelColor,
          fontSize: "11px",
        },
        formatter: (value: number) => {
          return filters.toCurrency(value);
        },
      },
    },
    fill: {
      type: "gradient",
      gradient: {
        shadeIntensity: 1,
        inverseColors: false,
        opacityFrom: 0.4,
        opacityTo: 0.05,
        stops: [20, 100, 100, 100],
      },
    },
    states: {
      normal: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      hover: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      active: {
        allowMultipleDataPointsSelection: false,
        filter: {
          type: "none",
          value: 0,
        },
      },
    },
    tooltip: {
      style: {
        fontSize: "11px",
      },

      y: {
        formatter: function (val) {
          return (
            filters.toCurrency(val) +
            " " +
            t(`type.currency.${currencyId.value}`)
          );
        },
      },
    },
    colors: [baseColor, secondaryColor],
    grid: {
      borderColor: borderColor,
      strokeDashArray: 4,
      yaxis: {
        lines: {
          show: true,
        },
      },
    },
  };
};

const hourlyChartOptions = (options?): ApexOptions => {
  const labelColor = getCSSVariableValue("--kt-gray-500");
  const borderColor = getCSSVariableValue("--kt-gray-200");
  const baseColor = "#0053AD";
  const secondaryColor = "#DBEDFF";

  return {
    chart: {
      fontFamily: "inherit",
      type: "area",
      toolbar: {
        show: false,
      },
      zoom: {
        enabled: false,
      },
    },
    legend: {
      show: false,
    },
    dataLabels: {
      enabled: false,
    },
    stroke: {
      show: true,
      width: 3,
    },
    xaxis: {
      categories: Array.from({ length: 24 }, (_, index) => index + 0),
      min: 1,
      max: 24,
      axisBorder: {
        show: false,
      },
      axisTicks: {
        show: false,
      },
      labels: {
        style: {
          colors: labelColor,
          fontSize: "11px",
        },
        formatter: (value: string) => {
          const val = parseInt(value);
          if (val % 2 === 1) return "";
          if (val === 0) return "12am";
          if (val === 12) return "12pm";
          if (val < 12) return val + "am";
          return val - 12 + "pm";
        },
      },
    },
    yaxis: {
      min: options?.yaxisMin ?? 0,
      max: options?.yaxisMax ?? 10,
      tickAmount: 5,
      labels: {
        style: {
          colors: labelColor,
          fontSize: "11px",
        },
        formatter: (value: number) => {
          return filters.toCurrency(value);
        },
      },
    },
    fill: {
      type: "gradient",
      gradient: {
        shadeIntensity: 1,
        inverseColors: false,
        opacityFrom: 0.4,
        opacityTo: 0.05,
        stops: [20, 100, 100, 100],
      },
    },
    states: {
      normal: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      hover: {
        filter: {
          type: "none",
          value: 0,
        },
      },
      active: {
        allowMultipleDataPointsSelection: false,
        filter: {
          type: "none",
          value: 0,
        },
      },
    },
    tooltip: {
      style: {
        fontSize: "11px",
      },
      x: {
        formatter: (val: number) => {
          val -= 1;
          if (val === 0) return "12am";
          if (val === 12) return "12pm";
          if (val < 12) return val + "am";
          return val - 12 + "pm";
        },
      },
      y: {
        formatter: function (val) {
          return (
            filters.toCurrency(val) +
            " " +
            t(`type.currency.${currencyId.value}`)
          );
        },
      },
    },
    colors: [baseColor, secondaryColor],
    grid: {
      borderColor: borderColor,
      strokeDashArray: 4,
      yaxis: {
        lines: {
          show: true,
        },
      },
    },
  };
};
</script>

<style lang="scss" scoped>
.account-tabs {
  background-color: #fafbfd;
  border-radius: 8px;
  padding: 1px;
  border: 1px solid #f2f4f7;
}
.account-tabs .account-tab {
  padding: 6px 30px;
  border-radius: 8px;
  cursor: pointer;
  border: 0px;
  background: none;
  font-size: 14px;
}
.account-tabs .active {
  background-color: #f2f4f7;
  &::after {
    background: none;
  }
}
</style>
