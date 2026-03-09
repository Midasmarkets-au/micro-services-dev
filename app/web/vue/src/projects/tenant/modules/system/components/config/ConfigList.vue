<template>
  <div v-if="$props.category != 'public'" class="card">
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        v-loading="isLoading"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>{{ $t("fields.name") }}</td>
            <td>
              {{ $t("fields.category") }}
            </td>
            <td>{{ $t("fields.updatedOn") }}</td>
            <td>{{ $t("fields.updatedBy") }}</td>
            <td>{{ $t("action.action") }}</td>
          </tr>
        </thead>

        <tbody>
          <tr v-for="(item, index) in data" :key="index">
            <td>{{ item.name }}</td>
            <td>
              {{ item.category }}
            </td>
            <td>
              <TimeShow
                type="inFields"
                :date-iso-string="item.updatedOn"
                v-if="item.updatedOn"
              />
            </td>
            <td>{{ item.updatedBy }}</td>

            <td class="d-flex align-items-center">
              <div v-if="item.type == 'switch'">
                <el-switch
                  v-if="item.key != 'TwoFactorAuthSetting'"
                  style="
                    --el-switch-on-color: #13ce66;
                    --el-switch-off-color: #ff4949;
                  "
                  v-model="item.value['value']"
                  inline-prompt
                  active-text="ON"
                  inactive-text="OFF"
                  :active-value="true"
                  :inactive-value="false"
                  size="large"
                  @change="onChange(item)"
                  :disabled="isSubmitting"
                ></el-switch>
                <el-switch
                  v-else
                  style="
                    --el-switch-on-color: #13ce66;
                    --el-switch-off-color: #ff4949;
                  "
                  v-model="item.value['loginCodeEnabled']"
                  inline-prompt
                  active-text="ON"
                  inactive-text="OFF"
                  :active-value="true"
                  :inactive-value="false"
                  size="large"
                  @change="onChange(item)"
                  :disabled="isSubmitting"
                ></el-switch>
              </div>
              <div v-else-if="item.type == 'text'" class="d-flex">
                <el-input
                  v-model="item.valueString"
                  class="w-100"
                  :disabled="true"
                ></el-input>
                <el-button
                  type="primary"
                  class="ms-2"
                  @click="show(item)"
                  :disabled="isSubmitting"
                  >{{ $t("action.edit") }}</el-button
                >
              </div>
              <div v-else-if="item.type == 'input'" class="d-flex">
                <div class="d-flex gap-2">
                  <el-input
                    v-for="(value, key) in item.value"
                    :key="key"
                    v-model="item.value[key]"
                    class="w-100px input-with-select"
                    :disabled="isSubmitting"
                    @input="(val) => handleInputChange(item, key, val)"
                  >
                    <template #prepend>
                      <el-icon
                        color="#ff4949"
                        class="cursor-pointer"
                        @click="removeInput(item, key)"
                        :disabled="isSubmitting"
                        ><CloseBold
                      /></el-icon>
                    </template>
                  </el-input>
                </div>
                <el-button
                  type="success"
                  class="ms-2"
                  @click="addInput(item)"
                  :disabled="isSubmitting"
                  >{{ $t("action.add") }}</el-button
                >
                <el-button
                  type="primary"
                  class="ms-2"
                  @click="onChange(item)"
                  :disabled="isSubmitting"
                  >{{ $t("action.save") }}</el-button
                >
              </div>
              <div v-if="$cans(['SuperAdmin'])">
                <el-button
                  type="warning"
                  @click="show(item)"
                  :disabled="isSubmitting"
                  class="ms-2"
                  >{{ $t("action.edit") }} (Super Admin)</el-button
                >
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <div v-else>
    <table class="table align-middle table-row-dashed fs-6 gy-5">
      <thead>
        <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
          <td>{{ $t("fields.name") }}</td>
          <td>{{ $t("fields.updatedOn") }}</td>
          <td>{{ $t("fields.updatedBy") }}</td>
          <td>{{ $t("fields.value") }}</td>
          <td>{{ $t("action.action") }}</td>
        </tr>
      </thead>
      <tbody v-if="isLoading">
        <LoadingRing />
      </tbody>
      <tbody v-else-if="!isLoading && data.length === 0">
        <NoDataBox />
      </tbody>
      <tbody v-else>
        <tr
          v-for="(item, index) in data"
          :key="index"
          :class="{ 'bg-success': !item.default && rowId != 0 }"
        >
          <td>{{ item.name }}</td>
          <td>
            <TimeShow
              type="inFields"
              :date-iso-string="item.updatedOn"
              v-if="item.updatedOn"
            />
          </td>
          <td>{{ item.updatedBy }}</td>
          <td>
            <div class="input-group">
              <input
                disabled
                type="text"
                class="form-control"
                v-model="item.valueString"
              />
            </div>
          </td>
          <td>
            <el-button type="primary" @click="show(item)">{{
              $t("action.edit")
            }}</el-button>
            <el-button type="danger" @click="openConfirmPanel(item)">{{
              $t("action.delete")
            }}</el-button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <ConfigDetail ref="modalRef" @submitted="fetchData" />
  <AddConfig ref="addConfigRef" @submitted="fetchData" />
</template>
<script setup lang="ts">
import { ref, onMounted, nextTick } from "vue";
import SystemService from "../../services/SystemService";
import ConfigDetail from "./ConfigDetail.vue";
import AddConfig from "./AddConfig.vue";
import { AccountRoleTypes } from "@/core/types/AccountInfos";
import { CloseBold } from "@element-plus/icons-vue";
import { ElNotification } from "element-plus";
const props = defineProps({
  rowId: {
    type: Number,
    required: true,
  },
  category: {
    type: String,
    required: true,
  },
  siteId: {
    type: Number,
    required: false,
  },
  role: {
    type: Number,
    required: false,
  },
  defaultData: {
    type: Array,
    required: false,
  },
});

const showedConfig = ref<any>({
  TwoFactorAuthSetting: {
    key: "TwoFactorAuthSetting",
    showedCategory: ["party"],
    showedRole: ["all"],
    type: "switch",
    getDefault: false,
  },
  LeverageAvailable: {
    key: "LeverageAvailable",
    showedCategory: ["account", "party"],
    showedRole: ["all"],
    type: "input",
    getDefault: true,
  },
  AutoCreateTradeAccountEnabled: {
    key: "AutoCreateTradeAccountEnabled",
    showedCategory: ["account"],
    showedRole: [AccountRoleTypes.IB, AccountRoleTypes.Sales],
    type: "switch",
    getDefault: false,
  },
  RebateSetting: {
    key: "DefaultRebateLevelSetting",
    showedCategory: ["account"],
    showedRole: [AccountRoleTypes.IB, AccountRoleTypes.Sales],
    type: "text",
    getDefault: false,
  },
});

const modalRef = ref<InstanceType<typeof ConfigDetail>>();
const addConfigRef = ref<any>(null);
const isLoading = ref(false);
const isSubmitting = ref(false);
const data = ref(<any>[]);
const tempData = ref<any>([]);

const fetchData = async () => {
  isLoading.value = true;

  try {
    const res = await SystemService.queryConfig(props.rowId, props.category);
    data.value = res.sort((a, b) => a.name.localeCompare(b.name));

    data.value.forEach((item) => {
      item.valueString = JSON.stringify(item.value);
    });
    if (props.category === "public" && props.rowId != 0) {
      processPublicDefaultData();
    } else if (props.category === "account" || props.category === "party") {
      await filterAvailableOptions();
    }
  } catch (e) {
    console.log(e);
  } finally {
    isLoading.value = false;
  }
};

const show = (item: any) => {
  modalRef.value?.show(props.category, props.rowId, item.key, item);
};

const filterAvailableOptions = async () => {
  var configs = <any>[];
  Object.keys(showedConfig.value).forEach((key) => {
    const item = showedConfig.value[key];
    if (
      item.showedCategory.includes(props.category) &&
      (item.showedRole.includes("all") || item.showedRole.includes(props.role))
    ) {
      configs.push(item);
    }
  });
  tempData.value = [];
  configs.forEach(async (item) => {
    await SystemService.queryConfigByKey(
      props.category,
      props.rowId,
      item.key,
      { isInherit: true }
    ).then((res) => {
      if (res) {
        res.valueString = JSON.stringify(res.value);
        res.rowId = props.rowId;
        res.type = item.type;
        if (!item.getDefault && res.category == "Public") {
          if (item.key == "AutoCreateTradeAccountEnabled") {
            res.value["value"] = true;
            res.category = "Account";
          }
        }
        tempData.value.push(res);
      }
    });
    tempData.value.sort((a, b) => a.key.localeCompare(b.key));
  });

  data.value = tempData.value;
};

const processPublicDefaultData = () => {
  const publicDefaultData = props.defaultData;
  publicDefaultData.forEach((item) => {
    const index = data.value.findIndex((i) => i.key === item.key);
    if (index === -1) {
      item.default = true;
      item.valueString = JSON.stringify(item.value);
      item.rowId = props.rowId;
      data.value.push(item);
    }
  });
};

const addInput = (item: any) => {
  const index = data.value.findIndex((i) => i.key === item.key);
  const length = Object.keys(data.value[index].value).length;
  if (index !== -1) {
    data.value[index].value[length] = 0;
  }
};

const removeInput = (item: any, key: number) => {
  const index = data.value.findIndex((i) => i.key === item.key);
  if (index !== -1) {
    data.value[index].value.splice(key, 1);
  }
};

const onChange = async (item: any) => {
  isSubmitting.value = true;
  if (item.type === "input") {
    item.value = item.value.sort((a: any, b: any) => a - b);
  }
  try {
    const submitData = {
      value: item.value,
      key: item.key,
      name: item.name,
      description: item.description,
      dataFormat: item.dataFormat,
    };
    await SystemService.updateConfigByKey(
      props.category,
      props.rowId,
      item.key,
      submitData
    ).then(() => {
      ElNotification({
        title: "Update Success",
        message: item.key,
        type: "success",
      });
    });
    await fetchData();
  } catch (e) {
    console.log(e);
    ElNotification({
      title: "Update Failed",
      message: item.key,
      type: "error",
    });
  }

  isSubmitting.value = false;
};

const handleInputChange = (item: any, key: string, val: string) => {
  const numVal = parseFloat(val);
  item.value[key] = isNaN(numVal) ? 0 : numVal;
};

onMounted(() => {
  fetchData();
});
</script>
<style scoped sccs>
:deep .input-with-select .el-input-group__prepend {
  background-color: var(--el-fill-color-blank);
  padding: 0 10px !important;
}
</style>
