<template>
  <div class="card p-8">
    <div class="card-header">
      <div class="card-title">Inventory</div>
      <div class="card-toolbar">
        <el-button type="success" @click="createOrEdit('create')">
          Add Item
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>Publish</th>
            <th>Item ID</th>
            <th>Item Name</th>
            <th>Item Points</th>
            <th>Item Quantity</th>
            <th>Available Sites</th>
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
            <td>
              <el-switch
                v-model="item.published"
                active-color="#13ce66"
                inactive-color="#ff4949"
                @change="() => publishToggle(item)"
              ></el-switch>
            </td>
            <td>{{ item.id }}</td>
            <td>{{ item.name }}</td>
            <td>{{ item.points }}</td>
            <td>{{ item.quantity }}</td>
            <td>
              <span
                v-for="(i, index) in ConfigSiteTypesSelections"
                :key="index"
              >
                <span
                  class="badge text-bg-warning me-1"
                  v-if="item.availableSites.includes(i.value)"
                  >{{ i.label }}</span
                >
                <span v-else class="badge text-bg-secondary me-1">{{
                  i.label
                }}</span>
              </span>
            </td>
            <td>
              <el-button
                type="primary"
                class="me-5"
                @click="createOrEdit('edit', item)"
              >
                Edit
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <!-- <div class="row row-cols-5 g-4">
        <div
          v-for="(item, index) in data"
          :key="index"
          class="col cursor-pointer mb-5"
          @click="openPurchasePage(item)"
        >
          <div class="card text-center text-bg-light" style="width: 18rem">
            <img :src="item.image" class="card-img-top" />
            <div class="card-body">
              <div>
                <h5 class="card-title">{{ item.name }}</h5>
                <div class="d-flex gap-4">
                  <p class="card-text">
                    <el-tag
                      ><span>Points : </span>
                      <span>{{ item.points }}</span></el-tag
                    >
                  </p>
                  <el-tag type="success">Quantity: {{ item.quantity }}</el-tag>
                </div>
              </div>
              <div class="mt-2">
                <el-button type="primary" class="me-5"> Edit </el-button>
                <el-switch
                  v-model="item.published"
                  active-color="#13ce66"
                  inactive-color="#ff4949"
                >
                  Publish</el-switch
                >
              </div>
            </div>
          </div>
        </div>
      </div> -->
    </div>
  </div>
  <CreateOrEditItem ref="modalRef" @submit="fetchData" />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import CreateOrEditItem from "./CreateOrEditItem.vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";

const isLoading = ref(false);
const modalRef = ref<InstanceType<typeof CreateOrEditItem>>();
const data = ref([
  {
    id: 1,
    name: "Item 1",
    availableSites: [1, 5],
    image: "https://picsum.photos/200/300",
    description: "Item 1 description",
    points: 100,
    quantity: 10,
    published: true,
  },
]);
const fetchData = async () => {
  isLoading.value = true;
  try {
    // const res = await ShopService.getItemList();
    // data.value = res.data;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const publishToggle = async (item: any) => {
  try {
    // const res = await ShopService.updateItem(item);
    // MsgPrompt.success("Item updated successfully");
  } catch (error) {
    console.log(error);
  }
};

const createOrEdit = (type: string, item: any = null) => {
  modalRef.value?.show(type, item);
};

onMounted(() => {
  fetchData();
});
</script>

<style scoped lang="scss">
.box-card {
  border-radius: 10px;
}

.image {
  border-radius: 10px;
}
.title {
  font-size: 18px;
  font-weight: 500;
  color: #333;
  margin: 10px 0;
  text-align: center;
}
.points-text {
  font-size: 16px;
  font-weight: 500;
  color: #999;
  text-align: center;
}
// I want a vintage white background color
.bg {
  background-color: #faebd7;
}
</style>
