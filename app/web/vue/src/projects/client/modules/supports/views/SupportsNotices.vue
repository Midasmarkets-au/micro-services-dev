<template>
  <div class="flex justify-between sm:d-inline-flex sm:flex-row px-2 sm:px-0">
    <div class="sub-menu sub-h2 d-flex" style="white-space: nowrap">
      <router-link to="/supports" class="sub-menu-item">{{
        $t("title.contactUs")
      }}</router-link>
      <router-link to="/supports/notices" class="sub-menu-item active">{{
        $t("title.notices")
      }}</router-link>
      <router-link to="/supports/documents" class="sub-menu-item">{{
        $t("title.documents")
      }}</router-link>
      <router-link
        to="/supports/cases"
        class="sub-menu-item"
        v-if="$cans(['TenantAdmin'])"
        >{{ $t("title.cases") }}</router-link
      >
    </div>
    <div class="card-toolbar mt-2 sm:mt-0" v-if="!isMobile">
      <div class="card-title"></div>
      <div class="me-lg-4 d-flex items-center">
        <label class="me-3 whitespace-nowrap" for=""
          >{{ $t("title.sortBy") }}:</label
        >
        <div class="card-select">
          <el-select
            v-model="criteria.sortFlag"
            size="large"
            style="width: 150px"
            @change="fetchData(1)"
          >
            <el-option
              v-for="(item, index) in sortSelections"
              :key="index"
              :value="item.value"
              :label="item.label"
            />
          </el-select>
          <SvgIcon
            name="filter"
            path="notice"
            style="position: absolute; top: 11px; left: 10px"
          />
        </div>
      </div>
    </div>
  </div>
  <div class="mb-5 mb-xl-10 mx-0 px-2 sm:px-0">
    <div class="card card-flush sm:py-4">
      <div class="card-header" v-if="isMobile">
        <div class="card-title"></div>
        <div class="card-toolbar">
          <label class="me-3 whitespace-nowrap" for=""
            >{{ $t("title.sortBy") }}:</label
          >
          <div class="card-select">
            <el-select
              v-model="criteria.sortFlag"
              size="large"
              style="width: 150px"
              @change="fetchData(1)"
            >
              <el-option
                v-for="(item, index) in sortSelections"
                :key="index"
                :value="item.value"
                :label="item.label"
              />
            </el-select>
            <SvgIcon
              name="filter"
              path="notice"
              style="position: absolute; top: 11px; left: 10px"
            />
          </div>
        </div>
      </div>

      <div class="pt-0">
        <table class="table table-row-dashed gy-5 my-0 notice-table">
          <thead>
            <tr class="text-start text-uppercase gs-0">
              <th class="col-4 col-lg-3 ps-10 px-lg-9 text-start">
                {{ $t("fields.date") }}
              </th>
              <th class="col-8 col-lg-7 px-8 text-start">
                {{ $t("fields.event") }}
              </th>
              <th v-if="!isMobile" class="text-start px-8 col-lg-2">
                {{ $t("fields.actions") }}
              </th>
            </tr>
          </thead>

          <tbody v-if="isLoading">
            <LoadingRing />
          </tbody>
          <tbody v-else-if="!isLoading && noticeList.length === 0">
            <NoDataBox />
          </tbody>

          <tbody v-else>
            <tr
              v-for="(item, index) in noticeList"
              :key="index"
              class="notice_tr"
            >
              <td v-if="!isMobile" class="ps-8 px-lg-9">
                <TimeShow
                  :date-iso-string="item.createdOn"
                  format="ddd, DD MMM YYYY"
                  style="font-size: 18px; color: #666666"
                />
              </td>
              <td v-if="isMobile" class="ps-8 px-lg-9">
                <div>
                  <TimeShow
                    :date-iso-string="item.createdOn"
                    format="dddd"
                    style="color: black"
                  />
                </div>
                <TimeShow
                  class="fs-7"
                  :date-iso-string="item.createdOn"
                  format="DD MMM YYYY"
                />
              </td>
              <td>
                <h4
                  class="fw-semibold cursor-pointer text-hover-primary"
                  :class="isMobile ? 'width-mobile' : ''"
                  @click="showTopicDetailPanel(item)"
                >
                  {{ item.title }}
                </h4>
                <p
                  class="fs-6 mt-1"
                  :class="isMobile ? 'width-mobile' : 'width-control'"
                >
                  {{ item.contentInText }}
                </p>
              </td>

              <td
                v-if="!isMobile"
                class="cursor-pointer text-hover-secondary"
                @click="showTopicDetailPanel(item)"
              >
                <button
                  class="btn btn-sm btn-light-secondary flex items-center btn-bordered group"
                >
                  <div
                    class="w-[14px] h-[14px] bg-[url('/images/icons/notice/eye.png')] bg-no-repeat bg-center bg-contain transition group-hover:bg-[url('/images/icons/notice/eye_light.png')]"
                  ></div>
                  <span
                    class="ml-2 fs-6 transition-colors text-[#868d98] group-hover:text-white"
                    >{{ $t("action.viewDetails") }}</span
                  >
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <TableFooter :page-change="fetchData" :criteria="criteria" />
      </div>
    </div>
  </div>
  <TopicDetailsCard ref="topicDetailsCardRef" />
</template>

<script lang="ts" setup>
import { useStore } from "@/store";
import { ref, onMounted, computed } from "vue";
import SupportService from "../services/SupportService";
import TopicDetailsCard from "../components/TopicDetailsCard.vue";
import { isMobile } from "@/core/config/WindowConfig";
import { useI18n } from "vue-i18n";
import { getLanguage } from "@/core/types/LanguageTypes";
import SvgIcon from "@/projects/client/components/SvgIcon.vue";

const isLoading = ref(true);
const noticeList = ref<any>([]);
const store = useStore();
// const language = store.state.AuthModule.user.language as string;

const topicDetailsCardRef = ref<InstanceType<typeof TopicDetailsCard>>();
const criteria = ref<any>({
  page: 1,
  size: 10,
  language: getLanguage,
  sortFlag: true,
  // keyword: "",
});

const { t } = useI18n();

const sortSelections = computed(() => [
  {
    label: t("fields.latest"),
    value: true,
  },
  {
    label: t("fields.earliest"),
    value: false,
  },
]);

const showTopicDetailPanel = (item) => {
  // console.log(item);
  topicDetailsCardRef.value?.show(item);
};
const extractText2 = (htmlString: string) => {
  const doc = htmlString.replace(/(<([^>]+)>)/gi, " ");
  return doc.replace(/&nbsp;/gi, " ");
};

const fetchData = async (_page: number) => {
  isLoading.value = true;
  criteria.value.page = _page;
  try {
    const res = await SupportService.getTopicNotices(criteria.value);
    noticeList.value = res.data.map((item: any) => ({
      topicId: item.id,
      ...item.contents[getLanguage.value],
      contentInText: extractText2(item.contents[getLanguage.value].content),
      createdOn: item.createdOn,
    }));
    isLoading.value = false;
  } catch (error) {
    // console.log(error);
  }
};

onMounted(() => {
  fetchData(1);
});
</script>
<style scoped lang="scss">
.width-control {
  display: inline-block;
  max-width: 650px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.width-mobile {
  display: inline-block;
  max-width: 200px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.notice_tr {
  &:hover {
    background-color: #f2f4f7;
  }
}
</style>
