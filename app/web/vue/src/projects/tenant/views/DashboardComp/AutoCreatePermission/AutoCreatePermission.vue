<template>
  <div class="card w-300px">
    <div class="card-header">
      <div class="card-title">
        <p class="fs-6 fw-bold">
          {{ $t("title.autoCreateAccountPermission") }}
        </p>
      </div>
    </div>
    <div class="card-body" v-loading="isLoading">
      <div v-for="(item, index) in ConfigSiteTypesSelections" :key="index">
        <div v-if="item.value === 0">
          <p class="fw-bold">{{ $t("fields.mainSwitch") }}</p>
        </div>
        <div v-if="item.value === 1">
          <p class="fw-bold">{{ $t("fields.siteSwitch") }}</p>
        </div>
        <div
          class="d-flex align-items-center gap-5 justify-content-between w-125px"
        >
          <el-tag
            effect="plain"
            type="success"
            class="w-50px"
            v-if="item.value === 0"
            >{{ item.label }}</el-tag
          >
          <el-tag v-else effect="plain" type="warning" class="w-50px">{{
            item.label
          }}</el-tag>

          <el-switch
            style="
              --el-switch-on-color: #13ce66;
              --el-switch-off-color: #ff4949;
            "
            v-model="item.switchValue"
            inline-prompt
            active-text="ON"
            inactive-text="OFF"
            :active-value="true"
            :inactive-value="false"
            size="large"
            @change="onChange(item)"
            :disabled="mainSwitch.switchValue === false && item.value !== 0"
          ></el-switch>
        </div>
        <el-divider v-if="item.value == 0"></el-divider>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
import { ref, onMounted } from "vue";
import {
  SiteTypesValues,
  ConfigSiteTypesSelections,
} from "@/core/types/SiteTypes";
import SystemService from "@/projects/tenant/modules/system/services/SystemService";

const isLoading = ref(false);
const data = ref<any>(ConfigSiteTypesSelections.value);
const mainSwitch = ref<any>(ConfigSiteTypesSelections.value[0]);
const criteria = ref<any>({
  category: "public",
  rowIds: SiteTypesValues.value,
  keys: ["AutoCreateTradeAccountEnabled"],
});

const fecthData = async () => {
  isLoading.value = true;
  try {
    const res = await SystemService.queryConfigs(criteria.value);
    data.value.map((item) => {
      const match = res.data.find((x) => x.rowId === item.value);
      const defaultData = res.data.find((data) => data.rowId === 0);

      if (match) {
        item.data = match;
        item.switchValue = match.value["value"];
        item.isDefault = false;
      } else if (defaultData) {
        item.data = defaultData;
        item.switchValue = false;
        item.isDefault = true;
      }
    });
    mainSwitch.value = data.value.find((x) => x.value === 0);
  } catch (e) {
    console.log(e);
  }
  isLoading.value = false;
};

const onChange = async (item: any) => {
  isLoading.value = true;
  try {
    item.data.value["value"] = item.switchValue;
    const submitData = {
      value: item.data.value,
      key: item.data.key,
      name: item.data.name,
      description: item.data.description,
      dataFormat: item.data.dataFormat,
    };
    await SystemService.updateConfigByKey(
      "public",
      item.value,
      "AutoCreateTradeAccountEnabled",
      submitData
    );
    await fecthData();
  } catch (e) {
    console.log(e);
  }

  isLoading.value = false;
};

onMounted(async () => {
  await fecthData();
});
</script>
