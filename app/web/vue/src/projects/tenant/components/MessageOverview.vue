<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <ul class="nav nav-pills nav-pills-custom row position-relative mx-0">
          <li class="nav-item col-4 mx-0 p-0">
            <a
              class="nav-link active d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#message-all"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.all")
              }}</span>
              <span
                class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
              ></span>
            </a>
          </li>
          <li class="nav-item col-4 mx-0 px-0">
            <a
              class="nav-link d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#message-notices"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.deposit")
              }}</span>
              <span
                class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
              ></span>
            </a>
          </li>
          <li class="nav-item col-4 mx-0 px-0">
            <a
              class="nav-link d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#message-news"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.withdraw")
              }}</span>
              <span
                class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
              ></span>
            </a>
          </li>

          <li class="nav-item col-4 mx-0 px-0">
            <a
              class="nav-link d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#dashboard-news"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.newUserOpenAccount")
              }}</span>
              <span
                class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
              ></span>
            </a>
          </li>

          <li class="nav-item col-4 mx-0 px-0">
            <a
              class="nav-link d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#dashboard-notices"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.notice")
              }}</span>
              <span
                class="bullet-custom position-absolute z-index-2 bottom-0 w-100 h-4px bg-primary rounded"
              ></span>
            </a>
          </li>
        </ul>
      </div>

      <div class="card-toolbar">
        <button
          class="btn btn-sm btn-icon btn-light-primary border-0 me-n3"
          @click="publicTopicFormRef?.show()"
        >
          <span class="svg-icon svg-icon-1">
            <inline-svg src="/images/icons/arrows/arr075.svg" />
          </span>
        </button>
        <PublishTopicForm
          ref="publicTopicFormRef"
          @event-submit="onEventSubmitted"
        />
      </div>
    </div>

    <div class="tab-content px-8 pb-4" v-if="!isLoading">
      <div
        v-for="(eventList, index) in [
          {
            id: 'dashboard-calendar',
            list: calendarList,
          },
          {
            id: 'dashboard-notices',
            list: noticeList,
          },
          {
            id: 'dashboard-news',
            list: newsList,
          },
        ]"
        :key="index"
        class="tab-pane fade show"
        :class="{ active: index === 0 }"
        :id="eventList.id"
      >
        <table
          class="table table-row-dashed fs-6 gy-5 my-0"
          id="kt_ecommerce_add_product_reviews"
        >
          <tbody v-if="!isLoading && eventList.list.length === 0">
            <tr>
              <td colspan="8">{{ $t("tip.nodata") }}</td>
            </tr>
          </tbody>
          <tbody v-else class="fw-semibold text-gray-900">
            <tr v-for="(item, idx) in eventList.list" :key="idx">
              <td>
                <div
                  class="d-flex text-dark text-gray-800 text-hover-primary cursor-pointer"
                  @click="showTopicDetailPanel(item.details)"
                >
                  <div class="symbol symbol-circle symbol-25px me-3">
                    <div :class="`symbol-label bg-light-${item.color}`">
                      <span :class="`text-${item.color}`">{{ item.icon }}</span>
                    </div>
                  </div>

                  <span class="fw-bold">{{ item.title }}</span>
                </div>
              </td>

              <td class="text-end">
                <span class="fw-semibold text-muted">{{ item.start }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
  <TopicDetailsCard ref="topicDetailCardRef" />
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
// import SupportService from "../modules/supports/services/SupportService";
import { useStore } from "@/store";
import DateUtils from "@/core/utils/DateUtils";
import TopicDetailsCard from "../../client/modules/supports/components/TopicDetailsCard.vue";
// import TopicDetailsCard from "@/components/TopicDetailsCard.vue";
import GlobalService from "../services/TenantGlobalService";
import { TopicTypes } from "@/core/types/TopicTypes";
import PublishTopicForm from "./PublishTopicForm.vue";

const isLoading = ref(true);
const calendarList = ref(Array<any>());
const newsList = ref(Array<any>());
const noticeList = ref(Array<any>());

const store = useStore();
const language = store.state.AuthModule.user.language;

const topicDetailCardRef = ref<InstanceType<typeof TopicDetailsCard>>();
const publicTopicFormRef = ref<InstanceType<typeof PublishTopicForm>>();

const parseData = (data: []) =>
  data.map((item: any, index: number) => ({
    icon: item.title[0].toUpperCase(),
    color: ["primary", "success", "danger", "warning"][index % 4],
    title: item.title,
    start: DateUtils.getDateAndTimeFromISOString(item.effectiveFrom),
    end: DateUtils.getDateAndTimeFromISOString(item.effectiveTo),
    details: {
      title: item.title,
      content: item.contents[language]?.content,
      start: DateUtils.getDateAndTimeFromISOString(item.effectiveFrom),
      end: DateUtils.getDateAndTimeFromISOString(item.effectiveTo),
    },
  }));

const getCalendarList = async () =>
  (calendarList.value = parseData(
    (
      await GlobalService.queryEventTopics({
        type: TopicTypes.Calendar,
        size: 10,
      })
    ).data
  ));

const getNewsList = async () => {
  newsList.value = parseData(
    (
      await GlobalService.queryEventTopics({
        type: TopicTypes.News,
        size: 10,
      })
    ).data
  );
};

const getNoticeList = async () => {
  noticeList.value = parseData(
    (
      await GlobalService.queryEventTopics({
        type: TopicTypes.Notice,
        size: 10,
      })
    ).data
  );
};

onMounted(() => {
  isLoading.value = true;
  Promise.all([getCalendarList(), getNewsList(), getNoticeList()])
    .catch((err) => {
      console.log(err);
    })
    .finally(() => {
      isLoading.value = false;
    });
});

const onEventSubmitted = (type: TopicTypes) =>
  ({
    [TopicTypes.Calendar]: getCalendarList,
    [TopicTypes.News]: getNewsList,
    [TopicTypes.Notice]: getNoticeList,
  }[type]());

const showTopicDetailPanel = (details: any) => {
  topicDetailCardRef.value?.show(details);
};
</script>

<style scoped></style>
