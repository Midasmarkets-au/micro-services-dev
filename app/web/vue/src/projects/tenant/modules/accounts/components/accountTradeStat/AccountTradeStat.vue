<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="fs-4 me-4">
          {{ $t("title.tradeStat") }}
        </div>
        <div v-if="!props.fromBrirfDetail" class="me-4">
          {{ $t("fields.accountNo") + ": " + accountDetail.accountNumber }}
        </div>

        <el-input
          v-if="props.fromBrirfDetail"
          class="w-250px me-3"
          v-model="accountNumbers"
          :clearable="true"
          type="textarea"
          placeholder="Account Number"
        >
        </el-input>

        <el-date-picker
          v-model="period"
          type="datetimerange"
          start-placeholder="Start date"
          end-placeholder="End date"
          range-separator="-"
          value-format="YYYY-MM-DD HH:mm:ss"
          :disabled="isLoading"
        />

        <el-button
          type="success"
          plain
          @click="fetchData"
          :loading="isLoading"
          class="ms-4"
        >
          {{ $t("action.search") }}
        </el-button>
        <el-button
          type="primary"
          v-if="isSearched"
          plain
          @click="copyText()"
          :disabled="isLoading"
          class="ms-4"
        >
          {{ $t("action.copy") }}
        </el-button>
      </div>
    </div>

    <div v-if="isSearched">
      <div
        v-if="isLoading"
        style="height: 300px"
        class="d-flex align-items-center justify-content-center"
      >
        <scale-loader :color="'#ffc730'"></scale-loader>
      </div>
      <div class="card-body" v-else>
        <div class="card">
          <div class="card-body copy-area">
            <div class="fs-4 mb-4">
              {{
                accountNumbers
                  .split(",")
                  .map((item) => item.trim())
                  .filter((item) => item !== "")
                  .join(", ")
              }}
            </div>
            <div v-for="item in data.openTradeStats" :key="item">
              <div class="d-flex fs-4">
                <div class="me-4">New {{ item.cmd == 0 ? "BUY" : "SELL" }}</div>
                <div class="me-4">{{ item.volume + " LOT" }}</div>
                <div class="me-4">{{ item.symbol }}</div>
                <div class="me-4">
                  {{ "@ AVG: " + item.averagePrice }}
                </div>
              </div>
            </div>
            <div v-for="item in data.closedTradeStats" :key="item">
              <div class="d-flex fs-4">
                <div class="me-4">
                  CLOSED {{ item.cmd == 0 ? "BUY" : "SELL" }}
                </div>
                <div class="me-4">{{ item.volume + " LOT" }}</div>
                <div class="me-4">{{ item.symbol }}</div>
                <div class="me-4">
                  {{ "@ AVG: " + item.averagePrice }}
                </div>
                <div class="me-4">{{ "PL: " + currencyShow(item.profit) }}</div>
              </div>
            </div>

            <div class="fs-3 mt-2">Current Positions:</div>
            <div v-for="item in data.openTrades" :key="item">
              <div class="d-flex fs-4">
                <div class="me-4">
                  {{ item.cmd == 0 ? "LONG" : "SHORT" }}
                </div>
                <div class="me-4">{{ item.volume + " LOT" }}</div>
                <div class="me-4">{{ item.symbol }}</div>
                <div class="me-4">
                  {{ "@ AVG: " + item.averagePrice }}
                </div>
              </div>
            </div>

            <div class="d-flex gap-4">
              <div class="fs-3 mt-2">
                Equity: {{ currencyShow(data.equity) }}
              </div>
            </div>

            <div class="fs-3 mt-2">Current Price ( bid / ask ):</div>
            <div v-for="item in data.openTradeCurrentPrices" :key="item">
              <div class="d-flex fs-4">
                <div class="me-4">{{ item.symbol }}:</div>
                <div class="me-4">{{ item.bid }} / {{ item.ask }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- OPEN LONG AND SHORT -->
        <div class="row rol-cols-2 gap-5 mt-5 px-2">
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">OPEN LONG</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.openTradeStats.filter(
                    (item) => item.cmd == 0
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">{{ item.averagePrice }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">OPEN SHORT</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.openTradeStats.filter(
                    (item) => item.cmd == 1
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">{{ item.averagePrice }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- CLOSED LONG AND SHORT -->
        <div class="row rol-cols-2 gap-5 mt-5 px-2">
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">CLOSED LONG</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.closedTradeStats.filter(
                    (item) => item.cmd == 0
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">{{ item.averagePrice }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">PROFIT</div>
                      <div class="fs-4">{{ item.profit }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">CLOSED SHORT</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.closedTradeStats.filter(
                    (item) => item.cmd == 1
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">{{ item.averagePrice }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- CURRENT LONG AND SHORT -->
        <div class="row rol-cols-2 gap-5 mt-5 px-2">
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">CURRENT LONG</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.openTrades.filter(
                    (item) => item.cmd == 0
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">
                        {{ (item.openPrice + item.closePrice) / 2 }}
                      </div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">SL</div>
                      <div class="fs-4">{{ item.sl }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">TP</div>
                      <div class="fs-4">{{ item.tp }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col card">
            <div class="card-body">
              <div class="fs-1 fw-bold mb-4">CURRENT SHORT</div>
              <div class="row row-cols-2">
                <div
                  v-for="item in data.openTrades.filter(
                    (item) => item.cmd == 1
                  )"
                  :key="item"
                  class="col mb-2"
                >
                  <div class="fs-2 fw-bold mb-2" style="color: #5cb85c">
                    {{ item.symbol }}
                  </div>
                  <div class="d-flex gap-6">
                    <div class="text-center">
                      <div class="fs-4 fw-bold">AVG</div>
                      <div class="fs-4">
                        {{ (item.openPrice + item.closePrice) / 2 }}
                      </div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">LOT</div>
                      <div class="fs-4">{{ item.volume }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">SL</div>
                      <div class="fs-4">{{ item.sl }}</div>
                    </div>
                    <div class="text-center">
                      <div class="fs-4 fw-bold">TP</div>
                      <div class="fs-4">{{ item.tp }}</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="card mt-5">
          <div class="card-body">
            <div class="fs-1 fw-bold">Summary</div>
            <div class="row row-cols-2">
              <div class="col">
                <div class="d-flex gap-10 mt-4">
                  <div class="fs-2">
                    Equity:
                    <span style="color: #5cb85c">{{ data.equity }}</span>
                  </div>
                  <div class="fs-2">
                    Balance:
                    <span style="color: #5cb85c">{{ data.balance }}</span>
                  </div>
                </div>
                <div class="fs-2">
                  <div>
                    Timestamp:
                    <span style="color: #5cb85c">{{ period[1] }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, inject, computed } from "vue";
import AccountService from "../../services/AccountService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import { ElMessage } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
import filters from "@/core/helpers/filters";

const props = defineProps<{
  accountId: any;
  fromBrirfDetail?: boolean;
}>();

const isLoading = ref(false);
const data = ref<any>({});
const today = new Date();
const isSearched = ref(false);
const accountNumbers = ref("");
const accountDetail = props.fromBrirfDetail
  ? {}
  : inject(AccountInjectionKeys.ACCOUNT_DETAILS);

const currencyShow = (_balance: any) => {
  return new Intl.NumberFormat().format(_balance);
};

const formatDate = (date: Date): string => {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  const seconds = String(date.getSeconds()).padStart(2, "0");

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
};

const period = ref([
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 0, 0, 0)
  ),
  formatDate(
    new Date(today.getFullYear(), today.getMonth(), today.getDate(), 23, 59, 59)
  ),
]);

const handleDateChange = () => {
  // Ensure that period.value[1] is set to 23:59:59
  const endDate = new Date(period.value[1]);
  endDate.setHours(23, 59, 59, 999);
  period.value[1] = formatDate(endDate);
};

const fetchData = async () => {
  isSearched.value = true;
  isLoading.value = true;
  handleDateChange();

  try {
    if (props.fromBrirfDetail) {
      const res = await AccountService.queryTradeStatByAccountNumbers({
        accountNumbers: accountNumbers.value
          .split(",")
          .map((item) => item.trim())
          .filter((item) => item !== ""),
        from: period.value[0],
        to: period.value[1],
      });
      data.value = res;
    } else {
      const res = await AccountService.queryTradeStatByAccountNumber(
        props.accountId,
        {
          from: period.value[0],
          to: period.value[1],
        }
      );
      data.value = res;
    }
  } catch (error) {
    console.error(error);
    MsgPrompt.error(error);
    isSearched.value = false;
  }
  isLoading.value = false;
};

const copyText = () => {
  let textToCopy = `${accountNumbers.value
    .split(",")
    .map((item) => item.trim())
    .filter((item) => item !== "")}\n\n`;
  // Add openTradeStats

  data.value.openTradeStats.forEach((item) => {
    textToCopy += `New ${item.cmd == 0 ? "BUY" : "SELL"} ${item.volume} LOT ${
      item.symbol
    } @ AVG: ${item.averagePrice}\n`;
  });
  // Add closedTradeStats
  data.value.closedTradeStats.forEach((item) => {
    textToCopy += `CLOSED ${item.cmd == 0 ? "BUY" : "SELL"} ${
      item.volume
    } LOT ${item.symbol} @ AVG: ${currencyShow(
      item.averagePrice
    )} PL: ${currencyShow(item.profit)}\n`;
  });

  // Add openTrades
  textToCopy += "\nCurrent Positions:\n";
  console.log(data.value.openTrades);

  data.value.openTrades.forEach((item) => {
    // const avgPrice = (item.openPrice + item.closePrice) / 2;
    textToCopy += `${item.cmd == 0 ? "LONG" : "SHORT"} ${item.volume} LOT ${
      item.symbol
    } @ AVG: ${item.averagePrice}\n`;
  });

  // Add equity and balance
  textToCopy += `\nEquity: ${currencyShow(data.value.equity)}\n`;

  // Add openTradeCurrentPrices
  textToCopy += "\nCurrent Price ( bid / ask ):\n";
  data.value.openTradeCurrentPrices.forEach((item) => {
    textToCopy += `${item.symbol}: ${item.bid} / ${currencyShow(item.ask)}\n`;
  });

  // Copy to clipboard
  navigator.clipboard
    .writeText(textToCopy)
    .then(() => {
      ElMessage.success("Copied to clipboard");
    })
    .catch((err) => {
      console.error("Failed to copy text: ", err);
    });
};
</script>
