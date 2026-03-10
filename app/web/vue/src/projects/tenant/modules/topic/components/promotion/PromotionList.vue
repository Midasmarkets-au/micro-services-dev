<template>
  <el-dialog v-model="dialogRef" width="800" align-center :show-close="false">
    <template #header>
      <div class="my-header d-flex justify-content-between">
        <p class="fs-4">{{ promotion.key }}</p>
        <el-button
          type="success"
          @click="showCreatePromotion(promotionId)"
          plain
          >{{ $t("action.create") }}</el-button
        >
      </div>
    </template>
    <table class="table table-tenant">
      <thead>
        <tr>
          <th>{{ $t("fields.status") }}</th>
          <th>{{ $t("fields.id") }}</th>
          <th>{{ $t("fields.startDate") }}</th>
          <th>{{ $t("fields.endDate") }}</th>
          <th>{{ $t("fields.createdOn") }}</th>
          <th>{{ $t("fields.updatedOn") }}</th>
          <th>{{ $t("fields.action") }}</th>
          <!-- <th>Last Operator</th> -->
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
            <el-tag v-if="item.is_active" type="success" size="small">
              {{ $t("status.active") }}
            </el-tag>
            <el-tag v-else type="info" size="small">
              {{ $t("status.inactive") }}
            </el-tag>
          </td>
          <td>{{ item.id }}</td>
          <td><TimeShow type="GMToneLiner" :date-iso-string="item.from" /></td>
          <td><TimeShow type="GMToneLiner" :date-iso-string="item.to" /></td>

          <td>
            <TimeShow type="GMToneLiner" :date-iso-string="item.updated_on" />
          </td>
          <td>
            <TimeShow type="GMToneLiner" :date-iso-string="item.created_on" />
          </td>

          <td>
            <el-button type="primary" @click="showEditPromotion(item)" plain>{{
              $t("action.edit")
            }}</el-button>
          </td>
        </tr>
      </tbody>
    </table>
    <template #footer>
      <div class="dialog-footer">
        <el-button @click="dialogRef = false">{{
          $t("action.close")
        }}</el-button>
      </div>
    </template>
  </el-dialog>

  <CreatePromotion ref="createPromotionRef" @submit="fetchData" />
  <EditPromotion ref="editPromotionRef" @submit="fetchData" />
</template>

<script lang="ts" setup>
import { ref } from "vue";
import SystemService from "../../../system/services/SystemService";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
import CreatePromotion from "./CreatePromotion.vue";
import EditPromotion from "./EditPromotion.vue";

const createPromotionRef = ref<any>(null);
const editPromotionRef = ref<any>(null);
const dialogRef = ref(false);
const isLoading = ref(false);
const data = ref<any>([]);
const promotionId = ref<any>(0);
const promotion = ref<any>({});
const show = (item) => {
  dialogRef.value = true;
  promotion.value = item;
  promotionId.value = item.id;
  fetchData();
};

const showCreatePromotion = (id) => {
  createPromotionRef.value.show(id);
};

const showEditPromotion = (item) => {
  editPromotionRef.value.show(item);
};

const fetchData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryPromotionList(promotionId.value);
    data.value = res.data;
  } catch (error) {
    console.error(error);
  } finally {
    isLoading.value = false;
  }
};

defineExpose({
  show,
});
</script>
