<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <h2 class="card-label">{{ $t("title.equityBelowCredit") }}</h2>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>{{ $t("title.login") }}</th>
            <th>{{ $t("fields.group") }}</th>
            <th>{{ $t("fields.credit") }}</th>
            <th>{{ $t("fields.equity") }}</th>
            <th>{{ $t("fields.balance") }}</th>
            <th>{{ $t("fields.difference") }}</th>
            <th>{{ $t("fields.lastUpdateTime") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && detail.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in detail" :key="index">
            <td>{{ item.login }}</td>
            <td>{{ item.group }}</td>
            <td>{{ item.credit }}</td>
            <td>{{ item.equity }}</td>
            <td>{{ item.balance }}</td>
            <td>{{ item.difference }}</td>
            <td>{{ item.lastUpdateTime }}</td>
            <td>
              <el-popconfirm
                confirm-button-text="Yes"
                cancel-button-text="No"
                :icon="InfoFilled"
                icon-color="#626AEF"
                title="Are you sure to send email?"
                @confirm="sendEmail(item)"
              >
                <template #reference>
                  <el-button type="success">{{
                    $t("action.sendEmail")
                  }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import ToolServices from "@/projects/tenant/modules/tools/services/ToolServices";
import { InfoFilled } from "@element-plus/icons-vue";
const isLoading = ref(false);
const detail = ref(Array<any>());

const fetchData = async () => {
  isLoading.value = true;
  try {
    const data = await ToolServices.getEquityBelowCredit();
    detail.value = data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const sendEmail = async (item) => {
  isLoading.value = true;
  let data = {
    to: item.email,
    title: "",
    language: "",
  };
  try {
    const data = await ToolServices.sendEquityBelowCreditEmail(data);
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

onMounted(() => {
  fetchData();
});
</script>
