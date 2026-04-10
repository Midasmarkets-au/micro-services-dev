<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">{{ $t("title.notice") }}</div>
      <div class="card-toolbar">
        <!-- <el-select
          v-model="criteria.category"
          class="me-3"
          style="width: 160px"
          @change="onCategoryChange"
        >
          <el-option
            v-for="item in categorySelections"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select> -->
        <button class="btn btn-success" @click="createNoticeRef?.show()">
          {{ $t("action.new") }}
        </button>
      </div>
    </div>
    <div class="card-body overflow-auto" style="white-space: nowrap">
      <table
        class="table align-middle fs-6 gy-5"
        id="kt_ecommerce_add_product_reviews"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th>{{ $t("tip.published") }}</th>
            <th>id</th>
            <th>{{ $t("fields.title") }}</th>
            <th>{{ $t("fields.category") }}</th>
            <th>{{ $t("fields.language") }}</th>
            <th>{{ $t("tip.publishedDate") }}</th>
            <th>{{ $t("action.detail") }}</th>
            <th>{{ $t("action.delete") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && noticeList.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, index) in noticeList" :key="index">
            <td class="d-flex gap-4 align-items-center">
              <el-switch
                v-model="item.publishStatus"
                @change="() => publishToggle(item)"
                width="50"
              ></el-switch>
              <div
                class="spinner-border text-primary"
                v-show="item.publishLoading"
              ></div>
            </td>
            <td>{{ item.id }}</td>
            <td class="">{{ item["contents"]["en-us"].title }}</td>
            <td>{{ getCategoryLabel(item.category) }}</td>
            <td>
              <span
                class="badge text-bg-warning me-1"
                v-for="(_, key, index) in item.contents"
                :key="index"
                >{{ key }}</span
              >
            </td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>

            <td>
              <div
                class="btn btn-light btn-success btn-sm"
                @click="showNoticeDetail(item)"
              >
                {{ $t("title.details") }}
              </div>
            </td>
            <td>
              <el-popconfirm
                width="160"
                title="Confirm to delete"
                confirm-button-text="Confirm"
                @confirm="deleteItem(item.id)"
              >
                <template #reference>
                  <el-button class="btn btn-light btn-danger btn-sm">{{
                    $t("action.delete")
                  }}</el-button>
                </template>
              </el-popconfirm>
            </td>
          </tr>
        </tbody>
      </table>
      <TableFooter @page-change="pageChange" :criteria="criteria" />
    </div>
    <NoticeDetail ref="noticeDetailRef" @event-submit="onEventSubmitted" />
    <CreateNotice ref="createNoticeRef" @event-submit="onEventSubmitted" />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import DateUtils from "@/core/utils/DateUtils";
import GlobalService from "../../../services/TenantGlobalService";
import { TopicTypes } from "@/core/types/TopicTypes";
import { TopicCategoryTypes } from "@/core/types/TopicCategoryTypes";
import LoadingRing from "@/components/LoadingRing.vue";
import NoDataBox from "@/components/NoDataBox.vue";
import CreateNotice from "../components/CreateNotice.vue";
import NoticeDetail from "../components/NoticeDetail.vue";
import TopicService from "../services/TopicService";
import TableFooter from "@/components/TableFooter.vue";
import { ElNotification } from "element-plus";

const noticeList = ref(Array<any>());
const isLoading = ref(true);
const createNoticeRef = ref<InstanceType<typeof CreateNotice>>();
const noticeDetailRef = ref<any>(null);

const criteria = ref({
  type: TopicTypes.Notice,
  category: TopicCategoryTypes.All,
  page: 1,
  size: 20,
  sortField: "createdOn",
} as any);

const categorySelections = [
  { label: "Activity", value: TopicCategoryTypes.Activity },
  { label: "Information", value: TopicCategoryTypes.Information },
];

onMounted(async () => {
  fetchData(1);
});

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await GlobalService.queryEventTopics(criteria.value);

    if (res && res.data) {
      noticeList.value = res.data.map((item) => ({
        ...item,
        publishStatus: switchOn(item.effectiveTo),
        publishLoading: false,
      }));
      criteria.value = {
        ...res.criteria,
        category: res?.criteria?.category ?? criteria.value.category,
      };
    }
  } catch (e) {
    console.log(e);
  }

  isLoading.value = false;
};
const pageChange = (page: number) => {
  fetchData(page);
};

const onCategoryChange = () => {
  fetchData(1);
};

const getCategoryLabel = (category: TopicCategoryTypes) => {
  if (category === TopicCategoryTypes.Activity) return "Activity";
  if (category === TopicCategoryTypes.Information) return "Information";
  if (category === TopicCategoryTypes.All) return "All";
  return "-";
};

const publishToggle = (item: any) => {
  isLoading.value = true;
  item.publishLoading = true;

  try {
    const newDate = new Date();
    if (item.publishStatus == true) {
      newDate.setFullYear(newDate.getFullYear() + 50);
    } else {
      newDate.setFullYear(newDate.getFullYear() - 50);
    }

    const data = {
      type: TopicTypes.Notice,
      title: item.title,
      effectiveFrom: item.effectiveFrom,
      effectiveTo: newDate,
    };
    TopicService.updateNoticeTime(item.id, data);
    notification(1);
  } catch (e) {
    notification(0);
  }
  isLoading.value = false;
  item.publishLoading = false;
};

const switchOn = (endDate) => {
  const today = new Date();
  const eventEndDate = new Date(endDate);
  today.setHours(0, 0, 0, 0);
  return eventEndDate > today;
};

const showNoticeDetail = (detail: any) => {
  noticeDetailRef.value?.show(detail);
};

const onEventSubmitted = () => {
  fetchData(1);
};

const deleteItem = async (itemId: number) => {
  isLoading.value = true;
  try {
    await TopicService.deleteNotice(itemId);
    notification(1);
    fetchData(1);
  } catch (e) {
    notification(0);
  }
  isLoading.value = false;
};

const notification = (type: number) => {
  if (type == 1) {
    ElNotification({
      title: "Success",
      message: "Update success!",
      type: "success",
      offset: 150,
    });
  } else {
    ElNotification({
      title: "Error",
      message: "Update failed!",
      type: "error",
      offset: 150,
    });
  }
};
</script>
<style scoped lang="scss"></style>
