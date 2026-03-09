<template>
  <div class="card-body px-0">
    <table class="table table-row-bordered fs-6 gy-5 my-0">
      <thead>
        <tr class="text-center fs-7 text-uppercase gs-0">
          <th>{{ $t("fields.date") }}</th>
          <th>Case Number</th>
          <th>{{ $t("fields.category") }}</th>
          <th>{{ $t("fields.status") }}</th>
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
        <tr v-for="(item, index) in data" :key="index" class="text-center">
          <td>
            <TimeShow :date-iso-string="item.createdOn" type="reportTime" />
          </td>
          <td>{{ item.caseId }}</td>
          <td>{{ $t("title." + item.categoryName) }}</td>
          <td>
            <el-tag :type="getTagType(item.status)">{{
              CaseStatusTypes[item.status]
            }}</el-tag>
          </td>
          <td>
            <el-button
              type="primary"
              class="btn btn-primary btn-sm btn-radius"
              @click="showDetail(item)"
              >{{ $t("action.view") }}</el-button
            >
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import SupportService from "../../services/SupportService";
import { ClientView } from "@/core/types/SupportStatusTypes";
import { CaseStatusTypes } from "@/core/types/SupportStatusTypes";
const isLoading = ref(false);
const changeView = inject<(status: any) => void>("changeView");
const item = inject<any>("item");
const data = ref(<any>[]);

const getTagType = (status: number) => {
  switch (status) {
    case 0:
      return "success";
    case 30:
      return "danger";
    case 10:
      return "warning";
    default:
      return "info";
  }
};
const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SupportService.queryCasesList();
    data.value = res.data;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const showDetail = (_item: any) => {
  item.value = _item;
  changeView(ClientView.CaseDetail);
};

onMounted(() => {
  fetchData();
});
</script>
