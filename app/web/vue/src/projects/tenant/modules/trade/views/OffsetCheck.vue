<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        <h2 class="card-label">{{ $t("title.offsetCheckList") }}</h2>
      </div>
      <div class="card-toolbar gap-4">
        <el-button type="primary" @click="showChecklistCreate">{{
          $t("action.addOffsetCheck")
        }}</el-button>
        <el-button type="warning" @click="resetCache">{{
          $t("action.resetCache")
        }}</el-button>
      </div>
    </div>

    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr>
            <th>{{ $t("status.active") }}</th>
            <th>{{ $t("fields.name") }}</th>
            <th>{{ $t("fields.members") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && checklist.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in checklist" :key="index">
            <td>
              <el-switch
                v-model="item.status"
                :active-value="1"
                :inactive-value="0"
                @change="activeToggle(item)"
              ></el-switch>
            </td>
            <td>{{ item.name }}</td>
            <td>{{ item.accountNumbers }}</td>
            <td style="min-width: 180px">
              <el-button type="primary" @click="showChecklistUpdate(item)">{{
                $t("action.update")
              }}</el-button>
              <el-popconfirm
                confirm-button-text="Yes"
                cancel-button-text="No"
                :icon="InfoFilled"
                icon-color="#626AEF"
                title="Are you sure?"
                @confirm="deleteChecklist(item.id)"
              >
                <template #reference>
                  <el-button type="danger">{{ $t("action.delete") }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <checklistCreate
      ref="checklistCreateRef"
      @event-submit="onEventSubmitted"
    />
    <checklistUpdate
      ref="checklistUpdateRef"
      @event-submit="onEventSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { useI18n } from "vue-i18n";
import { ref, onMounted } from "vue";
import { ElNotification } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TradeServices from "../services/TradeServices";
import { InfoFilled } from "@element-plus/icons-vue";
import checklistCreate from "@/projects/tenant/modules/trade/components/ChecklistCreate.vue";
import checklistUpdate from "@/projects/tenant/modules/trade/components/ChecklistUpdate.vue";

const { t } = useI18n();
const store = useStore();
const isLoading = ref(false);
const checklist = ref(Array<any>());
const checklistCreateRef = ref<any>(null);
const checklistUpdateRef = ref<any>(null);
const siteId = ref(store.state.AuthModule.config.siteId);

const showChecklistCreate = () => {
  checklistCreateRef.value?.show(siteId.value);
};

const showChecklistUpdate = (item: any) => {
  checklistUpdateRef.value?.show(item, siteId.value);
};

const getChecklist = async () => {
  isLoading.value = true;
  try {
    const res = await TradeServices.getChecklist();
    checklist.value = res;
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const activeToggle = async (item: any) => {
  isLoading.value = true;

  try {
    await TradeServices.updateChecklist(item.id, item);
    ElNotification.success({
      title: "Success",
      offset: 100,
    });
    getChecklist();
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const deleteChecklist = async (id: number) => {
  isLoading.value = true;
  try {
    await TradeServices.deleteChecklist(id);
    getChecklist();
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const resetCache = async () => {
  isLoading.value = true;
  try {
    await TradeServices.resetCache();
    MsgPrompt.success(t("tip.submitSuccess")).then(() => {
      getChecklist();
    });
  } catch (error) {
    MsgPrompt.error(t("tip.submitError"));
  }
  isLoading.value = false;
};

const onEventSubmitted = () => {
  getChecklist();
};

onMounted(() => {
  getChecklist();
});
</script>
