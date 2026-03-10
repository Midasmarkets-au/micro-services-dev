<template>
  <div>
    <div class="card mb-5">
      <div class="card-header">
        <div class="card-title">Lead Systems</div>
        <div class="card-toolbar">
          <div class="">
            <ul
              class="nav nav-pills nav-pills-custom row position-relative mx-0"
            >
              <li class="nav-item col-4 mx-0 p-0">
                <a
                  class="nav-link active d-flex justify-content-center w-100 border-0 h-100"
                  data-bs-toggle="pill"
                  @click="currentTab = LeadSystemTypes.Created"
                  href="#lead-system-created"
                >
                  <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                    $t("status.created")
                  }}</span>
                  <span
                    class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
                  ></span>
                </a>
              </li>

              <li class="nav-item col-4 mx-0 p-0">
                <a
                  class="nav-link d-flex justify-content-center w-100 border-0 h-100"
                  data-bs-toggle="pill"
                  @click="currentTab = LeadSystemTypes.Assigned"
                  href="#lead-system-assigned"
                >
                  <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                    $t("status.assigned")
                  }}</span>
                  <span
                    class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
                  ></span>
                </a>
              </li>

              <li class="nav-item col-4 mx-0 p-0">
                <a
                  class="nav-link d-flex justify-content-center w-100 border-0 h-100"
                  data-bs-toggle="pill"
                  @click="currentTab = LeadSystemTypes.Contacted"
                  href="#lead-system-contacted"
                >
                  <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                    $t("status.contacted")
                  }}</span>
                  <span
                    class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
                  ></span>
                </a>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div class="card-body">
        <table
          class="table table-row-dashed fs-6 gy-5 my-0"
          id="kt_ecommerce_add_product_reviews"
        >
          <!--begin::table head-->
          <thead>
            <tr
              class="text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0"
            >
              <th class="text-start">{{ $t("fields.id") }}</th>
              <th class="text-end">{{ $t("fields.name") }}</th>
              <th class="text-end">{{ $t("title.email") }}</th>
              <th class="text-end">{{ $t("fields.phoneNum") }}</th>
              <th class="text-end">{{ $t("action.details") }}</th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && leads.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, idx) in leads" :key="idx">
              <td class="text-start">{{ item.id }}</td>
              <td class="text-end">{{ item.name }}</td>
              <td class="text-end">{{ item.email }}</td>
              <td class="text-end">{{ item.phoneNumber }}</td>

              <td class="text-end">
                <button
                  class="btn btn-success btn-sm"
                  @click="openAssignPanel(item.id)"
                >
                  {{ $t("action.assignSales") }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter @page-change="fetchData" :criteria="criteria" />
      </div>
    </div>

    <SimpleForm
      ref="assignSalesFormRef"
      :title="$t('title.assignSales')"
      :isLoading="isLoading"
      :submit="assignSales"
      :discard="assignSalesFormRef?.hide"
    >
      <div class="d-flex gap-1 align-items-center justify-content-center">
        <label class="fs-6 text fw-semibold me-2">
          {{ $t("title.salesAccountId") }}
        </label>
        <el-autocomplete
          v-model="salesAccountId"
          :fetch-suggestions="querySearch"
          clearable
          class=""
          :placeholder="$t('tip.pleaseInput')"
          @select="handleSelect"
        />
      </div>
      <!-- <div class="btn btn-primary">{{ assignFormData }}</div> -->
      <!-- <div class="btn btn-primary">{{ salesAccountId.length }}</div> -->
    </SimpleForm>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import UserService from "../services/UserService";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import { LeadSystemTypes } from "@/core/types/LeadSystems";
import SimpleForm from "@/components/SimpleForm.vue";
import AccountService from "../../accounts/services/AccountService";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import TableFooter from "@/components/TableFooter.vue";

const assignSalesFormRef = ref<InstanceType<typeof SimpleForm>>();
const isLoading = ref(true);
const leads = ref(Array<any>());
const criteria = ref({
  page: 1,
  size: 5,
  status: LeadSystemTypes.Created,
});

const salesAccountId = ref("");
const salesAccounts = ref(Array<any>());
const salesAccountIdList = ref(Array<any>());

const assignFormData = ref({
  salesAccountId: -1,
  leadIds: Array<any>(),
});

const currentTab = ref(LeadSystemTypes.Created);

watch(currentTab, () => {
  criteria.value.status = currentTab.value;
  fetchData(1);
});

const openAssignPanel = (leadId: number) => {
  salesAccountId.value = "";
  assignFormData.value = {
    salesAccountId: -1,
    leadIds: [leadId],
  };
  assignSalesFormRef.value?.show();
};

const assignSales = async () => {
  assignFormData.value.salesAccountId = parseInt(salesAccountId.value);
  isLoading.value = true;
  try {
    assignSalesFormRef.value?.hide();
    await UserService.assignLeads2SalesAccount(assignFormData.value);
    fetchData(1);
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

// const checkSymbol = (symbol: string)
const createFilter = (queryString: string) => (inputId: any) =>
  inputId.value.toLowerCase().indexOf(queryString.toLowerCase()) === 0;

const querySearch = (queryString: string, cb: any) => {
  const results = queryString
    ? salesAccountIdList.value.filter(createFilter(queryString))
    : salesAccountIdList.value;
  // call callback function to return suggestions
  cb(results);
};

const handleSelect = (item: any) => {
  console.log(item);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await UserService.queryLeads(criteria.value);
    leads.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

onMounted(async () => {
  isLoading.value = true;
  try {
    await fetchData(1);
    const accountRes = await AccountService.queryAccounts({
      role: AccountRoleTypes.Sales,
    });
    salesAccounts.value = accountRes.data;
    salesAccountIdList.value = salesAccounts.value.map((item: any) => ({
      value: item.id + "",
    }));
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
});
</script>

<style scoped></style>
