<template>
  <div class="card mb-xl-8">
    <div class="card-header">
      <div class="card-title">{{ $t("title.accounts") }}</div>
      <div class="card-toolbar">
        <!-- <el-input
          v-model="criteria.partyId"
          class="w-100 m-2"
          placeholder="Please Input Party ID"
          :suffix-icon="Search"
          @keyup.enter="fetchData(1)"
        /> -->
      </div>
    </div>
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-1 table-striped table-hover"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class=""></th>
            <th class="">{{ $t("fields.id") }}</th>
            <th class="">{{ $t("fields.distributionType") }}</th>
            <th class="">{{ $t("fields.salesAccountId") }}</th>
            <th class="">{{ $t("fields.updateAt") }}</th>
            <th class="">{{ $t("fields.created_at") }}</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && accounts.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in accounts" :key="index">
            <td>
              <el-dropdown trigger="click">
                <el-button type="primary" class="btn btn-secondary btn-sm">
                  {{ $t("action.action")
                  }}<el-icon class="el-icon--right"><arrow-down /></el-icon>
                </el-button>
                <template #dropdown>
                  <el-dropdown-menu>
                    <el-dropdown-item>
                      {{ $t("title.rebate") }}
                    </el-dropdown-item>
                  </el-dropdown-menu>
                </template>
              </el-dropdown>
            </td>
            <td>{{ item.clientAccount.uid }}</td>
            <td>{{ item.distributionType }}</td>
            <td>{{ item.clientAccount.salesAccountId }}</td>
            <td><TimeShow :date-iso-string="item.updatedOn" /></td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
          </tr>
        </tbody>
      </table>

      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, nextTick } from "vue";
import RebateService from "../services/RebateService";
import TimeShow from "@/components/TimeShow.vue";
import i18n from "@/core/plugins/i18n";
import TableFooter from "@/components/TableFooter.vue";
import { ArrowDown } from "@element-plus/icons-vue";

const { t } = i18n.global;

const isLoading = ref(true);
const accounts = ref(Array<any>());

const criteria = ref<any>({
  page: 1,
  size: 20,
  numPages: 1,
  total: 0,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const responseBody = await RebateService.getRebateRuleClient(
      criteria.value
    );
    criteria.value = responseBody.criteria;
    accounts.value = responseBody.data;
  } catch (error) {
    console.log(error);
  } finally {
    console.log("finally");
    isLoading.value = false;
  }
};

onMounted(async () => {
  isLoading.value = true;
  await fetchData(1);
});
</script>
