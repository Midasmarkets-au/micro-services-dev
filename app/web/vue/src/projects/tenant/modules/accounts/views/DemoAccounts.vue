<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <!-- <el-input
          v-model="criteria.partyId"
          placeholder="Party Id"
          clearable
          :disabled="isLoading"
          class="me-3"
        /> -->
        <el-input
          v-model="criteria.accountNumber"
          :placeholder="$t('fields.accountNo')"
          clearable
          :disabled="isLoading"
          class="me-3"
        />
        <el-select
          v-model="criteria.serviceId"
          class="me-3"
          clearable
          :placeholder="$t('fields.mtPlatform')"
        >
          <el-option
            v-for="item in demoPlatformSelections"
            :key="item.platform"
            :label="item.label"
            :value="item.platform"
          />
        </el-select>
        <el-button type="success" @click="fetchData(1)" :disabled="isLoading">
          {{ $t("action.search") }}
        </el-button>
        <el-button @click="reset()" :disabled="isLoading">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.user") }}</th>
            <th>{{ $t("fields.accountNo") }}</th>
            <th>{{ $t("fields.accountType") }}</th>
            <th>{{ $t("fields.platform") }}</th>
            <th>{{ $t("fields.currency") }}</th>
            <th>{{ $t("fields.balance") }}</th>
            <th>{{ $t("fields.leverage") }}</th>
            <th>{{ $t("fields.expireDate") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("fields.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="item in data" :key="item.id">
            <td>
              <UserInfo
                :email="item.email"
                :name="item.name ? item.name : 'N/A'"
                :partyId="item.partyId"
                class="me-2"
              />
            </td>
            <td>{{ item.accountNumber }}</td>
            <td>{{ $t("type.account." + item.type) }}</td>
            <td>{{ ServiceToPlatform[item.serviceId] }}</td>
            <td>{{ $t("type.currency." + item.currencyId) }}</td>
            <td>
              <BalanceShow
                :balance="item.balance * 100"
                :currencyId="item.currencyId"
              />
            </td>
            <td>{{ item.leverage }}:1</td>
            <td>
              <TimeShow
                :date-iso-string="item.expireOn"
                :class="[
                  item.expireOn < currentTime ? 'text-danger' : 'text-success',
                ]"
              />
            </td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <el-button
                type="danger"
                size="small"
                @click="deleteDemoAccount(item)"
                :disabled="isLoading"
              >
                {{ $t("action.delete") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import AccountService from "../services/AccountService";
import {
  ServiceToPlatform,
  demoPlatformSelections,
} from "@/core/types/ServiceTypes";
import BalanceShow from "@/components/BalanceShow.vue";
import { ElNotification, ElMessageBox } from "element-plus";
const isLoading = ref(false);
const data = ref(<any>[]);
const currentTime = new Date().toISOString();
const criteria = ref<any>({
  page: 1,
  pageSize: 20,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountService.queryDemoAccounts(criteria.value);
    criteria.value = res.criteria;
    data.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const reset = () => {
  criteria.value = {
    page: 1,
    pageSize: 20,
  };
  fetchData(1);
};

const deleteDemoAccount = async (item: any) => {
  try {
    ElMessageBox.confirm(
      "Are you sure to delete this account?",
      " Account Number: " + item.accountNumber,
      {
        confirmButtonText: "OK",
        cancelButtonText: "Cancel",
        type: "warning",
      }
    ).then(async () => {
      await AccountService.deleteDemoAccount(item.id);
      fetchData(1);
      ElNotification({
        title: "Success",
        message: "Account deleted successfully",
        type: "success",
      });
    });
  } catch (error) {
    console.log(error);
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
