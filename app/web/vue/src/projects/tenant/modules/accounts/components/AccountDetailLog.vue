<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.action"
          class="me-3"
          :placeholder="$t('action.action')"
        ></el-input>
        <el-button type="primary" @click="fetchData(1)">
          {{ $t("action.search") }}
        </el-button>
        <el-button @click="reset()">
          {{ $t("action.reset") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("action.action") }}</th>
            <th>{{ $t("fields.beforeEdit") }}</th>
            <th>{{ $t("fields.afterEdit") }}</th>
            <th>{{ $t("fields.operatedBy") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.action }}</td>
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  <el-tag type="danger">{{ item.before }}</el-tag>
                </div>
                <el-button
                  size="small"
                  class="ms-2"
                  v-if="
                    item.before.length >= 60 || typeof item.before === 'object'
                  "
                  @click="showData(item.before)"
                  >{{ $t("tip.viewMore") }}</el-button
                >
              </div>
            </td>
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  <el-tag type="success">{{ item.after }}</el-tag>
                </div>
                <el-button
                  size="small"
                  class="ms-2"
                  v-if="
                    item.after.length >= 60 || typeof item.after === 'object'
                  "
                  @click="showData(item.after)"
                  >{{ $t("tip.viewMore") }}</el-button
                >
              </div>
            </td>
            <td>{{ item.operatorName }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <el-dialog
    v-model="show"
    width="600"
    align-center
    style="max-height: 95vh; overflow: auto"
  >
    <div v-if="typeof dialogData != 'object'">{{ dialogData }}</div>
    <VueJsonView v-else :src="dialogData" theme="rjv-default" />
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="show = false">{{ $t("action.close") }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, onMounted, inject } from "vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import VueJsonView from "@matpool/vue-json-view";
import AccountInjectionKeys from "@/projects/tenant/modules/accounts/types/AccountInjectionKeys";
const data = ref(<any>[]);
const isLoading = ref(false);
const show = ref(false);
const dialogData = ref(<any>[]);

const accountDetails = inject(AccountInjectionKeys.ACCOUNT_DETAILS);
const criteria = ref({
  page: 1,
  pageSize: 20,
  accountId: accountDetails?.value.id,
  sortFeild: "createdOn",
  sortFlag: true,
});
const showData = (data: any) => {
  show.value = true;
  dialogData.value = data;
};
const reset = () => {
  criteria.value = {
    page: 1,
    pageSize: 20,
    accountId: accountDetails?.value.id,
    sortFeild: "createdOn",
    sortFlag: true,
  };
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountService.queryAccountLog(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};
onMounted(() => {
  fetchData(1);
});
</script>
<style scoped>
.width-control {
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
</style>
