<template>
  <SalesLayout activeMenuItem="statistics">
    <!-- 一. 顶部搜索框区域 -->
    <div class="card">
      <div class="card-body">
        <div class="row g-3 align-items-end">
          <!-- 搜索输入框 - 占一半 -->
          <div class="col-md-6 col-12">
            <el-input
              v-model="searchCriteria.userUid"
              :placeholder="$t('sales.searchUserUid')"
              clearable
            />
          </div>

          <!-- 其他筛选项 - 占另一半 -->
          <div class="col-md-6 col-12">
            <div class="row g-2">
              <!-- 身份选择 -->
              <div class="col-3">
                <el-select
                  v-model="searchCriteria.userType"
                  :placeholder="$t('sales.selectIdentity')"
                >
                  <el-option :label="$t('fields.sales')" value="sale" />
                  <el-option :label="$t('fields.ib')" value="ib" />
                  <el-option :label="$t('fields.client')" value="client" />
                </el-select>
              </div>

              <!-- 时间选择 -->
              <div class="col-3">
                <el-select
                  v-model="searchCriteria.timeRange"
                  :placeholder="$t('sales.selectTimeRange')"
                  @change="onTimeRangeChange"
                >
                  <el-option :label="$t('sales.last30Days')" value="30" />
                  <el-option :label="$t('sales.last7Days')" value="7" />
                  <el-option :label="$t('sales.customTime')" value="custom" />
                </el-select>
              </div>

              <!-- 图表类型选择 -->
              <div class="col-3">
                <el-select
                  v-model="chartType"
                  :placeholder="$t('sales.selectChartType')"
                >
                  <el-option :label="$t('sales.areaChart')" value="area" />
                  <el-option :label="$t('sales.lineChart')" value="line" />
                </el-select>
              </div>

              <!-- 按钮组 -->
              <div class="col-3 d-flex gap-1">
                <el-button @click="resetSearch" size="large">
                  {{ $t("action.reset") }}
                </el-button>
                <el-button @click="fetchData" :loading="isLoading" size="large">
                  {{ $t("action.search") }}
                </el-button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 自定义时间选择对话框 -->
    <el-dialog
      v-model="showCustomTimeDialog"
      :title="$t('sales.selectCustomTime')"
      width="500px"
    >
      <div class="d-flex flex-column gap-3">
        <el-date-picker
          v-model="customStartTime"
          type="date"
          :placeholder="$t('sales.startTime')"
          value-format="YYYY-MM-DD"
          style="width: 100%"
        />
        <el-date-picker
          v-model="customEndTime"
          type="date"
          :placeholder="$t('sales.endTime')"
          value-format="YYYY-MM-DD"
          style="width: 100%"
        />
        <el-alert
          v-if="timeRangeError"
          :title="timeRangeError"
          type="error"
          :closable="false"
        />
      </div>
      <template #footer>
        <el-button @click="showCustomTimeDialog = false">{{
          $t("action.cancel")
        }}</el-button>
        <el-button
          type="primary"
          @click="confirmCustomTime"
          :disabled="!!timeRangeError"
        >
          {{ $t("action.confirm") }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 加载状态 -->
    <div v-if="isLoading" class="card mt-2">
      <div class="card-body text-center py-10">
        <LoadingRing />
      </div>
    </div>

    <template v-else-if="!isLoading && statisticsData">
      <!-- 二. 汇总统计卡片区域 -->
      <div class="row g-2 mt-1">
        <!-- 总交易笔数 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="trade" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalTrades") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    {{ statisticsData.summaryStats?.totalTrades || 0 }}
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 净入金 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="funding" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalNetDeposit") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    <BalanceShow
                      :balance="statisticsData.summaryStats?.totalNetDeposit"
                      :currency-id="840"
                    />
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 总返佣 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="rebate" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalRebate") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    <BalanceShow
                      :balance="statisticsData.summaryStats?.totalRebate"
                      :currency-id="840"
                    />
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 总入金 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="funding" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalDeposit") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    <BalanceShow
                      :balance="statisticsData.summaryStats?.totalDeposit"
                      :currency-id="840"
                    />
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 总出金 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="funding" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalWithdrawal") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    <BalanceShow
                      :balance="statisticsData.summaryStats?.totalWithdrawal"
                      :currency-id="840"
                    />
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 总手数 -->
        <div class="col-md-4">
          <div class="card h-100">
            <div class="card-header border-0">
              <div class="card-title-noicon">
                <span class="rebate-svg">
                  <SvgIcon name="trade" path="ibCenter" />
                </span>
                <h4 class="rebate-title">{{ $t("sales.totalLots") }}</h4>
              </div>
            </div>
            <div class="card-body pt-3">
              <div class="rebate-content">
                <div class="rebate-amount">
                  <h1 class="fw-semibold balance-show">
                    {{
                      (statisticsData.summaryStats?.totalLots || 0).toFixed(2)
                    }}
                  </h1>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 三. 图表区域 -->
      <div class="row g-2 mt-1">
        <!-- 交易趋势 -->
        <div class="col-md-6">
          <div class="card h-100">
            <div class="card-header">
              <h3 class="card-title">{{ $t("sales.tradeTrend") }}</h3>
            </div>
            <div class="card-body">
              <apexchart
                v-if="tradeChartOptions"
                type="area"
                height="300"
                :options="tradeChartOptions"
                :series="tradeChartSeries"
              />
            </div>
          </div>
        </div>

        <!-- 资金流向 -->
        <div class="col-md-6">
          <div class="card h-100">
            <div class="card-header">
              <h3 class="card-title">{{ $t("sales.fundFlow") }}</h3>
            </div>
            <div class="card-body">
              <apexchart
                v-if="fundFlowChartOptions"
                type="area"
                height="300"
                :options="fundFlowChartOptions"
                :series="fundFlowChartSeries"
              />
            </div>
          </div>
        </div>

        <!-- 返佣趋势 -->
        <div class="col-md-6">
          <div class="card h-100">
            <div class="card-header">
              <h3 class="card-title">{{ $t("sales.rebateTrend") }}</h3>
            </div>
            <div class="card-body">
              <apexchart
                v-if="rebateChartOptions"
                type="area"
                height="300"
                :options="rebateChartOptions"
                :series="rebateChartSeries"
              />
            </div>
          </div>
        </div>

        <!-- 产品分布 -->
        <div class="col-md-6">
          <div class="card h-100">
            <div class="card-header">
              <h3 class="card-title">{{ $t("sales.productDistribution") }}</h3>
            </div>
            <div class="card-body">
              <apexchart
                v-if="productChartOptions"
                type="pie"
                height="300"
                :options="productChartOptions"
                :series="productChartSeries"
              />
            </div>
          </div>
        </div>
      </div>

      <!-- 四. 层级结构数据树表格 -->
      <div class="card mt-3">
        <div class="card-header">
          <h3 class="card-title">{{ $t("sales.hierarchyData") }}</h3>
        </div>
        <div class="card-body overflow-auto" style="white-space: nowrap">
          <el-table
            :data="statisticsData.hierarchyData"
            class="sales-table"
            style="width: 100%"
            row-key="id"
            :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
            lazy
            :load="loadChildrenData"
          >
            <el-table-column
              v-for="column in tableColumns"
              :key="column.prop || column.label"
              :prop="column.prop"
              :label="column.label"
              :width="column.width"
              :min-width="column.minWidth"
              :align="column.align"
              :fixed="column.fixed"
            >
              <template v-if="column.slot" #default="scope">
                <!-- 余额显示 -->
                <BalanceShow
                  v-if="column.slot === 'balance' && column.prop"
                  :balance="scope.row[column.prop]"
                  :currency-id="840"
                />
                <span v-else-if="column.slot === 'type'">
                  {{ $t(`type.accountRole.${scope.row.type}`) }}
                </span>
                <!-- 交易笔数显示 -->
                <span v-else-if="column.slot === 'trades'">
                  {{ scope.row.trades }}
                  <span class="text-gray" v-if="scope.row.type != '400'">
                    {{ `(${$t("sales.includingSubordinates")})` }}
                  </span>
                </span>
                <!-- 手数显示 -->
                <span v-else-if="column.slot === 'lots'">
                  {{ (scope.row.lots || 0).toFixed(2) }}
                </span>
                <!-- 产品标签 -->
                <template v-else-if="column.slot === 'products'">
                  <el-tag
                    v-for="product in scope.row.products"
                    :key="product"
                    size="small"
                    class="me-1"
                  >
                    {{ product }}
                  </el-tag>
                </template>
                <!-- 操作按钮 -->
                <el-button
                  v-else-if="column.slot === 'action'"
                  link
                  style="color: rgb(10, 70, 170)"
                  @click="showDetail(scope.row)"
                >
                  {{ $t("title.details") }}
                </el-button>
              </template>
            </el-table-column>
          </el-table>
        </div>
      </div>
    </template>

    <!-- 无数据状态 -->
    <div v-else-if="!isLoading && !statisticsData" class="card mt-2">
      <div class="card-body text-center py-10">
        <NoDataBox />
      </div>
    </div>

    <!-- 详情弹窗 -->
    <el-dialog
      v-model="showDetailDialog"
      :title="$t('title.details')"
      width="650px"
    >
      <div v-if="selectedRow" class="row g-3">
        <!-- 用户名 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("fields.name") }}</div>
            <div class="detail-value">{{ selectedRow.name }}</div>
          </div>
        </div>

        <!-- 用户ID -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("fields.userId") }}</div>
            <div class="detail-value">{{ selectedRow.id }}</div>
          </div>
        </div>

        <!-- 用户身份 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("fields.userIdentity") }}</div>
            <div class="detail-value">
              {{ $t(`type.accountRole.${selectedRow.type}`) }}
            </div>
          </div>
        </div>

        <!-- 组别代码 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.groupCode") }}</div>
            <div class="detail-value">{{ selectedRow.groupCode }}</div>
          </div>
        </div>

        <!-- 交易笔数 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.tradesCount") }}</div>
            <div class="detail-value">{{ selectedRow.trades }}</div>
          </div>
        </div>

        <!-- 净入金 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.netDeposit") }}</div>
            <div class="detail-value">
              <BalanceShow
                :balance="selectedRow.netDeposit"
                :currency-id="840"
              />
            </div>
          </div>
        </div>

        <!-- 入金 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.deposit") }}</div>
            <div class="detail-value">
              <BalanceShow :balance="selectedRow.deposit" :currency-id="840" />
            </div>
          </div>
        </div>

        <!-- 出金 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.withdrawal") }}</div>
            <div class="detail-value">
              <BalanceShow
                :balance="selectedRow.withdrawal"
                :currency-id="840"
              />
            </div>
          </div>
        </div>

        <!-- 返佣 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.rebate") }}</div>
            <div class="detail-value">
              <BalanceShow :balance="selectedRow.rebate" :currency-id="840" />
            </div>
          </div>
        </div>

        <!-- 交易手数 -->
        <div class="col-6">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.lots") }}</div>
            <div class="detail-value">
              {{ (selectedRow.lots || 0).toFixed(2) }} {{ $t("sales.lots") }}
            </div>
          </div>
        </div>

        <!-- 交易品种 -->
        <div class="col-12">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.products") }}</div>
            <div class="detail-value">
              <el-tag
                v-for="product in selectedRow.products"
                :key="product"
                size="small"
                class="me-1"
              >
                {{ product }}
              </el-tag>
            </div>
          </div>
        </div>

        <!-- 下级人员 -->
        <div class="col-12">
          <div class="detail-item">
            <div class="detail-label">{{ $t("sales.subordinates") }}</div>
            <div class="detail-value">
              {{ getChildrenCount(selectedRow) }} {{ $t("fields.person") }}
            </div>
          </div>
        </div>
      </div>
    </el-dialog>
  </SalesLayout>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import SalesLayout from "../components/SalesLayout.vue";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import BalanceShow from "@/components/BalanceShow.vue";
import SalesService from "../services/SalesService";
import { ElMessage } from "element-plus";
import moment from "moment";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

// 表格列配置
const tableColumns = computed(() => [
  {
    prop: "name",
    label: t("fields.name"),
    width: "190",
  },
  {
    prop: "type",
    label: t("fields.userIdentity"),
    width: "110",
    slot: "type",
  },
  {
    prop: "groupCode",
    label: t("sales.groupCode"),
    width: "120",
  },
  {
    prop: "trades",
    label: t("sales.tradesCount"),
    width: "180",
    align: "right",
    slot: "trades",
  },
  {
    prop: "netDeposit",
    label: t("sales.netDeposit"),
    width: "170",
    align: "right",
    slot: "balance",
  },
  {
    prop: "deposit",
    label: t("sales.deposit"),
    width: "170",
    align: "right",
    slot: "balance",
  },
  {
    prop: "withdrawal",
    label: t("sales.withdrawal"),
    width: "170",
    align: "right",
    slot: "balance",
  },
  {
    prop: "rebate",
    label: t("sales.rebate"),
    width: "120",
    align: "right",
    slot: "balance",
  },
  {
    prop: "lots",
    label: t("sales.lots"),
    width: "100",
    align: "right",
    slot: "lots",
  },
  {
    prop: "products",
    label: t("sales.products"),
    minWidth: "200",
    slot: "products",
  },
  {
    label: t("action.action"),
    width: "100",
    align: "center",
    fixed: "right",
    slot: "action",
  },
]);

// 搜索条件
const searchCriteria = ref({
  userUid: "",
  userType: "sale",
  timeRange: "30",
  from: "",
  to: "",
});

// 图表类型
const chartType = ref("area");

// 加载状态
const isLoading = ref(false);

// 统计数据
const statisticsData = ref<any>(null);

// 详情弹窗
const showDetailDialog = ref(false);
const selectedRow = ref<any>(null);

// 自定义时间选择
const showCustomTimeDialog = ref(false);
const customStartTime = ref("");
const customEndTime = ref("");

// 时间范围错误
const timeRangeError = computed(() => {
  if (!customStartTime.value || !customEndTime.value) {
    return t("sales.pleaseSelectTime");
  }

  const start = moment(customStartTime.value);
  const end = moment(customEndTime.value);

  if (end.isBefore(start)) {
    return t("sales.endTimeBeforeStart");
  }

  const daysDiff = end.diff(start, "days");
  if (daysDiff > 30) {
    return t("sales.timeRangeExceeds30Days");
  }

  return "";
});

// 显示详情
const showDetail = (row: any) => {
  selectedRow.value = row;
  showDetailDialog.value = true;
};

// 统计下级人员数量
const getChildrenCount = (row: any): number => {
  if (!row.children || row.children.length === 0) {
    return 0;
  }
  return row.children.length;
};

// 时间范围变化
const onTimeRangeChange = (value: string) => {
  if (value === "custom") {
    showCustomTimeDialog.value = true;
  } else {
    const days = parseInt(value);
    searchCriteria.value.to = moment().format("YYYY-MM-DD");
    searchCriteria.value.from = moment()
      .subtract(days, "days")
      .format("YYYY-MM-DD");
    fetchData();
  }
};

// 确认自定义时间
const confirmCustomTime = () => {
  if (!timeRangeError.value) {
    searchCriteria.value.from = customStartTime.value;
    searchCriteria.value.to = customEndTime.value;
    showCustomTimeDialog.value = false;
    fetchData();
  }
};

// 重置搜索
const resetSearch = () => {
  searchCriteria.value = {
    userUid: "",
    userType: "sale",
    timeRange: "30",
    from: moment().subtract(30, "days").format("YYYY-MM-DD"),
    to: moment().format("YYYY-MM-DD"),
  };
  chartType.value = "area";
  customStartTime.value = "";
  customEndTime.value = "";
  fetchData();
};

// 获取数据
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SalesService.getSalesStatistics(searchCriteria.value);
    statisticsData.value = res;
  } catch (error: any) {
    ElMessage.error(error.message || t("error.failedToFetchData"));
  } finally {
    isLoading.value = false;
  }
};

// 懒加载子节点数据
const loadChildrenData = async (row: any, treeNode: any, resolve: any) => {
  // 这里可以根据需要实现懒加载逻辑
  // 如果后端已经返回了完整的children数据，则直接resolve
  resolve(row.children || []);
};

// 交易趋势图表配置
const tradeChartOptions = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return null;

  return {
    chart: {
      type: chartType.value,
      toolbar: { show: false },
      zoom: { enabled: false },
    },
    dataLabels: { enabled: false },
    stroke: {
      curve: "smooth",
      width: chartType.value === "area" ? 2 : 3,
    },
    fill: {
      type: chartType.value === "area" ? "gradient" : "solid",
      gradient: {
        opacityFrom: 0.6,
        opacityTo: 0.1,
      },
    },
    xaxis: {
      categories: statisticsData.value.timeSeriesData.map((d: any) => d.date),
      labels: { style: { fontSize: "12px" } },
    },
    yaxis: {
      labels: { style: { fontSize: "12px" } },
    },
    colors: ["#0095FF"],
    tooltip: {
      y: {
        formatter: (val: number) => val.toString(),
      },
    },
  };
});

const tradeChartSeries = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return [];

  return [
    {
      name: t("sales.tradesCount"),
      data: statisticsData.value.timeSeriesData.map((d: any) => d.trades),
    },
  ];
});

// 资金流向图表配置
const fundFlowChartOptions = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return null;

  return {
    chart: {
      type: chartType.value,
      toolbar: { show: false },
      zoom: { enabled: false },
    },
    dataLabels: { enabled: false },
    stroke: {
      curve: "smooth",
      width: chartType.value === "area" ? 2 : 3,
    },
    fill: {
      type: chartType.value === "area" ? "gradient" : "solid",
      gradient: {
        opacityFrom: 0.6,
        opacityTo: 0.1,
      },
    },
    xaxis: {
      categories: statisticsData.value.timeSeriesData.map((d: any) => d.date),
      labels: { style: { fontSize: "12px" } },
    },
    yaxis: {
      labels: {
        style: { fontSize: "12px" },
        formatter: (val: number) => `$${(val / 100).toFixed(2)}`,
      },
    },
    colors: ["#50CD89", "#F1416C", "#FFC700"],
    tooltip: {
      y: {
        formatter: (val: number) => `$${(val / 100).toFixed(2)}`,
      },
    },
    legend: {
      position: "top",
      horizontalAlign: "right",
    },
  };
});

const fundFlowChartSeries = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return [];

  return [
    {
      name: t("sales.deposit"),
      data: statisticsData.value.timeSeriesData.map((d: any) => d.deposit),
    },
    {
      name: t("sales.withdrawal"),
      data: statisticsData.value.timeSeriesData.map((d: any) => d.withdrawal),
    },
    {
      name: t("sales.netDeposit"),
      data: statisticsData.value.timeSeriesData.map((d: any) => d.netDeposit),
    },
  ];
});

// 返佣趋势图表配置
const rebateChartOptions = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return null;

  return {
    chart: {
      type: chartType.value,
      toolbar: { show: false },
      zoom: { enabled: false },
    },
    dataLabels: { enabled: false },
    stroke: {
      curve: "smooth",
      width: chartType.value === "area" ? 2 : 3,
    },
    fill: {
      type: chartType.value === "area" ? "gradient" : "solid",
      gradient: {
        opacityFrom: 0.6,
        opacityTo: 0.1,
      },
    },
    xaxis: {
      categories: statisticsData.value.timeSeriesData.map((d: any) => d.date),
      labels: { style: { fontSize: "12px" } },
    },
    yaxis: {
      labels: {
        style: { fontSize: "12px" },
        formatter: (val: number) => `$${(val / 100).toFixed(2)}`,
      },
    },
    colors: ["#7239EA"],
    tooltip: {
      y: {
        formatter: (val: number) => `$${(val / 100).toFixed(2)}`,
      },
    },
  };
});

const rebateChartSeries = computed(() => {
  if (!statisticsData.value?.timeSeriesData) return [];

  return [
    {
      name: t("sales.rebate"),
      data: statisticsData.value.timeSeriesData.map((d: any) => d.rebate),
    },
  ];
});

// 产品分布图表配置
const productChartOptions = computed(() => {
  if (!statisticsData.value?.productDistribution) return null;

  return {
    chart: {
      type: "pie",
    },
    labels: statisticsData.value.productDistribution.map((p: any) => p.symbol),
    colors: ["#0095FF", "#50CD89", "#FFC700", "#F1416C", "#7239EA"],
    legend: {
      position: "bottom",
    },
    tooltip: {
      y: {
        formatter: (val: number, opts: any) => {
          const percentage =
            statisticsData.value.productDistribution[opts.seriesIndex]
              ?.percentage || 0;
          return `${val} (${percentage.toFixed(1)}%)`;
        },
      },
    },
  };
});

const productChartSeries = computed(() => {
  if (!statisticsData.value?.productDistribution) return [];

  return statisticsData.value.productDistribution.map((p: any) => p.count);
});

// 初始化
onMounted(() => {
  searchCriteria.value.from = moment()
    .subtract(30, "days")
    .format("YYYY-MM-DD");
  searchCriteria.value.to = moment().format("YYYY-MM-DD");
  fetchData();
});
</script>

<style scoped lang="scss">
.card-flush {
  box-shadow: 0 0 20px 0 rgba(76, 87, 125, 0.02);
}

// 表格样式 - 模仿 /sales/customers 的表格样式
:deep(.sales-table) {
  // 去除默认边框
  &.el-table {
    border: none;

    &::before {
      display: none; // 移除底部边框线
    }

    &::after {
      display: none;
    }
  }

  // 表头样式
  .el-table__header-wrapper {
    .el-table__header {
      thead {
        tr {
          th {
            border-bottom: 1px solid #eff2f5;

            font-weight: 500;
            font-size: 0.95rem;
            padding: 0.75rem 0.75rem;

            .cell {
              color: #3a3e44;
              font-size: 16px;
              font-weight: normal;
            }
          }
        }
      }
    }
  }

  // 表体样式
  .el-table__body-wrapper {
    .el-table__body {
      tbody {
        tr {
          td {
            padding: 0.75rem 0.75rem;
            border-bottom: 1px solid #eff2f5 !important;
            vertical-align: middle;

            .cell {
              font-size: 15px;
            }
          }

          &:hover > td {
            background-color: #f5f8fa;
          }
        }
      }
    }
  }

  .el-table-fixed-column--right {
    background-color: none;
  }
}

@media (max-width: 768px) {
  :deep(.sales-table) {
    font-size: 12px !important;
  }
}

// 详情弹窗样式
.detail-item {
  padding: 16px;
  border-radius: 8px;
  background-color: #f5f8fa;

  .detail-label {
    font-size: 13px;
    color: #7e8299;
    margin-bottom: 8px;
    font-weight: 500;
  }

  .detail-value {
    font-size: 15px;
    color: #181c32;
    font-weight: 600;
  }
}
</style>
