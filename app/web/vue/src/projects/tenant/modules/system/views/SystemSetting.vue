<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <div class="d-flex justify-content-between">
          <ul
            class="nav nav-custom nav-tabs nav-line-tabs nav-line-tabs-2x border-0 fs-4 fw-semobold mb-8"
          >
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4 active"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(0)"
                >{{ $t("type.siteType." + SiteTypes.Default) }}</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(1)"
                >{{ $t("type.siteType." + SiteTypes.BritishVirginIslands) }}
              </a>
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(2)"
                >{{ $t("type.siteType." + SiteTypes.Australia) }}</a
              >
            </li>

            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(3)"
                >{{ $t("type.siteType." + SiteTypes.China) }}</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(4)"
                >{{ $t("type.siteType." + SiteTypes.Taiwan) }}</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(5)"
                >{{ $t("type.siteType." + SiteTypes.Vietnam) }}</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(6)"
                >{{ $t("type.siteType." + SiteTypes.Japan) }}</a
              >
            </li>
            <li class="nav-item">
              <a
                class="nav-link text-active-primary pb-4"
                data-bs-toggle="tab"
                href="#"
                @click="changeTab(7)"
                >{{ $t("type.siteType." + SiteTypes.Mongolia) }}</a
              >
            </li>
          </ul>
        </div>
      </div>
      <div class="card-toolbar">
        <button
          class="btn btn-danger btn-sm"
          :disabled="isReloading"
          @click="reload"
        >
          Reload
        </button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5">
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <td>ID</td>
            <td>Name</td>
            <td>Update On</td>
            <td width="*">Value</td>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && configurations.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold fs-4">
          <tr v-for="(item, index) in configurations" :key="index">
            <td>{{ item.id ? item.id : "Default Value" }}</td>
            <td>
              {{ item.name }}<br />
              <span class="text-gray-600">{{ item.key }}</span>
            </td>
            <td>
              <TimeShow
                :date-iso-string="item.updatedOn"
                v-if="item.updatedOn"
              />
            </td>
            <td>
              <div class="input-group">
                <input type="text" class="form-control" v-model="item.value" />
                <button
                  v-if="item.id"
                  class="btn btn-primary"
                  type="button"
                  @click="updateValue(item)"
                  :disabled="isSubmitting"
                >
                  Update
                </button>
                <button
                  v-if="!item.id"
                  class="btn btn-success"
                  type="button"
                  @click="updateValue(item)"
                  :disabled="isSubmitting"
                >
                  Update
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from "vue";
import SystemService from "../services/SystemService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import { SiteTypes } from "@/core/types/SiteTypes";

const isLoading = ref(true);
const isReloading = ref(false);
const isSubmitting = ref(false);
const configurations = ref([]);
const defaultConfig = ref([]);
const allConfig = ref([]);

onMounted(() => {
  fetchData();
});

const changeTab = (siteId: number) => {
  console.log(siteId);
  configurations.value = allConfig.value.filter(
    (item) => item.siteId === siteId
  );
  console.log(configurations.value);
  configurationsCheck(configurations.value, siteId);
  console.log(configurations.value);
};

const fetchData = async (/*siteId: number*/) => {
  isLoading.value = true;
  try {
    isLoading.value = false;
    allConfig.value = await SystemService.getAllConfigurations();
    changeTab(0);
  } catch (error) {
    console.log(error);
  }
  isLoading.value = false;
};

const configurationsCheck = async (config: any, siteId: number) => {
  for (let i = 0; i < defaultConfig.value.length; i++) {
    const defaultItem = defaultConfig.value[i];
    const configItem = config.find(
      (item: any) => item.name === defaultItem.name
    );
    if (!configItem) {
      const newItem = {
        ...defaultItem,
        id: null,
        siteId: siteId,
        updatedOn: null,
      };
      config.push(newItem);
    }
  }
};

const updateValue = (item) => {
  isSubmitting.value = true;
  SystemService.updateConfiguration(item.siteId, item.key, item.value).then(
    () => {
      MsgPrompt.success("Update successfully");
      isSubmitting.value = false;
    }
  );
};

const reload = () => {
  isReloading.value = true;
  SystemService.reloadConfiguration().then(() => {
    isReloading.value = false;
  });
};
</script>
