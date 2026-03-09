<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-select
          v-model="criteria.category"
          :placeholder="$t('fields.category')"
          :disabled="isLoading || statusSwitchLoading"
        >
          <el-option
            v-for="item in categories"
            :key="item"
            :label="item"
            :value="item"
          >
          </el-option>
        </el-select>
        <el-input
          v-model="criteria.symbol"
          :placeholder="$t('fields.symbol')"
          :disabled="isLoading || statusSwitchLoading"
          class="ms-4"
          @keyup.enter="fetchData(1)"
        />
        <el-button
          type="warning"
          @click="fetchData(1)"
          :loading="isLoading"
          :disabled="statusSwitchLoading"
          class="ms-4"
          >{{ $t("action.search") }}</el-button
        >
        <el-button @click="reset" :disabled="isLoading || statusSwitchLoading">
          {{ $t("action.clear") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table table-tenant">
        <thead>
          <tr>
            <th>{{ $t("fields.status") }}</th>
            <th>{{ $t("fields.symbol") }}</th>
            <th>{{ $t("fields.category") }}</th>
            <th>{{ $t("fields.contractSize") }}</th>
            <th>{{ $t("fields.tradeTime") }}</th>
            <th>{{ $t("fields.weekDay") }}</th>
            <th>{{ $t("fields.breakTime") }}</th>
            <th>{{ $t("fields.breakTime") }} - 2</th>
            <th>{{ $t("fields.commission") }}</th>
            <th>{{ $t("fields.marginRequirement") }}</th>
            <th>{{ $t("fields.uploadedBy") }}</th>
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
          <tr v-for="item in data" :key="item.id">
            <td>
              <el-switch
                v-model="item.is_enabled"
                style="
                  --el-switch-on-color: #13ce66;
                  --el-switch-off-color: #ff4949;
                "
                :active-value="1"
                :inactive-value="0"
                :loading="statusSwitchLoading"
                @change="switchStatus(item)"
              />
            </td>
            <td>{{ item.symbol }}</td>
            <td>{{ item.category }}</td>
            <td>{{ item.contract_size }}</td>
            <td>{{ item.trading_start_time }} - {{ item.trading_end_time }}</td>
            <td>
              {{ item.trading_start_weekday }} - {{ item.trading_end_weekday }}
            </td>
            <td>{{ item.break_start_time }} - {{ item.break_end_time }}</td>
            <td>
              <div v-if="item.more_break_start_time">
                {{ item.more_break_start_time }} -
                {{ item.more_break_end_time }}
              </div>
            </td>
            <td>{{ item.commission }}</td>
            <td>
              <el-tag
                v-for="(margin, index) in item.margin_requirements"
                :key="index"
                class="me-1"
                type="warning"
              >
                <div v-if="site == 'ba'">{{ index }}: {{ margin }}</div>
                <div v-else>
                  {{ margin }}
                </div>
              </el-tag>
            </td>
            <td>
              <el-popover :width="300" v-if="item.operator_info">
                <template #reference>
                  <el-button :icon="Memo" size="small" circle></el-button>
                </template>
                <div
                  v-for="(operator, index) in item.operator_info"
                  :key="index"
                >
                  <p class="fw-bold text-warning-emphasis d-flex">
                    {{ $t("fields.updated_at") }} :
                    <TimeShow
                      type="GMToneLiner"
                      :date-iso-string="operator.updatedAt"
                    />
                  </p>
                  <p>{{ operator.name }} - {{ operator.email }}</p>
                  <el-divider></el-divider>
                </div>
              </el-popover>
            </td>
            <td>
              <el-button
                size="small"
                @click="showEdit(item)"
                :disabled="statusSwitchLoading"
                >{{ $t("action.edit") }}</el-button
              >
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <EditContract ref="EditContractRef" @event-submit="fetchData(1)" />
</template>
<script lang="ts" setup>
import { ref, onMounted } from "vue";
import DocsServices from "../services/DocsServices";
import { useStore } from "@/store";
import EditContract from "../components/ContractSpecs/EditContract.vue";
import { ElNotification } from "element-plus";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { Memo } from "@element-plus/icons-vue";
import ScaleLoader from "vue-spinner/src/ScaleLoader.vue";
const store = useStore();
const user = store.state.AuthModule.user;
const EditContractRef = ref<any>(null);
const site = ref(user.tenancy == "au" ? "ba" : user.tenancy);
const isLoading = ref(false);
const statusSwitchLoading = ref(false);
const data = ref<any>([]);
const categories = ref<any>([]);
const criteria = ref<any>({
  page: 1,
  size: 25,
  site: site.value,
});
const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await DocsServices.queryContractSpecsList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
    categories.value = res.categories;
  } catch (error) {
    console.error(error);
    MsgPrompt.error("Fetch data failed");
  }
  isLoading.value = false;
};

const switchStatus = async (item: any) => {
  statusSwitchLoading.value = true;
  try {
    await DocsServices.updateContractSpecsStatus(item.id);
    ElNotification({
      title: "Success",
      message: "Update status successfully",
      type: "success",
    });
  } catch (error) {
    console.error(error);
    ElNotification({
      title: "Error",
      message: "Update status failed",
      type: "error",
    });
  }
  statusSwitchLoading.value = false;
};

const showEdit = (item: any) => {
  EditContractRef.value?.show(item);
};

const reset = async () => {
  criteria.value = {
    page: 1,
    size: 25,
    site: site.value,
  };
  await fetchData(1);
};

onMounted(async () => {
  await fetchData(1);
});
</script>
