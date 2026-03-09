<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.partyId"
          style="max-width: 200px"
          clearable
          placeholder="Search PartyId"
          class="input-with-select me-4"
        >
          <template #append>
            <el-button
              :icon="Search"
              :disabled="isLoading"
              @click="fetchData(1)"
            />
          </template>
        </el-input>

        <el-input
          v-model="criteria.action"
          style="max-width: 200px"
          clearable
          placeholder="Search Action"
          class="input-with-select me-4"
        >
          <template #append>
            <el-button
              :icon="Search"
              :disabled="isLoading"
              @click="fetchData(1)"
            />
          </template>
        </el-input>

        <el-input
          v-model="criteria.ip"
          style="max-width: 200px"
          clearable
          placeholder="Search IP"
          class="input-with-select me-4"
        >
          <template #append>
            <el-button
              :icon="Search"
              :disabled="isLoading"
              @click="fetchData(1)"
            />
          </template>
        </el-input>

        <el-select
          v-model="criteria.method"
          style="max-width: 150px"
          clearable
          placeholder="Method"
          class="input-with-select me-4"
          @change="fetchData(1)"
        >
          <el-option
            v-for="item in methods"
            :key="item"
            :label="item"
            :value="item"
          />
        </el-select>

        <el-button
          type="success"
          @click="getDepositError()"
          :disabled="isLoading"
          >Show Deposit Error Log</el-button
        >
        <el-button @click="reset" :disabled="isLoading">Reset</el-button>
      </div>
      <div class="card-toolbar"></div>
    </div>

    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <!--            "id": 1511658,-->
            <!--            "statusCode": 200,-->
            <!--            "connectionId": "0HN6P0NQ9I0BI",-->
            <!--            "method": "POST",-->
            <!--            "action": "/api/v1/tenant/user/18828/god-mode",-->
            <th class="">id</th>
            <th class="">partyId</th>
            <th class="">statusCode</th>
            <th class="">connectionId</th>
            <th class="">method</th>
            <th class="">url</th>
            <th class="">ip</th>
            <th class="">action</th>
          </tr>
        </thead>

        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && items.length === 0">
          <NoDataBox />
        </tbody>

        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in items" :key="index">
            <td>{{ item.id }}</td>
            <td>{{ item.partyId }}({{ item.email || "null" }})</td>
            <td>{{ item.statusCode }}</td>
            <td>{{ item.connectionId }}</td>
            <td>{{ item.method }}</td>
            <td>{{ item.action }}</td>
            <td>{{ item.ip }}</td>
            <td>
              <TimeShow type="exactTime" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <TimeShow type="exactTime" :date-iso-string="item.updatedOn" />
            </td>
            <td>
              <button
                class="btn btn-sm btn-primary"
                @click="
                  () => {
                    dialogItem = item;
                    detailVisible = true;
                  }
                "
              >
                Details
              </button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />

      <el-dialog v-model="detailVisible" title="Detail" width="1000">
        <div class="d-flex gap-5">
          <span>{{ dialogItem.id }}</span>
          <span>{{ dialogItem.statusCode }}</span>
          <span>{{ dialogItem.connectionId }}</span>
          <span>{{ dialogItem.method }}</span>
          <span>{{ dialogItem.action }}</span>
          <span>{{ dialogItem.ip }}</span>
        </div>
        <el-divider></el-divider>
        <div class="text-primary">Param: {{ dialogItem.parameters }}</div>
        <div class="text-info">Request: {{ dialogItem.requestContent }}</div>
        <div class="text-danger">
          Response: {{ dialogItem.responseContent }}
        </div>
        <div></div>
      </el-dialog>

      <!-- <AccountCreate
        ref="accountCreateRef"
        @account-submitted="mt4AccountSubmitted"
      /> -->
    </div>
  </div>
</template>
<script setup lang="ts">
import TableFooter from "@/components/TableFooter.vue";
import { onMounted, ref } from "vue";
import TenantGlobalService from "@/projects/tenant/services/TenantGlobalService";
import { Search } from "@element-plus/icons-vue";
import TimeShow from "@/components/TimeShow.vue";

const detailVisible = ref(false);

const isLoading = ref(true);
const dialogItem = ref<any>({});
const items = ref(Array<any>());
const criteria = ref<any>({
  page: 1,
  size: 20,
});

const methods = ["GET", "POST", "PUT", "DELETE"];

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  if (criteria.value.method !== null && criteria.value.method !== undefined) {
    criteria.value.method = criteria.value.method.trim().toUpperCase();
  }
  isLoading.value = true;

  try {
    const res = await TenantGlobalService.queryApiLogs(criteria.value);
    items.value = res.data;
    criteria.value = res.criteria;
    isLoading.value = false;
  } catch (error) {
    console.log(error);
  }
};

const getDepositError = async () => {
  criteria.value = {
    page: 1,
    size: 20,
    method: "POST",
    action: "deposit",
    statusCode: 400,
  };
  await fetchData(1);
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 20,
  };
  await fetchData(1);
};

onMounted(async () => {
  await fetchData(1);
});
</script>
<style scoped></style>
