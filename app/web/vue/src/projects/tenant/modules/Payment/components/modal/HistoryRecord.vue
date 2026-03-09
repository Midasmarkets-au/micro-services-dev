<template>
  <el-dialog
    v-model="dialogRef"
    width="1200"
    align-center
    :before-close="close"
  >
    <div
      v-if="isLoading"
      class="d-flex justify-content-center align-items-center"
    >
      <LoadingRing />
    </div>
    <div v-else>
      <h3 class="mb-3">State Changes</h3>
      <div v-for="(item, index) in dialogData.stateChanges" :key="index">
        <el-descriptions :column="2" border class="mb-8">
          <div v-for="(data, i) in item" :key="i" :label="i">
            <el-descriptions-item :label="i" label-width="100px">
              <div v-if="i == 'performedOn'">
                <TimeShow :date-iso-string="data" />
              </div>
              <div v-else>
                {{ data }}
              </div>
            </el-descriptions-item>
          </div>
        </el-descriptions>
      </div>

      <el-descriptions :column="2" border title="Callback" class="mt-5">
        <div v-for="(item, index) in dialogData.callbackBody" :key="index">
          <el-descriptions-item :label="index" v-if="index != 'body'">
            <div v-if="index == 'updatedOn'">
              <TimeShow :date-iso-string="item" />
            </div>
            <div v-else>
              {{ item }}
            </div>
          </el-descriptions-item>
        </div>
      </el-descriptions>
    </div>

    <template #footer>
      <div class="dialog-footer">
        <el-button type="primary" @click="dialogRef = false">
          {{ $t("action.close") }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script lang="ts" setup>
import { ref } from "vue";
import PaymentService from "@/projects/tenant/modules/Payment/services/PaymentService";

const dialogRef = ref(false);
const id = ref<any>(null);
const isLoading = ref(false);
const dialogData = ref<any>([]);
const fecthData = async () => {
  isLoading.value = true;
  try {
    const response = await PaymentService.getStateDetail(id.value);
    dialogData.value = response;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const show = async (_id) => {
  dialogRef.value = true;
  id.value = _id;
  await fecthData();
};

const close = () => {
  dialogRef.value = false;
  id.value = null;
  dialogData.value = [];
  isLoading.value = false;
};

defineExpose({
  show,
});
</script>
<style scoped>
::v-deep .el-descriptions__label {
  width: 150px;
}
::v-deep .el-descriptions__content {
  width: 450px;
}
</style>
