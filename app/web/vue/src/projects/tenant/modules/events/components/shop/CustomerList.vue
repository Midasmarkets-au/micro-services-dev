<template>
  <div class="card">
    <div class="card-header d-flex align-items-center">
      <ul
        class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-6 fw-semobold"
      >
        <li
          class="nav-item"
          v-for="tab in customerStatusOptions"
          :key="tab.value"
        >
          <a
            class="nav-link text-active-primary pb-2"
            :class="{ active: isTabActive(tab.value) }"
            data-bs-toggle="tab"
            href="#"
            @click="changeTab(tab.value)"
            >{{ tab.label }}</a
          >
        </li>
      </ul>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Name</th>
            <th>Account</th>
            <th>Role</th>
            <th>Created On</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="item in data" :key="item.id">
            <td>{{ item.name }}</td>
            <td>{{ item.phone }}</td>
            <td>{{ item.email }}</td>
            <td>{{ item.address }}</td>
            <td>
              <el-button type="success" size="small">Approve</el-button>
              <el-button type="danger" size="small">Reject</el-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import { customerStatusOptions } from "@/core/types/ShopTypes";

const isLoading = ref(false);
const data = ref([
  {
    id: 1,
    name: "John Doe",
    phone: "1234567890",
    email: "kay.wu@bacera.com",
    status: 0,
  },
  {
    id: 2,
    name: "John Doe",
    phone: "1234567890",
    email: "kay.wu@bacera.com",
    status: 1,
  },
  {
    id: 3,
    name: "John Doe",
    phone: "1234567890",
    email: "kay.wu@bacera.com",
    status: 2,
  },
  {
    id: 4,
    name: "John Doe",
    phone: "1234567890",
    email: "kay.wu@bacera.com",
    status: 3,
  },
]);
const tab = ref(customerStatusOptions[0].value);
const isTabActive = (_tab: any) => {
  return tab.value === _tab;
};

const changeTab = (_value: any) => {
  tab.value = _value;
};
</script>
