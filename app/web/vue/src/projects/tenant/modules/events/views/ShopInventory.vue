<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <el-switch
          class="me-4 min-w-150px"
          v-model="criteria.status"
          :active-value="null"
          :active-text="t('fields.all')"
          :inactive-value="EventShopItemStatusTypes.Published"
          :inactive-text="t('tip.published')"
          :disabled="isLoading"
          @change="fetchData(1)"
        ></el-switch>
        <el-select
          v-model="criteria.type"
          clearable
          @change="fetchData(1)"
          :placeholder="t('fields.type')"
          :disabled="isLoading"
          class="me-4"
        >
          <el-option
            v-for="item in allOPtions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
        <el-select
          v-model="criteria.category"
          clearable
          @change="fetchData(1)"
          :placeholder="t('fields.category')"
          :disabled="isLoading"
        >
          <el-option
            v-for="item in categoryData"
            :key="item.value"
            :label="getCategoryName(item.value)"
            :value="item.value"
          ></el-option>
        </el-select>
        <el-select
          v-model="criteria.accessSite"
          clearable
          @change="fetchData(1)"
          :placeholder="t('fields.sites')"
          :disabled="isLoading"
          class="ms-2"
        >
          <el-option
            v-for="item in ConfigSiteTypesSelections"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          ></el-option>
        </el-select>
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="create()">{{
          $t("action.addItem")
        }}</el-button>
      </div>
    </div>
    <div class="card-body">
      <table class="table align-middle table-row-dashed fs-6 gy-5 table-hover">
        <thead>
          <tr class="text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("tip.published") }}</th>
            <th>{{ $t("fields.itemId") }}</th>
            <th>{{ $t("fields.eventId") }}</th>
            <th>{{ $t("fields.itemName") }}</th>
            <th>{{ $t("fields.points") }}</th>
            <th>{{ $t("fields.type") }}</th>
            <th>{{ $t("fields.category") }}</th>
            <th>{{ $t("fields.site") }}</th>
            <th>{{ $t("fields.createdOn") }}</th>
            <th>{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && data.length == 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr
            v-for="(item, index) in data"
            :key="index"
            :class="{
              'tr-select': item.id === accountSelected,
            }"
            @click="selectedAccount(item.id)"
          >
            <td>
              <el-switch
                v-model="item.status"
                active-color="#13ce66"
                inactive-color="#ff4949"
                :active-value="EventShopItemStatusTypes.Published"
                @click="publishToggle(item)"
              ></el-switch>
            </td>
            <td>{{ item.id }}</td>
            <td>{{ item.eventId }}</td>
            <td>{{ item.name }}</td>
            <td><ShopPoints :points="item.point" /></td>
            <td>
              <el-tag
                v-if="item.type == EventShopItemTypes.Product"
                effect="dark"
                >{{ EventItemTypes[item.type] }}</el-tag
              >
              <el-tag v-else type="success" effect="dark">{{
                EventItemTypes[item.type]
              }}</el-tag>
            </td>
            <td>
              <span>
                {{ getCategoryName(item.category) }}
              </span>
            </td>
            <td>
              <span
                v-for="(i, index) in ConfigSiteTypesSelections"
                :key="index"
              >
                <span
                  class="badge text-bg-warning me-1"
                  v-if="item.accessSites.includes(i.value)"
                >
                  {{ i.label }}
                </span>
              </span>
            </td>
            <td>
              <TimeShow type="GMToneLiner" :date-iso-string="item.createdOn" />
            </td>
            <td>
              <el-button type="primary" class="me-5" @click="edit(item)">
                {{ $t("action.edit") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="fetchData" :criteria="criteria" />
    </div>
  </div>
  <CreateItem
    ref="createRef"
    @submit="fetchData(1)"
    :categoryData="categoryData"
    :getCategoryName="getCategoryName"
  />
  <EditItem
    ref="editRef"
    @submit="fetchData(1)"
    :categoryData="categoryData"
    :getCategoryName="getCategoryName"
  />
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import CreateItem from "../components/shop/CreateItem.vue";
import EditItem from "../components/shop/EditItem.vue";
import EventsServices from "../services/EventsServices";
import {
  EventShopItemStatusTypes,
  EventShopItemTypes,
  EventItemTypes,
  allOPtions,
} from "@/core/types/shop/ShopItemTypes";
import { ElNotification } from "element-plus";
import { ConfigSiteTypesSelections } from "@/core/types/SiteTypes";
import i18n from "@/core/plugins/i18n";
import { getLanguage } from "@/core/types/LanguageTypes";

const t = i18n.global.t;
const isLoading = ref(false);
const createRef = ref<InstanceType<typeof CreateItem>>();
const editRef = ref<InstanceType<typeof EditItem>>();
const data = ref(<any>[]);
const categoryData = ref(<any>[]);
const accountSelected = ref(0);
const eventOptions = ref(<any>[]);
const criteria = ref<any>({
  page: 1,
  pageSize: 10,
  status: null,
  sortField: "createdOn",
  sortFlag: true,
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await EventsServices.getShopItemList(criteria.value);
    data.value = res.data;
    criteria.value = res.criteria;
  } catch (error) {
    console.log(error);
  } finally {
    isLoading.value = false;
  }
};

const fetchCategoryData = async () => {
  try {
    const res = await EventsServices.queryShopCategoryList();
    categoryData.value = res;
  } catch (error) {
    console.log(error);
  }
};

const publishToggle = async (item: any) => {
  try {
    if (item.status == EventShopItemStatusTypes.Published) {
      await EventsServices.publishItem(item.id);
      ElNotification({
        title: "Success",
        message: t("tip.itemPublished"),
        type: "success",
      });
    } else {
      await EventsServices.unpublishItem(item.id);
      ElNotification({
        title: "Success",
        message: t("tip.itemUnpublished"),
        type: "success",
      });
    }
  } catch (error) {
    console.log(error);
    ElNotification({
      title: "Error",
      message: t("error.failedToUpdate"),
      type: "error",
    });
  }
};
const fetchEventsData = async () => {
  isLoading.value = true;
  const res = await EventsServices.queryEventsList();
  eventOptions.value = res.data;
  isLoading.value = false;
};
const create = () => {
  createRef.value?.show(eventOptions.value);
};

const edit = (item: any) => {
  editRef.value?.show(item);
};
const selectedAccount = (id: number) => {
  accountSelected.value = id;
};

const getCategoryName = (category: number) => {
  const categoryItem = Object.values(categoryData.value).find(
    (item: any) => item.value === category
  );

  if (categoryItem) {
    return categoryItem.data[getLanguage.value] || "";
  }
  return "";
};

onMounted(() => {
  fetchEventsData();
  fetchData(1);
  fetchCategoryData();
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
