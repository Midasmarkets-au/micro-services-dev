<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("title.violationList") }}</div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>{{ $t("title.accountNumber") }}</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("title.reasons") }}</th>
            <th>{{ $t("fields.staff") }}</th>
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
            <td>{{ item.accountNumber }}</td>
            <td>
              <UserInfo
                v-if="item.user"
                :user="item.user"
                :isAdmin="
                  item.user.isAdmin == undefined ? false : item.user.isAdmin
                "
                class="me-2"
              />
            </td>
            <td>{{ item.value }}</td>
            <td>
              <el-popconfirm
                confirm-button-text="Yes"
                cancel-button-text="No"
                :icon="InfoFilled"
                icon-color="#626AEF"
                title="Are you sure to remove this user from blocked list?"
                @confirm="deleteViolationUser(item.ip)"
              >
                <template #reference>
                  <el-button type="danger">{{ $t("action.remove") }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import { useStore } from "@/store";
import { InfoFilled } from "@element-plus/icons-vue";
import ToolServices from "../services/ToolServices";
import { ElNotification } from "element-plus";

const isLoading = inject<any>("isLoading");
const detail = ref(Array<any>());

const store = useStore();
const siteId = ref(store.state.AuthModule.config.siteId);

const fetchData = async () => {
  isLoading.value = true;
  try {
    const data = await ToolServices.getViolationList(siteId.value);
    detail.value = data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const deleteViolationUser = async (ip: string) => {
  try {
    await ToolServices.deleteViolationUser(siteId.value, ip);
    fetchData();
  } catch (error) {
    console.log(error);
  }
};

onMounted(() => {
  fetchData();
});
</script>
