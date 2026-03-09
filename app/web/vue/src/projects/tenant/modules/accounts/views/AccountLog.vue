<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-input
          v-model="criteria.action"
          class="me-3"
          :placeholder="t('action.action')"
        ></el-input>
        <el-input
          v-model="criteria.searchText"
          class="me-3"
          :placeholder="t('fields.accountId')"
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
            <th>{{ $t("fields.account") }}</th>
            <th>{{ $t("fields.before") }}</th>
            <th>{{ $t("fields.after") }}</th>
            <th>{{ $t("fields.operatedBy") }}</th>
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
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.action }}</td>
            <td>
              {{
                item.accountNumber == 0 ? item.accountUid : item.accountNumber
              }}
              ({{ item.accountId }})
            </td>
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  <el-tag type="danger" class="w-max"
                    ><span class="width-control">{{
                      item.before
                    }}</span></el-tag
                  >
                </div>
                <!-- <el-button
                  size="small"
                  class="ms-2"
                  v-if="
                    item.after.length >= 60 || typeof item.before === 'object'
                  "
                  @click="showData(item.before)"
                  >{{ $t("action.viewMore") }}</el-button
                > -->
              </div>
            </td>
            <td>
              <div class="d-flex align-items-center">
                <div class="width-control">
                  <el-tag type="success" class="w-max"
                    ><span class="width-control">{{ item.after }}</span></el-tag
                  >
                </div>
                <!-- <el-button
                  size="small"
                  class="ms-2"
                  v-if="
                    item.after.length >= 60 || typeof item.after === 'object'
                  "
                  @click="showData(item.after)"
                  >{{ $t("action.viewMore") }}</el-button
                > -->
              </div>
            </td>
            <td>{{ item.operatorName }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <el-button @click="compareData(item)"
                >{{ $t("action.view") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <el-dialog
    v-model="show"
    width="700"
    align-center
    style="max-height: 95vh; overflow: auto"
  >
    <div
      class="d-flex gap-4 justify-content-between"
      :class="{
        'flex-column gap-10':
          typeof beforeData != 'object' || typeof afterData != 'object',
      }"
    >
      <div>
        <h4>Before</h4>
        <div v-if="typeof beforeData != 'object'">{{ beforeData }}</div>
        <VueJsonView v-else :src="beforeData" theme="rjv-default" />
      </div>
      <div>
        <h4>After</h4>
        <div v-if="typeof afterData != 'object'">{{ afterData }}</div>
        <VueJsonView v-else :src="afterData" theme="rjv-default" />
      </div>
    </div>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="show = false">{{ $t("action.close") }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import VueJsonView from "@matpool/vue-json-view";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const data = ref(<any>[]);
const isLoading = ref(false);
const show = ref(false);
const beforeData = ref(<any>[]);
const afterData = ref(<any>[]);
const criteria = ref({
  page: 1,
  pageSize: 20,
  searchText: "",
  sortFlag: true,
  sortField: "createdOn",
  action: "",
});
const compareData = (item: any) => {
  isValidJSON(item.before)
    ? (beforeData.value = JSON.parse(item.before))
    : (beforeData.value = item.before);
  isValidJSON(item.after)
    ? (afterData.value = JSON.parse(item.after))
    : (afterData.value = item.after);
  show.value = true;
};
const reset = () => {
  criteria.value = {
    page: 1,
    pageSize: 20,
    searchText: "",
    sortFlag: true,
    sortField: "createdOn",
    action: "",
  };
  fetchData(1);
};
function isValidJSON(jsonString) {
  try {
    JSON.parse(jsonString);
    return true;
  } catch (error) {
    return false;
  }
}
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
  display: inline-block;
  max-width: 300px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.w-max {
  max-width: 300px;
}
:deep .el-tag__content {
  max-width: 300px;
}
</style>
