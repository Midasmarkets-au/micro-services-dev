<template>
  <div :class="isMobile ? 'pr-3 pt-3 pb-3' : ''">
    <!--begin::Menu 1-->
    <div
      v-if="!isMobile"
      v-show="showFilter"
      :class="props.fromPage == 'wallet' ? '' : 'd-flex py-5 overflow-auto '"
    >
      <div class="d-flex gap-5">
        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('closed')"
        >
          <el-tooltip
            :content="
              criteria.isClosed
                ? $t('tooltip.closedOrder')
                : $t('tooltip.openOrder')
            "
            placement="top"
          >
            <el-select
              v-model="criteria.isClosed"
              @change="onCloseTagChanged"
              :disabled="isLoading"
              class="w-125px"
            >
              <el-option :label="$t('title.openOrder')" :value="false" />
              <el-option :label="$t('title.closedOrder')" :value="true" />
            </el-select>
          </el-tooltip>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('size')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.pageSize") }}
          </label>
          <el-select
            v-model="criteria.size"
            class="w-75px"
            @change="fetchDataWithClearSumUp(1)"
            :disabled="isLoading"
          >
            <el-option
              v-for="item in pageSizes"
              :key="item"
              :label="item"
              :value="item"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('service')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("title.service") }}
          </label>
          <el-select
            v-model="criteria.serviceId"
            class="w-75px"
            :disabled="isLoading"
          >
            <el-option
              v-for="item in ConfigRealServiceSelections"
              :key="item.id"
              :label="item.label"
              :value="item.id"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('transactionType')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.transactionType") }}
          </label>
          <el-select
            v-model="criteria.ledgerSide"
            class="w-125px"
            :placeholder="$t('tip.all')"
            @change="trigger === 'change' && filterData(1)"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="(item, index) in transactionTypeSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('accountTransactionType')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.transactionType") }}
          </label>
          <el-select
            v-model="criteria.targetAccountType"
            class="w-125px"
            :placeholder="$t('tip.all')"
            @change="trigger === 'change' && filterData(1)"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="(item, index) in transactionTypeSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('transactionStatus')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.transactionStatus") }}
          </label>
          <el-select
            v-model="criteria.stateId"
            class="w-125px"
            @change="trigger === 'change' && filterData(1)"
            :placeholder="$t('tip.all')"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="(item, index) in transactionStatusSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('transferState')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.status") }}
          </label>
          <el-select
            v-model="tempStateIds"
            class="w-125px"
            @change="trigger === 'change' && filterData(1)"
            :disabled="isLoading"
          >
            <el-option
              v-for="(item, index) in simpleTransferSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('depositState')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.status") }}
          </label>
          <el-select
            v-model="tempStateIds"
            class="w-125px"
            @change="trigger === 'change' && filterData(1)"
            :disabled="isLoading"
          >
            <el-option
              v-for="(item, index) in simpleDepositSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            >
            </el-option>
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('withdrawalState')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.status") }}
          </label>
          <el-select
            v-model="tempStateIds"
            class="w-125px"
            @change="trigger === 'change' && filterData(1)"
            :placeholder="$t('tip.all')"
            :disabled="isLoading"
          >
            <el-option
              v-for="(item, index) in simpleWithdrawalSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('walletTransactionType')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.transactionType") }}
          </label>
          <el-select
            v-model="criteria.matterType"
            class="w-125px"
            @change="trigger === 'change' && filterData(1)"
            :placeholder="$t('tip.all')"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="(item, index) in walletTransactionTypesSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('walletTransactionStatus')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.transactionStatus") }}
          </label>
          <el-select
            v-model="walletTransactionStatus"
            class="w-125px"
            :placeholder="$t('tip.all')"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="(item, index) in walletTransactionStatusSelections"
              :key="index"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('period')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.period") }}
          </label>

          <BcrTimePeriodPicker
            v-model:from="from"
            v-model:to="to"
            :start-placeholder="$t('fields.startDate')"
            :end-placeholder="$t('fields.endDate')"
            :isLoading="isLoading"
          />
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('symbol')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.symbol") }}
          </label>
          <el-autocomplete
            v-model="criteria.symbol"
            :fetch-suggestions="querySearch"
            clearable
            class="w-100px"
            :placeholder="$t('tip.pleaseInput')"
            @keyup.enter="trigger === 'change' && filterData(1)"
            :disabled="isLoading"
          />
        </div>

        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('order')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.orderType") }}
          </label>
          <el-select
            v-model="selectedStock"
            class="w-150px"
            @change="onSelectedStockChange"
            :placeholder="$t('tip.all')"
            :disabled="isLoading"
          >
            <el-option :label="t('tip.all')" value="" />
            <el-option
              v-for="item in stockSelections"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            />
          </el-select>
        </div>
        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('accountUid')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.accountUid") }}
          </label>
          <el-input
            v-model="criteria.uid"
            class="w-125px"
            clearable
            @keyup.enter="filterData(1)"
            :disabled="isLoading"
          >
            <template #append v-if="trigger !== 'button'">
              <el-button :icon="Search as any" @click="filterData(1)" />
            </template>
          </el-input>
        </div>
        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('accountNumber')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.accountNo") }}
          </label>
          <el-input
            v-model="criteria.accountNumber"
            class="w-125px"
            clearable
            @keyup.enter="filterData(1)"
            :disabled="isLoading"
          >
            <template #append v-if="trigger !== 'button'">
              <el-button :icon="Search as any" @click="filterData(1)" />
            </template>
          </el-input>
        </div>
        <div
          class="d-flex align-items-center"
          v-if="filterOptions?.includes('target')"
        >
          <label class="filter-label me-2" style="">
            {{ $t("fields.accountNo") }}
          </label>
          <el-input
            v-model="criteria.target"
            class="w-125px"
            clearable
            @keyup.enter="filterData(1)"
            :disabled="isLoading"
          >
            <template #append v-if="trigger !== 'button'">
              <el-button :icon="Search as any" @click="filterData(1)" />
            </template>
          </el-input>
        </div>
      </div>
      <div
        v-if="trigger === 'button'"
        class="card-toolbar justify-content-center"
        :class="props.fromPage == 'wallet' ? 'my-8' : 'ms-1'"
        style="white-space: nowrap"
      >
        <!-- <button
          :class="`btn btn-${color} me-4 btn-sm`"
          :disabled="isLoading"
          @click="fetchDataWithClearSumUp(1)"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.search") }}
          </span>
        </button> -->
        <el-button
          :disabled="isLoading"
          size="large"
          @click="fetchDataWithClearSumUp(1)"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.search") }}
          </span>
        </el-button>
        <el-button @click="reset" size="large">{{
          $t("action.reset")
        }}</el-button>
        <el-button @click="handleAllHistory" size="large">{{
          $t("action.allHistory")
        }}</el-button>
        <!-- <button
          class="btn btn-light btn-sm"
          :disabled="isLoading"
          @click="reset()"
        >
          {{ $t("action.reset") }}
        </button> -->
      </div>
    </div>

    <div v-if="isMobile" class="row">
      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('closed')"
      >
        <el-tooltip
          :content="
            criteria.isClosed
              ? $t('tooltip.closedOrder')
              : $t('tooltip.openOrder')
          "
          placement="top"
        >
          <el-select
            v-model="criteria.isClosed"
            @change="onCloseTagChanged"
            class="w-125px"
          >
            <el-option :label="$t('title.openOrder')" :value="false" />
            <el-option :label="$t('title.closedOrder')" :value="true" />
          </el-select>
        </el-tooltip>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('symbol')"
      >
        <el-autocomplete
          class="w-150px"
          v-model="criteria.symbol"
          :fetch-suggestions="querySearch"
          clearable
          :placeholder="$t('fields.symbol')"
          @keyup.enter="trigger === 'change' && filterData(1)"
          :disabled="isLoading"
        />
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('service')"
      >
        <el-select
          v-model="criteria.serviceId"
          class="w-75px"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in ConfigRealServiceSelections"
            :key="item.id"
            :label="item.label"
            :value="item.id"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('walletTransactionType')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.type") }}
        </label>
        <el-select
          v-model="criteria.matterType"
          @change="trigger === 'change' && filterData(1)"
          :placeholder="$t('tip.all')"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="(item, index) in walletTransactionTypesSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('walletTransactionStatus')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.status") }}
        </label>
        <el-select
          v-model="walletTransactionStatus"
          :placeholder="$t('tip.all')"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="(item, index) in walletTransactionStatusSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('transactionType')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.transactionType") }}
        </label>
        <el-select
          v-model="criteria.ledgerSide"
          class="w-125px"
          :placeholder="$t('tip.all')"
          @change="trigger === 'change' && filterData(1)"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="(item, index) in transactionTypeSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('accountTransactionType')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.type") }}
        </label>
        <el-select
          v-model="criteria.targetAccountType"
          class="w-125px"
          :placeholder="$t('tip.all')"
          @change="trigger === 'change' && filterData(1)"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="(item, index) in transactionTypeSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('transactionStatus')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.status") }}
        </label>
        <el-select
          v-model="criteria.stateId"
          class="w-125px"
          @change="trigger === 'change' && filterData(1)"
          :placeholder="$t('tip.all')"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="(item, index) in transactionStatusSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('transferState')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.status") }}
        </label>
        <el-select
          v-model="tempStateIds"
          class="w-125px"
          @change="trigger === 'change' && filterData(1)"
          :disabled="isLoading"
        >
          <el-option
            v-for="(item, index) in simpleTransferSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('depositState')"
      >
        <label class="filter-label me-2" style="" v-if="!isMobile">
          {{ $t("fields.status") }}
        </label>
        <el-select
          v-model="tempStateIds"
          class="w-125px"
          @change="trigger === 'change' && filterData(1)"
          :disabled="isLoading"
        >
          <el-option
            v-for="(item, index) in simpleDepositSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('withdrawalState')"
      >
        <el-select
          v-model="tempStateIds"
          class="w-125px"
          @change="trigger === 'change' && filterData(1)"
          :placeholder="$t('tip.all')"
          :disabled="isLoading"
        >
          <el-option
            v-for="(item, index) in simpleWithdrawalSelections"
            :key="index"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('size')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.pageSize") }}
        </label>
        <el-select
          v-model="criteria.size"
          class="w-75px"
          @change="fetchDataWithClearSumUp(1)"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in pageSizes"
            :key="item"
            :label="item"
            :value="item"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('order')"
      >
        <label class="filter-label me-2" style="">
          {{ $t("fields.type") }}
        </label>
        <el-select
          v-model="selectedStock"
          class="w-100px"
          @change="onSelectedStockChange"
          :placeholder="$t('tip.all')"
          :disabled="isLoading"
        >
          <el-option :label="t('tip.all')" value="" />
          <el-option
            v-for="item in stockSelections"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </div>

      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('accountNumber')"
      >
        <el-input
          v-model="criteria.accountNumber"
          class="w-150px"
          :placeholder="t('fields.accountNo')"
          @keyup.enter="filterData(1)"
          :disabled="isLoading"
        >
          <template #append v-if="trigger !== 'button'">
            <el-button :icon="Search" @click="filterData(1)" />
          </template>
        </el-input>
      </div>
      <div
        class="col-6 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('target')"
      >
        <el-input
          v-model="criteria.target"
          class="w-150px"
          :placeholder="t('fields.accountNo')"
          @keyup.enter="filterData(1)"
          :disabled="isLoading"
        >
          <template #append v-if="trigger !== 'button'">
            <el-button :icon="Search" @click="filterData(1)" />
          </template>
        </el-input>
      </div>

      <div
        class="col-12 d-flex align-items-center mb-5"
        v-if="filterOptions?.includes('period')"
      >
        <BcrTimePeriodPicker
          v-model:from="from"
          v-model:to="to"
          :start-placeholder="$t('fields.startDate')"
          :end-placeholder="$t('fields.endDate')"
        />
      </div>

      <div
        v-if="trigger === 'button'"
        class="card-toolbar justify-content-center"
        :class="isMobile ? 'mb-2' : 'mb-8'"
      >
        <!-- <button
          :class="`btn btn-${color} me-4`"
          :disabled="isLoading"
          @click="fetchDataWithClearSumUp(1)"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.search") }}
          </span>
        </button>
        <button
          class="btn btn-light btn-bordered"
          :disabled="isLoading"
          @click="reset"
        >
          {{ $t("action.reset") }}
        </button> -->
        <el-button
          :disabled="isLoading"
          size="large"
          @click="fetchDataWithClearSumUp(1)"
        >
          <span v-if="isLoading">
            {{ $t("action.waiting") }}
            <span
              class="spinner-border h-15px w-15px align-middle text-gray-400"
            ></span>
          </span>
          <span v-else>
            {{ $t("action.search") }}
          </span>
        </el-button>
        <el-button @click="reset" size="large">{{
          $t("action.reset")
        }}</el-button>
        <el-button @click="handleAllHistory" size="large">{{
          $t("action.allHistory")
        }}</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch, nextTick } from "vue";
import {
  StockCmdType,
  StockPendingTypes,
  StockTransactionTypes,
} from "@/core/types/StockCmdTypes";

import { useI18n } from "vue-i18n";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { MatterTypes } from "@/core/types/MatterTypes";
import {
  CanceledStateTypes,
  ClientPendingStateTypes,
  CompletedStateTypes,
  CreatedStateTypes,
  DepositStateSelections,
  TransferStateSelections,
  WithdrawalStateSelections,
  simpleDepositSelections,
  simpleDepositToArray,
  simpleWithdrawalSelections,
  simpleWithdrawalToArray,
  simpleTransferSelections,
  simpleTransferToArray,
} from "@/core/types/StateInfos";
import moment from "moment";
import { isMobile } from "@/core/config/WindowConfig";
import { Search } from "@element-plus/icons-vue";
import IbService from "@/projects/client/modules/ib/services/IbService";
import ClientGlobalService from "@/projects/client/services/ClientGlobalService";
import BcrTimePeriodPicker from "@/components/BcrTimePeriodPicker.vue";
import { TimeZoneService } from "@/core/plugins/TimerService";
import { ConfigRealServiceSelections } from "@/core/types/ServiceTypes";
import { handleCriteriaTradeTime } from "@/core/helpers/helpers";
const props = withDefaults(
  defineProps<{
    fromPage?: string;
    showFilter?: boolean;
    defaultCriteria?: any;
    serviceHandler: (criteria: any) => Promise<any>;
    color?: string;
    filterOptions?: Array<string>;
    trigger?: string;
    type?: string;
  }>(),
  {
    showFilter: true,
    defaultCriteria: () => ({ page: 1, size: 10, symbol: "" }),
    color: "primary",
    filterOptions: () => [
      "closed",
      "size",
      "service",
      "period",
      "symbol",
      "order",
      "transactionType",
      "transactionStatus",
    ],
    trigger: "button",
    type: "",
  }
);

const { t } = useI18n();
const gmtTimeTypes = ref<any>([
  "trade",
  "withdraw",
  "deposit",
  "transfer",
  "rebate",
]);

const showFilter = ref(props.showFilter);
const filtered = ref(false); // flag to show whether the filter is applied
const isLoading = ref(true);
const pageSizes = [10, 15, 20, 25, 30, 50, 100];
const defaultTime = ref<[Date, Date]>([
  new Date(new Date().setHours(0, 0, 0, 0)),
  new Date(new Date().setHours(23, 59, 59, 999)),
]);
const symbolCodes = ref(Array<any>());
const symbols = ref(Array<any>());
const tempStateIds = ref<any>(null);
const selectedStock = ref<any>(null);
const onSelectedStockChange = async () => {
  if (props.trigger !== "change") return;
  criteria.value.cmds = {
    1: StockTransactionTypes,
    2: StockPendingTypes,
    3: [StockCmdType.BALANCE],
    4: [StockCmdType.CREDIT],
  }[selectedStock.value];
  filtered.value = true;
  await fetchDataWithClearSumUp(1);
};
const stockSelections = ref([
  { label: t("fields.transaction"), value: 1 },
  { label: t("fields.pendingStock"), value: 2 },
  { label: t("fields.balance"), value: 3 },
  { label: t("fields.credit"), value: 4 },
]);

const criteria = ref<any>({});

const from = ref<any>(null);
const to = ref<any>(null);

const transactionTypeSelections = ref([
  { label: t("fields.transactionTypeTransferIn"), value: 2 },
  { label: t("fields.transactionTypeTransferOut"), value: 1 },
]);

const transactionStatusSelections = ref([
  { value: 200, label: t("status.created") },
  { value: 205, label: t("status.cancelled") },
  { value: 210, label: t("status.pending") },
  { value: 215, label: t("status.rejected") },
  { value: 220, label: t("status.approved") },
  { value: 250, label: t("status.completed") },
]);

const walletTransactionTypesSelections = ref([
  { value: MatterTypes.Deposit, label: t("fields.transactionTypeDeposit") },
  { value: MatterTypes.Withdrawal, label: t("fields.transactionTypeWithdraw") },
  {
    value: MatterTypes.InternalTransfer,
    label: t("fields.transactionTypeTransfer"),
  },
  { value: MatterTypes.Rebate, label: t("fields.transactionTypeRebate") },
]);

// since the status key in criteria is an array, using it in v-model will cause display error(display the number)
// set a new variable and watch it to update the criteria.status

const enum ClientTransactionStatusType {
  Created = 0,
  Pending = 1,
  Completed = 2,
  Canceled = 3,
}

const walletTransactionStatus = ref<any>();

watch(walletTransactionStatus, (newStatus, oldStatus) => {
  if (oldStatus === "" && newStatus === null) return;
  criteria.value.stateIds = {
    [ClientTransactionStatusType.Created]: CreatedStateTypes,
    [ClientTransactionStatusType.Pending]: ClientPendingStateTypes, // concat two backend arrays
    [ClientTransactionStatusType.Completed]: CompletedStateTypes,
    [ClientTransactionStatusType.Canceled]: CanceledStateTypes,
  }[newStatus];
  if (props.trigger === "change") {
    criteria.value.stateIds ? filterData(1) : fetchDataWithClearSumUp(1);
  }
  if (walletTransactionStatus.value === "") {
    walletTransactionStatus.value = null;
  }
});

const walletTransactionStatusSelections = computed(() => [
  { value: ClientTransactionStatusType.Created, label: t("status.created") },
  {
    value: ClientTransactionStatusType.Pending,
    label: t("status.pending"),
  },
  {
    value: ClientTransactionStatusType.Completed,
    label: t("status.completed"),
  },
  { value: ClientTransactionStatusType.Canceled, label: t("status.cancelled") },
]);

const initCriteria = async (myCriteria?: any) => {
  filtered.value = false;

  if (myCriteria) {
    criteria.value = myCriteria;
    await initTime();
  } else {
    criteria.value = {
      ...props.defaultCriteria,
      isClosed: props.defaultCriteria.isClosed ?? false,
    };

    if (
      props.defaultCriteria.from != null &&
      props.defaultCriteria.to != null
    ) {
      await initTime();
    } else {
      from.value = null;
      to.value = null;
    }
  }

  await processStateIds();

  if (selectedStock.value) {
    selectedStock.value = null;
  }
  walletTransactionStatus.value = undefined;
};

const reset = async () => {
  await initCriteria();
  await fetchDataWithClearSumUp(1);
};

const handleAllHistory = async () => {
  from.value = "2025-10-01";
  await fetchDataWithClearSumUp(1);
};

const data = ref(Array<any>());

const querySearch = (queryString: string, cb: any) => {
  const results = queryString
    ? symbolCodes.value.filter(createFilter(queryString))
    : symbolCodes.value;
  cb(results);
};

// const checkSymbol = (symbol: string)
const createFilter = (queryString: string) => (symbol: any) =>
  symbol.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0;

const fetchDataWithClearSumUp = async (_page: number) => {
  criteria.value = {
    ...criteria.value,
    total: null,
    totalVolume: null,
    totalProfit: null,
    totalCommission: null,
    totalSwap: null,
  };
  await fetchData(_page);
};

const fetchData = async (selectedPage: number) => {
  isLoading.value = true;
  criteria.value.page = selectedPage;
  handleCriteriaTradeTime([from.value, to.value], criteria, false);
  try {
    const res = await props.serviceHandler(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
};
const fetchSymbol = async () => {
  if (props.filterOptions.includes("symbol")) {
    const { data } = await ClientGlobalService.getAllSymbols();
    symbols.value = data;
    symbolCodes.value = data
      .map((item: any) => ({ value: item.code }))
      .filter((item: any) => item.value !== "UNKNOWN");
  }
};

const filterData = (_page: number) => {
  filtered.value = true;
  fetchDataWithClearSumUp(_page);
};

onMounted(async () => {
  isLoading.value = true;
  try {
    await fetchSymbol();
    await initCriteria();
    await fetchDataWithClearSumUp(1);
  } catch (error) {
    MsgPrompt.error(error);
  }
  isLoading.value = false;
});

const onCloseTagChanged = async () => {
  criteria.value = {
    ...criteria.value,
    isClosed: criteria.value.isClosed,
  };
  await updateCriteriaTime();
  await fetchDataWithClearSumUp(1);
};

const updateCriteriaTime = async () => {
  if (criteria.value.isClosed == false) {
    from.value = null;
    to.value = null;
  } else {
    await initTime();
  }
};
const initTime = async () => {
  from.value = null;
  to.value = null;
  await nextTick();
  from.value = moment(defaultTime.value[0]).format("YYYY-MM-DD");
  to.value = moment(defaultTime.value[0]).format("YYYY-MM-DD");
};

const processStateIds = async () => {
  if (props.filterOptions.includes("depositState")) {
    tempStateIds.value = simpleDepositSelections[1].value;
    criteria.value.stateIds = simpleDepositToArray.filter(
      (item) => item.id === tempStateIds.value
    )[0].value;
  } else if (props.filterOptions.includes("withdrawalState")) {
    tempStateIds.value = simpleWithdrawalSelections[1].value;
    criteria.value.stateIds = simpleWithdrawalToArray.filter(
      (item) => item.id === tempStateIds.value
    )[0].value;
  } else if (props.filterOptions.includes("transferState")) {
    tempStateIds.value = simpleTransferSelections[1].value;
    criteria.value.stateIds = simpleTransferToArray.filter(
      (item) => item.id === tempStateIds.value
    )[0].value;
  }
};

watch(
  () => criteria.value.symbol,
  (val) => (criteria.value.symbol = val ?? "")
);

watch(
  () => tempStateIds.value,
  (val) => {
    if (val != null) {
      var data = [];
      if (props.filterOptions.includes("depositState")) {
        data = simpleDepositToArray.filter((item) => item.id === val);
      } else if (props.filterOptions.includes("withdrawalState")) {
        data = simpleWithdrawalToArray.filter((item) => item.id === val);
      } else if (props.filterOptions.includes("transferState")) {
        data = simpleTransferToArray.filter((item) => item.id === val);
      }
      delete criteria.value.stateIds;
      delete criteria.value.StateIds;
      criteria.value.StateIds = data[0].value;
    } else {
      tempStateIds.value = null;
      delete criteria.value.stateIds;
      delete criteria.value.StateIds;
    }
  }
);

// watch(
//   () => from.value,
//   (newVal) => {
//     if (newVal == null) {
//       criteria.value.from = null;
//       return;
//     }
//     if (gmtTimeTypes.value.includes(props.type)) {
//       criteria.value.from =
//         moment(newVal).subtract(1, "days").format("YYYY-MM-DD") +
//         "T21:59:59.000Z";
//     } else {
//       criteria.value.from =
//         moment(newVal).format("YYYY-MM-DD") + "T00:00:00.000Z";
//     }
//   }
// );

// watch(
//   () => to.value,
//   (newVal) => {
//     if (newVal == null) {
//       criteria.value.to = null;
//       return;
//     }
//     if (gmtTimeTypes.value.includes(props.type)) {
//       criteria.value.to =
//         moment(newVal).format("YYYY-MM-DD") + "T21:59:59.000Z";
//     } else {
//       criteria.value.to =
//         moment(newVal).format("YYYY-MM-DD") + "T23:59:59.999Z";
//     }
//   }
// );

//Math.round((num + Number.EPSILON) * 100) / 100
defineExpose({
  show: () => (showFilter.value = !showFilter.value),
  getData: () => data.value,
  data,
  fetchData,
  initCriteria,
  criteria,
  isLoading,
  filterData,
  total: computed<number>(() => criteria.value.total),
  filtered,
});
</script>

<style scoped lang="scss">
.filter-label {
  font-size: 14px;
  color: #000000;
  white-space: nowrap;
}
</style>
