<template>
  <div class="d-flex flex-column flex-column-fluid">
    <ul
      class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
    >
      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: currentTab === tab.active },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="
            (currentTab = tab.active), (currentSalesType = -1), fetchData(1)
          "
          >ALL</a
        >
      </li>
      <li
        v-for="(types, index) in salesTypeOptions"
        :key="index"
        class="nav-item"
      >
        <a
          class="nav-link text-active-primary pb-4"
          :class="[
            { active: currentSalesType === types.value },
            { 'disabled opacity-50 pe-none': isLoading },
          ]"
          data-bs-toggle="tab"
          href="#"
          @click="changeSalesType(types.value)"
          >{{ types.tab }}</a
        >
      </li>

      <li class="nav-item">
        <a
          class="nav-link text-active-primary pb-4"
          data-bs-toggle="tab"
          href="#"
          @click="waitingApprove"
          >Waiting Approve</a
        >
      </li>
    </ul>
    <div class="card mb-5 mb-xl-8">
      <div class="card-header">
        <div
          class="card-title d-flex justify-content-between"
          style="width: 100%"
        >
          <div class="d-flex">
            <el-input
              v-model="criteria.salesAccountUid"
              placeholder="Search Sales UID"
              @keyup.enter="fetchData(1)"
              :disabled="tab.pending === currentTab"
            >
            </el-input>

            <el-input
              class="ms-5"
              v-model="criteria.rebateAccountUid"
              placeholder="Search Target UID"
              @keyup.enter="fetchData(1)"
              :disabled="tab.pending === currentTab"
            >
            </el-input>

            <el-button
              class="ms-5"
              @click="fetchData(1)"
              :disabled="tab.pending === currentTab"
              >Search</el-button
            >
            <el-button
              class="ms-5"
              @click="reset"
              :disabled="tab.pending === currentTab"
              >Reset</el-button
            >
          </div>

          <div class="d-flex">
            <el-button class="ms-5" @click="showAddNewModal"
              >Add Rule</el-button
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

const criteria = ref({
  page: 1,
  size: 999,
  salesAccountUid: "",
  rebateAccountUid: "",
  Status: 0,
  sortField: "SalesAccountId",
});

const reset = () => {
  if (currentSalesType.value != -1) {
    currentTab.value = tab.value.active;
  }
  criteria.value.page = 1;
  criteria.value.size = 999;
  criteria.value.salesAccountUid = "";
  criteria.value.rebateAccountUid = "";
  criteria.value.sortField = "SalesAccountId";

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

const waitingApprove = () => {
  currentSalesType.value = -1;
  currentTab.value = tab.value.pending;
  reset();
};

const showAddNewModal = () => {
  AddSchamaRef.value.show();
};
</script>
