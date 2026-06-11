<template>
  <div class="d-flex flex-column flex-column-fluid">
    <div class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div
          class="card-title d-flex justify-content-between"
          style="width: 100%"
        >
          <div class="d-flex align-items-center">
            <el-input
              v-model="criteria.salesAccountUid"
              :placeholder="$t('rebate.salesRebateSchema.searchSalesUid')"
              @keyup.enter="fetchData(1)"
              :disabled="tab.pending === currentTab"
            >
            </el-input>

            <el-input
              class="ms-5"
              v-model="criteria.rebateAccountUid"
              :placeholder="$t('rebate.salesRebateSchema.searchTargetUid')"
              @keyup.enter="fetchData(1)"
              :disabled="tab.pending === currentTab"
            >
            </el-input>

            <el-select
              v-model="currentSalesType"
              :placeholder="$t('rebate.salesRebateSchema.salesType')"
              @change="onSalesTypeChange"
              :disabled="tab.pending === currentTab"
              class="ms-5"
              style="width: 220px; flex-shrink: 0"
            >
              <el-option
                :label="$t('rebate.salesRebateSchema.all')"
                :value="-1"
              />
              <el-option
                v-for="t in salesTypeOptions"
                :key="t.value"
                :label="$t('rebate.salesRebateSchema.type' + t.value)"
                :value="t.value"
              />
            </el-select>

            <el-button
              class="ms-5"
              @click="fetchData(1)"
              :disabled="tab.pending === currentTab"
              >{{ $t("rebate.salesRebateSchema.search") }}</el-button
            >
            <el-button
              class="ms-5"
              @click="reset"
              :disabled="tab.pending === currentTab"
              >{{ $t("rebate.salesRebateSchema.reset") }}</el-button
            >

            <div class="d-flex align-items-center ms-8">
              <el-switch v-model="showPending" @change="onPendingToggle" />
              <span class="ms-2 fs-6 text-nowrap">{{
                $t("rebate.salesRebateSchema.waitingApprove")
              }}</span>
            </div>
          </div>

          <div class="d-flex">
            <el-button class="ms-5" @click="showAddNewModal"
              >{{ $t("rebate.salesRebateSchema.addRule") }}</el-button
            >
          </div>
        </div>
      </div>
      <div v-if="data.length == 0 && isLoading">
        <LoadingCentralBox />
      </div>

      <div class="card-body row">
        <div class="mb-5 d-flex justify-content-center">
          <i
            v-if="hasErrorSetting"
            class="fa-solid fa-triangle-exclamation fa-fade"
            style="color: red; font-size: 48px"
          ></i>
        </div>

        <div class="col-6 mb-3" v-for="items in data" :key="items">
          <div
            class="px-5"
            style="
              border-radius: 10px;
              box-shadow: rgba(0, 0, 0, 0.24) 0px 3px 8px;
            "
          >
            <SalesRebateSchemaCard
              :items="items"
              :isLoading="isLoading"
              @refresh="refresh"
              @hasError="hasError"
            ></SalesRebateSchemaCard>
          </div>
        </div>
        <!-- <TableFooter @page-change="pageChange" :criteria="criteria" /> -->
      </div>
    </div>
    <AddSchama ref="AddSchamaRef" @refresh="refresh" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import RebateService from "../services/RebateService";
import SalesRebateSchemaCard from "../components/SalesRebateSchemaCard.vue";
import AddSchama from "../components/AddSalesRebateSchema.vue";
import { salesTypeOptions } from "@/core/types/SalesTypes";
import LoadingCentralBox from "@/components/LoadingCentralBox.vue";

const isLoading = ref(true);
const AddSchamaRef = ref<any>(null);
const data = ref({} as any);
const originalActiveData = ref({} as any);
const hasErrorSetting = ref(false);
const tab = ref({
  active: 0,
  pending: -1,
});

const currentTab = ref(tab.value.active);
const currentSalesType = ref(-1);
const showPending = ref(false);

const criteria = ref({
  page: 1,
  size: 999,
  salesAccountUid: "",
  rebateAccountUid: "",
  Status: 0,
  sortField: "SalesAccountId",
});

const reset = () => {
  currentSalesType.value = -1;
  showPending.value = false;
  currentTab.value = tab.value.active;
  criteria.value.page = 1;
  criteria.value.size = 999;
  criteria.value.salesAccountUid = "";
  criteria.value.rebateAccountUid = "";
  criteria.value.sortField = "SalesAccountId";

  fetchData(1);
};

// Sales type dropdown filter (client-side on the active set).
const onSalesTypeChange = () => {
  if (currentSalesType.value === -1) {
    data.value = originalActiveData.value;
  } else {
    changeSalesType(currentSalesType.value);
  }
};

// Waiting-approve toggle: switches the status filter (active <-> pending).
const onPendingToggle = () => {
  currentSalesType.value = -1;
  currentTab.value = showPending.value ? tab.value.pending : tab.value.active;
  criteria.value.salesAccountUid = "";
  criteria.value.rebateAccountUid = "";
  fetchData(1);
};

onMounted(async () => {
  fetchData(1);
});

const refresh = () => {
  fetchData(1);
};

const hasError = () => {
  hasErrorSetting.value = true;
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  criteria.value.salesAccountIds = null;
  criteria.value.status = currentTab.value;
  hasErrorSetting.value = false;

  try {
    const res = await RebateService.querySalesRebateSchemas(criteria.value);
    criteria.value = res.criteria;
    originalActiveData.value = res.data;
    data.value = res.data;

    if (currentTab.value == tab.value.pending) {
      data.value.forEach((items: any, index: number) => {
        const uniqueRebateAccountUids = new Set();
        const filteredItems = new Set();

        items.forEach((item: any) => {
          if (item.status === -1) {
            uniqueRebateAccountUids.add(item.rebateAccountUid);
            filteredItems.add(item);
          }
        });

        items.forEach((item: any) => {
          if (
            item.status === 0 &&
            !uniqueRebateAccountUids.has(item.rebateAccountUid)
          ) {
            uniqueRebateAccountUids.add(item.rebateAccountUid);
            filteredItems.add(item);
          }
        });
        data.value[index] = Array.from(filteredItems);
      });
    }

    if (currentSalesType.value != -1) {
      changeSalesType(currentSalesType.value);
    }
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const changeSalesType = async (type: any) => {
  isLoading.value = true;
  currentSalesType.value = type;

  if (currentTab.value == tab.value.pending) {
    currentTab.value = tab.value.active;
    await fetchData(1);
  }

  data.value = originalActiveData.value.filter((innerArray) =>
    innerArray.some((item) => item.salesType === type)
  );
  isLoading.value = false;
};

const showAddNewModal = () => {
  AddSchamaRef.value.show();
};
</script>
