<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <span class="me-3">Cheater IP</span>
        <el-input
          class="w-200px me-3"
          v-model="criteria.ip"
          :disabled="isLoading"
        />
        <el-button
          type="primary"
          @click="fetchData(1)"
          plain
          :disabled="isLoading"
          >{{ $t("action.search") }}
        </el-button>
        <el-button @click="reset" plain :disabled="isLoading"
          >{{ $t("action.clear") }}
        </el-button>
      </div>
      <div class="card-toolbar">
        <el-button type="primary" @click="showCreate()">{{
          $t("action.addCheaterIp")
        }}</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>{{ $t("fields.status") }}</th>
            <th>Id</th>
            <th>IP</th>
            <th>{{ $t("fields.note") }}</th>
            <th>{{ $t("fields.operator") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && detail?.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in detail" :key="index">
            <td>
              <el-switch
                active-color="#13ce66"
                inactive-color="#ff4949"
                v-model="item.enabled"
                @change="activeToggle(item)"
              ></el-switch>
            </td>
            <td>{{ item.id }}</td>
            <td>{{ item.ip }}</td>
            <td>{{ item.note }}</td>
            <td>{{ item.operatorName }}</td>
            <td><TimeShow :date-iso-string="item.createdOn" /></td>
            <td>
              <el-button type="primary" @click="showCreate(item)">
                {{ $t("action.edit") }}
              </el-button>
              <el-popconfirm
                confirm-button-text="Yes"
                cancel-button-text="No"
                :icon="InfoFilled"
                icon-color="#626AEF"
                title="Delete Cheater IP"
                @confirm="deleteCheaterIp(item.id)"
                width="300px"
              >
                <template #reference>
                  <el-button type="danger">{{ $t("action.delete") }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <CheaterIpCreate ref="createModalRef" @event-submit="onEventSubmitted" />
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import CheaterIpCreate from "../components/CheaterIpCreate.vue";
import { InfoFilled } from "@element-plus/icons-vue";
import ToolServices from "../services/ToolServices";
import { ElNotification } from "element-plus";
import TableFooter from "@/components/TableFooter.vue";

const isLoading = inject<any>("isLoading");
const detail = ref(Array<any>());
const criteria = ref({
  page: 1,
  size: 25,
  ip: "",
});
const createModalRef = ref(null);

const showCreate = (item?: any) => {
  createModalRef.value?.show(item);
};

const reset = () => {
  criteria.value = { page: 1, size: 25, ip: "" };
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const data = await ToolServices.getCheaterIpList(criteria.value);
    criteria.value = data.criteria;
    detail.value = data.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const activeToggle = async (item: any) => {
  try {
    await ToolServices.updateCheaterIp(item.id, item);
    ElNotification.success({
      title: "Status Update Success - " + item.ip,
      offset: 100,
    });
  } catch (error) {
    console.log(error);
  }
};

const deleteCheaterIp = async (id: number) => {
  try {
    await ToolServices.deleteCheaterIp(id);
    ElNotification.success({
      title: "Success",
      message: "Ip Deleted",
      offset: 100,
    });
    fetchData(1);
  } catch (error) {
    console.log(error);
    ElNotification.error({
      title: "Error",
      message: error.message,
      offset: 100,
    });
  }
};

const onEventSubmitted = () => {
  fetchData(1);
};

onMounted(() => {
  fetchData(1);
});
</script>
