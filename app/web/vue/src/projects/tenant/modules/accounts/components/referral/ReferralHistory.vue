<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("fields.referralHistory") }}</div>
    </div>
    <div class="card-body">
      <table class="table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.referralCode") }}</th>
            <th>{{ $t("fields.referrerPartyId") }}</th>
            <th>{{ $t("fields.InivtedPartyId") }}</th>
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
            <td>{{ item.code }}</td>
            <td>{{ item.referrerPartyId }}</td>
            <td>{{ item.referredPartyId }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
  </div>
  <el-dialog
    v-model="show"
    width="700"
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
import AccountService from "../../services/AccountService";
import VueJsonView from "@matpool/vue-json-view";
const isLoading = inject<any>("isLoading");
const show = ref(false);
const dialogData = ref(<any>[]);
const data = ref(<any>[]);
const criteria = ref({
  page: 1,
  size: 20,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await AccountService.getReferralHistory(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};
const showSchema = (data: any) => {
  show.value = true;
  dialogData.value = data;
};
const pageChange = (page: number) => {
  fetchData(page);
};
onMounted(() => {
  fetchData(1);
});
</script>
