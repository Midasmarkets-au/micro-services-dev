<template>
  <div class="card mb-5 mb-xl-8">
    <div class="card-header">
      <div class="card-title">
        {{ $t("title.emailTemplate") }}
      </div>
      <div class="card-toolbar">
        <el-button type="success" @click="CreateEmailTemplateRef?.show()">
          {{ $t("action.new") }}
        </el-button>
      </div>
    </div>
    <div class="card-body">
      <table
        class="table align-middle table-row-dashed fs-6 gy-5"
        id="table_accounts_requests"
      >
        <thead>
          <tr class="text-start text-muted fw-bold fs-7 text-uppercase gs-0">
            <th class="">id</th>
            <th class="">{{ $t("fields.key") }}</th>
            <th class="">{{ $t("fields.language") }}</th>
            <th class="">{{ $t("fields.updated_at") }}</th>
            <th class="">{{ $t("action.action") }}</th>
          </tr>
        </thead>
        <tbody v-if="isLoading">
          <LoadingRing />
        </tbody>
        <tbody v-else-if="!isLoading && emailList.length === 0">
          <NoDataBox />
        </tbody>
        <tbody v-else>
          <tr v-for="(item, index) in emailList" :key="index">
            <td>{{ item.id }}</td>
            <td>{{ item.title }}</td>
            <td>
              <span v-for="(i, index) in LanguageTypes.all" :key="index">
                <span
                  class="badge text-bg-warning me-1"
                  v-if="i.code in item.contents"
                  >{{ i.code }}</span
                >
                <span v-else class="badge text-bg-secondary me-1">{{
                  i.code
                }}</span>
              </span>
            </td>
            <td>
              <TimeShow type="inFields" :date-iso-string="item.createdOn" />
            </td>
            <!-- <td>
              <el-button type="success" @click="showEmailDetail(item)">
                {{ $t("title.details") }}
              </el-button>
            </td> -->
            <td>
              <el-button type="success" @click="detail(item)">
                {{ $t("title.details") }}
              </el-button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <EmailDetail ref="emailDetailRef" @event-submit="onCreateSubmitted" />
    <CreateEmailTemplate
      ref="CreateEmailTemplateRef"
      @event-submit="onCreateSubmitted"
    />
    <EmailTemplateDetail
      ref="emailDetailRefTwo"
      @event-submit="onCreateSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { TopicTypes } from "@/core/types/TopicTypes";
import GlobalService from "../../../services/TenantGlobalService";
import EmailDetail from "../components/EmailDetail.vue";
import CreateEmailTemplate from "../components/CreateEmailTemplate.vue";
import EmailTemplateDetail from "../components/EmailTemplateDetail.vue";
import { LanguageCodes, LanguageTypes } from "@/core/types/LanguageTypes";
const CreateEmailTemplateRef = ref<InstanceType<typeof CreateEmailTemplate>>();

const emailDetailRef = ref<any>(null);
const emailDetailRefTwo = ref<any>(null);
const isLoading = ref(false);
const emailList = ref(Array<any>());

const defaultLayout = ref(null);

onMounted(async () => {
  fetchData();
});

const fetchData = async () => {
  isLoading.value = true;
  emailList.value = [];
  emailList.value = (
    await GlobalService.queryEventTopics({
      type: TopicTypes.Email,
      size: 100,
    })
  ).data;
  emailList.value.forEach((item) => {
    const sortedContent: { [key: string]: any } = {};
    const keys = Object.keys(item.contents).sort();

    keys.forEach((key) => {
      sortedContent[key] = item.contents[key];
    });

    item.contents = sortedContent;
  });

  defaultLayout.value = emailList.value.filter(
    (item) => item.title == "DefaultLayout"
  )[0];

  isLoading.value = false;
};

const showEmailDetail = (detail) => {
  emailDetailRef.value.show(detail, defaultLayout.value);
};

const detail = (item) => {
  emailDetailRefTwo.value.show(item, defaultLayout.value);
};

const onCreateSubmitted = () => {
  fetchData();
};
</script>

<style scoped lang="scss"></style>
