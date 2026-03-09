<template>
  <div class="card">
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Order Number</th>
            <th>Item Name</th>
            <th>Item Quantity</th>
            <th>Order Date</th>
            <th>Order Status</th>
            <th>Order By</th>
            <th>Detail</th>
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
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.orderId }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.quantity }}</td>
            <td>{{ item.date }}</td>
            <td>{{ item.status }}</td>
            <td>{{ item.customer?.name }}</td>
            <td>
              <el-button type="primary" @click="orderDetail(item)"
                >Detail</el-button
              >
            </td>
            <td>
              <el-button type="success" @click="shipOrder(item)"
                >Ship</el-button
              >
              <el-button type="danger" @click="cancelOrder(item)"
                >Cancel</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <ShipOrder ref="modalRef" @submit="fetchData" />
  <CancelOrder ref="cancelModalRef" @submit="fetchData" />
  <OrderDetail ref="detailModalRef" />
</template>
<script setup lang="ts">
import { ref } from "vue";
import ShopServices from "../services/ShopServices";
import ShipOrder from "./OrderListComponents/ShipOrder.vue";
import CancelOrder from "./OrderListComponents/CancelOrder.vue";
import OrderDetail from "./OrderListComponents/OrderDetail.vue";
const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof ShipOrder>>();
const cancelModalRef = ref<InstanceType<typeof CancelOrder>>();
const detailModalRef = ref<InstanceType<typeof OrderDetail>>();

const shipOrder = async (item: any) => {
  modalRef.value?.show(item);
};
const cancelOrder = async (item: any) => {
  cancelModalRef.value?.show(item);
};
const orderDetail = async (item: any) => {
  detailModalRef.value?.show(item);
};
const data = ref([]);

const fetchData = async () => {
  isLoading.value = true;
  try {
    // const res = await ShopServices.getOrderList();
    // data.value = res.data;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};
</script>
