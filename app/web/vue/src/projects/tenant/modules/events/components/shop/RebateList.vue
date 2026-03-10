<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">Rebate List</div>
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
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref } from "vue";

const isLoading = ref(false);
const data = ref([]);
</script>
