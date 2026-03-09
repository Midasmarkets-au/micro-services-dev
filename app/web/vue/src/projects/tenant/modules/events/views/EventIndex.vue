<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">{{ $t("title.events") }}</div>
      <div class="card-toolbar">
        <el-button type="success" @click="create()">
          {{ $t("action.new") }}
        </el-button>
      </div>
    </div>

    <div class="card-body">
      <table
        class="table align-middle fs-6 gy-5"
        id="kt_ecommerce_add_product_reviews"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("tip.published") }}</th>
            <th>{{ $t("fields.eventId") }}</th>
            <th>{{ $t("fields.title") }}</th>
            <th>{{ $t("fields.key") }}</th>
            <th>{{ $t("fields.accessRoles") }}</th>
            <th>{{ $t("fields.site") }}</th>
            <th>{{ $t("fields.startDate") }}</th>
            <th>{{ $t("fields.endDate") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in data" :key="index">
            <td class="d-flex gap-4 align-items-center">
              <el-switch
                v-model="item.status"
                active-color="#13ce66"
                inactive-color="#ff4949"
                :active-value="EventShopItemStatusTypes.Published"
                @click="publishToggle(item)"
              ></el-switch>
            </td>
            <td>{{ item.id }}</td>
            <td>{{ item.title }}</td>
            <td>{{ item.key }}</td>
            <td>
              <el-tag
                v-for="(role, index) in item.accessRoles"
                :key="index"
                type="warning"
                size="small"
                class="me-1"
                effect="dark"
              >
                {{ role }}
              </el-tag>
            </td>

            <td>
              <span
                v-for="(i, index) in ConfigSiteTypesSelections"
                :key="index"
              >
                <span
                  class="badge text-bg-warning me-1"
                  v-if="item.accessSites.includes(String(i.value))"
                >
                  {{ i.label }}
                </span>
              </span>
            </td>

            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.startOn" />
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.endOn" />
            </td>
            <td>
              <el-button type="primary" @click="edit(item)">
                {{ $t("action.edit") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
    <CreateEvent ref="createEventRef" @submit="fetchData(1)" />
    <EditEvent ref="editEventRef" @submit="fetchData(1)" />
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import EventsServices from "../services/EventsServices";
import { EventShopItemStatusTypes } from "@/core/types/shop/ShopItemTypes";
import { ElNotification } from "element-plus";
import CreateEvent from "../components/event/CreateEvent.vue";
import EditEvent from "../components/event/EditEvent.vue";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";

const isLoading = ref(false);
const editEventRef = ref<InstanceType<typeof EditEvent>>();
const createEventRef = ref<InstanceType<typeof CreateEvent>>();
const data = ref(<any>[]);
const criteria = ref({
  page: 1,
  size: 25,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await EventsServices.queryEventsList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const create = () => {
  createEventRef.value.show();
};

const edit = (item) => {
  editEventRef.value.show(item);
};

const publishToggle = async (item: any) => {
  try {
    if (item.status == EventShopItemStatusTypes.Published) {
      await EventsServices.publishEvent(item.id);
      ElNotification({
        title: "Success",
        message: "Event published",
        type: "success",
      });
    } else {
      await EventsServices.unpublishEvent(item.id);
      ElNotification({
        title: "Success",
        message: "Event unpublished",
        type: "success",
      });
    }
  } catch (error) {
    console.log(error);
    ElNotification({
      title: "Error",
      message: "Failed to publish/unpublish event",
      type: "error",
    });
  }
};
onMounted(() => {
  fetchData(1);
});
</script>
