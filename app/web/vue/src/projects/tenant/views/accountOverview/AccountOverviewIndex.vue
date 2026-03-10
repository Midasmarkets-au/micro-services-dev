<template>
  <div class="card mb-2">
    <div class="card-header">
      <div class="card-title">
        <el-input v-model="criteria.searchText" @keyup.enter="fetchData()">
          <template #append>
            <el-button :icon="Search" @click="fetchData()" /> </template
        ></el-input>
        <el-button class="ms-5" @click="reset">{{
          $t("action.reset")
        }}</el-button>
      </div>
    </div>
    <div class="card-body py-2 px-8">
      <table class="table align-middle table-row-dashed fs-6 gy-1">
        <thead>
          <tr class="text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>{{ $t("fields.email") }}</td>
            <td>{{ $t("fields.accountNumber") }}</td>
            <td>UID</td>
            <td>{{ $t("action.action") }}</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.user.email }}</td>
            <td>{{ item.accountNumber }}</td>
            <td>{{ item.uid }}</td>
            <td>
              <el-button
                type="primary"
                size="small"
                @click="handleDetail(item)"
                >{{ $t("action.detail") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <AccountDetail v-if="showDetail" :account="selectedAccount" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import { Search } from "@element-plus/icons-vue";
import AccountOverviewService from "@/projects/tenant/views/accountOverview/services/AccountOverviewServices";
import AccountDetail from "@/projects/tenant/views/accountOverview/components/AccountDetail.vue";
const isLoading = ref(false);
const showDetail = ref(false);
const selectedAccount = ref({});
const data = ref<any>([]);
const criteria = ref({
  page: 1,
  size: 5,
  searchText: "",
});

const fetchData = async () => {
  isLoading.value = true;
  const res = await AccountOverviewService.queryAccounts(criteria.value);
  data.value = res.data;
  isLoading.value = false;
};

const handleDetail = async (item: any) => {
  showDetail.value = true;
  selectedAccount.value = item;
};

const reset = () => {
  criteria.value.searchText = "";
  showDetail.value = false;
  data.value = [];
};
</script>
