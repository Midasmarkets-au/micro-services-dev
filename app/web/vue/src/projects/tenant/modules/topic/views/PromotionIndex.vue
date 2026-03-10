<template>
  <div class="card">
    <div class="card-header">
      <h3 class="card-title">{{ $t("title.promotion") }}</h3>
      <div class="card-toolbar">
        <el-button type="success" @click="showCreatePromotion()" plain>{{
          $t("action.create")
        }}</el-button>
      </div>
    </div>
    <div class="card-body" v-loading="statusLoading">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.key") }}</th>
            <th>{{ $t("fields.sites") }}</th>
            <th>{{ $t("fields.updatedOn") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading" style="height: 300px">
          <tr>
            <td colspan="12">
              <scale-loader :color="'#ffc730'"></scale-loader>
            </td>
          </tr>
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in data" :key="index">
            <td>
              <el-switch
                v-model="item.is_published"
                active-color="#13ce66"
                inactive-color="#ff4949"
                :active-value="1"
                :inactive-value="0"
                @click="updateStatus(item)"
              />
            </td>
            <td>{{ item.key }}</td>
            <td>
              <el-tag
                v-for="(site, index) in item.access_sites"
                :key="index"
                plain
                class="me-3"
                type="warning"
              >
                {{ site }}
              </el-tag>
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.updated_at" />
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.created_at" />
            </td>
            <td>
              <el-button type="warning" @click="showEditPromotion(item)" plain>
                {{ $t("action.edit") }}
              </el-button>
              <el-button type="primary" @click="showPromotionList(item)" plain>
                {{ $t("action.view") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <CreatePromotionKey ref="createPromotionRef" @submit="fetchData" />
  <EditPromotionKey ref="editPromotionRef" @submit="fetchData" />
  <PromotionList ref="promotionListRef" />
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import CreatePromotionKey from "../components/promotion/CreatePromotionKey.vue";
import EditPromotionKey from "../components/promotion/EditPromotionKey.vue";
import PromotionList from "../components/promotion/PromotionList.vue";
import SystemService from "../../system/services/SystemService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import notification from "@/core/plugins/notification";

const isLoading = ref(false);
const statusLoading = ref(false);
const data = ref<any>([]);

const createPromotionRef = ref<any>(null);
const editPromotionRef = ref<any>(null);
const promotionListRef = ref<any>(null);

const showCreatePromotion = () => {
  createPromotionRef.value.show();
};

const showPromotionList = (item: any) => {
  promotionListRef.value.show(item);
};

const showEditPromotion = (item: any) => {
  editPromotionRef.value.show(item);
};

const updateStatus = async (item: any) => {
  statusLoading.value = true;
  try {
    await SystemService.updatePromotionStatus(item.id);
    notification.success();
  } catch (error) {
    console.error(error);
    notification.danger();
  } finally {
    statusLoading.value = false;
  }
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryPromotions();
    data.value = res.data;
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

onMounted(() => {
  fetchData();
});
</script>
