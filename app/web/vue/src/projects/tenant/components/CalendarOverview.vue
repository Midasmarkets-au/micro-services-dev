<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <ul class="nav nav-pills nav-pills-custom row position-relative mx-0">
          <li class="nav-item col-4 mx-0 px-0">
            <a
              class="nav-link active d-flex justify-content-center w-100 border-0 h-100"
              data-bs-toggle="pill"
              href="#dashboard-notices"
            >
              <span class="nav-text text-gray-800 fw-bold fs-6 mb-3">{{
                $t("title.notices")
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

    <div class="card-body">
      <table
        class="table table-row-dashed fs-6 gy-5 my-0"
        id="kt_ecommerce_add_product_reviews"
      >
        <tbody v-if="!isLoading && noticeList.length === 0">
          <tr>
            <td colspan="8">{{ $t("tip.nodata") }}</td>
          </tr>
        </tbody>
        <tbody v-else class="fw-semibold text-gray-900">
          <tr v-for="(item, idx) in noticeList" :key="idx">
            <td>
              <div
                class="d-flex text-dark text-gray-800 text-hover-primary cursor-pointer"
                @click="showTopicDetailPanel(item)"
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
  <TopicDetailsCard ref="topicDetailCardRef" />
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useStore } from "@/store";
import TopicDetailsCard from "../../client/modules/supports/components/TopicDetailsCard.vue";
import GlobalService from "../services/TenantGlobalService";
import { TopicTypes } from "@/core/types/TopicTypes";
import PublishTopicForm from "./PublishTopicForm.vue";

const isLoading = ref(true);
const noticeList = ref(Array<any>());

const store = useStore();
const language = ref(store.state.AuthModule?.user?.language ?? "en-us");

const topicDetailCardRef = ref<InstanceType<typeof TopicDetailsCard>>();
const publicTopicFormRef = ref<InstanceType<typeof PublishTopicForm>>();

const extractText = (htmlString: string) => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(htmlString, "text/html");
  return doc.body.textContent || "";
};

const getNoticeList = async () => {
  const res = await GlobalService.queryEventTopics({
    type: TopicTypes.Notice,
    size: 10,
  });
  noticeList.value = res.data.map((item, index) => ({
    icon: item.title[0].toUpperCase(),
    color: ["primary", "success", "danger", "warning"][index % 4],
    topicId: item.id,
    ...item.contents[language.value],
    contentInText: extractText(item.contents[language.value]?.content ?? ""),
    createdOn: item.createdOn,
    effectiveFrom: item.effectiveFrom,
  }));
};

onMounted(() => {
  isLoading.value = true;
  getNoticeList();
});

const onEventSubmitted = () => getNoticeList();

const showTopicDetailPanel = (details: any) => {
  topicDetailCardRef.value?.show(details);
};
</script>

<style scoped></style>
