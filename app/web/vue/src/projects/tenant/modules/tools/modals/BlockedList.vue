<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="selectedOption"
          placeholder="Select option"
          class="w-150px me-3"
          :disabled="isLoading"
        >
          <el-option label="Email" value="email"></el-option>
          <el-option label="Name" value="name"></el-option>
          <el-option label="Phone" value="phone"></el-option>
          <el-option label="ID Number" value="idNumber"></el-option>
        </el-select>
        <el-input
          class="w-200px me-3"
          v-model="inputValue"
          :disabled="isLoading"
        />
        <el-button type="primary" @click="search" plain :disabled="isLoading"
          >{{ $t("action.search") }}
        </el-button>
        <el-button @click="reset" plain :disabled="isLoading"
          >{{ $t("action.clear") }}
        </el-button>
      </div>
      <div class="card-toolbar">
        <el-button type="primary" @click="showCreate()">Block User</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>{{ $t("fields.name") }}</th>
            <th>ID</th>
            <th>{{ $t("fields.phone") }}</th>
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
              <UserInfo :email="item.email" :name="item.name" class="me-2" />
            </td>
            <td>{{ item.idNumber }}</td>
            <td>{{ item.phone }}</td>
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
                title="Remove this user from blocked list"
                @confirm="removeBlockedUser(item.id)"
                width="300px"
              >
                <template #reference>
                  <el-button type="danger">{{ $t("action.remove") }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <BlockListCreate ref="createModalRef" @event-submit="fetchData(1)" />
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import { InfoFilled } from "@element-plus/icons-vue";
import ToolServices from "../services/ToolServices";
import { ElNotification } from "element-plus";
import BlockListCreate from "../components/BlockListCreate.vue";
const isLoading = inject<any>("isLoading");
const selectedOption = ref("email");
const detail = ref(Array<any>());
const createModalRef = ref(null);
const criteria = ref({
  page: 1,
  size: 25,
});

const inputValue = ref("");

const search = () => {
  delete criteria.value.email;
  delete criteria.value.phone;
  delete criteria.value.name;
  delete criteria.value.idNumber;
  criteria.value[selectedOption.value] = inputValue.value;
  fetchData(1);
};

const reset = () => {
  criteria.value = { page: 1, size: 25 };
  inputValue.value = "";
  fetchData(1);
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const data = await ToolServices.getBlockedList(criteria.value);
    criteria.value = data.criteria;
    detail.value = data.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const showCreate = (item?: any) => {
  createModalRef.value?.show(item);
};
const removeBlockedUser = async (id: number) => {
  try {
    await ToolServices.removeBlockedUser(id);
    ElNotification.success({
      title: "Success",
      message: "User removed from blocked list successfully.",
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

onMounted(() => {
  fetchData(1);
});
</script>
