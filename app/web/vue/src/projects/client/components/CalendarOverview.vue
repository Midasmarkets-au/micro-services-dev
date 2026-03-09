<template>
  <div class="card">
    <div class="card-header">
      <div class="card-title">
        <h3 class="d-flex align-items-center mx-1 m-0 fw-bold">
          {{ $t("title.notices") }}
        </h3>
      </div>
      <div class="card-toolbar">
        <router-link to="/supports/notices">{{
          $t("action.viewMore")
        }}</router-link>
      </div>
    </div>
    <div class="card-body pt-0 account-no-calander">
      <NoDataCentralBox v-if="noticeList.length === 0" />
      <template v-else>
        <div
          v-for="(item, index) in noticeList"
          :key="index"
          class="row row-color-border h-40px mb-10 mt-7"
        >
          <div class="col-4 col-lg-3">
            <label class="text-gray-600">
              <TimeShow :date-iso-string="item.effectiveFrom" format="dddd" />
            </label>
            <label class="text-black"
              ><TimeShow
                :date-iso-string="item.effectiveFrom"
                format="MMM YYYY"
            /></label>
          </div>
          <div class="col-8 col-lg-9">
            <div class="px-2">
              <label
                class="text-nowrap overflow-hidden w-100 cursor-pointer text-hover-link"
                style="text-overflow: ellipsis"
                @click="showTopicDetailPanel(item)"
                >{{ item.title }}</label
              >
              <label class="text-gray-600 d-flex gap-1 fs-8">
                {{ $t("tip.publishedOn") }}
                <TimeShow
                  :date-iso-string="item.createdOn"
                  format="YYYY-M-D HH:mm"
                />
              </label>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
  <TopicDetailsCard ref="topicDetailCardRef" />
</template>

<script setup lang="ts">
import { useStore } from "@/store";
import { onMounted, ref } from "vue";
import SupportService from "../modules/supports/services/SupportService";
import TopicDetailsCard from "../modules/supports/components/TopicDetailsCard.vue";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import TimeShow from "@/components/TimeShow.vue";
import { getLanguage } from "@/core/types/LanguageTypes";

const store = useStore();
const isLoading = ref(true);
const noticeList = ref(Array<any>());
// const language = store.state.AuthModule.user.language;
const topicDetailCardRef = ref<InstanceType<typeof TopicDetailsCard>>();

const showTopicDetailPanel = (detail) => {
  console.log(detail);
  topicDetailCardRef.value?.show(detail);
};

const extractText = (htmlString: string) => {
  const parser = new DOMParser();
  const doc = parser.parseFromString(htmlString, "text/html");
  return doc.body.textContent || "";
};

const getTopicContentByLanguage = (topic: any) => {
  if (topic.contents[getLanguage.value]) {
    return topic.contents[getLanguage.value].content;
  } else {
    return topic.contents["en-us"].content;
  }
};

const getTopicTitleByLanguage = (topic: any) => {
  if (topic.contents[getLanguage.value]) {
    return topic.contents[getLanguage.value].title;
  } else {
    return topic.contents["en-us"].title;
  }
};

onMounted(async () => {
  isLoading.value = true;
  try {
    const res = await SupportService.getTopicNotices({ size: 8 });
    noticeList.value = res.data.map((item: any) => ({
      ...item.contents[getLanguage.value],
      topicId: item.id,
      title: getTopicTitleByLanguage(item),
      contentInText: extractText(getTopicContentByLanguage(item)),
      createdOn: item.createdOn,
      effectiveFrom: item.effectiveFrom,
    }));
    isLoading.value = false;
  } catch (error) {
    // console.log(error);
  }
});
</script>

<style scoped lang="scss">
.row-color-border {
  //border-left: 3px solid rgba(255, 212, 0, 1);
  margin-left: 5px;
  &:nth-child(3n + 1) {
    border-left: 3px solid #000f32;
  }

  &:nth-child(3n + 2) {
    border-left: 3px solid #0c45a4;
  }

  &:nth-child(3n + 3) {
    border-left: 3px solid #6b7ad4;
  }
}

.card-toolbar > a {
  color: #868d98;
  &:hover {
    color: #000f32;
    font-weight: 500;
  }
}
.account-no-calander {
  min-height: 648px;
  @media (max-width: 1512px) {
    min-height: 648px;
  }
}
</style>
